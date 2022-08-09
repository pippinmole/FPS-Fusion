using System;
using System.Linq;
using System.Threading.Tasks;
using FusionFps.Core;
using FusionFps.Steamworks;
using Steamworks;
using TMPro;
using UnityEngine;

public class SteamLobbyDebugUI : MonoBehaviour {
    
    [SerializeField] private TMP_Text _text;

    private ISessionManager _sessionManager;
    
    private void Awake() {
        _sessionManager = ServiceProvider.Get<ISessionManager>();
        
        _text.SetText($"No arguments supplied");
        
        SteamManager.SteamStarted += SteamStarted;
    }

    private async void SteamStarted() {
        var arg = GetArg("+connect_lobby");
        if (arg == null) return;
        
        _text.SetText($"+connect_lobby {arg}");

        await JoinSteamLobby(ulong.Parse(arg));
    }

    private async Task JoinSteamLobby(ulong lobbyId) {
        var lobbies = await SteamMatchmaking.LobbyList.RequestAsync();
        var lobby = lobbies.FirstOrDefault(x => x.Id == lobbyId);

        if ( lobby.Id == lobbyId ) {
            var sessionName = lobby.GetData(SteamLobbyProps.PhotonLobbyName);
            await _sessionManager.JoinSession(sessionName);
        } else {
            Debug.LogError($"Failed to join steam lobby with id {lobbyId}. Does it exist?");
        }
    }

    private static string GetArg(string name) {
        var args = Environment.GetCommandLineArgs();
        for ( var i = 0; i < args.Length; i++ ) {
            if ( args[i] == name && args.Length > i + 1 ) {
                return args[i + 1];
            }
        }
    
        return null;
    }
}
