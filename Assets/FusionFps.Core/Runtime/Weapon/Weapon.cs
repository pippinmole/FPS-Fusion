using UnityEngine;

public class Weapon : MonoBehaviour {

    [SerializeField] private Transform _muzzlePoint;

    public Transform MuzzlePoint => _muzzlePoint;

}

