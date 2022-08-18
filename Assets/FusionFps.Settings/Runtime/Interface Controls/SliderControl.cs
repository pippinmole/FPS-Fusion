using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenvin.Settings.Framework;
using Zenvin.Settings.UI;

namespace FusionFps.Settings {
    public class SliderControl : SettingControl<SettingBase<float>, float> {

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

            if ( mode is not SettingBase.ValueChangeMode.Set or SettingBase.ValueChangeMode.Apply ) {
                Debug.Log($"Setting {Setting.Name} slider to {Setting.CurrentValue}");
                _slider.value = Setting.CurrentValue;
            }
        }
    }
}