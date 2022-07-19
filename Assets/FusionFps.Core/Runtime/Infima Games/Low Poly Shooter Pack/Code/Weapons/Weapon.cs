// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack {
    /// <summary>
    /// Weapon. This class handles most of the things that weapons need.
    /// </summary>
    public class Weapon : WeaponBehaviour {
        #region FIELDS SERIALIZED

        [Header("Settings")]
        [Tooltip("Weapon Name. Currently not used for anything, but in the future, we will use this for pickups!")]
        [SerializeField]
        private string weaponName;

        [Tooltip("How much the character's movement speed is multiplied by when wielding this weapon.")]
        [SerializeField]
        private float multiplierMovementSpeed = 1.0f;

        [Header("Firing")]
        [Tooltip("Is this weapon automatic? If yes, then holding down the firing button will continuously fire.")]
        [SerializeField]
        private bool automatic;

        [Tooltip("Is this weapon bolt-action? If yes, then a bolt-action animation will play after every shot.")]
        [SerializeField]
        private bool boltAction;

        [Tooltip(
            "Amount of shots fired at once. Helpful for things like shotguns, where there are multiple projectiles fired at once.")]
        [SerializeField]
        private int shotCount = 1;

        [Tooltip("How far the weapon can fire from the center of the screen.")] [SerializeField]
        private float spread = 0.25f;

        [Tooltip("How fast the projectiles are.")] [SerializeField]
        private float projectileImpulse = 400.0f;

        [Tooltip("Amount of shots this weapon can shoot in a minute. It determines how fast the weapon shoots.")]
        [SerializeField]
        private int roundsPerMinutes = 200;

        [Tooltip("Mask of things recognized when firing.")] [SerializeField]
        private LayerMask mask;

        [Tooltip(
            "Maximum distance at which this weapon can fire accurately. Shots beyond this distance will not use linetracing for accuracy.")]
        [SerializeField]
        private float maximumDistance = 500.0f;

        [Header("Reloading")]
        [Tooltip("Determines if this weapon reloads in cycles, meaning that it inserts one bullet at a time, or not.")]
        [SerializeField]
        private bool cycledReload;

        [Tooltip("Determines if the player can reload this weapon when it is full of ammunition.")] [SerializeField]
        private bool canReloadWhenFull = true;

        [Tooltip("Should this weapon be reloaded automatically after firing its last shot?")] [SerializeField]
        private bool automaticReloadOnEmpty;

        [Tooltip("Time after the last shot at which a reload will automatically start.")] [SerializeField]
        private float automaticReloadOnEmptyDelay = 0.25f;

        [Header("Animation")]
        [Tooltip(
            "Transform that represents the weapon's ejection port, meaning the part of the weapon that casings shoot from.")]
        [SerializeField]
        private Transform socketEjection;

        [Tooltip("Weapon Bone Offsets.")] [SerializeField]
        private Offsets weaponOffsets;

        [Tooltip("Sway smoothing value. Makes the weapon sway smoother.")] [SerializeField]
        private float swaySmoothValue = 10.0f;

        [Tooltip("Character arms sway when wielding this weapon.")] [SerializeField]
        private Sway sway;

        [Header("Resources")] [Tooltip("Casing Prefab.")] [SerializeField]
        private GameObject prefabCasing;

        [Tooltip("Projectile Prefab. This is the prefab spawned when the weapon shoots.")] [SerializeField]
        private GameObject prefabProjectile;

        [Tooltip("The AnimatorController a player character needs to use while wielding this weapon.")] [SerializeField]
        public RuntimeAnimatorController controller;

        [Tooltip("Weapon Body Texture.")] [SerializeField]
        private Sprite spriteBody;

        [Header("Audio Clips Holster")] [Tooltip("Holster Audio Clip.")] [SerializeField]
        private AudioClip audioClipHolster;

        [Tooltip("Unholster Audio Clip.")] [SerializeField]
        private AudioClip audioClipUnholster;

        [Header("Audio Clips Reloads")] [Tooltip("Reload Audio Clip.")] [SerializeField]
        private AudioClip audioClipReload;

        [Tooltip("Reload Empty Audio Clip.")] [SerializeField]
        private AudioClip audioClipReloadEmpty;

        [Header("Audio Clips Reloads Cycled")] [Tooltip("Reload Open Audio Clip.")] [SerializeField]
        private AudioClip audioClipReloadOpen;

        [Tooltip("Reload Insert Audio Clip.")] [SerializeField]
        private AudioClip audioClipReloadInsert;

        [Tooltip("Reload Close Audio Clip.")] [SerializeField]
        private AudioClip audioClipReloadClose;

        [Header("Audio Clips Other")]
        [Tooltip("AudioClip played when this weapon is fired without any ammunition.")]
        [SerializeField]
        private AudioClip audioClipFireEmpty;

        [Tooltip("")] [SerializeField] private AudioClip audioClipBoltAction;

        #endregion

        #region FIELDS

        private Animator _animator;
        private WeaponAttachmentManagerBehaviour _attachmentManager;
        private int _ammunitionCurrent;

        #region Attachment Behaviours

        /// <summary>
        /// Equipped scope Reference.
        /// </summary>
        private ScopeBehaviour scopeBehaviour;

        /// <summary>
        /// Equipped Magazine Reference.
        /// </summary>
        private MagazineBehaviour magazineBehaviour;

        /// <summary>
        /// Equipped Muzzle Reference.
        /// </summary>
        private MuzzleBehaviour muzzleBehaviour;

        /// <summary>
        /// Equipped Laser Reference.
        /// </summary>
        private LaserBehaviour laserBehaviour;

        /// <summary>
        /// Equipped Grip Reference.
        /// </summary>
        private GripBehaviour gripBehaviour;

        #endregion

        private PlayerWeapon _characterBehaviour;
        private Transform _playerCamera;

        #endregion

        protected override void Start() { }

        public override void Init(PlayerWeapon player) {
            base.Init(player);
            
            _animator = GetComponent<Animator>();
            _attachmentManager = GetComponent<WeaponAttachmentManagerBehaviour>();
            _characterBehaviour = player;
            
            // We use this in line traces.
            _playerCamera = player.GetComponent<PlayerCamera>().GetCameraRoot();

            scopeBehaviour = _attachmentManager.GetEquippedScope();
            magazineBehaviour = _attachmentManager.GetEquippedMagazine();
            muzzleBehaviour = _attachmentManager.GetEquippedMuzzle();
            laserBehaviour = _attachmentManager.GetEquippedLaser();
            gripBehaviour = _attachmentManager.GetEquippedGrip();

            // Max Out Ammo.
            _ammunitionCurrent = magazineBehaviour.GetAmmunitionTotal();
        }

        #region GETTERS

        public override Offsets GetWeaponOffsets() => weaponOffsets;

        public override float GetFieldOfViewMultiplierAim() {
            //Make sure we don't have any issues even with a broken setup!
            if ( scopeBehaviour != null )
                return scopeBehaviour.GetFieldOfViewMultiplierAim();

            Debug.LogError("Weapon has no scope equipped!");
            
            return 1.0f;
        }

        public override float GetFieldOfViewMultiplierAimWeapon() {
            // Make sure we don't have any issues even with a broken setup!
            if ( scopeBehaviour != null )
                return scopeBehaviour.GetFieldOfViewMultiplierAimWeapon();

            Debug.LogError("Weapon has no scope equipped!");

            return 1.0f;
        }

        public override Animator GetAnimator() => _animator;

        public override Sprite GetSpriteBody() => spriteBody;
        public override float GetMultiplierMovementSpeed() => multiplierMovementSpeed;

        public override AudioClip GetAudioClipHolster() => audioClipHolster;
        public override AudioClip GetAudioClipUnholster() => audioClipUnholster;

        public override AudioClip GetAudioClipReload() => audioClipReload;
        public override AudioClip GetAudioClipReloadEmpty() => audioClipReloadEmpty;

        public override AudioClip GetAudioClipReloadOpen() => audioClipReloadOpen;
        public override AudioClip GetAudioClipReloadInsert() => audioClipReloadInsert;
        public override AudioClip GetAudioClipReloadClose() => audioClipReloadClose;

        public override AudioClip GetAudioClipFireEmpty() => audioClipFireEmpty;
        public override AudioClip GetAudioClipBoltAction() => audioClipBoltAction;

        public override AudioClip GetAudioClipFire() => muzzleBehaviour.GetAudioClipFire();

        public override int GetAmmunitionCurrent() => _ammunitionCurrent;

        public override int GetAmmunitionTotal() => magazineBehaviour.GetAmmunitionTotal();
        public override bool HasCycledReload() => cycledReload;

        public override bool IsAutomatic() => automatic;
        public override bool IsBoltAction() => boltAction;

        public override bool GetAutomaticallyReloadOnEmpty() => automaticReloadOnEmpty;
        public override float GetAutomaticallyReloadOnEmptyDelay() => automaticReloadOnEmptyDelay;

        public override bool CanReloadWhenFull() => canReloadWhenFull;
        public override float GetRateOfFire() => roundsPerMinutes;

        public override bool IsFull() => _ammunitionCurrent == magazineBehaviour.GetAmmunitionTotal();
        public override bool HasAmmunition() => _ammunitionCurrent > 0;

        public override RuntimeAnimatorController GetAnimatorController() => controller;
        public override WeaponAttachmentManagerBehaviour GetAttachmentManager() => _attachmentManager;

        public override Sway GetSway() => sway;
        public override float GetSwaySmoothValue() => swaySmoothValue;

        #endregion

        #region METHODS

        public override void Reload() {
            // Set Reloading Bool. This helps cycled reloads know when they need to stop cycling.
            const string boolName = "Reloading";
            _animator.SetBool(boolName, true);

            //Play Reload Animation.
            _animator.Play(cycledReload ? "Reload Open" : (HasAmmunition() ? "Reload" : "Reload Empty"), 0, 0.0f);
        }

        public override void Fire(float spreadMultiplier = 1.0f) {
            if ( muzzleBehaviour == null ) return;
            if ( _playerCamera == null ) return;

            // This is the point we fire from.
            var muzzleSocket = muzzleBehaviour.GetSocket();

            const string stateName = "Fire";
            _animator.Play(stateName, 0, 0.0f);

            _ammunitionCurrent = Mathf.Clamp(_ammunitionCurrent - 1, 0, magazineBehaviour.GetAmmunitionTotal());

            // Set the slide back if we just ran out of ammunition.
            if ( _ammunitionCurrent == 0 )
                SetSlideBack(1);

            //Play all muzzle effects.
            muzzleBehaviour.Effect();

            //Spawn as many projectiles as we need.
            for ( var i = 0; i < shotCount; i++ ) {
                //Determine a random spread value using all of our multipliers.
                var spreadValue = Random.insideUnitSphere * (spread * spreadMultiplier);
                //Remove the forward spread component, since locally this would go inside the object we're shooting!
                spreadValue.z = 0;
                //Convert to world space.
                spreadValue = muzzleSocket.TransformDirection(spreadValue);

                //Determine the rotation that we want to shoot our projectile in.
                var rotation =
                    Quaternion.LookRotation(_playerCamera.forward * 1000.0f + spreadValue - muzzleSocket.position);

                //If there's something blocking, then we can aim directly at that thing, which will result in more accurate shooting.
                if ( Physics.Raycast(new Ray(_playerCamera.position, _playerCamera.forward),
                        out var hit, maximumDistance, mask) )
                    rotation = Quaternion.LookRotation(hit.point + spreadValue - muzzleSocket.position);

                //Spawn projectile from the projectile spawn point.
                var projectile = Instantiate(prefabProjectile, muzzleSocket.position, rotation);
                //Add velocity to the projectile.
                projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * projectileImpulse;
            }
        }

        public override void FillAmmunition(int amount) {
            //Update the value by a certain amount.
            _ammunitionCurrent = amount != 0
                ? Mathf.Clamp(_ammunitionCurrent + amount,
                    0, GetAmmunitionTotal())
                : magazineBehaviour.GetAmmunitionTotal();
        }

        public override void SetSlideBack(int back) {
            //Set the slide back bool.
            const string boolName = "Slide Back";
            _animator.SetBool(boolName, back != 0);
        }

        public override void EjectCasing() {
            //Spawn casing prefab at spawn point.
            if ( prefabCasing != null && socketEjection != null )
                Instantiate(prefabCasing, socketEjection.position, socketEjection.rotation);
        }

        #endregion
    }
}