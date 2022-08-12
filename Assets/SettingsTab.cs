using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenvin.Settings.Framework;

public class SettingsTab : MonoBehaviour {

    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _text;

    private SettingsTabUI _parent;
    private SettingsGroup _group;
    private int _index;
    
    public void Setup(SettingsTabUI parent, SettingsGroup group, int index) {
        _parent = parent;
        _group = group;
        _index = index;
        
        _button.onClick.AddListener(EnablePanel);
        _text.SetText(group.Name);
    }

    private void EnablePanel() {
        _parent.EnablePanel(_index);
    }
}