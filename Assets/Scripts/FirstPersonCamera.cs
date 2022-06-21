using Fusion;
using UnityEngine;

public class FirstPersonCamera : NetworkBehaviour, IBeforeUpdate {

    [SerializeField] private Transform _cameraRoot;
    [SerializeField] private float _topClamp = 90.0f;
    [SerializeField] private float _bottomClamp = -90.0f;

    [Networked] public Angle Yaw { get; set; }
    [Networked] public Angle Pitch { get; set; }
    
    // private FirstPersonController _controller;
    // private FirstPersonInput _input;
    private FirstPersonHealth _health;

    private Angle _yawDelta;
    private Angle _pitchDelta;

    private void Awake() {
        // _controller = GetComponent<FirstPersonController>();
        // _input = GetComponent<FirstPersonInput>();

        _health = GetComponent<FirstPersonHealth>();
        
        _cameraRoot.gameObject.SetActive(false);
    }

    public override void Render() {
        base.Render();

        if ( _health.IsAlive ) {
            var yaw = Yaw + _yawDelta;
            var pitch = Pitch + _pitchDelta;

            pitch = CheckAndClamp((float) pitch);

            _cameraRoot.transform.rotation = Quaternion.Euler((float) pitch, (float) yaw, 0.0f);
            _cameraRoot.gameObject.SetActive(Object.HasInputAuthority);
        }
    }

    public void BeforeUpdate() {
        _yawDelta += Input.GetAxis("Mouse X");
        _pitchDelta -= Input.GetAxis("Mouse Y");
    }

    public void YawAndPitch(FirstPersonInput.NetworkInputData input) {
        Pitch += (float) input.PitchDelta;
        Pitch = CheckAndClamp((float) Pitch);
        
        Yaw += (float) input.YawDelta;
    }
    
    public (Angle, Angle) ConsumeYawPitch() {
        var yawPitch = (_yawDelta, _pitchDelta);
        _yawDelta = 0;
        _pitchDelta = 0;
        return yawPitch;
    }

    private float CheckAndClamp(float pitch) {
        float returnedPitch;
        
        if ( WrapAngle(pitch) > _topClamp ) {
            returnedPitch = _topClamp;
        } else if ( WrapAngle(pitch) < _bottomClamp ) {
            returnedPitch = _bottomClamp;
        } else {
            returnedPitch = pitch;
        }

        return returnedPitch;
    }

    private static float WrapAngle(float angle) {
        angle %= 360;
        if ( angle > 180 )
            return angle - 360;

        return angle;
    }

    public Transform GetCameraRoot() => _cameraRoot;
}