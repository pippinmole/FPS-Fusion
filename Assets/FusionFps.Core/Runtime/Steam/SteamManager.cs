using System;
using FusionFps.Core;
using Michsky.UI.ModernUIPack;
using Steamworks;
using UnityEngine;

namespace FusionFps.Steamworks {
    public class SteamManager : MonoBehaviour {

        [SerializeField] private ModalWindowManager _errorConnectingPanel;

        private void Awake() {
            try {
                Debug.Log("[SteamManager] Initialized Steam Client successfully");

                
                SteamClient.Init(480);

            }
            catch ( Exception e ) {
                // Something went wrong - it's one of these:
                //
                //     Steam is closed?
                //     Can't find steam_api dll?
                //     Don't have permission to play app?
                //

                Debug.LogException(e, this);

                _errorConnectingPanel.OpenWindow();
            }
        }

        private void Update() {
            SteamClient.RunCallbacks();
        }

        private void OnDestroy() {
            SteamClient.Shutdown();
        }

        public void OnCloseClicked() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}