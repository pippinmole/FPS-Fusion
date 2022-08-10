using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageColorSwitch : MonoBehaviour {
    [SerializeField] private Image _image;
    [SerializeField] private Color _color1;
    [SerializeField] private Color _color2;

    private void Start() {
        var siblingIndex = transform.GetSiblingIndex();
        var siblingCount = transform.parent.childCount;

        var fromBottomIndex = siblingCount - siblingIndex;
        SetColor(fromBottomIndex % 2 == 0);
    }

    public void SetColor(bool first) {
        _image.color = first ? _color1 : _color2;
    }
}
