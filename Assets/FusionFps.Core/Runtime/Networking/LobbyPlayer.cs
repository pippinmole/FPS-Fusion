using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

[DisallowMultipleComponent]
public class LobbyPlayer : NetworkBehaviour, INetworkRunnerCallbacks {

    public static readonly List<LobbyPlayer> Players = new();

    public static event Action<NetworkRunner> Connected;
    public static event Action<NetworkRunner, PlayerRef> PlayerJoined;
    public static event Action<NetworkRunner, PlayerRef> PlayerLeft;

    [Networked, HideInInspector] public PlayerController Controller { get; set; }

    [SerializeField] protected PlayerController _playerPrefab;
    
    public override void Spawned() {
        base.Spawned();

        Players.Add(this);
        Connected?.Invoke(Runner);
        
        DontDestroyOnLoad(gameObject);
    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);

        Players.Remove(this);
    }
    
    /// <summary>
    /// Spawns the player at a random spawn point in the given PlayerSpawnManager set of spawn points.
    /// </summary>
    /// <param name="spawnManager">The spawn manager to choose a spawn point from</param>
    /// <param name="player"></param>
    public void Spawn(PlayerSpawnManager spawnManager, PlayerRef player) {
        if ( !Runner.IsServer ) return;
        
        var spawnPoint = spawnManager.GetNextSpawnPoint(Runner, player);
        
        Controller = Runner.Spawn(_playerPrefab, spawnPoint.position, spawnPoint.rotation, player);
        Controller.CanMove = false;

        Debug.Log($"Spawned {player} at {spawnPoint.position}");
    }

    public virtual void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
        PlayerJoined?.Invoke(runner, player);
    }

    public virtual void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        PlayerLeft?.Invoke(runner, player);
    }
    
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
    public virtual void OnSceneLoadDone(NetworkRunner runner) { }
    public virtual void OnSceneLoadStart(NetworkRunner runner) { }
    
    protected static LobbyPlayer GetPlayer(PlayerRef player) => Players.First(x => x.Object.InputAuthority == player);
}