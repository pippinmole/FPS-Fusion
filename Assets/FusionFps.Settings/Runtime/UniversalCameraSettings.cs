using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(UniversalAdditionalCameraData))]
public class UniversalCameraSettings : MonoBehaviour {

    private static readonly List<UniversalCameraSettings> All = new();
    private static event Action SettingsUpdated;

    private static AntialiasingMode _antialiasing;

    public static AntialiasingMode Antialiasing {
        get => _antialiasing;
        set {
            _antialiasing = value;
            SettingsUpdated?.Invoke();
        }
    }
    
    private static AntialiasingQuality _antialiasingQuality;

    public static AntialiasingQuality AntialiasingQuality {
        get => _antialiasingQuality;
        set {
            _antialiasingQuality = value;
            SettingsUpdated?.Invoke();
        }
    }
    
    private UniversalAdditionalCameraData _data;

    private void Awake() {
        _data = GetComponent<UniversalAdditionalCameraData>();
    }

    private void UpdateCameraSettings() {
        _data.antialiasing = _antialiasing;
        _data.antialiasingQuality = _antialiasingQuality;
    }

    private void OnEnable() {
        SettingsUpdated += UpdateCameraSettings;
    }

    private void OnDisable() {
        SettingsUpdated -= UpdateCameraSettings;
    }
}
