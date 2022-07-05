using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class Rotator : MonoBehaviour {

    [SerializeField] private Vector3 _speed = new(0f, 3f, 0f);

    private void Update() {
        transform.Rotate(_speed * Time.deltaTime);
    }
}
