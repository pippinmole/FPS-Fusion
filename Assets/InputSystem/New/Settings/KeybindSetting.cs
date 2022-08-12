using System;
using System.Collections.Generic;
using UnityEngine;
using Zenvin.Settings.Framework;

public class KeybindSetting : SettingBase<KeyCode> {

    public readonly IEnumerable<KeyCode> Options = (KeyCode[])Enum.GetValues(typeof(KeyCode));

    protected override KeyCode OnDeserialize(byte[] data) {
        return (KeyCode)BitConverter.ToInt32(data, 0);
    }

    protected override byte[] OnSerialize() {
        return BitConverter.GetBytes((int)CurrentValue);
    }
}
