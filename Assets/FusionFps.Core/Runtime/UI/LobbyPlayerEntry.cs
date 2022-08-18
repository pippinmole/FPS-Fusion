using System;
using System.Threading.Tasks;
using Fusion;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerEntry : MonoBehaviour {

    [SerializeField] private RawImage _profilePicture;
    [SerializeField] private TMP_Text _usernameText;
    [SerializeField] private Button _steamProfileButton;
    [SerializeField] private Button _kickPlayerButton;

    private uint _steamId;
    private NetworkRunner _runner;
    private LobbyPlayer _player;

    private void Awake() {
        _kickPlayerButton.onClick.AddListener(KickPlayerPressed);
        _steamProfileButton.onClick.AddListener(OpenSteamProfile);
    }

    private async void Start() {
        await SetSteamProfile();
        _usernameText.SetText(_player.Username);
    }

    private void KickPlayerPressed() {
        if ( _player == null ) return;
        if ( _runner.IsServer && _runner.LocalPlayer == _player ) return;
        
        Debug.Log($"Attempting to kick player: {_player}");
        
        _runner.Disconnect(_player.Object.InputAuthority);
    }

    public void Set(NetworkRunner runner, LobbyPlayer player) {
        _runner = runner;
        _player = player;

        var isSelf = _runner.LocalPlayer == _player;
        _kickPlayerButton.interactable = _runner.IsServer && !isSelf;
    }
    
    private void OpenSteamProfile() {
        Debug.Log("Open steam profile page");
        SteamFriends.OpenUserOverlay(_player.SteamId, "chat");
    }

    private async Task SetSteamProfile() {
        var result = await SteamFriends.GetSmallAvatarAsync(_player.SteamId);
        if ( result == null ) return;

        _profilePicture.texture = result.Value.ToUnityTexture();
    }
}
