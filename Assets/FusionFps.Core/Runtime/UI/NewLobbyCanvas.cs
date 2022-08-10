using FusionFps.Core;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class NewLobbyCanvas : MonoBehaviour {

    [SerializeField] private Button[] _disableButtonIfInLobby;
    
    [SerializeField] private Button _createGameButton;
    [SerializeField] private Button _joinGameButton;
    [SerializeField] private Button _exitGameButton;

    [SerializeField] private GameObject _createGamePanel;
    [SerializeField] private GameObject _joinGamePanel;
    [SerializeField] private ModalWindowManager _exitGameModal;

    private ISessionManager _sessionManager;

    private void Awake() {
        _sessionManager = ServiceProvider.Get<ISessionManager>();
        
        _createGameButton.onClick.AddListener(CreateGameClicked);
        _joinGameButton.onClick.AddListener(JoinGameClicked);
        _exitGameButton.onClick.AddListener(ExitGameClicked);
    }
    
    private void Update() {
        var active = !_sessionManager.IsInSession && !_sessionManager.IsBusy;
        
        foreach ( var button in _disableButtonIfInLobby ) {
            button.interactable = active;
        }
    }

    private void CreateGameClicked() {
        _createGamePanel.SetActive(true);
        _joinGamePanel.SetActive(false);
    }

    private async void JoinGameClicked() {
        _createGamePanel.SetActive(false);
        _joinGamePanel.SetActive(true);

        await _sessionManager.StartClient();
    }

    private void ExitGameClicked() {
        _exitGameModal.OpenWindow();
    }

    public void CloseGame() {
        _sessionManager.Shutdown();
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
