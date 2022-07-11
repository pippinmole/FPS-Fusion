using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace FusionFps.Core {
    public interface IMatchManager {
        EGameState GameState { get; }
        TickTimer WaitForPlayersTimer { get; }
        TickTimer Countdown { get; }
        NetworkDictionary<PlayerRef, PlayerController> Players { get; }
        bool IsRunning { get; }
        bool IsServer { get; }

        event Action<NetworkRunner, List<PlayerRef>> Connected;
        event Action<NetworkRunner, PlayerRef> PlayerJoined;
        event Action<NetworkRunner, PlayerRef> PlayerLeft;
        event Action<EGameState> GameStateChanged;
        event Action<NetworkRunner> MatchLoaded; 

        void LoadSessionMap();
        void OnMatchLoaded();
    }

    public enum EGameState {
        None,
        LobbyConnected,
        WaitingForPlayers,
        GameInProgress
    }
    
    /// <summary>
    /// This is the store of all networked data pertaining to the game state. This 'Game State' is referring to time between
    /// starting a game from the lobby, and the finish of a game (and back to the lobby).
    /// </summary>
    internal class MatchManager : NetworkBehaviour, IMatchManager, INetworkRunnerCallbacks {

        [Networked(OnChanged = nameof(OnGameStateChanged))]
        public EGameState GameState { get; private set; } = EGameState.None;

        [Networked] public TickTimer WaitForPlayersTimer { get; private set; }
        [Networked] public TickTimer Countdown { get; private set; }
        [Networked] public NetworkDictionary<PlayerRef, PlayerController> Players { get; }

        public bool IsRunning => Object != null && Object.IsValid;
        public bool IsServer => Runner != null && Runner.IsServer; 

        [SerializeField] private float _matchTimeInSeconds = 120f;
        [SerializeField] private float _waitForPlayersTimer = 15f;
        [SerializeField] private PlayerController _playerPrefab;

        public event Action<NetworkRunner, List<PlayerRef>> Connected;
        public event Action<NetworkRunner, PlayerRef> PlayerJoined;
        public event Action<NetworkRunner, PlayerRef> PlayerLeft;
        public event Action<NetworkRunner> MatchLoaded;

        public event Action<EGameState> GameStateChanged;
        
        private void Awake() {
            ServiceProvider.AddSingleton<IMatchManager>(() => this);
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

            // Handle wait for players timer
            if ( WaitForPlayersTimer.ExpiredOrNotRunning(Runner) && GameState == EGameState.WaitingForPlayers ) {
                if ( !Runner.IsServer ) return;

                GameState = EGameState.GameInProgress;
                Countdown = TickTimer.CreateFromSeconds(Runner, _matchTimeInSeconds);

                SetAllPlayers(true);
                
                Debug.Log($"Match is set to end on tick {Countdown.TargetTick}");
            }
            
            // Handle game ending
            if ( Countdown.ExpiredOrNotRunning(Runner) && GameState == EGameState.GameInProgress ) {
                // End game
                Debug.Log($"Match ended on tick {(int) Runner.Simulation.Tick}");

                // Clear players from field
                DespawnAllPlayers();

                // Reset the state of the game
                GameState = EGameState.LobbyConnected;
            }
        }

        public void LoadSessionMap() {
            if ( !Runner.IsServer ) return;

            var sessionProperties = Runner.SessionInfo.Properties;
            var mapBuildIndex = (int) sessionProperties["mapBuildIndex"];

            GameState = EGameState.WaitingForPlayers;
            WaitForPlayersTimer = TickTimer.CreateFromSeconds(Runner, _waitForPlayersTimer);
            
            Runner.SetActiveScene(mapBuildIndex);
        }

        private void SpawnAllPlayers(PlayerSpawnManager spawnManager) {
            foreach ( var (player, _) in Players ) {
                SpawnPlayer(spawnManager, player);
            }
        }

        private void SetAllPlayers(bool active) {
            foreach ( var (_, controller) in Players ) {
                if ( controller == null ) continue;

                controller.CanMove = active;
            }
        }

        private void SpawnPlayer(PlayerSpawnManager spawnManager, PlayerRef player) {
            var spawnPoint = spawnManager.GetNextSpawnPoint(Runner, player);
            var playerObject = Runner.Spawn(_playerPrefab, spawnPoint.position, Quaternion.identity, player);

            playerObject.CanMove = false;
            
            Debug.Log($"Spawning player at {spawnPoint.position}");

            Players.Set(player, playerObject);
        }

        private void DespawnAllPlayers() {
            foreach ( var (player, controller) in Players ) {
                if ( controller == null ) continue;

                Runner.Despawn(controller.Object);

                Players.Set(player, null);
            }
        }

        public void OnMatchLoaded() {
            if ( Object.HasStateAuthority ) {
                var spawnManager = FindObjectOfType<PlayerSpawnManager>();
                SpawnAllPlayers(spawnManager);
            }

            MatchLoaded?.Invoke(Runner);
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
            Players.Add(player, null);
            PlayerJoined?.Invoke(Runner, player);

            if ( GameState == EGameState.GameInProgress ) {
                // Spawn them as a spectator
                throw new NotImplementedException();
            } else if ( GameState == EGameState.WaitingForPlayers ) {
                // Spawn them as a player
                var spawnManager = FindObjectOfType<PlayerSpawnManager>();
                SpawnPlayer(spawnManager, player);
            }
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

        public void OnSceneLoadStart(NetworkRunner runner) {
            Debug.Log("Loaded scene");
        }

        private static void OnGameStateChanged(Changed<MatchManager> changed) {
            changed.Behaviour.GameStateChanged?.Invoke(changed.Behaviour.GameState);
        }
    }
}