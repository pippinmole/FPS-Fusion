using System;
using Fusion;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
#endif

namespace StarterAssets {
    [OrderBefore(typeof(HitboxManager))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkBehaviour {
        [Header("Player")] [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 4.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.1f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Networked] private PlayerInput.NetworkInputData Inputs { get; set; }
        [Networked] public int Kills { get; set; }
        [Networked] public int Deaths { get; set; }

        public float KillDeathRatio => Deaths == 0 ? Kills : Kills / Deaths;

        private PlayerMotor _motor;
        private PlayerCamera _camera;
        private PlayerInput _input;
        private PlayerHealth _health;

        private int _tick;

        private void Awake() {
            _motor = GetComponent<PlayerMotor>();
            _camera = GetComponent<PlayerCamera>();
            _input = GetComponent<PlayerInput>();
            _health = GetComponent<PlayerHealth>();
        }

        public override void Spawned() {
            base.Spawned();

            var layer = Object.HasInputAuthority ? "LocalPlayer" : "RemotePlayer";
            foreach ( var t in GetComponentsInChildren<Transform>(true) ) {
                t.gameObject.layer = LayerMask.NameToLayer(layer);
            }
        }
        
        public override void FixedUpdateNetwork() {
            base.FixedUpdateNetwork();

            if ( GetInput(out PlayerInput.NetworkInputData input) ) {
                //
                // Copy our inputs that we have received, to a [Networked] property, so other clients can predict using our
                // tick-aligned inputs. This is the core of the Client Prediction system.
                //
                Inputs = input;

                if ( _health.IsAlive && _tick > 5 ) {
                    if ( Inputs.IsDownThisFrame(PlayerInput.NetworkInputData.ButtonJump) ) {
                        Debug.Log("Jump start");
                        _motor.Jump();
                    }

                    Move();
                }
            }

            if ( _health.IsAlive ) {
                _camera.YawAndPitch(Inputs);
            
                // rotate the player left and right
                transform.rotation = Quaternion.Euler(0, (float) _camera.Yaw, 0);
            }

            _tick++;
        }

        private void Move() {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            var targetSpeed = Inputs.IsDown(PlayerInput.NetworkInputData.ButtonSprint) ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon
            if ( Inputs.Move == Vector2.zero ) targetSpeed = 0.0f;

            // normalise input direction
            var inputDirection = new Vector3(Inputs.Move.x, 0.0f, Inputs.Move.y).normalized * targetSpeed;

            if ( Inputs.Move != Vector2.zero ) {
                inputDirection = transform.right * Inputs.Move.x + transform.forward * Inputs.Move.y;
            }

            // move the player
            _motor.Move(inputDirection.normalized * Runner.DeltaTime);
        }
    }
}