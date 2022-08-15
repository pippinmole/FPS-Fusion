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
        _slider.onValueChanged.AddListener(SetAndSaveValue);
    }

    private void SetAndSaveValue(float value) {
        Setting.SetValue(value);
        Setting.ApplyValue();
    }

    protected override void OnSettingValueChanged(SettingBase.ValueChangeMode mode) {
        base.OnSettingValueChanged(mode);
        
        if ( mode == SettingBase.ValueChangeMode.Initialize ) {
            Debug.Log(
                $"Initialising slider {Setting.Name} with currentValue:{Setting.CurrentValue} and cached value:{Setting.CachedValue}");
            _slider.value = Setting.CurrentValue;
        }
    }
}
