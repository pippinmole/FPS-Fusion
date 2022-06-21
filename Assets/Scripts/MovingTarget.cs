using Fusion;
using UnityEngine;

public class MovingTarget : NetworkTransform {

    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _maxX = 15f;

    private float _direction = 1f;
    
    public override void FixedUpdateNetwork() {
        base.FixedUpdateNetwork();

        var position = transform.position;
        position += Vector3.right * Runner.DeltaTime * _direction * _speed;

        if ( position.x >= _maxX || position.x <= -_maxX ) {
            _direction = -_direction;
        }

        position.x = Mathf.Clamp(position.x, -_maxX, _maxX);
        transform.position = position;
    }
}
