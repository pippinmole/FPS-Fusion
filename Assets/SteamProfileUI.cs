using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SteamProfileUI : MonoBehaviour {
    [SerializeField] private RawImage _profilePicture;
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private TMP_Text _usernameText;

    private void Start() {
        SteamFriends.OnPersonaStateChange += UpdateProfile;

        UpdateProfile(new Friend(SteamClient.SteamId));
    }

    private void UpdateProfile(Friend friend) {
        if ( !friend.IsMe ) return;

        SetStatusText(friend);
        SetUsernameText(friend);
        SetProfilePicture(friend);
    }

    private void SetUsernameText(Friend friend) {
        _usernameText.SetText(friend.Name);
    }

    private async Task SetProfilePicture(Friend friend) {
        var image = await SteamFriends.GetMediumAvatarAsync(friend.Id);

        if ( image.HasValue ) {
            _profilePicture.texture = image.Value.ToUnityTexture();
        }
    }

    private void SetStatusText(Friend friend) {
        _statusText.SetText(friend.State.ToString());
        _statusText.color = friend.State.ToStatusColor();
    }
}
