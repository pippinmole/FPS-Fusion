// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack {
    /// <summary>
    /// Bolt Action Behaviour. Makes sure that the weapon's animator also matches the bolt action animation.
    /// </summary>
    public class BoltBehaviour : StateMachineBehaviour {
        private PlayerAnimator _playerAnimator;
        private PlayerWeapon _playerInventory;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _playerAnimator ??= animator.GetComponentInParent<PlayerAnimator>();
            _playerInventory ??= _playerAnimator.GetInventory();

            //Try to get the equipped weapon's Weapon component.
            if ( !(_playerInventory.GetEquipped() is { } weaponBehaviour) )
                return;

            //Get the weapon animator.
            var weaponAnimator = weaponBehaviour.gameObject.GetComponent<Animator>();
            //Play Bolt Action Animation.
            weaponAnimator.Play("Bolt Action");
        }
    }
}