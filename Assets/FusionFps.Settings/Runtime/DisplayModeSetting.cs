using FusionFps.Settings;
using UnityEngine;

public class DisplayModeSetting : UserSetting<int> {
    public DisplayModeSetting(string name) : base(name, (int)FullScreenMode.ExclusiveFullScreen) { }

    protected override void OnValueChanged(int value, int oldValue) {
        Screen.fullScreenMode = (FullScreenMode)value;
    }
}