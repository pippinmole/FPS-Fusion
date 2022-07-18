using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFootsteps : MonoBehaviour {

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _audioClipWalking;
    [SerializeField] private AudioClip _audioClipRunning;

    private CharacterController _characterController;
    private PlayerController _controller;

    private void Awake() {
        _characterController = GetComponent<CharacterController>();
        _controller = GetComponent<PlayerController>();
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _audioClipWalking;
        _audioSource.loop = true;
    }

    private void Update() {
        PlayFootstepSounds();
    }

    private void PlayFootstepSounds() {
        // Check if we're moving on the ground. We don't need footsteps in the air.
        var isMoving = _characterController.isGrounded && _characterController.velocity.sqrMagnitude > 0.1f;

        if ( isMoving ) {
            // _audioSource.clip = _playerCharacter.IsRunning() ? _audioClipRunning : _audioClipWalking;
            _audioSource.clip = false ? _audioClipRunning : _audioClipWalking;

            if ( !_audioSource.isPlaying )
                _audioSource.Play();
        } else if ( _audioSource.isPlaying ) {
            _audioSource.Pause();
        }
    }
}