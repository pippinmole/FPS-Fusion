using FusionFps.Settings;
using UnityEngine;

public class ResolutionSetting : UserSetting<int> {
    public ResolutionSetting(string name) : base(name, Application.isPlaying ? Screen.resolutions.Length - 1 : 0) { }

    protected override void OnValueChanged(int value, int oldValue) {
        var resolution = Screen.resolutions[value];
        var displayMode = Screen.fullScreenMode;

        Debug.LogError($"Switching resolution to {resolution.ToString()}");

        Screen.SetResolution(resolution.width, resolution.height, displayMode);
    }
}