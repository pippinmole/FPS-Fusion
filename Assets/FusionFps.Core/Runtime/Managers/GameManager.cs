using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

/// <summary>
/// This is the store of all networked data pertaining to the game state. This 'Game State' is referring to time between
/// starting a game from the lobby, and the finish of a game (and back to the lobby).
/// </summary>
public class GameManager : NetworkBehaviour, INetworkRunnerCallbacks {
    
    public struct GameStateData : INetworkStruct {
        public TickTimer Countdown { get; set; }
    }
    
    public enum EGameState {
        None,
        LobbyConnected,
        GameInProgress
    }

    [Networked(OnChanged = nameof(OnGameStateChanged))]
    public EGameState GameState { get; set; } = EGameState.None;
    [Networked] public GameStateData GameData { get; set; }
    [Networked] public NetworkDictionary<PlayerRef, PlayerController> Players { get; }

    public bool IsRunning => Object != null && Object.IsValid;
    
    [SerializeField] private float _matchTimeInSeconds = 120f;
    [SerializeField] private PlayerController _playerPrefab;
    
    public static event Action<List<PlayerRef>> Connected; 
    public static event Action<PlayerRef> PlayerJoined;
    public static event Action<PlayerRef> PlayerLeft;

    public static event Action<EGameState> GameStateChanged;

    public static GameManager Instance;
    
    private NetworkRunner _runner;
    private PlayerSpawnManager _spawnManager;

    private void Awake() {
        Instance = this;
        
        _spawnManager = FindObjectOfType<PlayerSpawnManager>();
    }

    public override void Spawned() {
        base.Spawned();
        
        DontDestroyOnLoad(gameObject);

        Runner.AddCallbacks(this);

        GameState = EGameState.LobbyConnected;

        var players = new List<PlayerRef>();
        foreach ( var (player, _) in Players ) players.Add(player);

        Connected?.Invoke(players);
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
        
        PlayerJoined?.Invoke(player);
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        if ( !Players.ContainsKey(player) ) return;

        var controller = Players[player];
        if ( controller != null ) {
            runner.Despawn(controller.Object);
        }
        
        Players.Remove(player);
        PlayerLeft?.Invoke(player);
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

    private static void OnGameStateChanged(Changed<GameManager> changed) {
        GameStateChanged?.Invoke(changed.Behaviour.GameState);
    }
}