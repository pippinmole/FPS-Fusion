using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Audio;
using Zenvin.Settings.Framework;
using Zenvin.Settings.Framework.Serialization;

public class AudioSetting : SettingBase<float>, ISerializable<JObject> {

    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private AudioMixerGroup _group;

    protected override void OnValueChanged(ValueChangeMode mode) {
        base.OnValueChanged(mode);
        
        // We have to make the number really small because log_10(0) does not exist.
        var value = CachedValue == 0f ? 0.00001f : CachedValue;
        var db = Mathf.Log10(value / 100f) * 20f;
        
        _mixer.SetFloat(_group.name, db);
    }

    void ISerializable<JObject>.OnSerialize(JObject obj) {
        obj.Add("value", CurrentValue);
    }

    void ISerializable<JObject>.OnDeserialize(JObject obj) {
        Debug.Log ($"{GUID}: {obj.ToString()}");
        
        if ( obj.TryGetValue("value", out var val) ) {
            Debug.Log($"got value of {(int)val}");
            SetValue((int)val);
            ApplyValue();
        }
    }
}
