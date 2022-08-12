using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenvin.Settings.Framework;
using Zenvin.Settings.Framework.Serialization;

public class ResolutionSetting : SettingBase<Resolution> {
    
    public IEnumerable<string> Options => Screen.resolutions.Cast<string>();

    protected override byte[] OnSerialize() {
        var p = new ValuePacket();
        
        p.Write("height", CurrentValue.height);
        p.Write("height", CurrentValue.width);
        p.Write("height", CurrentValue.refreshRate);

        return p;
    }

    protected override Resolution OnDeserialize(byte[] data) {
        var p = new ValuePacket(data);

        var res = new Resolution {
            width = p.ReadInt32("width"),
            height = p.ReadInt32("height"),
            refreshRate = p.ReadInt32("refreshRate"),
        };

        return res;
    }
}