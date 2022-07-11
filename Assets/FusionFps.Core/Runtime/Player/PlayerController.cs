using Fusion;
using FusionFps.Core;
using UnityEngine;

[OrderBefore(typeof(HitboxManager))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour {

    [SerializeField] private float _moveSpeed = 5.5f;
    [SerializeField] private float _sprintSpeed = 8f;

    [Networked] private PlayerInput.NetworkInputData Inputs { get; set; }
    [Networked] public int Kills { get; set; }
    [Networked] public int Deaths { get; set; }
    [Networked] public bool CanMove { get; set; }

    public float KillDeathRatio => Deaths == 0 ? Kills : Kills / Deaths;
    
    private PlayerMotor _motor;
    private PlayerCamera _camera;
    private PlayerHealth _health;
    
    private void Awake() {
        _motor = GetComponent<PlayerMotor>();
        _camera = GetComponent<PlayerCamera>();
        _health = GetComponent<PlayerHealth>();
    }

    public override void Spawned() {
        base.Spawned();

        SetLayerWithChildren();
    }

    public override void FixedUpdateNetwork() {
        base.FixedUpdateNetwork();
        
        if ( GetInput(out PlayerInput.NetworkInputData input) ) {
            //
            // Copy our inputs that we have received, to a [Networked] property, so other clients can predict using our
            // tick-aligned inputs. This is the core of the Client Prediction system.
            //
            Inputs = input;

            if ( _health.IsAlive && CanMove ) {
                if ( Inputs.IsDownThisFrame(PlayerInput.NetworkInputData.ButtonJump) ) {
                    _motor.Jump();
                }

                Move();
                
                _camera.YawAndPitch(Inputs);

                // rotate the player left and right
                transform.rotation = Quaternion.Euler(0, (float) _camera.CameraYaw, 0);
            }
        }
    }

    private void Move() {
        var targetSpeed = Inputs.IsDown(PlayerInput.NetworkInputData.ButtonSprint) ? _sprintSpeed : _moveSpeed;
        var inputDirection = transform.right * Inputs.Move.x + transform.forward * Inputs.Move.y;

        _motor.Move(inputDirection.normalized * targetSpeed);
    }

    private void SetLayerWithChildren() {
        var layer = Object.HasInputAuthority ? "LocalPlayer" : "RemotePlayer";
        foreach ( var t in GetComponentsInChildren<Transform>(true) ) {
            t.gameObject.layer = LayerMask.NameToLayer(layer);
        }
    }
}