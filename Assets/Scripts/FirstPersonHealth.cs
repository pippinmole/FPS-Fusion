using System;
using Fusion;
using StarterAssets;
using UnityEngine;

public class FirstPersonHealth : NetworkBehaviour, IHealthProvider {

    [SerializeField] private float _respawnTimer = 5f;
    
    [Networked(OnChanged = nameof(OnHealthUpdate))]
    private float NetworkedHealth { get; set; }
    [Networked] public TickTimer DeathTimer { get; set; }

    public bool IsAlive => DeathTimer.ExpiredOrNotRunning(Runner);

    public float Health {
        get => Object != null && Object.IsValid ? NetworkedHealth : 0f;
        set {
            NetworkedHealth = value;

            if ( NetworkedHealth <= 0f ) {
                // We should die now
                DeathTimer = TickTimer.CreateFromSeconds(Runner, _respawnTimer);
                _controller.Deaths++;
            }
        }
    }

    public float MaxHealth { get; set; } = 100f;
    public event Action<float> HealthUpdated;

    private FirstPersonController _controller;

    private void Awake() {
        _controller = GetComponent<FirstPersonController>();
    }

    public override void FixedUpdateNetwork() {
        base.FixedUpdateNetwork();

        if ( NetworkedHealth <= 0f && DeathTimer.ExpiredOrNotRunning(Runner) ) {
            Respawn();
        }
    }

    private void Respawn() {
        Health = MaxHealth;
        DeathTimer = TickTimer.None;

        var spawnPointManager = FindObjectOfType<PlayerSpawnPointManagerPrototype>();
        var point = spawnPointManager.GetNextSpawnPoint(Runner, Object.InputAuthority);

        transform.position = point.position;
    }
    
    public static void OnHealthUpdate(Changed<FirstPersonHealth> changed) {
        changed.Behaviour.HealthUpdated?.Invoke(changed.Behaviour.NetworkedHealth);
    }
}
