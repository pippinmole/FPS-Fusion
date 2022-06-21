using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCanvas : MonoBehaviour {

    private static readonly Dictionary<PlayerRef, PlayerStatEntry> ListItems = new();

    [SerializeField] private PlayerStatEntry _entryPrefab;
    [SerializeField] private Transform _entryParent;
    [SerializeField] private Transform _playerStatScreen;

    [SerializeField] private Button _startGameButton;
    
    [SerializeField] private LobbyManager _lobbyManager;

    private void Awake() {
        LobbyManager.Connected += UpdateBoard;
        LobbyManager.PlayerJoined += AddPlayer;
        LobbyManager.PlayerLeft += RemovePlayer;

        _startGameButton.onClick.AddListener(LobbyManager.StartGame);
    }

    private void UpdateBoard(List<PlayerRef> players) {
        // TODO: Maybe clear the board here?
        
        foreach ( var player in players ) {
            AddPlayer(player);
        }
    }

    private void OnDestroy() {
        LobbyManager.Connected -= UpdateBoard;
        LobbyManager.PlayerJoined -= AddPlayer;
        LobbyManager.PlayerLeft -= RemovePlayer;
        
        _startGameButton.onClick.RemoveListener(LobbyManager.StartGame);
    }

    private void Update() {
        var lobbyExists = _lobbyManager != null && _lobbyManager.Object != null && _lobbyManager.Object.IsValid;
        if ( lobbyExists ) {
            _startGameButton.gameObject.SetActive(_lobbyManager.Runner.IsServer);

            var isLobbyState = _lobbyManager.GameState == LobbyManager.EGameState.Lobby;
            _playerStatScreen.gameObject.SetActive(isLobbyState || Input.GetKey(KeyCode.Tab));
        }
    }

    private void RemovePlayer(PlayerRef player) {
        if ( !ListItems.ContainsKey(player) )
            return;

        var obj = ListItems[player];
        if ( obj == null ) return;
        
        Destroy(obj.gameObject);
        ListItems.Remove(player);
    }

    private void AddPlayer(PlayerRef player) {
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
