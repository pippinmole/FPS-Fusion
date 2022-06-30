using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ProfileUI : MonoBehaviour {

    [SerializeField] private Vector3 _openState;
    [SerializeField] private Vector3 _closeState;
    [SerializeField] private float _speed = 3f;
    
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
        var rect = GetWorldRect(_rect);
        
        _isOpen = rect.Contains(Input.mousePosition);
    }

    private static Rect GetWorldRect(RectTransform rectTransform) {
        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        
        // Get the bottom left corner.
        var position = corners[0];

        var size = new Vector2(
            rectTransform.lossyScale.x * rectTransform.rect.size.x,
            rectTransform.lossyScale.y * rectTransform.rect.size.y);

        return new Rect(position, size);
    }
}
