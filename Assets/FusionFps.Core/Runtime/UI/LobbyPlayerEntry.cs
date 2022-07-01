using System;
using System.Threading.Tasks;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerEntry : MonoBehaviour {

    [SerializeField] private RawImage _profilePicture;
    [SerializeField] private TMP_Text _usernameText;
    [SerializeField] private Button _steamProfileButton;
    [SerializeField] private Button _kickPlayerButton;

    private NetworkRunner _runner;
    private PlayerRef _player;

    private void Awake() {
        _kickPlayerButton.onClick.AddListener(KickPlayerPressed);
    }

    private void KickPlayerPressed() {
        if ( !_player.IsValid ) return;
        if ( _runner.IsServer && _runner.LocalPlayer == _player ) return;
        
        Debug.Log($"Attempting to kick player: {_player}");
        
        _runner.Disconnect(_player);
    }

    public async Task Set(NetworkRunner runner, PlayerRef player) {
        _runner = runner;
        _player = player;

        var isSelf = _runner.LocalPlayer == _player;
        _kickPlayerButton.interactable = _runner.IsServer && !isSelf;

        await SetSteamProfile(0);
    }

    private async Task SetSteamProfile(uint steamId) {
        throw new NotImplementedException();
    }
}
