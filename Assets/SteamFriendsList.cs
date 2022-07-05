using System;
using System.Collections.Generic;
using System.Linq;
using FusionFps.Core;
using Steamworks;
using UnityEngine;

public class SteamFriendsList : MonoBehaviour {

    private static readonly List<int> Map = new() {
        (int) FriendState.Online,
        (int) FriendState.Away,
        (int) FriendState.Snooze,
        (int) FriendState.Offline,
        (int) FriendState.Invisible
    };

    [SerializeField] private SteamFriendItemUI _prefab;
    [SerializeField] private Transform _parent;

    private void Awake() {
        SteamFriends.OnPersonaStateChange += (_) => UpdateList();
    }

    private void Start() {
        UpdateList();
    }

    private void UpdateList() {
        ClearParent(_parent);

        var friends = SteamFriends.GetFriends()
            .OrderBy(x => Map.IndexOf((int) x.State))
            .ToList();

        foreach ( var friend in friends ) {
            var obj = Instantiate(_prefab, _parent);
            obj.Set(friend);
        }
    }

    private void ClearParent<T>(T parent) where T : Component {
        foreach (var item in parent.GetComponentsInChildren<T>()) {
            if ( parent == item ) continue;
            
            Destroy(item.gameObject);
        }
    }
}
