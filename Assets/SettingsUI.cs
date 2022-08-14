using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenvin.Settings.Framework;
using Zenvin.Settings.Framework.Serialization;
using Zenvin.Settings.UI;

public partial class SettingsUI {

    private static string SettingsLocation =>
        Application.isPlaying ? Path.Combine(Application.persistentDataPath, "Settings.ini") : "";
    
    [SerializeField] private SettingsAsset _config;
    [SerializeField] private SettingsTabUI _tabUI;
    [SerializeField] private SettingControlCollection _prefabs;
    [SerializeField] private Button _resetKeybindsButton;

    private JsonFileSerializer _serializer;
    
    private void LoadSettings() {
        var result = _config.DeserializeSettings(_serializer);
        if ( result ) {
            Debug.Log("Successfully loaded settings from config file.");    
        } else {
            Debug.LogWarning("Unsuccessfully loaded settings from config file.");
        }
        
    }

    private static readonly Dictionary<FullScreenMode, string> ScreenModeNames = new() {
        { FullScreenMode.Windowed, "Windowed" },
        { FullScreenMode.MaximizedWindow, "Maximized Windowed" },
        { FullScreenMode.ExclusiveFullScreen, "Fullscreen" },
        { FullScreenMode.FullScreenWindow, "Borderless" }
    };

    private void InitUI() {
        _serializer = new JsonFileSerializer(SettingsLocation);
        
        _config.Initialize();
        LoadSettings();

        _resetKeybindsButton.onClick.RemoveAllListeners();
        _resetKeybindsButton.onClick.AddListener(() => _config.ResetAllSettings(true));

        var groups = _config.GetGroups();
        foreach ( var group in groups ) {
            // Create UI for group
            var parent = _tabUI.AddTab(group);
            var settings = group.GetAllSettings();

            foreach ( var setting in settings ) {
                if ( _prefabs.TryGetControl(setting.GetType(), out var prefab) ) {
                    // Try instantiating the found prefab with the given setting. If successful, this will automatically spawn and initialize the prefab.
                    if ( prefab.TryInstantiateWith(setting, out var control) ) {
                        
                        // make instance a child of the layout group
                        control.transform.SetParent(parent);
                        
                        // reset instance scale, because parenting UI elements likes to mess that up
                        control.transform.localPosition = Vector3.zero;
                        control.transform.localScale = Vector3.one;
                        control.transform.localRotation = Quaternion.identity;
                    }
                } else {
                    Debug.LogWarning($"Failed to get control prefab for {setting.GetType()}.");
                }
            }
        }
    }

    public void SaveSettings() {
        var result = _config.SerializeSettings(_serializer);
        if ( result ) {
            Debug.Log($"Saved settings json to file: {SettingsLocation}");   
        } else {
            Debug.LogError(
                $"Failed to save settings json to file: {SettingsLocation}. Serializer is null: {_serializer == null}");
        }
    }
}

public partial class SettingsUI : MonoBehaviour {
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeTime = 0.4f;
    [SerializeField] private bool _defaultValue;
    
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