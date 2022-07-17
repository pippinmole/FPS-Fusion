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
        GameInProgress,
        Finished
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

        public float? WaitForPlayersTimeLeft => WaitForPlayersTimer.RemainingTime(Runner);
        public bool IsWaitingForPlayers => !WaitForPlayersTimer.ExpiredOrNotRunning(Runner);
        public bool IsGameStarted => GameState == EGameState.GameInProgress;

        [SerializeField] private float _matchTimeInSeconds = 120f;
        [SerializeField] private float _waitForPlayersTimer = 15f;
        
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
                
                Debug.Log($"Match is set to end on tick {Countdown.TargetTick}");
            }
            
            // Handle game ending
            if ( Countdown.ExpiredOrNotRunning(Runner) && GameState == EGameState.GameInProgress ) {
                // End game
                Debug.Log($"Match ended on tick {(int) Runner.Simulation.Tick}");
                
                GameState = EGameState.Finished;
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
    }
}