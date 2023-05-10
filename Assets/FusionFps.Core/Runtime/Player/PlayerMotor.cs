using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[OrderBefore(typeof(NetworkTransform))]
[DisallowMultipleComponent]
public class PlayerMotor : NetworkTransform {

    [SerializeField] private float _jumpHeight = 1.2f;
    [SerializeField] private float _gravity = -20f;
    [SerializeField] private float _groundedOffset = -0.14f;

    // The radius of the grounded check. Should match the radius of the CharacterController
    [SerializeField] private float _groundedRadius = 0.5f;
    [SerializeField] private LayerMask _groundLayers;

    [SerializeField] private float _acceleration = 10.0f;
    [SerializeField] private float _braking = 10.0f;
    // [SerializeField] private float _maxSpeed = 2f;

    private CharacterController _controller;

    [Networked, HideInInspector] public bool IsGrounded { get; set; }
    [Networked, HideInInspector] public Vector3 Velocity { get; set; }
    
    /// <summary>
    /// Sets the default teleport interpolation velocity to be the CC's current velocity.
    /// For more details on how this field is used, see <see cref="NetworkTransform.TeleportToPosition"/>.
    /// </summary>
    protected override Vector3 DefaultTeleportInterpolationVelocity => Velocity;
    
    protected override void CopyFromBufferToEngine() {
        // Trick: CC must be disabled before resetting the transform state
        _controller.enabled = false;

        // Pull base (NetworkTransform) state from networked data buffer
        base.CopyFromBufferToEngine();

        // Re-enable CC
        _controller.enabled = true;
    }
    
    protected override void Awake() {
        base.Awake();

        _controller = GetComponent<CharacterController>();
    }

    public override void FixedUpdateNetwork() {
        base.FixedUpdateNetwork();

        GroundedCheck();
    }

    private void GroundedCheck() {
        // set sphere position, with offset
        var position = transform.position;
        var spherePosition = new Vector3(position.x, position.y - _groundedOffset, position.z);
        IsGrounded =
            Physics.CheckSphere(spherePosition, _groundedRadius, _groundLayers, QueryTriggerInteraction.Ignore);
    }

    public void Move(Vector3 vector) {
        var deltaTime = Runner.DeltaTime;
        var previousPos = transform.position;
        var moveVelocity = Velocity;

        if ( IsGrounded && moveVelocity.y < 0 ) {
            moveVelocity.y = 0f;
        }

        moveVelocity.y += _gravity * Runner.DeltaTime;

        var horizontalVel = default(Vector3);
        horizontalVel.x = moveVelocity.x;
        horizontalVel.z = moveVelocity.z;

        horizontalVel = vector == default 
            ? Vector3.Lerp(horizontalVel, default, _braking * deltaTime) 
            : Vector3.Lerp(horizontalVel, vector, _acceleration * Runner.DeltaTime);

        moveVelocity.x = horizontalVel.x;
        moveVelocity.z = horizontalVel.z;

        _controller.Move(moveVelocity * deltaTime);

        Velocity = moveVelocity;
        IsGrounded = _controller.isGrounded;
    }

    public void Jump() {
        if ( !IsGrounded ) return;
        
        // the square root of H * -2 * G = how much velocity needed to reach desired height
        var newVel = Velocity;
        newVel.y += Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        Velocity = newVel;
    }

    private void OnDrawGizmosSelected() {
        if ( Object == null || !Object.IsValid ) return;

        var transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        var transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = IsGrounded ? transparentGreen : transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - _groundedOffset, transform.position.z),
            _groundedRadius);
    }
}