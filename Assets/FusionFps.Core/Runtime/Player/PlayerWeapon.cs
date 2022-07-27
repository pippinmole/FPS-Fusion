using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

[OrderAfter(typeof(HitboxManager), typeof(NetworkTransform))]
public class PlayerWeapon : NetworkBehaviour {
    
    [SerializeField] private Transform _weaponRoot;
    [SerializeField] private Transform _weaponParent;
    [SerializeField] private List<WeaponTemplate> _weapons = new();
    [SerializeField] private Material _tracerMaterial;
    
    [Networked(OnChanged = nameof(OnWeaponIndexChanged))]
    private int WeaponIndex { get; set; }
    [Networked] private float Cooldown { get; set; }
    [Networked] private PlayerInput.NetworkInputData Input { get; set; }
    
    private WeaponTemplate CurrentWeaponTemplate => WeaponIndex == -1 ? null : _weapons[WeaponIndex - 1];

    private readonly List<LagCompensatedHit> _hitBuffer = new();
    
    private Weapon _currentWeapon;
    private PlayerCamera _camera;
    private NetworkTransform _networkTransform;
    
    private void Awake() {
        _camera = GetComponent<PlayerCamera>();
        _networkTransform = GetComponent<NetworkTransform>();
    }

    public override void Spawned() {
        base.Spawned();

        WeaponIndex = -1;

        if ( Object.HasInputAuthority ) {
            Cursor.lockState = CursorLockMode.Locked;
        } else {
            // We want to parent the weapon root to the interpolation transform so that the weapon moves smoothly.
            _weaponRoot.SetParent(_networkTransform.InterpolationTarget);
        }
    }

    public override void Render() {
        base.Render();

        _weaponRoot.rotation =
            Quaternion.Euler((float) _camera.CameraPitch, (float) _camera.CameraYaw, _weaponRoot.rotation.z);
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
            Input = input;
        }

        if ( Input.WeaponIndex != -1 && Input.WeaponIndex != WeaponIndex ) {
            WeaponIndex = Input.WeaponIndex;
        }

        if ( Input.IsDown(PlayerInput.NetworkInputData.ButtonShoot) && Cooldown <= 0f ) {
            if ( WeaponIndex == -1 || CurrentWeaponTemplate == null ) return;

            Cooldown = Shoot(Runner);
        }
    }

    private void SetWeapon() {
        ClearWeapon();
        CreateWeapon();
    }

    private void ClearWeapon() {
        // Clear current weapon
        if ( _currentWeapon == null )
            return;

        Destroy(_currentWeapon.gameObject);
        _currentWeapon = null;
    }

    private void CreateWeapon() {
        if ( WeaponIndex == -1 ) return;
        if ( WeaponIndex > _weapons.Count ) return;

        try {
            var weapon = _weapons[WeaponIndex - 1];

            // Spawn new weapon
            _currentWeapon = Instantiate(weapon.Prefab, Vector3.zero, Quaternion.identity);
            _currentWeapon.transform.SetParent(_weaponParent);
            _currentWeapon.transform.localPosition = Vector3.zero;
            _currentWeapon.transform.localRotation = Quaternion.identity;
        }
        catch {
            Debug.LogError($"Error with weapon index of {WeaponIndex}");
        }
    }

    /// <summary>
    /// Shoots the weapon and returns how long to wait until you can shoot again
    /// </summary>
    /// <param name="runner"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    private float Shoot(NetworkRunner runner) {
        var w = CurrentWeaponTemplate;
        if ( w == null ) return -1f;

        var originTransform = GetComponent<PlayerCamera>().GetCameraRoot();
        var origin = originTransform.position;
        var direction = originTransform.forward;

        Debug.DrawRay(origin, direction * w.Range, Color.red, w.ShotDelaySeconds);

        const HitOptions options = HitOptions.SubtickAccuracy | HitOptions.IgnoreInputAuthority;

        runner.LagCompensation.RaycastAll(origin, direction, w.Range, Object.InputAuthority, _hitBuffer, w.Mask,
            true, options);

        if ( _hitBuffer.Count > 0 ) {
            var hit = _hitBuffer.Last();
            
            DrawTracer(origin, hit.Point);
            DealDamage(runner, hit, w.Damage);
        }
        
        RenderMuzzle(w.MuzzlePrefab, _currentWeapon.MuzzlePoint);

        // Add delay
        return w.ShotDelaySeconds;
    }

    private static void RenderMuzzle(GameObject prefab, Transform muzzle) {
        if ( prefab == null ) return;
        if ( muzzle == null ) return;

        var obj = Instantiate(prefab, muzzle.position, muzzle.rotation);
        Destroy(obj, 10f);
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

    private void DealDamage(NetworkRunner runner, LagCompensatedHit hit, float damage) {
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
            Debug.Log($"{Object.InputAuthority.ToString()} has been killed on tick: {runner.Simulation.Tick}");
            
            // We killed them
            var controller = GetComponent<PlayerController>();
            controller.Kills++;
        }
    }

    private static void OnWeaponIndexChanged(Changed<PlayerWeapon> changed) {
        changed.Behaviour.SetWeapon();
    }
}

