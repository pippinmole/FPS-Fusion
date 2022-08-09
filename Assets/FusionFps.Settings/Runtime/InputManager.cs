using FusionFps.Settings;
using UnityEngine;

public static class InputManager {
    public static readonly ResolutionSetting Resolution = new ResolutionSetting("Resolution");
    public static readonly DisplayModeSetting DisplayMode = new DisplayModeSetting("Display Mode");
    public static readonly MonitorSetting Monitor = new MonitorSetting("Monitor");
    
    public static readonly KeyBinding ForwardKey = new KeyBinding("ForwardKey", KeyCode.W);
    public static readonly KeyBinding BackwardKey = new KeyBinding("BackwardKey", KeyCode.S);
    public static readonly KeyBinding StrafeLeftKey = new KeyBinding("StrafeLeftKey", KeyCode.A);
    public static readonly KeyBinding StrafeRightKey = new KeyBinding("StrafeRightKey", KeyCode.D);
    public static readonly KeyBinding JumpKey = new KeyBinding("JumpKey", KeyCode.Space);
    
    
    
    public static readonly KeyBinding NotReferencingThisAnywhere = new KeyBinding("helpme", KeyCode.Space);
}

public class KeyBinding : UserSetting<int> {
    public KeyBinding(string name, KeyCode defaultValue) : base(name, (int)defaultValue) { }
    protected override void OnValueChanged(int value, int oldValue) { }
}