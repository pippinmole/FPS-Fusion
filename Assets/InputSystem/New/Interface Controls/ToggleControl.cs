using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenvin.Settings.Framework;
using Zenvin.Settings.UI;

public class ToggleControl : SettingControl<SettingBase<bool>, bool> {

    [SerializeField] private TMP_Text _title;
    [SerializeField] private Toggle _toggle;

    protected override void OnSetup() {
        base.OnSetup();
        
        _title.SetText(Setting.Name);
        _toggle.onValueChanged.AddListener(UpdateValue);
    }

    private void UpdateValue(bool value) {
        Setting.SetValue(value);
        Setting.ApplyValue();
    }

    protected override void OnSettingValueChanged(SettingBase.ValueChangeMode mode) {
        base.OnSettingValueChanged(mode);

        if ( mode == SettingBase.ValueChangeMode.Deserialize ) {
            _toggle.isOn = Setting.CurrentValue;
        }
    }
}
