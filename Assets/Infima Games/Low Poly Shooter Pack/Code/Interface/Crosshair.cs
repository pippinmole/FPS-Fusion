// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack.Interface {
    public class Crosshair : Element {
        [SerializeField] private float _smoothing = 8.0f;
        [SerializeField] private float _minimumScale = 0.25f;

        private float _current = 1.0f;
        private float _target = 1.0f;
        private RectTransform _rectTransform;
        
        protected override void Awake() {
            base.Awake();

            _rectTransform = GetComponent<RectTransform>();
        }

        protected override void Tick() {
            var visible = playerCharacter.IsCrosshairVisible();

            _target = visible ? 1.0f : 0.0f;

            _current = Mathf.Lerp(_current, _target, Time.deltaTime * _smoothing);
            _rectTransform.localScale = Vector3.one * _current;

            // Hide cross hair objects when too small
            for ( var i = 0; i < transform.childCount; i++ ) {
                transform.GetChild(i).gameObject.SetActive(_current > _minimumScale);
            }
        }
    }
}