// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack {
    public abstract class WeaponBehaviour : MonoBehaviour {
        public virtual void Init(PlayerWeapon player) { }
        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void Update() { }
        protected virtual void LateUpdate() { }

        /// <summary>
        /// Returns the sprite to use when displaying the weapon's body.
        /// </summary>
        /// <returns></returns>
        public abstract Sprite GetSpriteBody();

        public abstract float GetMultiplierMovementSpeed();

        public abstract AudioClip GetAudioClipHolster();
        public abstract AudioClip GetAudioClipUnholster();
        public abstract AudioClip GetAudioClipReload();
        public abstract AudioClip GetAudioClipReloadEmpty();

        public abstract AudioClip GetAudioClipReloadOpen();
        public abstract AudioClip GetAudioClipReloadInsert();
        public abstract AudioClip GetAudioClipReloadClose();

        public abstract AudioClip GetAudioClipFireEmpty();

        public abstract AudioClip GetAudioClipBoltAction();
        public abstract AudioClip GetAudioClipFire();
        public abstract int GetAmmunitionCurrent();
        public abstract int GetAmmunitionTotal();
        public abstract bool HasCycledReload();
        public abstract Animator GetAnimator();
        public abstract bool IsAutomatic();
        public abstract bool HasAmmunition();
        public abstract bool IsFull();
        public abstract bool IsBoltAction();
        public abstract bool GetAutomaticallyReloadOnEmpty();

        /// <summary>
        /// Returns the delay after firing the last shot when the weapon should start automatically reloading.
        /// </summary>
        public abstract float GetAutomaticallyReloadOnEmptyDelay();

        public abstract bool CanReloadWhenFull();
        public abstract float GetRateOfFire();
        public abstract Offsets GetWeaponOffsets();
        public abstract float GetFieldOfViewMultiplierAim();

        /// <summary>
        /// Returns the field of view multiplier when aiming for the weapon camera.
        /// </summary>
        public abstract float GetFieldOfViewMultiplierAimWeapon();

        /// <summary>
        /// Returns the RuntimeAnimationController the Character needs to use when this Weapon is equipped!
        /// </summary>
        public abstract RuntimeAnimatorController GetAnimatorController();

        /// <summary>
        /// Returns the weapon's attachment manager component.
        /// </summary>
        public abstract WeaponAttachmentManagerBehaviour GetAttachmentManager();

        public abstract Sway GetSway();
        public abstract float GetSwaySmoothValue();

        public abstract void Fire(float spreadMultiplier = 1.0f);
        public abstract void Reload();

        public abstract void FillAmmunition(int amount);
        public abstract void SetSlideBack(int back);
        public abstract void EjectCasing();
    }
}