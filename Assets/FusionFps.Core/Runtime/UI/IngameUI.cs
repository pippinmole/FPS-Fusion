using System;
using Fusion;
using FusionFps.Core;
using TMPro;
using UnityEngine;

public interface IUIComponent {
    void Setup(LobbyPlayer player, NetworkRunner runner);
}

public class IngameUI : MonoBehaviour {

    public void Setup(LobbyPlayer player, NetworkRunner runner) {
        var interfaces = GetComponentsInChildren<IUIComponent>();
        foreach ( var ui in interfaces ) ui.Setup(player, runner);
    }
}