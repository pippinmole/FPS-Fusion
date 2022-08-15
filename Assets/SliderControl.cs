using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenvin.Settings.Framework;
using Zenvin.Settings.UI;

public class SliderControl : SettingControl<FloatSetting, float> {

    [SerializeField] private TMP_Text _title;
    [SerializeField] private Slider _slider;

    protected override void OnSetup() {
        base.OnSetup();
        
        _title.SetText(Setting.Name);
        _slider.onValueChanged.AddListener(SetValue);
    }

    protected override void OnSettingValueChanged(SettingBase.ValueChangeMode mode) {
        base.OnSettingValueChanged(mode);
        
        if ( mode == SettingBase.ValueChangeMode.Initialize ) {
            _slider.value = Setting.CurrentValue;
        }
    }
}
