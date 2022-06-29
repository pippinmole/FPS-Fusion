using Fusion;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour, IBeforeUpdate {

    [SerializeField] private Transform _cameraRoot;
    [SerializeField] private Camera _camera;
    [SerializeField] private float _topClamp = 90.0f;
    [SerializeField] private float _bottomClamp = -90.0f;
    [SerializeField] private float _sensitivity = 0.6f;
    
    [Networked] public Angle Yaw { get; set; }
    [Networked] public Angle Pitch { get; set; }
    
    private PlayerHealth _health;
    private Angle _yawDelta;
    private Angle _pitchDelta;

    private void Awake() {
        _health = GetComponent<PlayerHealth>();
        
        _camera.gameObject.SetActive(false);
    }

    public override void Render() {
        base.Render();

        if ( _health.IsAlive ) {
            var yaw = Yaw + _yawDelta;
            var pitch = Pitch + _pitchDelta;

            pitch = CheckAndClamp((float) pitch);

            _cameraRoot.transform.rotation = Quaternion.Euler((float) pitch, (float) yaw, 0.0f);
            _camera.gameObject.SetActive(Object.HasInputAuthority);
        }
    }

    public void BeforeUpdate() {
        _yawDelta += Input.GetAxis("Mouse X") * _sensitivity;
        _pitchDelta -= Input.GetAxis("Mouse Y") * _sensitivity;
    }

    public void YawAndPitch(PlayerInput.NetworkInputData input) {
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