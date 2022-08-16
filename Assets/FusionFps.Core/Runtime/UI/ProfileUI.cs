using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ProfileUI : MonoBehaviour {

    [SerializeField] private Vector3 _openState;
    [SerializeField] private Vector3 _closeState;
    [SerializeField] private float _speed = 3f;
    
    private readonly Vector3[] _bounds = new Vector3[4];
    private RectTransform _rect;
    private bool _isOpen;

    private void Awake() {
        _rect = GetComponent<RectTransform>();

        _rect.anchoredPosition = _isOpen ? _openState : _closeState;
    }

    private void Update() {
        UpdateIsOpen();
        
        var to = _isOpen ? _openState : _closeState;

        _rect.anchoredPosition = Vector3.Lerp(_rect.anchoredPosition, to, _speed * Time.deltaTime);
    }

    private void UpdateIsOpen() {
        _rect.GetLocalCorners(_bounds);

        var screenPoint = Input.mousePosition;

        _isOpen = !IsMouseOutOfBounds() &&
                  RectTransformUtility.RectangleContainsScreenPoint(_rect, screenPoint, Camera.main);
    }

    private static bool IsMouseOutOfBounds() {
        return Input.mousePosition.x > Screen.width || 
               Input.mousePosition.y > Screen.height ||
               Input.mousePosition.x < 0f ||
               Input.mousePosition.y < 0f;
    }
}
