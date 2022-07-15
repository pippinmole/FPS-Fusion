using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace FusionFps.Core {
    
    public enum EGameState {
        None,
        LobbyConnected,
        WaitingForPlayers,
        GameInProgress
    }
    
    /// <summary>
    /// This is the store of all networked data pertaining to the game state. This 'Game State' is referring to time between
    /// starting a game from the lobby, and the finish of a game (and back to the lobby).
    /// </summary>
    public class MatchManager : NetworkBehaviour {
        [Networked] public EGameState GameState { get; private set; } = EGameState.None;
        [Networked] public TickTimer WaitForPlayersTimer { get; private set; }
        [Networked] public TickTimer Countdown { get; private set; }

        public bool IsRunning => Object != null && Object.IsValid;
        public bool IsServer => Runner != null && Runner.IsServer; 

        [SerializeField] private float _matchTimeInSeconds = 120f;
        [SerializeField] private float _waitForPlayersTimer = 15f;
        
        public static event Action<NetworkRunner> MatchLoaded;
        
        public static MatchManager Instance;
        
        private void Awake() {
            Instance = this;
        }
        
        public override void Spawned() {
            base.Spawned();

            DontDestroyOnLoad(gameObject);
            
            GameState = EGameState.LobbyConnected;
        }

        public override void Despawned(NetworkRunner runner, bool hasState) {
            base.Despawned(runner, hasState);

            GameState = EGameState.None;
        }

        public override void FixedUpdateNetwork() {
            base.FixedUpdateNetwork();

            // Handle wait for players timer
            if ( WaitForPlayersTimer.ExpiredOrNotRunning(Runner) && GameState == EGameState.WaitingForPlayers ) {
                if ( !Runner.IsServer ) return;

                GameState = EGameState.GameInProgress;
                Countdown = TickTimer.CreateFromSeconds(Runner, _matchTimeInSeconds);

                SetAllPlayers(true);
                
                Debug.Log($"Match is set to end on tick {Countdown.TargetTick}");
            }
            
            // Handle game ending
            if ( Countdown.ExpiredOrNotRunning(Runner) && GameState == EGameState.GameInProgress ) {
                // End game
                Debug.Log($"Match ended on tick {(int) Runner.Simulation.Tick}");

                LoadLobby();
            }
        }

        public static void LoadSessionMap() {
            if ( !Instance.Runner.IsServer ) return;

            var sessionProperties = Instance.Runner.SessionInfo.Properties;
            var mapBuildIndex = (int) sessionProperties["mapBuildIndex"];

            Instance.GameState = EGameState.WaitingForPlayers;
            Instance.WaitForPlayersTimer = TickTimer.CreateFromSeconds(Instance.Runner, Instance._waitForPlayersTimer);
            Instance.Runner.SetActiveScene(mapBuildIndex);
        }

        public static void LoadLobby() {
            if ( !Instance.Runner.IsServer ) return;
            
            // Clear players from field
            Instance.DespawnAllPlayers();


            // Reset the state of the game
            // Instance.GameState = EGameState.LobbyConnected;
            
            // Instance.Runner.SetActiveScene(0);
            // Instance.Runner.Shutdown();

            // throw new NotImplementedException();
        }
        
        public static void OnMatchLoaded() => MatchLoaded?.Invoke(Instance.Runner);

        private static void SetAllPlayers(bool active) {
            var players = LobbyPlayer.Players;
            foreach ( var player in players.Where(player => player.Controller != null) ) {
                player.Controller.CanMove = active;
            }
        }

        private void DespawnAllPlayers() {
            var players = LobbyPlayer.Players.ToList();
            foreach ( var player in players ) {
                if ( player.Controller == null ) {
                    Debug.Log($"{player.Object.InputAuthority} has a null controller! Continuing...");
                    continue;
                }
                
                player.Despawn();
            }
        }
    }
}