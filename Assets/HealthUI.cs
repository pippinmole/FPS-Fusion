using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour, IUIComponent {
    [SerializeField] private TMP_Text _playerHealthText;

    private LobbyPlayer _player;
    private PlayerHealth _health;
    private NetworkRunner _runner;
    
    public void Setup(LobbyPlayer player, NetworkRunner runner) {
        _player = player;
        _runner = runner;

        _player.PlayerSpawned += PlayerSpawned;
        
        UpdateHealthText(-1f);
    }

    private void OnDestroy() {
        _player.PlayerSpawned -= PlayerSpawned;
    }

    private void PlayerSpawned(NetworkRunner runner, PlayerController controller) {
        _health = controller.GetComponent<PlayerHealth>();
        _health.HealthUpdated += UpdateHealthText;
    }
    
    private void UpdateHealthText(float _) {
        if ( _playerHealthText == null ) return;
        if ( _player == null || _health == null ) {
            _playerHealthText.SetText("");
            return;
        }

        _playerHealthText.SetText($"{(int) _health.Health}/{(int) _health.MaxHealth}");
    }
}
