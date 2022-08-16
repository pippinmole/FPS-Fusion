using Newtonsoft.Json.Linq;
using UnityEngine;
using Zenvin.Settings.Framework;
using Zenvin.Settings.Framework.Serialization;

namespace FusionFps.Settings {
    public class KeybindSetting : SettingBase<KeyCode>, ISerializable<JObject> {

        void ISerializable<JObject>.OnSerialize(JObject obj) {
            obj.Add("keycode", (int)CurrentValue);
        }

        void ISerializable<JObject>.OnDeserialize(JObject obj) {
            var mode = obj.GetValue("keycode");
            if ( mode == null ) return;

            SetValue(mode.ToObject<KeyCode>());
            ApplyValue();
        }
    }
}