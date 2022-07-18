// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack {
    public interface ICharacterAnimationCallbacks {
        void EjectCasing();
        void FillAmmunition(int amount);
        void SetActiveKnife(int active);
        void ThrowGrenade();
        void SetActiveMagazine(int active);
        void AnimationEndedBolt();
        void AnimationEndedReload();
        void AnimationEndedGrenadeThrow();
        void AnimationEndedMelee();
        void AnimationEndedInspect();
        void AnimationEndedHolster();
        void SetSlideBack(int back);
    }

    /// <summary>
    /// Handles all the animation events that come from the character in the asset.
    /// </summary>
    public class CharacterAnimationEventHandler : MonoBehaviour {
        private ICharacterAnimationCallbacks _callbacks;

        private void Awake() {
            _callbacks = GetComponent<ICharacterAnimationCallbacks>();
        }

        /// <summary>
        /// Ejects a casing from the character's equipped weapon. This function is called from an Animation Event.
        /// </summary>
        private void OnEjectCasing() {
            _callbacks?.EjectCasing();
        }

        /// <summary>
        /// Fills the character's equipped weapon's ammunition by a certain amount, or fully if set to 0. This function is called
        /// from a Animation Event.
        /// </summary>
        private void OnAmmunitionFill(int amount = 0) {
            _callbacks?.FillAmmunition(amount);
        }

        /// <summary>
        /// Sets the character's knife active value. This function is called from an Animation Event.
        /// </summary>
        private void OnSetActiveKnife(int active) {
            _callbacks?.SetActiveKnife(active);
        }

        /// <summary>
        /// Spawns a grenade at the correct location. This function is called from an Animation Event.
        /// </summary>
        private void OnGrenade() {
            _callbacks?.ThrowGrenade();
        }

        /// <summary>
        /// Sets the equipped weapon's magazine to be active or inactive! This function is called from an Animation Event.
        /// </summary>
        private void OnSetActiveMagazine(int active) {
            _callbacks?.SetActiveMagazine(active);
        }

        /// <summary>
        /// Bolt Animation Ended. This function is called from an Animation Event.
        /// </summary>
        private void OnAnimationEndedBolt() {
            _callbacks?.AnimationEndedBolt();
        }

        /// <summary>
        /// Reload Animation Ended. This function is called from an Animation Event.
        /// </summary>
        private void OnAnimationEndedReload() {
            _callbacks?.AnimationEndedReload();
        }

        /// <summary>
        /// Grenade Throw Animation Ended. This function is called from an Animation Event.
        /// </summary>
        private void OnAnimationEndedGrenadeThrow() {
            _callbacks?.AnimationEndedGrenadeThrow();
        }

        /// <summary>
        /// Melee Animation Ended. This function is called from an Animation Event.
        /// </summary>
        private void OnAnimationEndedMelee() {
            _callbacks?.AnimationEndedMelee();
        }

        /// <summary>
        /// Inspect Animation Ended. This function is called from an Animation Event.
        /// </summary>
        private void OnAnimationEndedInspect() {
            _callbacks?.AnimationEndedInspect();
        }

        /// <summary>
        /// Holster Animation Ended. This function is called from an Animation Event.
        /// </summary>
        private void OnAnimationEndedHolster() {
            _callbacks?.AnimationEndedHolster();
        }

        /// <summary>
        /// Sets the character's equipped weapon's slide back pose. This function is called from an Animation Event.
        /// </summary>
        private void OnSlideBack(int back) {
            _callbacks?.SetSlideBack(back);
        }
    }
}