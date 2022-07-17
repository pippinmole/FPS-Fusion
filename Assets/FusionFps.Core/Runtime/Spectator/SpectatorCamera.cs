using System;
using UnityEngine;

public class SpectatorCamera : MonoBehaviour {

    public static SpectatorCamera Instance;
    
    [SerializeField] private Camera _camera;
    [SerializeField] private float _accelerationSpeed = 3f;
    [SerializeField] private float _minSpeed = 3f;
    [SerializeField] private float _maxSpeed = 10f;

    private Vector3 _velocity;
    private float _moveSpeed = 1f;
    private float _sensitivity = 0.9f;
    private float _pitch;
    private float _yaw;

    private void Awake() {
        Instance = this;
    }

    public void SetState(bool state) {
        if ( state )
            Enable();
        else
            Disable();
    }

    public void Enable() {
        gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Disable() {
        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update() {
        ConsumeInput();
        UpdateSensitivity();
        SetRotation();
        Move();
    }

    private void Move() {
        var move = new Vector3();

        if ( Input.GetKey(KeyCode.W) ) move.z += 1;
        if ( Input.GetKey(KeyCode.S) ) move.z -= 1;
        if ( Input.GetKey(KeyCode.D) ) move.x += 1;
        if ( Input.GetKey(KeyCode.A) ) move.x -= 1;
        if ( Input.GetKey(KeyCode.Space) ) move.y += 1;
        if ( Input.GetKey(KeyCode.LeftControl) ) move.y -= 1;

        if ( Input.GetKey(KeyCode.LeftShift) ) {
            _moveSpeed += 3f * Time.deltaTime;
        } else {
            _moveSpeed -= 7f * Time.deltaTime;
        }
        
        _moveSpeed = Mathf.Clamp(_moveSpeed, _minSpeed, _maxSpeed);

        _velocity = Vector3.Lerp(_velocity, move * _moveSpeed, _accelerationSpeed * Time.deltaTime);
        
        _camera.transform.Translate(_velocity * Time.deltaTime, Space.Self);
    }

    private void ConsumeInput() {
        var mouseX = Input.GetAxis("Mouse X") * _sensitivity;
        var mouseY = Input.GetAxis("Mouse Y") * _sensitivity;

        _pitch += -mouseY;
        _yaw += mouseX;
    }

    private void UpdateSensitivity() {
        _sensitivity += Input.mouseScrollDelta.y * 0.4f;
    }

    private void SetRotation() {
        _camera.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0.0f);
    }
}
