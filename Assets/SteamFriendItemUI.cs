using System;
using System.Threading.Tasks;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SteamFriendItemUI : MonoBehaviour {

    [SerializeField] private Image _statusBar;
    [SerializeField] private RawImage _profilePicture;
    [SerializeField] private TMP_Text _usernameText;
    [SerializeField] private Button _steamProfileButton;

    private Friend _friend;
    
    public void Set(Friend friend) {
        _friend = friend;
        
        _usernameText.SetText(friend.Name);
        _steamProfileButton.onClick.AddListener(OpenSteamProfile);

        SetStatusBarColor();
        SetSteamProfile(friend.Id);
    }

    private async Task SetSteamProfile(SteamId steamId) {
        var result = await SteamFriends.GetSmallAvatarAsync(steamId);
        if ( result == null ) return;

        _profilePicture.texture = Convert(result.Value);
    }

    private void OpenSteamProfile() {
        SteamFriends.OpenUserOverlay(_friend.Id, "steamid");
    }

    private void SetStatusBarColor() {
        var color = _friend.State switch {
            FriendState.Offline => Color.gray,
            FriendState.Online => Color.green,
            FriendState.Busy => Color.red,
            FriendState.Away => Color.yellow,
            FriendState.Snooze => Color.yellow,
            // FriendState.LookingToTrade => expr,
            // FriendState.LookingToPlay => expr,
            FriendState.Invisible => Color.gray,
            FriendState.Max => Color.gray,
            _ => throw new ArgumentOutOfRangeException()
        };

        _statusBar.color = color;
    }

    private static Texture2D Convert(Steamworks.Data.Image image) {
        // Create a new Texture2D
        var avatar = new Texture2D((int) image.Width, (int) image.Height, TextureFormat.ARGB32, false) {
            // Set filter type, or else its really blury
            filterMode = FilterMode.Trilinear
        };

        // Flip image
        for ( int x = 0; x < image.Width; x++ ) {
            for ( int y = 0; y < image.Height; y++ ) {
                var p = image.GetPixel(x, y);
                avatar.SetPixel(x, (int) image.Height - y,
                    new UnityEngine.Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
            }
        }

        avatar.Apply();
        return avatar;
    }
}
