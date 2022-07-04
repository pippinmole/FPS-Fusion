using System;
using System.Collections;
using System.Collections.Generic;
using FusionFps.Core;
using UnityEngine;

public class SessionManagerUI : MonoBehaviour {

    [SerializeField] private GameObject _loadingUI;
    
    private ISessionManager _sessionManager;

    private void Awake() {
        _sessionManager = GetComponent<ISessionManager>();
    }

    private void Update() {
        _loadingUI.SetActive(_sessionManager.IsBusy);
    }
}
