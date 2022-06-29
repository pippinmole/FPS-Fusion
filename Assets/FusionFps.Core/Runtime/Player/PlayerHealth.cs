using System;
using Fusion;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour, IHealthProvider {

    [SerializeField] private float _respawnTimer = 5f;

    [Networked(OnChanged = nameof(OnHealthUpdate))]
    private float NetworkedHealth { get; set; } = 100f;
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

    private PlayerController _controller;

    private void Awake() {
        _controller = GetComponent<PlayerController>();
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

        var spawnPointManager = FindObjectOfType<PlayerSpawnManager>();
        var point = spawnPointManager.GetNextSpawnPoint(Runner, Object.InputAuthority);

        Debug.Log("Respawning");
        
        transform.position = point.position;
    }
    
    public static void OnHealthUpdate(Changed<PlayerHealth> changed) {
        changed.Behaviour.HealthUpdated?.Invoke(changed.Behaviour.NetworkedHealth);
    }
}
