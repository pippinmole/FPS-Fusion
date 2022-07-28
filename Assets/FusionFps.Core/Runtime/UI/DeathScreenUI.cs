using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

public class DeathScreenUI : MonoBehaviour, IUIComponent {
    
    [SerializeField] private Transform _deathScreen;
    [SerializeField] private TMP_Text _deathCountdownText;

    private LobbyPlayer _player;
    private PlayerHealth _health;
    private NetworkRunner _runner;
    
    public void Setup(LobbyPlayer player, NetworkRunner runner) {
        _player = player;
        _runner = runner;

        _player.PlayerSpawned += PlayerSpawned;
    }

    private void OnDestroy() {
        _player.PlayerSpawned += PlayerSpawned;
        // _health.HealthUpdated -= UpdateDeathScreen;
    }

    private void PlayerSpawned(NetworkRunner runner, PlayerController controller) {
        _health = controller.GetComponent<PlayerHealth>();
        _health.HealthUpdated += UpdateDeathScreen;
    }
    
    private void UpdateDeathScreen(float _) {
        if ( _health == null ) return;
        
        var isAlive = _health.IsAlive;
        _deathScreen.gameObject.SetActive(!isAlive);

        if ( !isAlive ) {
            var secondsUntilRespawn = _health.DeathTimer.RemainingTime(_health.Runner);
            if ( secondsUntilRespawn == null ) return;

            _deathCountdownText.SetText($"Respawning in {secondsUntilRespawn:F1}s");
        }
    }
}
