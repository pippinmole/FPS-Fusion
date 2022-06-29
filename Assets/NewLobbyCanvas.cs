using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;

public class NewLobbyCanvas : MonoBehaviour {

    [SerializeField] private Button _createGameButton;
    [SerializeField] private Button _joinGameButton;
    [SerializeField] private Button _exitGameButton;

    [SerializeField] private GameObject _createGamePanel;
    [SerializeField] private GameObject _joinGamePanel;
    [SerializeField] private ModalWindowManager _exitGameModal;

    private void Awake() {
        _createGameButton.onClick.AddListener(CreateGameClicked);
        _joinGameButton.onClick.AddListener(JoinGameClicked);
        _exitGameButton.onClick.AddListener(ExitGameClicked);
    }

    private void CreateGameClicked() {
        _createGamePanel.SetActive(true);
        _joinGamePanel.SetActive(false);
    }

    private void JoinGameClicked() {
        _createGamePanel.SetActive(false);
        _joinGamePanel.SetActive(true);

        SessionManager.Instance.StartClient();
    }

    private void ExitGameClicked() {
        _exitGameModal.OpenWindow();
    }

    public void CloseGame() {
        SessionManager.Instance.Shutdown();
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
