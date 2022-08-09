using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FusionFps.Settings;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;

public partial class SettingsUI {

    [SerializeField] private CustomDropdown _resolutionDropdown;
    [SerializeField] private CustomDropdown _displayModeDropdown;
    [SerializeField] private CustomDropdown _monitorDropdown;

    [SerializeField] private Button _resetKeybindsButton;
    [SerializeField] private KeybindUI _keybindPrefab;
    [SerializeField] private Transform _keybindParent;

    private static readonly Dictionary<FullScreenMode, string> ScreenModeNames = new() {
        { FullScreenMode.Windowed, "Windowed" },
        { FullScreenMode.MaximizedWindow, "Maximized Windowed" },
        { FullScreenMode.ExclusiveFullScreen, "Fullscreen" },
        { FullScreenMode.FullScreenWindow, "Borderless" }
    };

    private static readonly List<KeyBinding> KeyBindings = new() {
        InputManager.ForwardKey,
        InputManager.BackwardKey,
        InputManager.StrafeLeftKey,
        InputManager.StrafeRightKey,
        InputManager.JumpKey,
    };

    private void InitUI() {
        _resetKeybindsButton.onClick.RemoveAllListeners();
        _resetKeybindsButton.onClick.AddListener(Reset);
        
        InitDropdown(_resolutionDropdown, InputManager.Resolution, Screen.resolutions);
        InitDropdown(_displayModeDropdown, InputManager.DisplayMode,
            (FullScreenMode[])Enum.GetValues(typeof(FullScreenMode)), ToStringFullscreenMode);
        InitDropdown(_monitorDropdown, InputManager.Monitor, Display.displays, ToStringDisplay);

        for ( var i = KeyBindings.Count - 1; i >= 0; i-- ) {
            var keyBinding = KeyBindings[i];
            var obj = Instantiate(_keybindPrefab, _keybindParent);

            obj.Bind(keyBinding);

            var color = obj.GetComponent<ImageColorSwitch>();
            if ( color != null ) {
                color.SetColor(i % 2 == 0);
            }
        }
    }

    private void Reset() {
        foreach ( var bind in KeyBindings ) {
            bind.Reset();
        }
    }
    
    private static void InitDropdown<T>(CustomDropdown dropdown, UserSetting<int> setting, IEnumerable<T> items,
        Func<T, string> toString = null) {
        dropdown.dropdownEvent.RemoveAllListeners();
        dropdown.dropdownEvent.AddListener(val => setting.Value = val);
        dropdown.index = setting.Value;
        dropdown.selectedItemIndex = setting.Value;

        var newList = new List<CustomDropdown.Item>();
        var index = 0;

        foreach ( var item in items ) {
            newList.Add(new CustomDropdown.Item {
                itemName = toString == null ? item.ToString() : toString(item),
                itemIndex = index++
            });
        }

        dropdown.dropdownItems = newList;
        dropdown.SetupDropdown();
    }

    private static string ToStringDisplay(Display value) => $"Monitor {Array.IndexOf(Display.displays, value) + 1}";
    private static string ToStringFullscreenMode(FullScreenMode mode) => ScreenModeNames[mode];
}

public partial class SettingsUI : MonoBehaviour {
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeTime = 0.4f;
    [SerializeField] private bool _defaultValue;

    [SerializeField] private GameObject[] _panels;

    private bool _isClosed;
    private Tween _currentTween;

    private void Awake() {
        if ( _defaultValue ) {
            Open();
        } else {
            Close();
        }

        InitUI();
    }

    public void CloseAllPanels() {
        foreach ( var panel in _panels ) {
            panel.SetActive(false);
        }
    }

    public void Open() {
        if ( !_isClosed ) return;

        _currentTween?.Kill();
        _currentTween = _canvasGroup.DOFade(1f, _fadeTime).Play();

        _canvasGroup.blocksRaycasts = true;

        _isClosed = false;
    }

    public void Close() {
        if ( _isClosed ) return;

        _currentTween?.Kill();
        _currentTween = _canvasGroup.DOFade(0f, _fadeTime).Play();

        _canvasGroup.blocksRaycasts = false;

        _isClosed = true;
    }

    public void Toggle() {
        if ( _isClosed ) {
            Open();
        } else {
            Close();
        }
    }
}