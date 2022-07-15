using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using Steamworks;
using UnityEngine;

[DisallowMultipleComponent]
public class LobbyPlayer : NetworkBehaviour, INetworkRunnerCallbacks {

    public static readonly List<LobbyPlayer> Players = new();

    public static event Action<NetworkRunner> Connected;
    public static event Action<NetworkRunner, LobbyPlayer> PlayerJoined;
    public static event Action<NetworkRunner, LobbyPlayer> PlayerLeft;

    [Networked, HideInInspector] public PlayerController Controller { get; private set; }
    [Networked] public ulong SteamId { get; set; }

    [SerializeField] protected PlayerController _playerPrefab;
    
    public override void Spawned() {
        base.Spawned();

        if ( Object.HasInputAuthority ) {
            RPC_SetSteamId(SteamClient.SteamId);
        }
        
        Players.Add(this);
        Connected?.Invoke(Runner);
        PlayerJoined?.Invoke(Runner, this);

        DontDestroyOnLoad(gameObject);
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    private void RPC_SetSteamId(ulong steamId) {
        SteamId = steamId;
    }
    
    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);

        PlayerLeft?.Invoke(runner, this);
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
    public virtual void OnSceneLoadDone(NetworkRunner runner) { }
    public virtual void OnSceneLoadStart(NetworkRunner runner) { }
    
    protected static LobbyPlayer GetPlayer(PlayerRef player) => Players.First(x => x.Object.InputAuthority == player);
}