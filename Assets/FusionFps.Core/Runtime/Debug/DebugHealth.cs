using System;
using Fusion;
using UnityEngine;

public class DebugHealth : NetworkBehaviour, IHealthProvider {

    [Networked] private float _health { get; set; } = 100f;
    [Networked] private float _maxHealth { get; set; } = 100f;
    
    public float Health {
        get => Object != null && Object.IsValid ? _health : 0f;
        set {
            _health = value;
            HealthUpdated?.Invoke(value);
        }
    }

    public float MaxHealth {
        get => Object != null && Object.IsValid ? _maxHealth : 0f;
        set => _health = value;
    }

    public event Action<float> HealthUpdated;

    [Networked] public TickTimer ActiveTimer { get; set; } // If Expired, should be Enabled

    [SerializeField] private Renderer _renderer;
    [SerializeField] private GameObject _healthBar;
    [SerializeField] private Hitbox _hitbox;
    [SerializeField] private float _respawnTime = 4f;

    public override void FixedUpdateNetwork() {
        base.FixedUpdateNetwork();

        if ( Health <= 0f && ActiveTimer.ExpiredOrNotRunning(Runner) ) {
            ActiveTimer = TickTimer.CreateFromSeconds(Runner, _respawnTime);
            Health = MaxHealth;
        }

        var enable = ActiveTimer.ExpiredOrNotRunning(Runner);
        
        _renderer.enabled = enable;
        _healthBar.SetActive(enable);
        _hitbox.HitboxActive = enable;
    }
}
