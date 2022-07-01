using UnityEngine;

public class LoadingPanelUI : MonoBehaviour {
    [SerializeField] private CanvasGroup _canvasGroup;

    private float _time;

    private void OnEnable() {
        _canvasGroup.alpha = 0f;
        _time = 0f;
    }

    private void Update() {
        _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, 1f, _time);

        _time += Time.deltaTime;
    }
}
