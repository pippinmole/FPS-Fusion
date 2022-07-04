using System;
using Steamworks;
using UnityEngine;

public class SteamManager : MonoBehaviour {
    
    private void Start() {
        try {
            SteamClient.Init(480);
            
            Debug.Log("[SteamManager] Initialized Steam Client successfully");
        }
        catch ( Exception e ) {
            // Something went wrong - it's one of these:
            //
            //     Steam is closed?
            //     Can't find steam_api dll?
            //     Don't have permission to play app?
            //

            Debug.LogException(e, this);
        }
    }

    private void Update() {
        SteamClient.RunCallbacks();
    }

    private void OnDestroy() {
        SteamClient.Shutdown();
    }
}