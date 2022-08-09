using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingsDebugUI : MonoBehaviour {
    [SerializeField] private TMP_Text _text;

    private void Update() {
        var text = "";

        var displayLayout = new List<DisplayInfo>();
        Screen.GetDisplayLayout(displayLayout);

        text += $"\n Current resolution: {Screen.currentResolution}";
        text += $"\n Current display mode: {Screen.fullScreenMode.ToString()}";
        text += $"\n Current Monitor Index: {displayLayout.IndexOf(Screen.mainWindowDisplayInfo)}";
        
        _text.SetText(text);
    }
}
