using System;
using StarterAssets;
using TMPro;
using UnityEngine;

public class IngameUI : MonoBehaviour {
    
    [SerializeField] private TMP_Text _roundTimerText;
    [SerializeField] private TMP_Text _playerHealthText;

    [SerializeField] private Transform _deathScreen;
    [SerializeField] private TMP_Text _deathCountdownText;

    private FirstPersonController _player;
    private FirstPersonHealth _health;
    
    public void Setup(FirstPersonController player) {
        _player = player;
        _health = player.GetComponent<FirstPersonHealth>();

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
        
        var lobbyManager = LobbyManager.Instance;

        switch ( lobbyManager.GameState ) {
            case LobbyManager.EGameState.Lobby:
                _roundTimerText.SetText("");
                break;
            case LobbyManager.EGameState.GameInProgress:
                var timer = lobbyManager.GameData.Countdown;
                var secondsLeft = timer.RemainingTime(lobbyManager.Runner);

                _roundTimerText.SetText(secondsLeft == null
                    ? "0:00"
                    : $"{(int) secondsLeft / 60}:{secondsLeft % 60:00}");
                
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
