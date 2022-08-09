using FusionFps.Settings;
using UnityEngine;

public class ResolutionSetting : UserSetting<int> {
    // public ResolutionSetting(string name) : base(name, Screen.resolutions.Length - 1) { }
    public ResolutionSetting(string name) : base(name, 0) { }

    protected override void OnValueChanged(int value, int oldValue) {
        var resolution = Screen.resolutions[value];
        var displayMode = InputManager.DisplayMode.Value;
        
        Screen.SetResolution(resolution.width, resolution.height, (FullScreenMode)displayMode);
    }
}