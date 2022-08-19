using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine.Rendering.Universal;
using Zenvin.Settings.Framework;
using Zenvin.Settings.Framework.Serialization;
using Zenvin.Settings.Utility;

namespace FusionFps.Settings {
    [HasDeviatingDefaultValue]
    public class AntialiasingSetting : ValueArraySetting<int>, ISerializable<JObject> {

        private static readonly Dictionary<string, int> Qualities = new() {
            { "Low", (int)AntialiasingMode.None },
            { "FXAA", (int)AntialiasingMode.FastApproximateAntialiasing },
            { "SMAA", (int)AntialiasingMode.SubpixelMorphologicalAntiAliasing },
        };

        protected override int OnSetupInitialDefaultValue() => 1;
        
        protected override void OnValueChanged(ValueChangeMode mode) {
            base.OnValueChanged(mode);

            if ( mode is ValueChangeMode.Set or ValueChangeMode.Deserialize ) {
                if ( Qualities.Count < CachedValue )
                    return;

                var value = Qualities.ElementAt(CachedValue);
                UniversalCameraSettings.Antialiasing = (AntialiasingMode)value.Value;
                UniversalCameraSettings.AntialiasingQuality = AntialiasingQuality.Medium;
            }
        }

        protected override object[] GetValueArray() {
            return Qualities.Keys.Cast<object>().ToArray();
        }

        void ISerializable<JObject>.OnSerialize(JObject value) {
            value.Add("value", CurrentValue);
        }

        void ISerializable<JObject>.OnDeserialize(JObject value) {
            if ( value.TryGetValue("value", out var val) ) {
                SetValue((int)val);
                ApplyValue();
            }
        }
    }
}