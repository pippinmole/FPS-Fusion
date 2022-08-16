using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zenvin.Settings.Framework;
using Zenvin.Settings.Framework.Serialization;

public class ShadowQualitySetting : ValueArraySetting<int>, ISerializable<JObject> {
    
    private static readonly Dictionary<string, int> Qualities = new() {
        {"Low", 1},
        {"Medium", 2},
        {"High", 3},
        {"Ultra", 4},
    };
    
    protected override void OnValueChanged(ValueChangeMode mode) {
        base.OnValueChanged(mode);
        
        if ( mode is ValueChangeMode.Set or ValueChangeMode.Deserialize ) {
            if ( Qualities.Count < CachedValue )
                return;
            
            var value = Qualities.ElementAt(CachedValue);
            ((UniversalRenderPipelineAsset)QualitySettings.renderPipeline).shadowCascadeCount = value.Value;
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