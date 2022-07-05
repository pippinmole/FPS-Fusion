using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public static class SteamExtensions {
    
    public static Texture2D ToUnityTexture(this Steamworks.Data.Image image) {
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

    public static Color ToStatusColor(this FriendState state) {
        return state switch {
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
    }
}
