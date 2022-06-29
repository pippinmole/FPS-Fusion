using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionListObject : MonoBehaviour {
    
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _lobbyTitleText;
    [SerializeField] private TMP_Text _playerCountText;
    [SerializeField] private TMP_Text _pingText;

    private SessionInfo _session;
    
    private void Awake() {
        _button.onClick.AddListener(JoinServer);
    }

    public void Set(SessionInfo session) {
        _session = session;

        _lobbyTitleText.SetText(session.Name);
        _playerCountText.SetText($"{session.PlayerCount}/{session.MaxPlayers}");
    }

    private void JoinServer() {
        Debug.Log($"Attempting to join session {_session.Name}");
        SessionManager.Instance.JoinSession(_session.Name);
    }
}
