using System;
using UnityEngine;
using Zenvin.Settings.Framework;

public class DisplayModeSetting : SettingBase<FullScreenMode> {
    protected override byte[] OnSerialize() {
        return BitConverter.GetBytes((int)CurrentValue);
    }

    protected override FullScreenMode OnDeserialize(byte[] data) {
        return (FullScreenMode)BitConverter.ToInt32(data);
    }
}