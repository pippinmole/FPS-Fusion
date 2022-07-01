using System;
using System.Threading.Tasks;
using Fusion;
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

        SessionManager.SessionJoined += OnSessionJoined;
        SessionManager.SessionLeft += OnSessionLeft;
    }

    private void OnDestroy() {
        SessionManager.SessionJoined -= OnSessionJoined;
        SessionManager.SessionLeft -= OnSessionLeft;
    }

    private void Update() {
        _joinGameButton.interactable = !SessionManager.IsBusy;
        _createGameButton.interactable = !SessionManager.IsBusy;
    }

    private void CreateGameClicked() {
        _createGamePanel.SetActive(true);
        _joinGamePanel.SetActive(false);
    }

    private async void JoinGameClicked() {
        _createGamePanel.SetActive(false);
        _joinGamePanel.SetActive(true);

        await SessionManager.Instance.StartClient();
    }

    private void OnSessionJoined(NetworkRunner runner) {
        _createGameButton.interactable = false;
    }
    
    private void OnSessionLeft(NetworkRunner runner) {
        _createGameButton.interactable = true;
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
