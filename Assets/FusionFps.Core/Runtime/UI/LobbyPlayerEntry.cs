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

    private NetworkRunner _runner;
    private Friend _friend;
    private LobbyPlayer _player;

    private void Awake() {
        _kickPlayerButton.onClick.AddListener(KickPlayerPressed);
        _steamProfileButton.onClick.AddListener(OpenSteamProfile);
    }

    private async void Start() {
        await SetSteamProfile(_player.SteamId);
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
        SteamFriends.OpenUserOverlay(_friend.Id, "chat");
    }

    private async Task SetSteamProfile(SteamId steamId) {
        var result = await SteamFriends.GetSmallAvatarAsync(steamId);
        if ( result == null ) return;

        _profilePicture.texture = result.Value.ToUnityTexture();
    }
}
