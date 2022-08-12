using System;
using System.Collections.Generic;
using UnityEngine;
using Zenvin.Settings.Framework;

public class DropdownSetting : SettingBase<int> {

    [SerializeField] private string[] _values;

    public IEnumerable<string> Options => _values;

    protected override byte[] OnSerialize() {
        return BitConverter.GetBytes(CurrentValue);
    }

    protected override int OnDeserialize(byte[] data) {
        return BitConverter.ToInt32(data, 0);
    }
}