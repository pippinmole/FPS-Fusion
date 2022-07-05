using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ConnectionStatus {
    Disconnected,
    Connecting,
    Failed,
    Connected
}

namespace FusionFps.Core {

    public interface ISessionManager {
        public event Action<List<SessionInfo>> SessionListUpdated;
        public event Action<NetworkRunner> SessionJoined;
        public event Action<NetworkRunner> SessionLeft;
        public event Action<NetworkRunner> SessionCreated;
        public event Action<NetworkRunner> SessionDestroyed;
        public event Action<NetworkRunner, ShutdownReason> RunnerShutdown;
        public event Action<NetworkRunner, ConnectionStatus> ConnectionStatusChanged;
        
        bool IsBusy { get; }
        bool IsInSession { get; }
        ConnectionStatus ConnectionStatus { get; }

        Task<StartGameResult> CreateSession(string lobbyName, Dictionary<string, SessionProperty> sessionProperties);
        Task<StartGameResult> StartClient();
        Task<StartGameResult> JoinSession(string session);
        Task Shutdown();
    }
    
    internal class SessionManager : MonoBehaviour, ISessionManager, INetworkRunnerCallbacks {
        
        public event Action<List<SessionInfo>> SessionListUpdated;
        public event Action<NetworkRunner> SessionJoined;
        public event Action<NetworkRunner> SessionLeft;
        public event Action<NetworkRunner> SessionCreated;
        public event Action<NetworkRunner> SessionDestroyed;
        public event Action<NetworkRunner, ShutdownReason> RunnerShutdown;
        public event Action<NetworkRunner, ConnectionStatus> ConnectionStatusChanged;

        public ConnectionStatus ConnectionStatus { get; private set; } = ConnectionStatus.Disconnected;
        public bool IsInSession => ConnectionStatus is ConnectionStatus.Connected;
        public bool IsBusy { get; private set; }

        private AuthTicket _authTicket;
        private NetworkRunner _runner;

        private void Awake() {
            // Register accessor method for singleton
            ServiceProvider.AddSingleton<ISessionManager>(() => this);
        }

        private async Task SetupRunner(GameMode mode) {
            if ( _runner != null ) {
                Debug.LogWarning("A runner exists when trying to create another, destroying.");
                await Shutdown();
            }

            var go = new GameObject("Runner Object");
            _runner = go.AddComponent<NetworkRunner>();
            _runner.ProvideInput = mode != GameMode.Server;
            _runner.AddCallbacks(this);
        }

        public async Task<StartGameResult> CreateSession(string lobbyName,
            Dictionary<string, SessionProperty> sessionProperties) {
            IsBusy = true;

            SetConnectionStatus(ConnectionStatus.Connecting);

            await SetupRunner(GameMode.Host);

            var sceneManager = _runner.GetComponent<INetworkSceneManager>() ??
                               _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

            //
            // Remember: You need to provide a scene index for scene objects to attach to FUN in that scene
            //
            var steamAuth = GetSteamAuthenticationValues();
            var args = new StartGameArgs {
                GameMode = GameMode.Host,
                Address = NetAddress.Any(27015),
                Scene = SceneManager.GetActiveScene().buildIndex,
                SessionName = lobbyName,
                SessionProperties = sessionProperties,
                SceneManager = sceneManager,
                PlayerCount = 8,
                DisableClientSessionCreation = true,
                AuthValues = steamAuth
            };

            Debug.Log($"Starting game with game mode {args.GameMode}");
            var result = await _runner.StartGame(args);

            if ( result.Ok ) {
                Debug.Log($"Successfully started session");

                SessionCreated?.Invoke(_runner);

                // For some reason OnConnectedToServer does not run as a host when creating a session
                SessionJoined?.Invoke(_runner);
            } else {
                Debug.LogError($"Failed to Start: {result.ShutdownReason}");

                await Shutdown();
            }

            IsBusy = false;

            return result;
        }

