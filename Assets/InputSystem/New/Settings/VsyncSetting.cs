using UnityEngine;
using Zenvin.Settings.Framework;

public class VsyncSetting : BoolSetting {
    protected override void OnValueChanged(ValueChangeMode mode) {
        base.OnValueChanged(mode);
        
        QualitySettings.vSyncCount = CurrentValue ? 1 : 0;
    }
}
