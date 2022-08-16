using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenvin.Settings.Framework;

public class TextureQualitySetting : ValueArraySetting<int> {

    private static readonly Dictionary<string, int> Qualities = new() {
        {"Low", 5},
        {"Medium", 3},
        {"High", 1},
    };

    protected override void OnValueChanged(ValueChangeMode mode) {
        base.OnValueChanged(mode);
        
        if ( mode is ValueChangeMode.Set or ValueChangeMode.Deserialize ) {
            if ( Qualities.Count < CachedValue )
                return;
            
            var value = Qualities.ElementAt(CachedValue);

            Debug.Log($"Value changed to {value}. Index {CachedValue}");

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
}
    