using System.Collections.Generic;
using System.Linq;
using Fusion;
using FusionFps.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerListUI : MonoBehaviour {
    
    private static Dictionary<PlayerRef, LobbyPlayerEntry> _listItems = new();
    
    [SerializeField] private TMP_Text _lobbyTitleText;
    [SerializeField] private LobbyPlayerEntry _entryPrefab;
    [SerializeField] private Transform _entryParent;
    [SerializeField] private GameObject _lobbyObject;
    [SerializeField] private Button _startGameButton;

    private IMatchManager _matchManager;
    private ISessionManager _sessionManager;
    
    private void Awake() {
        _matchManager = ServiceProvider.Get<IMatchManager>();
        _sessionManager = ServiceProvider.Get<ISessionManager>();
        
        _matchManager.Connected += UpdateBoard;
        _matchManager.PlayerJoined += AddPlayer;
        _matchManager.PlayerLeft += RemovePlayer;

        _sessionManager.SessionJoined += OnSessionJoined;
        _sessionManager.SessionLeft += OnSessionLeft;
        
        ClearParent(_entryParent);
        
        _startGameButton.onClick.AddListener(StartGameClicked);
        
        // By default a session will not be available
        _lobbyObject.SetActive(false);
    }
    
    private void OnDestroy() {
        _matchManager.Connected -= UpdateBoard;
        _matchManager.PlayerJoined -= AddPlayer;
        _matchManager.PlayerLeft -= RemovePlayer;
        
        _sessionManager.SessionJoined -= OnSessionJoined;
        _sessionManager.SessionLeft -= OnSessionLeft;

        _listItems = new Dictionary<PlayerRef, LobbyPlayerEntry>();
    }

    private void StartGameClicked() {
        _matchManager.LoadSessionMap();
    }

    public void LeaveGame() {
        _sessionManager.Shutdown();
    }

    private void OnSessionJoined(NetworkRunner runner) {
        _lobbyObject.SetActive(true);
        
        _startGameButton.gameObject.SetActive(runner.IsServer);
    }

    private void OnSessionLeft(NetworkRunner runner) {
        _lobbyObject.SetActive(false);
    }

    private void UpdateBoard(NetworkRunner runner,List<PlayerRef> players) {
        // TODO: Maybe clear the board here?
        
        foreach ( var player in players ) {
            AddPlayer(runner, player);
        }
        
        UpdateLobbyTitleText(runner);
    }

    private void ClearParent<T>(T parent) where T : Component {
        foreach (var item in parent.GetComponentsInChildren<T>()) {
            if ( item == parent ) continue;
            
            Destroy(item.gameObject);
        }
    }
    
    private void AddPlayer(NetworkRunner runner, PlayerRef player) {
        if ( _listItems.ContainsKey(player) ) {
            var toRemove = _listItems[player];
            Destroy(toRemove.gameObject);

            _listItems.Remove(player);
        }

        var obj = Instantiate(_entryPrefab, _entryParent).GetComponent<LobbyPlayerEntry>();
        obj.Set(runner, player);

        _listItems.Add(player, obj);

        UpdateLobbyTitleText(runner);
    }
    
    private void RemovePlayer(NetworkRunner runner, PlayerRef player) {
        if ( !_listItems.ContainsKey(player) )
            return;

        var obj = _listItems[player];
        if ( obj == null ) return;
        
        Destroy(obj.gameObject);
        _listItems.Remove(player);

        UpdateLobbyTitleText(runner);
    }
    
    private void UpdateLobbyTitleText(NetworkRunner runner) {
        var players = runner.ActivePlayers.Count();
        var maxPlayers = runner.Simulation.Config.DefaultPlayers;

        _lobbyTitleText.SetText($"Lobby ({players}/{maxPlayers})");
    }
}