using UnityEngine;

[CreateAssetMenu(order = 1, fileName = "New Weapon Template", menuName = "Weapon/Create New Weapon Template")]
public class WeaponTemplate : ScriptableObject {
    [SerializeField] private Weapon _prefab;
    [SerializeField] private float _shotDelaySeconds = 0.2f;
    [SerializeField] private float _range = 25f;
    [SerializeField] private float _damage = 15f;
    [SerializeField] private LayerMask _mask = ~0;
    
    public Weapon Prefab => _prefab;
    public float ShotDelaySeconds => _shotDelaySeconds;
    public float Range => _range;
    public float Damage => _damage;
    public LayerMask Mask => _mask;
}
