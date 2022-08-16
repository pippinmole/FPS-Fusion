using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Zenvin.Settings.Framework;
using Zenvin.Settings.Framework.Serialization;

namespace FusionFps.Settings {
    public class DisplayModeSetting : ValueArraySetting<FullScreenMode>, ISerializable<JObject> {
        void ISerializable<JObject>.OnSerialize(JObject data) {
            data.Add("mode", CurrentValue);
        }

        void ISerializable<JObject>.OnDeserialize(JObject data) {
            var mode = data.GetValue("mode");
            if ( mode == null ) return;

            SetValue(mode.ToObject<int>());
            ApplyValue();
        }

        protected override void OnValueChanged(ValueChangeMode mode) {
            base.OnValueChanged(mode);

            var currentResolution = Screen.currentResolution;
            Screen.SetResolution(currentResolution.width, currentResolution.height, CurrentValueTyped);
        }

        protected override object[] GetValueArray() {
            return Enum.GetValues(typeof(FullScreenMode)).Cast<object>().ToArray();
        }
    }
}