using System;
using FusionFps.Settings;
using UnityEngine;

[CreateAssetMenu(order = 1, fileName = "New User Configuration", menuName = "User Config/New User Configuration")]
public class UserConfig : ScriptableObject {
    public readonly ResolutionSetting Resolution = new("Resolution");
    public readonly DisplayModeSetting DisplayMode = new("Display Mode");
    public readonly MonitorSetting Monitor = new("Monitor");

    public readonly KeyBinding ForwardKey = new KeyBinding("ForwardKey", KeyCode.W);
    public readonly KeyBinding BackwardKey = new KeyBinding("BackwardKey", KeyCode.S);
    public readonly KeyBinding StrafeLeftKey = new KeyBinding("StrafeLeftKey", KeyCode.A);
    public readonly KeyBinding StrafeRightKey = new KeyBinding("StrafeRightKey", KeyCode.D);
    public readonly KeyBinding JumpKey = new KeyBinding("JumpKey", KeyCode.Space);

    public UserConfig() {
        
    }
    
    public void ResetSettings() {
        foreach ( var bind in UserSetting.All ) {
            bind.Reset();
        }
    }
}