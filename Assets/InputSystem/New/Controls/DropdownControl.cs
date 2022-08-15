using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;
using Zenvin.Settings.Framework;
using Zenvin.Settings.UI;

public class DropdownControl : SettingControl<ValueArraySetting, int> {

    [SerializeField] private TMP_Text _title;
    [SerializeField] private CustomDropdown _dropdown;

    protected override void OnSetup() {
        _title.SetText(Setting.Name);
        _dropdown.dropdownEvent.AddListener(UpdateSetting);
    }

    private void UpdateSetting(int value) {
        Setting.SetValue(value);
        Setting.ApplyValue();
    }

    protected override void OnSettingValueChanged(SettingBase.ValueChangeMode mode) {
        RedrawDropdown();
    }

    private void RedrawDropdown() {
        _dropdown.selectedItemIndex = Setting.CurrentValue;
        _dropdown.dropdownItems = new List<CustomDropdown.Item>();

        for ( var i = 0; i < Setting.Length; i++ ) {
            var itemName = Setting.GetValueString(i);
            
            _dropdown.dropdownItems.Add(new CustomDropdown.Item {
                itemIcon = null,
                itemName = itemName,
                itemIndex = i
            });
        }

        _dropdown.SetupDropdown();
    }
}