using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ConnectionStatus {
    Disconnected,
    Connecting,
    Failed,
    Connected
}

public class SessionManager : MonoBehaviour, INetworkRunnerCallbacks {

    public static ConnectionStatus ConnectionStatus = ConnectionStatus.Disconnected;

    public static event Action<List<SessionInfo>> SessionListUpdated;

    public static SessionManager Instance;

    private NetworkRunner _runner;

    private void Awake() {
        Instance = this;
    }

    private void SetupRunner(GameMode mode) {
        if ( _runner != null ) {
            Debug.LogWarning("A runner exists when trying to create another, destroying.");
            Shutdown();
        }
        
        var go = new GameObject("Runner Object");
        _runner = go.AddComponent<NetworkRunner>();
        _runner.ProvideInput = mode != GameMode.Server;
        _runner.AddCallbacks(this);
    }

    public async Task CreateSession(string lobbyName) {
        SetConnectionStatus(ConnectionStatus.Connecting);
        
        SetupRunner(GameMode.Host);
        
        var sceneManager = _runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();
        if ( sceneManager == null ) {
            // Debug.Log($"NetworkRunner does not have any component implementing {nameof(INetworkSceneManager)} interface, adding {nameof(NetworkSceneManagerDefault)}.",
            //     _runner);
            sceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }
        
        //
        // Remember: You need to provide a scene index for scene objects to attach to FUN
        //
        var args = new StartGameArgs {
            GameMode = GameMode.Host,
            Address = NetAddress.Any(27015),
            Scene = SceneManager.GetActiveScene().buildIndex,
            SessionName = lobbyName,
            SceneManager = sceneManager,
            PlayerCount = 8,
            DisableClientSessionCreation = true
        };
        
        Debug.Log($"Starting game with game mode {args.GameMode}");
        var result = await _runner.StartGame(args);
        
        if (result.Ok) {
            Debug.Log($"Successfully started session");
        } else {
            Debug.LogError($"Failed to Start: {result.ShutdownReason}");
        }
    }

    public async void StartClient() {
        SetupRunner(GameMode.Client);

        var result = await _runner.JoinSessionLobby(SessionLobby.ClientServer);

        if ( result.Ok ) {
            Debug.Log("Started client successfully");
        } else {
            Debug.LogError($"Failed to start client with ShutdownReason: {result.ShutdownReason}");
        }
    }

    public void JoinSession(string session) {
        if ( _runner == null )
            throw new InvalidOperationException("Cannot join a session without a runner!");

        var args = new StartGameArgs {
            GameMode = GameMode.Client,
            SessionName = session,
        };

        _runner.StartGame(args);
    }

    private void SetConnectionStatus(ConnectionStatus status) {
        ConnectionStatus = status;
    }
    
    public void Shutdown() {
        if ( _runner != null ) {
            _runner.Shutdown();
        } else {
            SetConnectionStatus(ConnectionStatus.Disconnected);
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
        SetConnectionStatus(ConnectionStatus.Connected);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"{player.PlayerId} disconnected.");

        // RoomPlayer.RemovePlayer(runner, player);

        SetConnectionStatus(ConnectionStatus);
    }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        SetConnectionStatus(ConnectionStatus.Disconnected);

        // (string status, string message) = ShutdownReasonToHuman(shutdownReason);
        // _disconnectUI.ShowMessage( status, message);
        //
        // RoomPlayer.Players.Clear();

        if(_runner)
            Destroy(_runner.gameObject);
		
        // Reset the object pools
        // _pool.ClearPools();
        // _pool = null;

        _runner = null;
    }
    
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
        byte[] token) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> list) {
        Debug.LogError("Session list updated");
        SessionListUpdated?.Invoke(list);
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}