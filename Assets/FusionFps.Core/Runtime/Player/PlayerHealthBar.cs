using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : NetworkBehaviour {
    [SerializeField] private Image _fill;

    private IHealthProvider _healthProvider;

    private void Awake() {
        _healthProvider = GetComponent<IHealthProvider>();
        _healthProvider.HealthUpdated += UpdateHealth;
    }

    private void OnDestroy() {
        _healthProvider.HealthUpdated -= UpdateHealth;
    }

    private void UpdateHealth(float _) {
        if ( _healthProvider == null ) return;

        var health = _healthProvider.Health;
        var maxHealth = _healthProvider.MaxHealth;
        _fill.fillAmount = health / maxHealth;
    }
}
