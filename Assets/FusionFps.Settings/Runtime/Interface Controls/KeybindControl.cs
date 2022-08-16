using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenvin.Settings.Framework;
using Zenvin.Settings.UI;

namespace FusionFps.Settings {
    public class KeybindControl : SettingControl<KeybindSetting, KeyCode> {

        private static KeybindControl _currentFocused;

        private bool IsCapturing => _currentFocused == this;

        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _text;

        // OnSetup is called when the Control is spawned via TryInstantiateWith()
        protected override void OnSetup() {
            _title.SetText(Setting.Name);
            _button.onClick.AddListener(FocusKeybind);
        }

        protected override void OnSettingValueChanged(SettingBase.ValueChangeMode mode) {
            // make sure the dropdown's selection is "in sync" with the Setting's value
        }

        private void Update() {
            var text = IsCapturing ? "..." : Setting.CachedValue.ToString();
            _text.SetText(text);
        }

        private void OnGUI() {
            if ( !IsCapturing ) return;

            var keyEvent = Event.current;
            if ( keyEvent.isKey && !keyEvent.isMouse ) {
                var key = keyEvent.keyCode;

                Setting.SetValue(key);
                Setting.ApplyValue();

                _currentFocused = null;
            }
        }

        // private void Update() {
        //     if ( Setting == null )
        //         return;
        //
        //     var text = IsCapturing ? "..." : Setting.CurrentValue.ToString();
        //     _text.SetText(text);
        // }

        private void OnDestroy() {
            _button.onClick.RemoveListener(FocusKeybind);
        }

        private void FocusKeybind() {
            Debug.Log("Attempting to focus keybind");
            if ( Setting != null ) {
                Debug.Log("Setting focused");
                _currentFocused = this;
            }
        }
    }
}