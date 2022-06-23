using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using StarterAssets;
using UnityEngine;

public class LobbyManager : NetworkBehaviour, INetworkRunnerCallbacks {

    public struct GameStateData : INetworkStruct {
        
        public TickTimer Countdown { get; set; }

    }
    
    public enum EGameState {
        Lobby,
        GameInProgress
    }
    
    [Networked] public NetworkDictionary<PlayerRef, PlayerController> Players { get; }
    [Networked] public EGameState GameState { get; set; }
    [Networked] public GameStateData GameData { get; set; }

    [SerializeField] private PlayerController _playerPrefab;
    [SerializeField] private float _matchTimeInSeconds = 3 * 60f;

    public static event Action<List<PlayerRef>> Connected; 
    public static event Action<PlayerRef> PlayerJoined;
    public static event Action<PlayerRef> PlayerLeft;

    public static LobbyManager Instance;

    private PlayerSpawnPointManagerPrototype _spawnManager;

    private void Awake() {
        _spawnManager = GetComponent<PlayerSpawnPointManagerPrototype>();

        Instance = this;
    }

    public override void Spawned() {
        base.Spawned();

        Runner.AddCallbacks(this);
        GameState = EGameState.Lobby;

        var players = new List<PlayerRef>();
        foreach ( var (player, _) in Players ) {
            players.Add(player);
        }
        
        Connected?.Invoke(players);
    }

    public override void FixedUpdateNetwork() {
        base.FixedUpdateNetwork();
        
        // Handle game ending
        if ( GameData.Countdown.ExpiredOrNotRunning(Runner) && GameState == EGameState.GameInProgress ) {
            // End game
            Debug.Log($"Match ended on tick {Runner.Simulation.Tick}");
            
            // Clear players from field
            DespawnAllPlayers();

            // Reset the state of the game
            GameState = EGameState.Lobby;
        }
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
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    public static void StartGame() {
        Instance.StartGame_Internal();
    }
    
    private void StartGame_Internal() {
        if ( !Runner.IsServer ) return;
        if ( GameState != EGameState.Lobby ) return;

        SpawnAllPlayers();

        GameState = EGameState.GameInProgress;
        GameData = new GameStateData {
            Countdown = TickTimer.CreateFromSeconds(Runner, _matchTimeInSeconds)
        };
        
        Debug.Log($"Match is set to end on tick {GameData.Countdown.TargetTick}");
    }

    private void SpawnAllPlayers() {
        foreach ( var (player, _) in Players ) {
            var spawnPoint = _spawnManager.GetNextSpawnPoint(Runner, player);
            var playerObject = Runner.Spawn(_playerPrefab, spawnPoint.position, Quaternion.identity, player);
            
            playerObject.GetComponent<NetworkCharacterControllerPrototype>().TeleportToPosition(spawnPoint.position);
            
            Debug.Log($"Spawning player at {spawnPoint.position}");
            
            playerObject.Object.AssignInputAuthority(player);
            
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
}