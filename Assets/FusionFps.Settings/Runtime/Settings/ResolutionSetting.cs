using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Zenvin.Settings.Framework;
using Zenvin.Settings.Framework.Serialization;

namespace FusionFps.Settings {
    public class ResolutionSetting : ValueArraySetting<Resolution>, ISerializable<JObject> {

        protected override object[] GetValueArray() {
            return Screen.resolutions.Cast<object>().ToArray();
        }

        protected override int OnSetupInitialDefaultValue() {
            return Screen.resolutions.Length - 1;
        }

        protected override void OnPostInitialize() {
            base.OnPostInitialize();

            UpdateScreenResolution();
        }

        /// <summary>
        /// Updates the screen resolution using the current saved value
        /// </summary>
        private void UpdateScreenResolution() {
            Screen.SetResolution(CurrentValueTyped.width, CurrentValueTyped.height, Screen.fullScreenMode);
        }

        protected override void OnValueChanged(ValueChangeMode mode) {
            base.OnValueChanged(mode);

            if ( mode is ValueChangeMode.Apply or ValueChangeMode.Deserialize ) {
                UpdateScreenResolution();
            }

#if UNITY_EDITOR
            Debug.LogWarning("Cannot change resolution of editor game window.");
#endif
        }

        private int SanitiseResolution(int index, int width, int height, int refreshRate) {
            // data could not be fully loaded
            if ( width == -1 || height == -1 || refreshRate == -1 ) {
                return 0;
            }

            // check resolution at saved index first
            if ( index >= 0 && index < Length ) {
                var res = this[index];
                if ( res.width == width && res.height == height && res.refreshRate == refreshRate ) {
                    return index;
                }
            }

            // iterate through available resolutions to see, if any fit the saved values
            for ( var i = 0; i < Length; i++ ) {
                var res = this[i];
                if ( res.width == width && res.height == height && res.refreshRate == refreshRate ) {
                    return i;
                }
            }

            Debug.Log($"No resolution found for resolution index: {index}.");

            // no resolution fitting the save was found
            return Mathf.Max(Screen.resolutions.Length - 1, index);
        }

        void ISerializable<JObject>.OnSerialize(JObject obj) {
            var res = CurrentValueTyped;

            obj.Add("index", CurrentValue);
            obj.Add("width", res.width);
            obj.Add("height", res.height);
            obj.Add("refreshRate", res.refreshRate);
        }

        void ISerializable<JObject>.OnDeserialize(JObject obj) {
            var index = (int)obj.GetValue("index");
            var width = (int)obj.GetValue("width");
            var height = (int)obj.GetValue("height");
            var refreshRate = (int)obj.GetValue("refreshRate");

            var sanitisedIndex = SanitiseResolution(index, width, height, refreshRate);

            SetValue(sanitisedIndex);
            ApplyValue();
        }
    }
}