using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using Zenvin.Settings.Framework;
using Zenvin.Settings.UI;

public class DropdownControl : SettingControl<DropdownSetting, int> {

    [SerializeField] private CustomDropdown _dropdown;

    protected override void OnSetup() {
        RedrawOptions();
    }

    protected override void OnSettingValueChanged(SettingBase.ValueChangeMode mode) {
        _dropdown.index = Setting.CachedValue;
        
        RedrawOptions();
    }

    private void RedrawOptions() {
        _dropdown.dropdownItems = new List<CustomDropdown.Item>();
        var i = 0;
        foreach ( var item in Setting.Options ) {
            _dropdown.dropdownItems.Add(new CustomDropdown.Item {
                itemIcon = null,
                itemName = item,
                itemIndex = i++
            });
        }
        
        _dropdown.UpdateItemLayout();
    }
}