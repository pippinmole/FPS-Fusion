using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zenvin.Settings.Framework;

public class AntialiasingSetting : ValueArraySetting<int> {

    private static readonly Dictionary<string, int> Qualities = new() {
        { "Low", (int)AntialiasingMode.None },
        { "FXAA", (int)AntialiasingMode.FastApproximateAntialiasing },
        { "SMAA", (int)AntialiasingMode.SubpixelMorphologicalAntiAliasing },
    };

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
}