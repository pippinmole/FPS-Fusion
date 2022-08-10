using FusionFps.Settings;
using UnityEngine;

public class KeyBinding : UserSetting<int> {
    public KeyBinding(string name, KeyCode defaultValue) : base(name, (int)defaultValue) { }
    protected override void OnValueChanged(int value, int oldValue) { }
}