using System;
using FusionFps.Settings;
using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(order = 1, fileName = "New User Configuration", menuName = "User Config/New User Configuration")]
public class UserConfig : ScriptableObject {

    public ResolutionSetting Resolution { get; private set; }
    public DisplayModeSetting DisplayMode { get; private set; }
    public MonitorSetting Monitor { get; private set; }

    public KeyBinding ForwardKey { get; private set; }
    public KeyBinding BackwardKey { get; private set; }
    public KeyBinding StrafeLeftKey { get; private set; }
    public KeyBinding StrafeRightKey { get; private set; }
    public KeyBinding JumpKey { get; private set; }

    public void Init() {
        Resolution = new ResolutionSetting("Resolution");
        DisplayMode = new DisplayModeSetting("Display Mode");
        Monitor = new MonitorSetting("Monitor");

        ForwardKey = new KeyBinding("ForwardKey", KeyCode.W);
        BackwardKey = new KeyBinding("BackwardKey", KeyCode.S);
        StrafeLeftKey = new KeyBinding("StrafeLeftKey", KeyCode.A);
        StrafeRightKey = new KeyBinding("StrafeRightKey", KeyCode.D);
        JumpKey = new KeyBinding("JumpKey", KeyCode.Space);
    }
    
    public void ResetSettings() {
        foreach ( var bind in UserSetting.All ) {
            bind.Reset();
        }
    }
}