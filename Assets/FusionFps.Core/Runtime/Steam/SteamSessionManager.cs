using System.Threading.Tasks;
using Fusion;
using FusionFps.Core;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

/// <summary>
/// Manages the local steam clients' lobby status. Behaviour should be as follows: <br />
///     - If a client joins a photon session 'SessionJoined', we should join that lobby                         <br />
///     - If a client leaves a photon session 'SessionLeft', we should leave the lobby we are in                <br />
///     - If a server creates a photon session 'SessionCreated', we should create a steam lobby alongside it    <br />
///     - If a server destroys a photon session 'SessionDestroyed', we should destroy the steam lobby           
/// </summary>
public class SteamSessionManager : MonoBehaviour {

    protected ISessionManager SessionManager;
    
    private Lobby? _lobby;

    private void Awake() {
        SessionManager = ServiceProvider.Get<ISessionManager>();
        
        // SessionManager.SessionCreated += CreateSteamLobby;
        // SessionManager.SessionDestroyed += ShutdownSteamLobby;

        SessionManager.SessionJoined += JoinSteamLobby;
        SessionManager.SessionLeft += LeaveSteamLobby;

        SteamMatchmaking.OnLobbyInvite += SteamLobbyInvited;
        SteamMatchmaking.OnLobbyEntered += SteamLobbyJoined;
    }
    
    protected void ShutdownSteamLobby(NetworkRunner runner) {
        _lobby?.Leave();
    }

    protected async void JoinSteamLobby(NetworkRunner runner) {
        if ( runner.IsServer ) 
            return;
        
        var lobby = runner.SessionInfo.GetSteamLobby();
        if ( !lobby.HasValue )
            return;
        
        var result = await lobby.Value.Join();
        if ( result == RoomEnter.Success ) {
            Debug.Log($"Successfully joined steam lobby of id {lobby.Value.Id}");
            _lobby = lobby;
        } else {
            Debug.LogError($"Failed to join steam lobby of id {lobby.Value.Id}. Reason: {result}");
        }
    }

    protected void LeaveSteamLobby(NetworkRunner runner) {
        _lobby?.Leave();
    }

    protected async void SteamLobbyInvited(Friend friend, Lobby lobby) {
        Debug.LogError($"{friend.Name} has invited you to a lobby of id {lobby.Id}");
    }

    protected void SteamLobbyJoined(Lobby lobby) {
        // var sessionName = lobby.GetData(SteamLobbyProps.PhotonLobbyName);
        //
        // SessionManager.JoinSession(sessionName);
    }
}