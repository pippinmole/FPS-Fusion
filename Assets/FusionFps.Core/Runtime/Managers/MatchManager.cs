using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace FusionFps.Core {
    public interface IMatchManager {
        EGameState GameState { get; }
        GameStateData GameData { get; set; }
        NetworkDictionary<PlayerRef, PlayerController> Players { get; }
        bool IsRunning { get; }
        bool IsServer { get; }
        
        event Action<NetworkRunner, List<PlayerRef>> Connected;
        event Action<NetworkRunner, PlayerRef> PlayerJoined;
        event Action<NetworkRunner, PlayerRef> PlayerLeft;
        event Action<EGameState> GameStateChanged;

        void LoadSessionMap();
        void StartGame();
    }

    public struct GameStateData : INetworkStruct {
        public TickTimer Countdown { get; set; }
    }

    public enum EGameState {
        None,
        LobbyConnected,
        GameInProgress
    }
    
    /// <summary>
    /// This is the store of all networked data pertaining to the game state. This 'Game State' is referring to time between
    /// starting a game from the lobby, and the finish of a game (and back to the lobby).
    /// </summary>
    internal class MatchManager : NetworkBehaviour, IMatchManager, INetworkRunnerCallbacks {

        [Networked(OnChanged = nameof(OnGameStateChanged))]
        public EGameState GameState { get; private set; } = EGameState.None;

        [Networked] public GameStateData GameData { get; set; }
        [Networked] public NetworkDictionary<PlayerRef, PlayerController> Players { get; }

        public bool IsRunning => Object != null && Object.IsValid;
        public bool IsServer => Runner != null && Runner.IsServer; 

        [SerializeField] private float _matchTimeInSeconds = 120f;
        [SerializeField] private PlayerController _playerPrefab;

        public event Action<NetworkRunner, List<PlayerRef>> Connected;
        public event Action<NetworkRunner, PlayerRef> PlayerJoined;
        public event Action<NetworkRunner, PlayerRef> PlayerLeft;

        public event Action<EGameState> GameStateChanged;

        private PlayerSpawnManager _spawnManager;

        private void Awake() {
            SingletonProvider.AddSingleton<IMatchManager>(() => this);

            _spawnManager = FindObjectOfType<PlayerSpawnManager>();
        }

        public override void Spawned() {
            base.Spawned();

            DontDestroyOnLoad(gameObject);

            Runner.AddCallbacks(this);

            GameState = EGameState.LobbyConnected;

            var players = new List<PlayerRef>();
            foreach ( var (player, _) in Players ) players.Add(player);

            Connected?.Invoke(Runner, players);
        }

        public override void Despawned(NetworkRunner runner, bool hasState) {
            base.Despawned(runner, hasState);

            GameState = EGameState.None;
            GameStateChanged?.Invoke(EGameState.None);
        }

        public override void FixedUpdateNetwork() {
            base.FixedUpdateNetwork();

            // Handle game ending
            if ( GameData.Countdown.ExpiredOrNotRunning(Runner) && GameState == EGameState.GameInProgress ) {
                // End game
                Debug.Log($"Match ended on tick {(int) Runner.Simulation.Tick}");

                // Clear players from field
                DespawnAllPlayers();

                // Reset the state of the game
                GameState = EGameState.LobbyConnected;
            }
        }

        public void LoadSessionMap() {
            Debug.Log($"SessionProperties IsReady: {Runner.SessionInfo.IsValid}");

            var sessionProperties = Runner.SessionInfo.Properties;
            var mapBuildIndex = (int) sessionProperties["mapBuildIndex"];

            Runner.SetActiveScene(mapBuildIndex);
        }

        private void SpawnAllPlayers() {
            foreach ( var (player, _) in Players ) {
                var spawnPoint = _spawnManager.GetNextSpawnPoint(Runner, player);
                var playerObject = Runner.Spawn(_playerPrefab, spawnPoint.position, Quaternion.identity, player);

                Debug.Log($"Spawning player at {spawnPoint.position}");

                Players.Set(player, playerObject);
            }
        }

        private void DespawnAllPlayers() {
            foreach ( var (player, controller) in Players ) {
                if ( controller == null ) continue;

                Runner.Despawn(controller.Object);

                Players.Set(player, null);
            }
        }

        public void StartGame() {
            if ( !Runner.IsServer ) return;
            if ( GameState != EGameState.LobbyConnected ) return;

            SpawnAllPlayers();

            GameState = EGameState.GameInProgress;
            GameData = new GameStateData {
                Countdown = TickTimer.CreateFromSeconds(Runner, _matchTimeInSeconds)
            };

            Debug.Log($"Match is set to end on tick {GameData.Countdown.TargetTick}");
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
            Players.Add(player, null);

            PlayerJoined?.Invoke(Runner, player);
        }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
            if ( !Players.ContainsKey(player) ) return;

            var controller = Players[player];
            if ( controller != null ) {
                runner.Despawn(controller.Object);
            }

            Players.Remove(player);
            PlayerLeft?.Invoke(Runner, player);
        }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> list) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }

        private static void OnGameStateChanged(Changed<MatchManager> changed) {
            changed.Behaviour.GameStateChanged?.Invoke(changed.Behaviour.GameState);
        }
    }
}