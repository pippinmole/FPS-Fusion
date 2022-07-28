using System.Collections;
using System.Collections.Generic;
using Fusion;
using Steamworks.Data;
using UnityEngine;

public static class SessionPropertyProps {
    public static string MapName => "mapBuildIndex";
    public static string SteamLobbyId => "sLobbyId";
    
    public static Lobby? GetSteamLobby(this SessionInfo sessionProperties) {
        if ( sessionProperties.Properties.TryGetValue(SteamLobbyId, out var val) ) {
            return new Lobby(ulong.Parse(val));
        }

        return null;
    }
}
