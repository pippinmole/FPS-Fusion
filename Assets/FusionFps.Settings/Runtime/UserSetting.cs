using System;
using UnityEngine;

namespace FusionFps.Settings {
    public abstract class UserSetting<T> where T : struct {

        public event Action<T> Changed;

        public string Name { get; }
        private string PersistString => $"{typeof(T)}_{Name}";

        private readonly T _defaultValue;
        private T? _value;

        public T Value {
            get {
                if ( _value == null ) {
                    _value = GetSavedValueOrDefault();
                    SaveValue();
                }

                return _value ?? throw new NullReferenceException();
            }
            set {
                var oldValue = _value.GetValueOrDefault();

                _value = value;
                Changed?.Invoke(value);
                OnValueChanged(value, oldValue);
                SaveValue();
            }
        }

        public void Reset(bool saveDefaultValue = true) {
            Value = _defaultValue;

            if ( saveDefaultValue ) {
                SaveValue();
            }
        }

        protected UserSetting(string name, T defaultValue) {
            Name = name;

            _defaultValue = defaultValue;
        }

        private T? GetSavedValueOrDefault() {
            try {
                var strPref = PlayerPrefs.GetString(PersistString);
                return (T)Convert.ChangeType(strPref, typeof(T));
            }
            catch ( Exception e ) {
                Debug.LogException(e);

                return _defaultValue;
            }
        }

        private void SaveValue() {
            if ( !_value.HasValue ) {
                Debug.LogWarning($"Trying to save, but '{nameof(_value)}' is null!");
                return;
            }

            PlayerPrefs.SetString(PersistString, _value?.ToString());
            PlayerPrefs.Save();
        }

        protected abstract void OnValueChanged(T value, T oldValue);
    }
}