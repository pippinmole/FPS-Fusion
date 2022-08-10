using System;
using System.Collections.Generic;
using DG.Tweening;
using FusionFps.Settings;
using UnityEngine;
using UnityEngine.UI;

public partial class SettingsUI {
    
    [SerializeField] private Button _resetKeybindsButton;
    [SerializeField] private SettingKeybindUI _settingKeybindPrefab;
    [SerializeField] private Transform _keybindParent;

    [SerializeField] private SettingDropdownUI _settingDropdownPrefab;
    [SerializeField] private Transform _dropdownParent;

    [SerializeField] private UserConfig _config;

    private static readonly Dictionary<FullScreenMode, string> ScreenModeNames = new() {
        { FullScreenMode.Windowed, "Windowed" },
        { FullScreenMode.MaximizedWindow, "Maximized Windowed" },
        { FullScreenMode.ExclusiveFullScreen, "Fullscreen" },
        { FullScreenMode.FullScreenWindow, "Borderless" }
    };

    private void InitUI() {
        _config.Init();

        _resetKeybindsButton.onClick.RemoveAllListeners();
        _resetKeybindsButton.onClick.AddListener(_config.ResetSettings);

        InitKeybind(_config.Resolution, Screen.resolutions);
        InitKeybind(_config.DisplayMode, (FullScreenMode[])Enum.GetValues(typeof(FullScreenMode)), ToStringFullscreenMode);
        InitKeybind(_config.Monitor, Display.displays, ToStringDisplay);
        
        InitControl(_config.JumpKey);
        InitControl(_config.StrafeRightKey);
        InitControl(_config.StrafeLeftKey);
        InitControl(_config.BackwardKey);
        InitControl(_config.ForwardKey);
    }

    private void InitKeybind<T>(UserSetting<int> setting, IEnumerable<T> items, Func<T, string> toString = null) {
        var obj = Instantiate(_settingDropdownPrefab, _dropdownParent);

        obj.Bind(setting, items, toString);
    }

    private void InitControl(KeyBinding keyBinding) {
        var obj = Instantiate(_settingKeybindPrefab, _keybindParent);

        obj.Bind(keyBinding);
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