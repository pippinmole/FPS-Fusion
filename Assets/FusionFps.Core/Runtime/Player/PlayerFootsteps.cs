using Fusion;
using UnityEngine;

public class PlayerFootsteps : NetworkBehaviour {

    [Networked] private TickTimer LastStepTick { get; set; }

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _audioClipWalking;
    // [SerializeField] private AudioClip _audioClipRunning;
    [SerializeField] private float _stepDelay = 0.6f;

    private CharacterController _characterController;

    private void Awake() {
        _characterController = GetComponent<CharacterController>();

        if ( _audioSource != null ) {
            _audioSource.clip = _audioClipWalking;
        }
    }
    
    public override void FixedUpdateNetwork() {
        base.FixedUpdateNetwork();

        if ( _characterController.isGrounded && _characterController.velocity.sqrMagnitude > 0.1f ) {
            PlayFootstepSounds();
        }
    }

    private void PlayFootstepSounds() {
        if ( _audioSource == null )
            return;

        if ( LastStepTick.ExpiredOrNotRunning(Runner) ) {
            _audioSource.Play();

            LastStepTick = TickTimer.CreateFromSeconds(Runner, _stepDelay);
        }
    }
}