using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FusionFps.Settings;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public partial class SettingsUI {

    // [SerializeField] private SettingDropdownUI _resolutionDropdown;
    // [SerializeField] private SettingDropdownUI _displayModeDropdown;
    // [SerializeField] private SettingDropdownUI _monitorDropdown;

    [SerializeField] private Button _resetKeybindsButton;
    [SerializeField] private SettingKeybindUI _settingKeybindPrefab;
    [SerializeField] private Transform _keybindParent;

    [SerializeField] private SettingDropdownUI _settingDropdownPrefab;
    [SerializeField] private Transform _dropdownParent;

    private static readonly Dictionary<FullScreenMode, string> ScreenModeNames = new() {
        { FullScreenMode.Windowed, "Windowed" },
        { FullScreenMode.MaximizedWindow, "Maximized Windowed" },
        { FullScreenMode.ExclusiveFullScreen, "Fullscreen" },
        { FullScreenMode.FullScreenWindow, "Borderless" }
    };

    // private static readonly List<UserSetting<int>> AllVideo = new() {
    //     InputManager.Resolution,
    //     InputManager.Monitor,
    //     InputManager.DisplayMode
    // };
    
    private static readonly List<KeyBinding> AllControls = new() {
        InputManager.ForwardKey,
        InputManager.BackwardKey,
        InputManager.StrafeLeftKey,
        InputManager.StrafeRightKey,
        InputManager.JumpKey,
    };

    private void InitUI() {
        _resetKeybindsButton.onClick.RemoveAllListeners();
        _resetKeybindsButton.onClick.AddListener(Reset);

        InitKeybind(InputManager.Resolution, Screen.resolutions);
        InitKeybind(InputManager.DisplayMode, (FullScreenMode[])Enum.GetValues(typeof(FullScreenMode)), ToStringFullscreenMode);
        InitKeybind(InputManager.Monitor, Display.displays, ToStringDisplay);
        
        for ( var i = AllControls.Count - 1; i >= 0; i-- ) {
            var keyBinding = AllControls[i];
            var obj = Instantiate(_settingKeybindPrefab, _keybindParent);

            obj.Bind(keyBinding);
        }
    }

    private void InitKeybind<T>(UserSetting<int> setting, IEnumerable<T> items, Func<T, string> toString = null) {
        var obj = Instantiate(_settingDropdownPrefab, _dropdownParent);

        obj.Bind(setting, items, toString);
    }

    private void Reset() {
        foreach ( var bind in AllControls ) {
            bind.Reset();
        }
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