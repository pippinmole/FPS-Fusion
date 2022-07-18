// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack {
    public abstract class CharacterBehaviour : MonoBehaviour {
        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void Update() { }
        protected virtual void LateUpdate() { }

        public abstract Camera GetCameraWorld();
        public abstract Camera GetCameraDepth();

        public abstract InventoryBehaviour GetInventory();

        public abstract int GetGrenadesCurrent();
        public abstract int GetGrenadesTotal();

        public abstract bool IsCrosshairVisible();
        public abstract bool IsRunning();

        public abstract bool IsAiming();
        public abstract bool IsCursorLocked();

        /// <summary>
        /// Returns true if the tutorial text should be visible on the screen.
        /// </summary>
        public abstract bool IsTutorialTextVisible();

        public abstract Vector2 GetInputMovement();
        public abstract Vector2 GetInputLook();

        public abstract AudioClip[] GetAudioClipsGrenadeThrow();
        public abstract AudioClip[] GetAudioClipsMelee();

        public abstract void EjectCasing();

        /// <summary>
        /// Fills the character's equipped weapon's ammunition by a certain amount, or fully if set to -1.
        /// </summary>
        public abstract void FillAmmunition(int amount);

        public abstract void ThrowGrenade();

        /// <summary>
        /// Sets the equipped weapon's magazine to be active or inactive!
        /// </summary>
        public abstract void SetActiveMagazine(int active);

        public abstract void AnimationEndedBolt();
        public abstract void AnimationEndedReload();

        public abstract void AnimationEndedGrenadeThrow();
        public abstract void AnimationEndedMelee();

        public abstract void AnimationEndedInspect();
        public abstract void AnimationEndedHolster();

        public abstract void SetSlideBack(int back);

        public abstract void SetActiveKnife(int active);
    }
}