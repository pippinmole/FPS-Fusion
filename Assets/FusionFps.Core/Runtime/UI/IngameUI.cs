using System;
using Fusion;
using FusionFps.Core;
using TMPro;
using UnityEngine;

public class IngameUI : MonoBehaviour {
    
    [SerializeField] private TMP_Text _roundTimerText;
    [SerializeField] private TMP_Text _playerHealthText;

    [SerializeField] private Transform _deathScreen;
    [SerializeField] private TMP_Text _deathCountdownText;

    private IMatchManager _matchManager;
    private PlayerController _player;
    private PlayerHealth _health;
    private NetworkRunner _runner;

    private void Awake() {
        _matchManager = ServiceProvider.Get<IMatchManager>();
    }

    public void Setup(PlayerController player, NetworkRunner runner) {
        _player = player;
        _runner = runner;
        _health = player.GetComponent<PlayerHealth>();

        _health.HealthUpdated += UpdateHealthText;
        _health.HealthUpdated += UpdateDeathScreen;
    }

    private void OnDestroy() {
        _health.HealthUpdated -= UpdateHealthText;
        _health.HealthUpdated -= UpdateDeathScreen;
    }

    private void Update() {
        UpdateRoundTimer();
        UpdateDeathScreen(_health.Health);
    }

    private void UpdateHealthText(float _) {
        if ( _playerHealthText == null ) return;
        if ( _player == null ) return;

        _playerHealthText.SetText($"{(int) _health.Health}/{(int) _health.MaxHealth}");
    }

    private void UpdateRoundTimer() {
        if ( _roundTimerText == null ) return;

        switch ( _matchManager.GameState ) {
            case EGameState.LobbyConnected:
                _roundTimerText.SetText("");
                break;
            case EGameState.WaitingForPlayers:
                var timer1 = _matchManager.WaitForPlayersTimer;
                var secondsLeft1 = timer1.RemainingTime(_runner);

                if ( !secondsLeft1.HasValue )
                    return;

                _roundTimerText.SetText($"Waiting for Players \n {(int) secondsLeft1 / 60}:{secondsLeft1 % 60:00}");
                break;
            case EGameState.GameInProgress:
                var timer = _matchManager.Countdown;
                var secondsLeft = timer.RemainingTime(_runner);

                _roundTimerText.SetText(secondsLeft == null
                    ? "0:00"
                    : $"{(int) secondsLeft / 60}:{secondsLeft % 60:00}");
                
                break;
            case EGameState.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void UpdateDeathScreen(float _) {
        var isAlive = _health.IsAlive;
        _deathScreen.gameObject.SetActive(!isAlive);

        if ( !isAlive ) {
            var secondsUntilRespawn = _health.DeathTimer.RemainingTime(_health.Runner);
            if ( secondsUntilRespawn == null ) return;

            _deathCountdownText.SetText($"Respawning in {secondsUntilRespawn:F1}s");
        }
    }
}
