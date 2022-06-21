using System;
using Fusion;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
#endif

namespace StarterAssets {
    [OrderBefore(typeof(HitboxManager))]
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : NetworkBehaviour {
        [Header("Player")] [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 4.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6.0f;

        [Space(10)] [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.1f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Tooltip("Useful for rough ground")] public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.5f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Networked] private FirstPersonInput.NetworkInputData Inputs { get; set; }
        [Networked] private bool IsGrounded { get; set; }
        [Networked] public int Kills { get; set; }
        [Networked] public int Deaths { get; set; }

        public float KillDeathRatio => Deaths == 0 ? Kills : Kills / Deaths;

        private NetworkCharacterControllerPrototype _controller;
        private FirstPersonCamera _camera;
        private FirstPersonInput _input;
        private FirstPersonHealth _health;

        private void Awake() {
            _controller = GetComponent<NetworkCharacterControllerPrototype>();
            _camera = GetComponent<FirstPersonCamera>();
            _input = GetComponent<FirstPersonInput>();
            _health = GetComponent<FirstPersonHealth>();
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
            
            if ( GetInput(out FirstPersonInput.NetworkInputData input) ) {
                //
                // Copy our inputs that we have received, to a [Networked] property, so other clients can predict using our
                // tick-aligned inputs. This is the core of the Client Prediction system.
                //
                Inputs = input;

                GroundedCheck();

                if ( _health.IsAlive ) {
                    Jump();
                    Move();
                }
            }

            if ( _health.IsAlive ) {
                _camera.YawAndPitch(Inputs);

                // rotate the player left and right
                _controller.transform.rotation = Quaternion.Euler(0, (float) _camera.Yaw, 0);
            }
        }

        private void GroundedCheck() {
            // set sphere position, with offset
            var position = transform.position;
            var spherePosition = new Vector3(position.x, position.y - GroundedOffset, position.z);
            IsGrounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);
        }

        private void Move() {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            var targetSpeed = Inputs.IsDown(FirstPersonInput.NetworkInputData.ButtonSprint) ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon
            if ( Inputs.Move == Vector2.zero ) targetSpeed = 0.0f;

            // normalise input direction
            var inputDirection = new Vector3(Inputs.Move.x, 0.0f, Inputs.Move.y).normalized * targetSpeed;

            if ( Inputs.Move != Vector2.zero ) {
                inputDirection = transform.right * Inputs.Move.x + transform.forward * Inputs.Move.y;
            }

            // move the player
            _controller.Move(inputDirection.normalized * Runner.DeltaTime);
        }

        private void Jump() {
            if ( !IsGrounded ) return;

            // Jump
            if ( Inputs.IsDownThisFrame(FirstPersonInput.NetworkInputData.ButtonJump) ) {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                var newVel = _controller.Velocity;
                newVel.y += Mathf.Sqrt(JumpHeight * -2f * _controller.gravity);
                _controller.Velocity = newVel;
            }
        }

        private void OnDrawGizmosSelected() {
            if ( Object == null || !Object.IsValid ) return;

            var transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            var transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = IsGrounded ? transparentGreen : transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }
    }
}