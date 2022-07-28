using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using FusionFps.Core;
using TMPro;
using UnityEngine;

public class RoundTimerUI : MonoBehaviour, IUIComponent {
    
    [SerializeField] private TMP_Text _roundTimerText;

    private LobbyPlayer _player;
    private NetworkRunner _runner;
    
    public void Setup(LobbyPlayer player, NetworkRunner runner) {
        _player = player;
        _runner = runner;
    }

    private void Update() {
        UpdateRoundTimer();
    }

    private void UpdateRoundTimer() {
        if ( _roundTimerText == null ) return;

        var matchManager = MatchManager.Instance;
        
        switch ( matchManager.GameState ) {
            case EGameState.LobbyConnected:
                _roundTimerText.SetText("");
                break;
            case EGameState.WaitingForPlayers:
                var timer1 = matchManager.WaitForPlayersTimer;
                var secondsLeft1 = timer1.RemainingTime(_runner);

                if ( !secondsLeft1.HasValue )
                    return;

                _roundTimerText.SetText($"Waiting for Players \n {(int) secondsLeft1 / 60}:{secondsLeft1 % 60:00}");
                break;
            case EGameState.GameInProgress:
                var timer = matchManager.Countdown;
                var secondsLeft = timer.RemainingTime(_runner);

                _roundTimerText.SetText(secondsLeft == null
                    ? "0:00"
                    : $"{(int) secondsLeft / 60}:{secondsLeft % 60:00}");
                
                break;
            case EGameState.None:
                break;
            case EGameState.Finished:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
