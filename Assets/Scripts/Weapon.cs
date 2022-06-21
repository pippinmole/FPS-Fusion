using System.Collections.Generic;
using System.Linq;
using Fusion;
using StarterAssets;
using UnityEngine;

public class Weapon : MonoBehaviour {

    [SerializeField] private float _shotDelaySeconds = 0.2f;
    [SerializeField] private float _range = 25f;
    [SerializeField] private float _damage = 15f;

    [SerializeField] private Material _tracerMaterial;
    [SerializeField] private LayerMask _mask = ~0;
    
    private HitboxManager _hitboxManager;
    private readonly List<LagCompensatedHit> _hitBuffer = new();

    private void Awake() {
        _hitboxManager = FindObjectOfType<HitboxManager>();
    }

    /// <summary>
    /// Shoots the weapon and returns how long to wait until you can shoot again
    /// </summary>
    /// <param name="runner"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public float Shoot(NetworkRunner runner, FirstPersonWeaponController owner) {
        // Raycast
        // if ( runner.Stage != SimulationStages.Resimulate ) {

        var originTransform = owner.GetComponent<FirstPersonCamera>().GetCameraRoot();
        var origin = originTransform.position;
        var direction = originTransform.forward;

        Debug.DrawRay(origin, direction * _range, Color.red, _shotDelaySeconds);

        const HitOptions options = HitOptions.SubtickAccuracy;

        _hitboxManager.RaycastAll(origin, direction, _range, owner.Object.InputAuthority, _hitBuffer, _mask,
            true, options);

        // Filter hit buffer
        _hitBuffer.RemoveAll(x => x.GameObject.layer == owner.gameObject.layer);

        if ( _hitBuffer.Count > 0 ) {
            var hit = _hitBuffer.Last(x => x.GameObject != owner.gameObject);

            DrawTracer(origin, hit.Point);
            DealDamage(runner, hit, owner);
        } else {
            Debug.Log("No result");
        }

        // Add delay
        return _shotDelaySeconds;
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

    private void DealDamage(NetworkRunner runner, LagCompensatedHit hit, FirstPersonWeaponController owner) {
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

            health -= _damage;
            health = Mathf.Clamp(health, 0f, healthProvider.MaxHealth);

            healthProvider.Health = health;
        }
        
        if ( !healthProvider.IsAlive ) {
            Debug.Log($"{owner.Object.InputAuthority.ToString()} has been killed on tick: {runner.Simulation.Tick}");
            
            // We killed them
            var controller = owner.GetComponent<FirstPersonController>();
            controller.Kills++;
        }
    }
}

