using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using FusionFps.Core;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SteamFriendItemUI : MonoBehaviour {

    [SerializeField] private Image _statusBar;
    [SerializeField] private RawImage _profilePicture;
    [SerializeField] private TMP_Text _usernameText;
    [SerializeField] private Button _steamProfileButton;
    [SerializeField] private Button _inviteToGroupButton;

    private const float SECONDS_COOLDOWN = 8f;
    
    private float _timeInvited;
    private ISessionManager _sessionManager;
    private Friend _friend;

    private void Awake() {
        _sessionManager = ServiceProvider.Get<ISessionManager>();

        SteamFriends.OnGameLobbyJoinRequested += async (lobby, steamId) => {
            Debug.Log("Got OnGameLobbyJoinRequested event ");
            await lobby.Join();
        };
    }

    public void Set(Friend friend) {
        _friend = friend;
        
        _usernameText.SetText(friend.Name);
        _steamProfileButton.onClick.AddListener(OpenSteamProfile);
        _inviteToGroupButton.onClick.AddListener(() => InviteToLobby());

        SetStatusBarColor();
        SetSteamProfile(friend.Id);
    }

    private async Task SetSteamProfile(SteamId steamId) {
        var result = await SteamFriends.GetSmallAvatarAsync(steamId);
        if ( result == null ) return;

        _profilePicture.texture = result.Value.ToUnityTexture();
    }

    private void OpenSteamProfile() {
        Debug.Log("Open steam profile page");
        SteamFriends.OpenUserOverlay(_friend.Id, "chat");
    }

    private async Task InviteToLobby() {
        if ( Time.realtimeSinceStartup - _timeInvited < SECONDS_COOLDOWN ) return;
        
        if ( !_sessionManager.IsInSession ) {
            // Create session
            await _sessionManager.CreateSession("lobby", new Dictionary<string, SessionProperty>());   
        }

        // Invite player
        var success = _friend.InviteToGame("");
        Debug.Log(success ? "Successfully sent steam message" : "Failed to send steam message");

        _timeInvited = Time.realtimeSinceStartup;
    }

    private void SetStatusBarColor() {
        _statusBar.color = _friend.State.ToStatusColor();
    }
}
