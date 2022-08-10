using System;
using FusionFps.Settings;
using UnityEngine;

[CreateAssetMenu(order = 1, fileName = "New User Configuration", menuName = "User Config/New User Configuration")]
public class UserConfig : ScriptableObject {
    public readonly ResolutionSetting Resolution = new("Resolution");
    public readonly DisplayModeSetting DisplayMode = new("Display Mode");
    public readonly MonitorSetting Monitor = new("Monitor");

    public readonly KeyBinding ForwardKey = new("ForwardKey", KeyCode.W);
    public readonly KeyBinding BackwardKey = new("BackwardKey", KeyCode.S);
    public readonly KeyBinding StrafeLeftKey = new("StrafeLeftKey", KeyCode.A);
    public readonly KeyBinding StrafeRightKey = new("StrafeRightKey", KeyCode.D);
    public readonly KeyBinding JumpKey = new("JumpKey", KeyCode.Space);

    public void ResetSettings() {
        foreach ( var bind in UserSetting.All ) {
            bind.Reset();
        }
    }
}