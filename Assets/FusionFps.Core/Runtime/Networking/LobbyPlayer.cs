using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using FusionFps.Core;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ETeam {
    None,
    Red,
    Blue,
    Spectator
}

[DisallowMultipleComponent]
public class LobbyPlayer : NetworkBehaviour, INetworkRunnerCallbacks {

    public static readonly List<LobbyPlayer> Players = new();

    public static event Action<NetworkRunner> Connected;
    public static event Action<NetworkRunner, LobbyPlayer> Joined;
    public static event Action<NetworkRunner, LobbyPlayer> Left;

    public event Action<NetworkRunner, PlayerController> PlayerSpawned;

    [Networked(OnChanged = nameof(ControllerChanged)), HideInInspector]
    public PlayerController Controller { get; private set; }

    [Networked] public string Username { get; set; }
    [Networked] public ulong SteamId { get; set; }

    [Networked(OnChanged = nameof(TeamChanged))]
    public ETeam Team { get; set; } = ETeam.None;

    [SerializeField] private IngameUI _uiPrefab;
    [SerializeField] private SpectatorCamera _spectatorPrefab;
    [SerializeField] protected PlayerController _playerPrefab;

    private SpectatorCamera _spectatorCamera;
    private IngameUI _ui;
    
    public static LobbyPlayer LocalPlayer { get; private set; }
    
    public override void Spawned() {
        base.Spawned();

        Runner.AddCallbacks(this);
        
        if ( Object.HasInputAuthority ) {
            LocalPlayer = this;
            
            RPC_SetSteamId(SteamClient.SteamId, SteamClient.Name);
        }
        
        Players.Add(this);
        Connected?.Invoke(Runner);
        Joined?.Invoke(Runner, this);

        DontDestroyOnLoad(gameObject);
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    private void RPC_SetSteamId(ulong steamId, string username) {
        SteamId = steamId;
        Username = username;
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_SetTeam(ETeam team) {
        Team = team;
    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);

        Left?.Invoke(runner, this);
        Players.Remove(this);
    }
    
    /// <summary>
    /// Spawns the player at a random spawn point in the given PlayerSpawnManager set of spawn points.
    /// </summary>
    /// <param name="spawnManager">The spawn manager to choose a spawn point from</param>
    /// <param name="player">The input authority for this player</param>
    public void Spawn(PlayerSpawnManager spawnManager, PlayerRef player) {
        if ( !Runner.IsServer ) return;
        
        var spawnPoint = spawnManager.GetNextSpawnPoint(Runner, player);
        
        Controller = Runner.Spawn(_playerPrefab, spawnPoint.position, spawnPoint.rotation, player);

        Debug.Log($"Spawned {player} at {spawnPoint.position}");
    }

    /// <summary>
    /// Removes the player controller object from the scene.
    /// </summary>
    public void Despawn() {
        if ( !Runner.IsServer ) return;
        if ( Controller == null ) return;
        
        Runner.Despawn(Controller.Object);
    }

    public virtual void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public virtual void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public virtual void OnInput(NetworkRunner runner, NetworkInput input) { }
    public virtual void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public virtual void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public virtual void OnConnectedToServer(NetworkRunner runner) { }
    public virtual void OnDisconnectedFromServer(NetworkRunner runner) { }
    public virtual void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
        byte[] token) { }
    public virtual void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public virtual void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public virtual void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public virtual void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public virtual void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public virtual void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

    public virtual void OnSceneLoadDone(NetworkRunner runner) {
        var matchScene = (int) runner.SessionInfo.Properties[SessionPropertyProps.MapName];
        var currentScene = SceneManager.GetActiveScene().buildIndex;
        
        if ( currentScene == matchScene) {
            // Spawn UI?
            _ui = Instantiate(_uiPrefab);
            _ui.Setup(this, Runner);
        }
    }
    public virtual void OnSceneLoadStart(NetworkRunner runner) { }

    private static void ControllerChanged(Changed<LobbyPlayer> changed) {
        var runner = changed.Behaviour.Runner;
        var controller = changed.Behaviour.Controller;
        
        changed.Behaviour.PlayerSpawned?.Invoke(runner, controller);
    }
    
    private static void TeamChanged(Changed<LobbyPlayer> changed) {
        Debug.Log($"Team changed to {changed.Behaviour.Team}");
        var behaviour = changed.Behaviour;
        var team = behaviour.Team;

        if ( behaviour.Object.HasInputAuthority ) {
            if ( team == ETeam.Spectator ) {
                behaviour._spectatorCamera = Instantiate(behaviour._spectatorPrefab);
            } else {
                var spec = behaviour._spectatorCamera;
                if ( spec != null ) Destroy(spec.gameObject);
            }
        }

        if ( behaviour.Runner.IsServer ) {
            var isWaitingForPlayers = MatchManager.Instance.IsWaitingForPlayers;

            // De-spawn the current controller
            behaviour.Despawn();
            
            // if we're still waiting for players and they arent a spectator, we can spawn them.
            if ( team != ETeam.Spectator ) {
                if ( isWaitingForPlayers ) {
                    behaviour.Spawn(FindObjectOfType<PlayerSpawnManager>(), behaviour.Object.InputAuthority);
                }
            }
        }
    }

    public static LobbyPlayer GetPlayer(PlayerRef player) =>
        Players.FirstOrDefault(x => x.Object != null && x.Object.InputAuthority == player);
}