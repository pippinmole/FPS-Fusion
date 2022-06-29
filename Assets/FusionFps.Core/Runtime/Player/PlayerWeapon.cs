using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

[OrderAfter(typeof(HitboxManager))]
public class PlayerWeapon : NetworkBehaviour {
    
    [SerializeField] private Transform _weaponRoot; 
    [SerializeField] private List<WeaponTemplate> _weapons = new();
    [SerializeField] private Material _tracerMaterial;
    
    [Networked(OnChanged = nameof(OnWeaponIndexChanged))]
    private int WeaponIndex { get; set; }
    [Networked] private float Cooldown { get; set; }
    
    private WeaponTemplate CurrentWeaponTemplate => _weapons[WeaponIndex - 1];

    private Weapon _currentWeapon;
    private readonly List<LagCompensatedHit> _hitBuffer = new();
    
    public override void Spawned() {
        base.Spawned();

        WeaponIndex = -1;

        if ( Object.HasInputAuthority ) {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);
        
        if ( Object.HasInputAuthority ) {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public override void FixedUpdateNetwork() {
        base.FixedUpdateNetwork();
        
        Cooldown -= Runner.DeltaTime;

        if ( GetInput(out PlayerInput.NetworkInputData input) ) {
            if ( input.WeaponIndex != -1 ) {
                UpdateWeapon(input.WeaponIndex);
            }

            if ( input.IsDown(PlayerInput.NetworkInputData.ButtonShoot) && Cooldown <= 0f ) {
                if ( WeaponIndex == -1 || CurrentWeaponTemplate == null ) return;

                Cooldown = Shoot(Runner, this);
            }
        }
    }

    private void UpdateWeapon(int index) {
        if ( index == -1 ) return;
        if ( index == WeaponIndex ) return;
        
        ClearWeapon();
        CreateWeapon(index);
    }

    private void ClearWeapon() {
        // Clear current weapon
        if ( _currentWeapon == null )
            return;

        Destroy(_currentWeapon.gameObject);
        _currentWeapon = null;
    }

    private void CreateWeapon(int index) {
        if ( index == -1 ) return;
        if ( index > _weapons.Count ) return;

        try {
            var weapon = _weapons[index - 1];
            
            // Spawn new weapon
            _currentWeapon = Instantiate(weapon.Prefab, Vector3.zero, Quaternion.identity);
            _currentWeapon.transform.SetParent(_weaponRoot);
            _currentWeapon.transform.localPosition = Vector3.zero;
            _currentWeapon.transform.localRotation = Quaternion.identity;

            WeaponIndex = index;
        }
        catch {
            Debug.LogError($"Error with weapon index of {index}");
        }
    }

    /// <summary>
    /// Shoots the weapon and returns how long to wait until you can shoot again
    /// </summary>
    /// <param name="runner"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    private float Shoot(NetworkRunner runner, PlayerWeapon owner) {
        var w = CurrentWeaponTemplate;
        if ( w == null ) return -1f;

        var originTransform = owner.GetComponent<PlayerCamera>().GetCameraRoot();
        var origin = originTransform.position;
        var direction = originTransform.forward;

        Debug.DrawRay(origin, direction * w.Range, Color.red, w.ShotDelaySeconds);

        const HitOptions options = HitOptions.SubtickAccuracy;

        runner.LagCompensation.RaycastAll(origin, direction, w.Range, owner.Object.InputAuthority, _hitBuffer, w.Mask,
            true, options);

        // Filter hit buffer
        _hitBuffer.RemoveAll(x => x.GameObject.layer == owner.gameObject.layer);

        if ( _hitBuffer.Count > 0 ) {
            var hit = _hitBuffer.Last(x => x.GameObject != owner.gameObject);

            DrawTracer(origin, hit.Point);
            DealDamage(runner, hit, owner, w.Damage);
        }

        // Add delay
        return w.ShotDelaySeconds;
    }

    private void DrawTracer(Vector3 start, Vector3 end) {
        const float duration = 2f;
        const float width = 0.02f;

        if ( _tracerMaterial == null ) return;
        
        var myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        var lr = myLine.GetComponent<LineRenderer>();
        lr.material = _tracerMaterial;

        lr.startColor = Color.white;
        lr.endColor = Color.white;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        Destroy(myLine, duration);
    }

    private static void DealDamage(NetworkRunner runner, LagCompensatedHit hit, PlayerWeapon owner, float damage) {
        if ( hit.Type == HitType.Hitbox ) {
            hit.GameObject = hit.Hitbox.Root.gameObject;
        }
        
        var gameObj = hit.GameObject;
        if ( gameObj == null )
            return;

        var healthProvider = gameObj.GetComponent<IHealthProvider>();
        if ( healthProvider == null || !healthProvider.IsAlive )
            return;
        
        if ( healthProvider.IsAlive ) {
            var health = healthProvider.Health;

            health -= damage;
            health = Mathf.Clamp(health, 0f, healthProvider.MaxHealth);

            healthProvider.Health = health;
        }
        
        if ( !healthProvider.IsAlive ) {
            Debug.Log($"{owner.Object.InputAuthority.ToString()} has been killed on tick: {runner.Simulation.Tick}");
            
            // We killed them
            var controller = owner.GetComponent<PlayerController>();
            controller.Kills++;
        }
    }

    private static void OnWeaponIndexChanged(Changed<PlayerWeapon> changed) {
        changed.Behaviour.WeaponIndexChanged();
    }

    private void WeaponIndexChanged() {
        UpdateWeapon(WeaponIndex);
    }
}

