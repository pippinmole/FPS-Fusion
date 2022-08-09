using FusionFps.Settings;
using UnityEngine;

public class MonitorSetting : UserSetting<int> {
    public MonitorSetting(string name) : base(name, Display.displays.Length - 1) { }

    protected override void OnValueChanged(int index, int oldIndex) {
        PlayerPrefs.SetInt("UnitySelectMonitor", index);
        PlayerPrefs.Save();

        Debug.LogError($"Activating display {index}");
    }
}