using System;
using System.Collections.Generic;
using System.Linq;
using FusionFps.Settings;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;

public class SettingDropdownUI : MonoBehaviour {

    [SerializeField] private TMP_Text _title;
    [SerializeField] private CustomDropdown _dropdown;
    
    private UserSetting<int> _setting;

    private void Awake() {
        _dropdown.dropdownEvent.AddListener(OnDropdownUpdated);
    }

    private void OnDestroy() {
        _dropdown.dropdownEvent.RemoveListener(OnDropdownUpdated);
    }

    public void Bind<T>(UserSetting<int> setting, IEnumerable<T> items, Func<T, string> toString = null) {
        _setting = setting;
        
        _dropdown.index = setting.Value;
        _dropdown.selectedItemIndex = setting.Value;
        
        var newList = new List<CustomDropdown.Item>();
        var index = 0;

        foreach ( var item in items ) {
            newList.Add(new CustomDropdown.Item {
                itemName = toString == null ? item.ToString() : toString(item),
                itemIndex = index++
            });
        }

        _dropdown.dropdownItems = newList;
        _dropdown.SetupDropdown();
        
        _title.SetText(ToSentence(_setting.Name));
    }

    private void OnDropdownUpdated(int value) {
        _setting.Value = value;
    }

    public static string ToSentence(string input) {
        return new string(input.SelectMany((c, i) => i > 0 && char.IsUpper(c) ? new[] { ' ', c } : new[] { c })
            .ToArray());
    }
}