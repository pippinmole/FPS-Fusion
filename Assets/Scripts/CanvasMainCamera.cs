using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasMainCamera : MonoBehaviour {

    private Canvas _canvas;

    private void Awake() {
        _canvas = GetComponent<Canvas>();
    }

    private void Update() {
        _canvas.worldCamera = Camera.main;
    }
}