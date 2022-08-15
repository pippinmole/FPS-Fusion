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
        Debug.Log($"OnSetup for {Setting.Name}. Cached: {Setting.CachedValue}. Current: {Setting.CurrentValue}");

        _title.SetText(Setting.Name);
        _dropdown.dropdownEvent.AddListener(UpdateSetting);
        
        RedrawDropdown();
    }

    private void UpdateSetting(int value) {
        Debug.LogError("HELP");
        Setting.SetValue(value);
        Setting.ApplyValue();
    }

    protected override void OnSettingValueChanged(SettingBase.ValueChangeMode mode) {
        if ( mode == SettingBase.ValueChangeMode.Apply ) {
            Debug.Log($"SettingValueChanged for {Setting.Name} has changed to {Setting.CurrentValue}");
        }

        if ( mode == SettingBase.ValueChangeMode.Initialize ) {
            Debug.Log("Initialising dropdown value changed");
        }
        
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