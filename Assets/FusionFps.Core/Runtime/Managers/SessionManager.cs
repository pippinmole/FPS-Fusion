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
using NetAddress = Fusion.Sockets.NetAddress;

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
        bool IsSessionOwner { get; }
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
        public bool IsSessionOwner => _matchManager != null && _matchManager.IsServer;
        public bool IsBusy { get; private set; }

        [SerializeField] private ushort _port = 27271;
        [SerializeField] private bool _steamAuth = true;
        [SerializeField] private MatchManager _matchManagerPrefab;
        [SerializeField] private ServerLobbyPlayer _serverPlayerPrefab;
        [SerializeField] private LobbyPlayer _clientPlayerPrefab;

        private MatchManager _matchManager;
        private AuthTicket _authTicket;
        private NetworkRunner _runner;

        private void Awake() {
            ServiceProvider.AddSingleton<ISessionManager>(() => this);
        }

        private async Task SetupRunner(GameMode mode) {
            //
            // This is a small hack - basically a 'Client' will not be established as a client if they are on the 
            // server list.
            //

            if ( (_runner != null && _runner.GameMode != mode) || (_runner != null && _runner.GameMode == 0 && mode == GameMode.Client) ) {
                Debug.LogWarning(
                    $"A runner exists when trying to create another in mode {mode} (Current mode: {_runner.GameMode}), destroying.");
                await Shutdown();
            }

            var go = new GameObject("Runner Object");
            _runner = go.AddComponent<NetworkRunner>();
            _runner.ProvideInput = mode != GameMode.Server;
            _runner.AddCallbacks(this);
        }

        public async Task<StartGameResult> CreateSession(string lobbyName, Dictionary<string, SessionProperty> sessionProperties) {
            IsBusy = true;

            SetConnectionStatus(ConnectionStatus.Connecting);

            await SetupRunner(GameMode.Host);

            var sceneManager = _runner.GetComponent<INetworkSceneManager>() ??
                               _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

            // Add steam lobby
            var lobby = await SteamMatchmaking.CreateLobbyAsync();

            if ( lobby.HasValue ) {
                sessionProperties.Add(SessionPropertyProps.SteamLobbyId, lobby.Value.Id.ToString());
            }
            
            //
            // Remember: You need to provide a scene index for scene objects to attach to FUN in that scene
            //
            var steamAuth = GetSteamAuthenticationValues();
            var args = new StartGameArgs {
                GameMode = GameMode.Host,
                Address = NetAddress.Any(_port),
                Scene = SceneManager.GetActiveScene().buildIndex,
                SessionName = lobbyName,
                SessionProperties = sessionProperties,
                SceneManager = sceneManager,
                PlayerCount = 8,
                DisableClientSessionCreation = true,
                AuthValues = _steamAuth ? steamAuth : null
            };

            Debug.Log($"Starting game with game mode {args.GameMode}");
            var result = await _runner.StartGame(args);

            if ( result.Ok ) {
                Debug.Log($"Successfully started session");

                if ( lobby.HasValue ) {
                    lobby.Value.SetData(SteamLobbyProps.PhotonLobbyName, _runner.SessionInfo.Name);
                    lobby.Value.SetPublic();
                    
                    Debug.Log($"[SessionManager] Successfully created steam lobby of id {lobby.Value.Id}");
                } else {
                    Debug.LogError("[SessionManager] Failed to create steam lobby. You won't be able to invite friends");
                }

                _matchManager = _runner.Spawn(_matchManagerPrefab);
                
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
            if ( _runner == null ) {
                await StartClient();
            }

            IsBusy = true;

            await SetupRunner(GameMode.Client);

            var steamAuth = GetSteamAuthenticationValues();
            var args = new StartGameArgs {
                GameMode = GameMode.Client,
                SessionName = session,
                AuthValues = _steamAuth ? steamAuth : null
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

            if ( runner.IsServer ) {
                var spawnServer = player == runner.LocalPlayer;
                ulong steamId = 0U; // How do access the steam id from PlayerRef?

                runner.Spawn(
                    prefab: spawnServer ? _serverPlayerPrefab : _clientPlayerPrefab,
                    position: Vector3.zero,
                    rotation: Quaternion.identity,
                    player,
                    onBeforeSpawned: (_, obj) => obj.GetComponent<LobbyPlayer>().SteamId = steamId);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
            Debug.Log($"{player.PlayerId} disconnected.");

            var lobbyPlayer = LobbyPlayer.Players.First(x => x.Object.InputAuthority == player);
            if ( lobbyPlayer != null ) {
                runner.Despawn(lobbyPlayer.Object);
            }

            SetConnectionStatus(ConnectionStatus);
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
            SetConnectionStatus(ConnectionStatus.Disconnected);

            SessionDestroyed?.Invoke(runner);
            SessionLeft?.Invoke(runner);

            RunnerShutdown?.Invoke(runner, shutdownReason);

            // Clear steam ticket data
            // if ( _authTicket != null ) {
            //     _authTicket.Cancel();
            //     _authTicket = null;
            // }
            
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
            Debug.Log("Successfully connected to server");
            SetConnectionStatus(ConnectionStatus.Connected);
            SessionJoined?.Invoke(runner);
        }

        public void OnDisconnectedFromServer(NetworkRunner runner) {
            SessionLeft?.Invoke(runner);
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token) {
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