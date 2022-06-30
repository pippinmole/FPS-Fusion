using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class LobbyPlayerListUI : MonoBehaviour {
    
    private static readonly Dictionary<PlayerRef, LobbyPlayerEntry> ListItems = new();
    
    [SerializeField] private LobbyPlayerEntry _entryPrefab;
    [SerializeField] private Transform _entryParent;
    [SerializeField] private GameObject _lobbyObject;

    private void Awake() {
        GameManager.Connected += UpdateBoard;
        GameManager.PlayerJoined += AddPlayer;
        GameManager.PlayerLeft += RemovePlayer;

        ClearParent(_entryParent);
        _lobbyObject.SetActive(false);
    }

    private void OnDestroy() {
        GameManager.Connected -= UpdateBoard;
        GameManager.PlayerJoined -= AddPlayer;
        GameManager.PlayerLeft -= RemovePlayer;
    }

    private void UpdateBoard(NetworkRunner runner,List<PlayerRef> players) {
        // TODO: Maybe clear the board here?
        
        foreach ( var player in players ) {
            AddPlayer(runner, player);
        }
    }

    private void ClearParent<T>(T parent) where T : Component {
        foreach (var item in parent.GetComponentsInChildren<T>()) {
            if ( item == parent ) continue;
            
            Destroy(item.gameObject);
        }
    }
    
    private void AddPlayer(NetworkRunner runner, PlayerRef player) {
        if ( ListItems.ContainsKey(player) ) {
            var toRemove = ListItems[player];
            Destroy(toRemove.gameObject);

            ListItems.Remove(player);
        }

        var obj = Instantiate(_entryPrefab, _entryParent).GetComponent<LobbyPlayerEntry>();
        obj.Set(runner, player);

        ListItems.Add(player, obj);
        
        _lobbyObject.SetActive(true);
    }
    
    private void RemovePlayer(NetworkRunner runner, PlayerRef player) {
        if ( !ListItems.ContainsKey(player) )
            return;

        var obj = ListItems[player];
        if ( obj == null ) return;
        
        Destroy(obj.gameObject);
        ListItems.Remove(player);
        
        _lobbyObject.SetActive(true);
    }
    
}