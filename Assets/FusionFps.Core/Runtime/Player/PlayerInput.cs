using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class PlayerInput : NetworkBehaviour, INetworkRunnerCallbacks {

    [SerializeField] private float _sensitivity = 2.5f;

    private PlayerCamera _camera;

    private void Awake() {
        _camera = GetComponent<PlayerCamera>();
    }

    public struct NetworkInputData : INetworkInput {

        public const uint ButtonSprint = 1 << 0;
        public const uint ButtonJump = 1 << 1;
        public const uint ButtonShoot = 1 << 2;
        public const uint ButtonScope = 1 << 3;
        public const uint ButtonMelee = 1 << 4;
        public const uint ButtonGrenade = 1 << 5;
        public const uint ButtonInspect = 1 << 6;
        public const uint ButtonReload = 1 << 7;

        public uint Buttons;
        public uint OneShots;
        
        public Vector2 Move;
        public Vector2 Scroll;

        public Angle YawDelta;
        public Angle PitchDelta;

        public int WeaponIndex;

        public bool IsUp(uint button) => !IsDown(button);
        public bool IsDown(uint button) => (Buttons & button) == button;

        public bool IsDownThisFrame(uint button) => (OneShots & button) == button;
    }

    private bool _jumpPressed;

    private void Update() {
        if ( Input.GetKeyDown(KeyCode.Space) ) _jumpPressed = true;
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) {
        var userInput = new NetworkInputData();

        if ( Input.GetKey(KeyCode.W) ) userInput.Move.y = 1;
        if ( Input.GetKey(KeyCode.S) ) userInput.Move.y = -1;
        if ( Input.GetKey(KeyCode.A) ) userInput.Move.x = -1;
        if ( Input.GetKey(KeyCode.D) ) userInput.Move.x = 1;
        
        userInput.WeaponIndex = -1;
        if ( Input.GetKey(KeyCode.Alpha1) ) userInput.WeaponIndex = 1;
        if ( Input.GetKey(KeyCode.Alpha2) ) userInput.WeaponIndex = 2;

        if ( Input.GetKey(KeyCode.LeftShift) ) userInput.Buttons |= NetworkInputData.ButtonSprint;
        if ( Input.GetKey(KeyCode.Mouse0) ) userInput.Buttons |= NetworkInputData.ButtonShoot;
        if ( Input.GetKey(KeyCode.Mouse1) ) userInput.Buttons |= NetworkInputData.ButtonScope;
        if ( _jumpPressed ) userInput.OneShots |= NetworkInputData.ButtonJump;

        var yawPitch = _camera.ConsumeYawPitch();
        userInput.YawDelta = (float) yawPitch.Item1 * _sensitivity;
        userInput.PitchDelta = (float) yawPitch.Item2 * _sensitivity;
        
        input.Set(userInput);

        _jumpPressed = false;
    }

    public override void Spawned() {
        base.Spawned();
        
        Runner.AddCallbacks(this);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}
