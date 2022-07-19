// Copyright 2021, Infima Games. All Rights Reserved.

using System.Linq;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack {
    // [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    // public class Movement : MonoBehaviour {
    //     [Header("Speeds")] [SerializeField] private float _speedWalking = 8f;
    //
    //     [Tooltip("How fast the player moves while aiming.")] [SerializeField]
    //     private float _speedAiming = 3.2f;
    //
    //     [Tooltip("How fast the player moves while running."), SerializeField]
    //     private float _speedRunning = 8.8f;
    //
    //     [Header("Walking Multipliers")] [Tooltip("How fast the character moves forward."), SerializeField]
    //     private float _walkingMultiplierForward = 1.0f;
    //
    //     [Tooltip("How fast the character moves sideways.")] [SerializeField]
    //     private float _walkingMultiplierSideways = 0.8f;
    //
    //     [Tooltip("How fast the character moves backwards.")] [SerializeField]
    //     private float _walkingMultiplierBackwards = 0.8f;
    //
    //     // [Header("Interpolation")]
    //     // [Tooltip(
    //     //     "Approximately the amount of time it will take for the player to reach maximum running or walking speed.")]
    //     // [SerializeField]
    //     // private float _movementSmoothness = 0.08f;
    //
    //     private WeaponBehaviour EquippedWeapon => _playerCharacter.GetInventory().GetEquipped();
    //
    //     private AudioSource _audioSource;
    //     private CharacterBehaviour _playerCharacter;
    //     
    //     private void Awake() {
    //         _playerCharacter = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
    //     }
    //
    //     private void FixedUpdate() => MoveCharacter();
    //
    //     private void MoveCharacter() {
    //         var input = _playerCharacter.GetInputMovement();
    //         var movement = new Vector3(input.x, 0.0f, input.y);
    //
    //         if ( _playerCharacter.IsRunning() ) {
    //             movement *= _speedRunning;
    //         } else {
    //             if ( _playerCharacter.IsAiming() )
    //                 movement *= _speedAiming;
    //             else {
    //                 movement *= _speedWalking;
    //                 movement.x *= _walkingMultiplierSideways;
    //                 movement.z *= input.y > 0 ? _walkingMultiplierForward : _walkingMultiplierBackwards;
    //             }
    //         }
    //
    //         // World space velocity calculation. This allows us to add it to the rigidbody's velocity properly.
    //         movement = transform.TransformDirection(movement);
    //         // Multiply by the weapon movement speed multiplier. This helps us modify speeds based on the weapon!
    //         if ( EquippedWeapon != null )
    //             movement *= EquippedWeapon.GetMultiplierMovementSpeed();
    //
    //         Velocity = new Vector3(movement.x, 0.0f, movement.z);
    //     }
    //
    //     public float GetMultiplierForward() => _walkingMultiplierForward;
    //     public float GetMultiplierSideways() => _walkingMultiplierSideways;
    //     public float GetMultiplierBackwards() => _walkingMultiplierBackwards;
    // }
}