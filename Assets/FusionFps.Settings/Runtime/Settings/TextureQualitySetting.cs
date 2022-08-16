using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Zenvin.Settings.Framework;
using Zenvin.Settings.Framework.Serialization;

namespace FusionFps.Settings {
    public class TextureQualitySetting : ValueArraySetting<int>, ISerializable<JObject> {

        private static readonly Dictionary<string, int> Qualities = new() {
            { "Low", 5 },
            { "Medium", 3 },
            { "High", 1 },
        };

        protected override void OnValueChanged(ValueChangeMode mode) {
            base.OnValueChanged(mode);

            if ( mode is ValueChangeMode.Set or ValueChangeMode.Deserialize ) {
                if ( Qualities.Count < CachedValue )
                    return;

                var value = Qualities.ElementAt(CachedValue);
                
                QualitySettings.streamingMipmapsAddAllCameras = true;
                QualitySettings.masterTextureLimit = value.Value;
            }
        }

        public override string GetValueString(int index) {
            return Qualities.ElementAt(index).Key;
        }

        protected override object[] GetValueArray() {
            return Qualities.Values.Cast<object>().ToArray();
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