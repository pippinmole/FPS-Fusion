using System.Collections.Generic;
using Fusion;
using StarterAssets;
using UnityEngine;

[OrderAfter(typeof(HitboxManager))]
public class FirstPersonWeaponController : NetworkBehaviour {
    
    [SerializeField] private Transform _weaponRoot;
    [SerializeField] private List<Weapon> _weaponPrefabs = new();

    [Networked] public int WeaponIndex { get; set; }
    [Networked] private float Cooldown { get; set; }

    private FirstPersonController _controller;
    private Weapon _currentWeapon;

    private void Awake() {
        _controller = GetComponent<FirstPersonController>();
    }

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

        if ( GetInput(out FirstPersonInput.NetworkInputData input) ) {
            if ( input.WeaponIndex != WeaponIndex ) {
                SetWeapon(input.WeaponIndex);
            }

            if ( input.IsDown(FirstPersonInput.NetworkInputData.ButtonShoot) && Cooldown <= 0f ) {
                if ( WeaponIndex == -1 || _currentWeapon == null ) return;

                Cooldown = _currentWeapon.Shoot(Runner, this);
            }
        }
    }
    
    public void SetWeapon(int index) {
        if ( index == -1 ) return;
        if ( index == WeaponIndex ) return;
        
        ClearWeapon();
        CreateWeapon(index);

        WeaponIndex = index;
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
        if ( index > _weaponPrefabs.Count ) return;

        try {
            var weapon = _weaponPrefabs[index - 1];
            // Spawn new weapon
            _currentWeapon = Instantiate(weapon, Vector3.zero, Quaternion.identity);
            _currentWeapon.transform.SetParent(_weaponRoot);
            _currentWeapon.transform.localPosition = Vector3.zero;
            _currentWeapon.transform.localRotation = Quaternion.identity;

            WeaponIndex = index;
        }
        catch {
           Debug.LogError($"Error with weapon index of {index}");
        }
    }
}

