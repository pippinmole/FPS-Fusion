using System;
using Fusion;
using FusionFps.Core;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FusionErrorUI : MonoBehaviour {

    [SerializeField] private ModalWindowManager _errorModal;
    
    private void Awake() {
        SessionManager.RunnerShutdown += OnRunnerShutdown;
    }

    private void OnDestroy() {
        SessionManager.RunnerShutdown -= OnRunnerShutdown;
    }

    private void OnRunnerShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
        if ( shutdownReason is ShutdownReason.Ok or ShutdownReason.AlreadyRunning ) return;
        
        var (title, description) = ShutdownReasonToHuman(shutdownReason);

        _errorModal.titleText = title;
        _errorModal.descriptionText = description;
        _errorModal.UpdateUI();
        _errorModal.OpenWindow();
    }

    private static (string, string) ShutdownReasonToHuman(ShutdownReason shutdownReason) {
        return shutdownReason switch {
            // ShutdownReason.Ok => ("", ""),
            ShutdownReason.Error => ("Internal Error", "An internal error has happened. You may need to restart your game."),
            ShutdownReason.IncompatibleConfiguration => ("Incompatible Configuration", "Your configuration is incompatible with the server, please restart your game."),
            ShutdownReason.ServerInRoom => ("Session Exists", "There already exists a game with that name! Please try another name."),
            ShutdownReason.DisconnectedByPluginLogic => ("Disconnected", "You have been disconnected for an unknown reason."),
            ShutdownReason.GameClosed => ("Game Closed", "The host has closed the session."),
            ShutdownReason.GameNotFound => ("Game Not Found", "The session specified does not exist! Please try again later."),
            ShutdownReason.MaxCcuReached => ("Max CCU Reached", "There isn't much you can do, other than attempting to play later."),
            ShutdownReason.InvalidRegion => ("Invalid Region", "The region your game is set to is invalid. Please change it and try again."),
            ShutdownReason.GameIdAlreadyExists => ("Game Exists", "There already exists a game with that name! Please try another name."),
            ShutdownReason.GameIsFull => ("Game Full", "The server is full. Please try again later when there is a free space available"),
            // ShutdownReason.InvalidAuthentication => ("", ""),
            // ShutdownReason.CustomAuthenticationFailed => ("", ""),
            // ShutdownReason.AuthenticationTicketExpired => ("", ""),
            ShutdownReason.PhotonCloudTimeout => ("Cloud Timeout", ""),
            // ShutdownReason.AlreadyRunning => ("", ""),
            ShutdownReason.InvalidArguments => ("Invalid Arguments", ""),
            // ShutdownReason.HostMigration => ("", ""),
            ShutdownReason.ConnectionTimeout => ("Connection Timed Out", ""),
            ShutdownReason.ConnectionRefused => ("Connection Refused", "The server has refused your connection for an unknown reason."),
            _ => throw new ArgumentException(nameof(shutdownReason), shutdownReason.ToString())
        };
    }

    public void OkButtonPressed() {
        // Reload the scene
        SceneManager.LoadScene(0);
    }
}