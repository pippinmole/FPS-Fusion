using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zenvin.Settings.Framework;

public class ShadowQualitySetting : ValueArraySetting<int> {
    
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
}