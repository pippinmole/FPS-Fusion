using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingKeybindUI : MonoBehaviour {

    private static SettingKeybindUI _currentFocused;
    private bool IsCapturing => _currentFocused == this;

    [SerializeField] private TMP_Text _title;
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _text;

    private KeyBinding _binding;

    private void Awake() {
        _button.onClick.AddListener(FocusKeybind);
    }

    private void OnDestroy() {
        _button.onClick.RemoveListener(FocusKeybind);
    }

    private void Update() {
        if ( _binding == null )
            return;

        var text = IsCapturing ? "..." : ((KeyCode)_binding.Value).ToString();
        _text.SetText(text);
    }

    private void OnGUI() {
        if ( !IsCapturing ) return;

        var keyEvent = Event.current;
        if ( keyEvent.isKey && !keyEvent.isMouse ) {
            var key = keyEvent.keyCode;

            _binding.Value = (int)key;

            _currentFocused = null;
        }
    }

    private void FocusKeybind() {
        if ( _binding != null ) {
            _currentFocused = this;
        }
    }

    public void Bind(KeyBinding binding) {
        _binding = binding;

        _title.SetText(ToSentence(binding.Name));
    }

    public static string ToSentence(string input) {
        return new string(input.SelectMany((c, i) => i > 0 && char.IsUpper(c) ? new[] { ' ', c } : new[] { c })
            .ToArray());
    }
}
