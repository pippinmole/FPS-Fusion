using System;
using UnityEngine;
using UnityEngine.UI;

public class DebugHealthBar : MonoBehaviour {
    
    [SerializeField] private Image _fill;
    
    private IHealthProvider _healthProvider;

    private void Awake() {
        _healthProvider = GetComponent<IHealthProvider>();
    }

    private void Update() {
        if ( _healthProvider == null ) return;
        
        var health = _healthProvider.Health;
        var maxHealth = _healthProvider.MaxHealth;
        _fill.fillAmount = health / maxHealth;
    }
}
