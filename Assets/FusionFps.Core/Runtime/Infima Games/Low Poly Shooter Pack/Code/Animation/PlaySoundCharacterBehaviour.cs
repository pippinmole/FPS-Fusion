// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack {
    /// <summary>
    /// Helper StateMachineBehaviour that allows us to more easily play a specific weapon sound.
    /// </summary>
    public class PlaySoundCharacterBehaviour : StateMachineBehaviour {
        private enum SoundType {
            GrenadeThrow,
            Melee,
            Holster,
            Unholster,
            Reload,
            ReloadEmpty,
            ReloadOpen,
            ReloadInsert,
            ReloadClose,
            Fire,
            FireEmpty,
            BoltAction
        }

        [Header("Setup")] [Tooltip("Delay at which the audio is played.")] [SerializeField]
        private float delay;

        [Tooltip("Type of weapon sound to play.")] [SerializeField]
        private SoundType soundType;

        [Header("Audio Settings")] [Tooltip("Audio Settings.")] [SerializeField]
        private AudioSettings audioSettings = new(1.0f, 0.0f, true);

        private PlayerAnimator _playerAnimator;
        private PlayerWeapon _playerInventory;
        private IAudioManagerService _audioManagerService;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _playerAnimator ??= animator.GetComponentInParent<PlayerAnimator>();
            _playerInventory ??= _playerAnimator.GetInventory();

            // Try to get the equipped weapon's Weapon component.
            if ( !(_playerInventory.GetEquipped() is { } weaponBehaviour) )
                return;

            //Try grab a reference to the sound managing service.
            _audioManagerService ??= ServiceLocator.Current.Get<IAudioManagerService>();
            
            var clip = soundType switch {
                SoundType.GrenadeThrow => _playerAnimator.GetAudioClipsGrenadeThrow().GetRandom(),
                SoundType.Melee => _playerAnimator.GetAudioClipsMelee().GetRandom(),
                SoundType.Holster => weaponBehaviour.GetAudioClipHolster(),
                SoundType.Unholster => weaponBehaviour.GetAudioClipUnholster(),
                SoundType.Reload => weaponBehaviour.GetAudioClipReload(),
                SoundType.ReloadEmpty => weaponBehaviour.GetAudioClipReloadEmpty(),
                SoundType.ReloadOpen => weaponBehaviour.GetAudioClipReloadOpen(),
                SoundType.ReloadInsert => weaponBehaviour.GetAudioClipReloadInsert(),
                SoundType.ReloadClose => weaponBehaviour.GetAudioClipReloadClose(),
                SoundType.Fire => weaponBehaviour.GetAudioClipFire(),
                SoundType.FireEmpty => weaponBehaviour.GetAudioClipFireEmpty(),
                SoundType.BoltAction => weaponBehaviour.GetAudioClipBoltAction(),
                _ => default
            };

            // Play with some delay. Granted, if the delay is set to zero, this will just straight-up play!
            _audioManagerService.PlayOneShotDelayed(clip, audioSettings, delay);
        }
    }
}