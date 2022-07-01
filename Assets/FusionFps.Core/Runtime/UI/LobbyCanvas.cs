using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCanvas : MonoBehaviour {

    private static readonly Dictionary<PlayerRef, PlayerStatEntry> ListItems = new();

    [SerializeField] private PlayerStatEntry _entryPrefab;
    [SerializeField] private Transform _entryParent;
    [SerializeField] private Transform _startScreen;
    [SerializeField] private Transform _playerStatScreen;

    [SerializeField] private Transform _sessionListParent;
    [SerializeField] private SessionListObject _sessionItemPrefab;

    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _leaveGameButton;
    
    private void Awake() {
        MatchManager.Connected += UpdateBoard;
        MatchManager.PlayerJoined += AddPlayer;
        MatchManager.PlayerLeft += RemovePlayer;
        
        MatchManager.GameStateChanged += UpdateButtons;

        SessionManager.SessionListUpdated += UpdateSessionList;

        _startGameButton.onClick.AddListener(() => MatchManager.Instance.StartGame());
        _leaveGameButton.onClick.AddListener(() => SessionManager.Instance.Shutdown());
    }
    
    private void UpdateButtons(MatchManager.EGameState state) {
        
        Debug.Log($"State changed to {state}");
        
        switch ( state ) {
            case MatchManager.EGameState.None:
                _playerStatScreen.gameObject.SetActive(false);
                break;
            case MatchManager.EGameState.LobbyConnected:
                _playerStatScreen.gameObject.SetActive(true);
                _startGameButton.gameObject.SetActive(MatchManager.Instance.Runner.IsServer);
                break;
            case MatchManager.EGameState.GameInProgress:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void UpdateSessionList(List<SessionInfo> sessions) {
        Debug.Log("SessionListUpdated");
        
        ClearParent(_sessionListParent);

        foreach ( var session in sessions ) {
            var obj = Instantiate(_sessionItemPrefab, _sessionListParent);
            obj.Set(session);
        }
    }

    private void UpdateBoard(NetworkRunner runner, List<PlayerRef> players) {
        // TODO: Maybe clear the board here?
        
        foreach ( var player in players ) {
            AddPlayer(runner, player);
        }
    }

    private void ClearParent<T>(T parent) where T : Component {
        foreach (var item in parent.GetComponentsInChildren<T>()) {
            Destroy(item.gameObject);
        }
    }

    private void OnDestroy() {
        MatchManager.Connected -= UpdateBoard;
        MatchManager.PlayerJoined -= AddPlayer;
        MatchManager.PlayerLeft -= RemovePlayer;

        MatchManager.GameStateChanged -= UpdateButtons;
        
        SessionManager.SessionListUpdated -= UpdateSessionList;
        
        _startGameButton.onClick.RemoveListener(() => MatchManager.Instance.StartGame());
        _leaveGameButton.onClick.RemoveListener(() => SessionManager.Instance.Shutdown());
    }

    private void Update() {
        _startScreen.gameObject.SetActive(SessionManager.ConnectionStatus == ConnectionStatus.Disconnected);
    }

    private void RemovePlayer(NetworkRunner runner, PlayerRef player) {
        if ( !ListItems.ContainsKey(player) )
            return;

        var obj = ListItems[player];
        if ( obj == null ) return;
        
        Destroy(obj.gameObject);
        ListItems.Remove(player);
    }

    private void AddPlayer(NetworkRunner runner, PlayerRef player) {
        if ( ListItems.ContainsKey(player) ) {
            var toRemove = ListItems[player];
            Destroy(toRemove.gameObject);

            ListItems.Remove(player);
        }

        var obj = Instantiate(_entryPrefab, _entryParent).GetComponent<PlayerStatEntry>();
        obj.Set(-1, player);

        ListItems.Add(player, obj);
    }
}
