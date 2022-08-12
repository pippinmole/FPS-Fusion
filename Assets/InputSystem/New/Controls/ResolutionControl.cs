using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using Zenvin.Settings.Framework;
using Zenvin.Settings.UI;

public class ResolutionControl : SettingControl<ResolutionSetting, Resolution> {
    
    [SerializeField] private CustomDropdown _dropdown;

    protected override void OnSetup() {
        RedrawOptions();
    }

    protected override void OnSettingValueChanged(SettingBase.ValueChangeMode mode) {
        var index = _dropdown.dropdownItems.FindIndex(x => x.itemName == Setting.CachedValue.ToString());
        if ( index != -1 ) {
            _dropdown.index = index;
        } else {
            Debug.LogWarning("");
        }

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