        public async Task<StartGameResult> StartClient() {
            IsBusy = true;

            SetConnectionStatus(ConnectionStatus.Connecting);

            await SetupRunner(GameMode.Client);

            var result = await _runner.JoinSessionLobby(SessionLobby.ClientServer);

            if ( result.Ok ) {
                Debug.Log("Started client successfully");

                SetConnectionStatus(ConnectionStatus.Connected);
            } else {
                Debug.LogError($"Failed to start client with ShutdownReason: {result.ShutdownReason}");
            }

            IsBusy = false;

            return result;
        }

        public async Task<StartGameResult> JoinSession(string session) {
            if ( _runner == null )
                throw new InvalidOperationException("Cannot join a session without a runner!");

            await SetupRunner(GameMode.Client);

            var steamAuth = GetSteamAuthenticationValues();
            var args = new StartGameArgs {
                GameMode = GameMode.Client,
                SessionName = session,
                AuthValues = steamAuth
            };

            Debug.Log($"Joining session {session} with auth ticket type of {steamAuth.AuthType}");

            var result = await _runner.StartGame(args);

            if ( result.Ok ) { } else {
                await Shutdown();
            }
            
            IsBusy = false;

            return result;
        }

        private AuthenticationValues GetSteamAuthenticationValues() {
            var auth = GetSteamAuthTicket();
            var authValues = new AuthenticationValues();
            authValues.UserId = SteamClient.SteamId.ToString();
            authValues.AuthType = CustomAuthenticationType.Steam;
            authValues.AddAuthParameter("ticket", auth);
            return authValues;
        }
        
        private string GetSteamAuthTicket() {
            _authTicket ??= SteamUser.GetAuthSessionTicket();
            
            var ticketString = new StringBuilder();
            foreach ( var b in _authTicket.Data ) {
                ticketString.AppendFormat("{0:x2}", b);
            }

            ticketString[0] = ' ';

            return ticketString.ToString();
        }

        private void SetConnectionStatus(ConnectionStatus status) {
            ConnectionStatus = status;
            ConnectionStatusChanged?.Invoke(_runner, status);
        }

        public Task Shutdown() {
            if ( _runner != null ) {
                return _runner.Shutdown();
            } else {
                SetConnectionStatus(ConnectionStatus.Disconnected);
                return Task.CompletedTask;
            }
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
            SetConnectionStatus(ConnectionStatus.Connected);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
            Debug.Log($"{player.PlayerId} disconnected.");

            // RoomPlayer.RemovePlayer(runner, player);

            SetConnectionStatus(ConnectionStatus);
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
            SetConnectionStatus(ConnectionStatus.Disconnected);

            SessionDestroyed?.Invoke(runner);
            SessionLeft?.Invoke(runner);

            RunnerShutdown?.Invoke(runner, shutdownReason);

            // (string status, string message) = ShutdownReasonToHuman(shutdownReason);
            // _disconnectUI.ShowMessage( status, message);
            //
            // RoomPlayer.Players.Clear();

            // Clear steam ticket data
            if ( _authTicket != null ) {
                _authTicket.Cancel();
                _authTicket = null;
            }
            
            if ( _runner )
                Destroy(_runner.gameObject);

            // Reset the object pools
            // _pool.ClearPools();
            // _pool = null;

            _runner = null;
        }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnConnectedToServer(NetworkRunner runner) {
            Debug.Log("connected to server...");
            SetConnectionStatus(ConnectionStatus.Connected);
            SessionJoined?.Invoke(runner);
        }

        public void OnDisconnectedFromServer(NetworkRunner runner) {
            SessionLeft?.Invoke(runner);
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token) {

            // var playerToken = new PlayerConnectionToken(token);
            // Debug.Log($"Player connecting with steam id: {playerToken.SteamId}");

            // authenticate the token before accepting
            
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> list) {
            Debug.Log("[Session Manager] Session list updated");
            SessionListUpdated?.Invoke(list);
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}