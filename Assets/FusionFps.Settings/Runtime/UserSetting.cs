using System;
using System.Collections.Generic;
using UnityEngine;

namespace FusionFps.Settings {

    // public abstract class UserSetting {
    //
    //     public static readonly List<UserSetting> All = new();
    //
    //     protected UserSetting() {
    //         All.Add(this);
    //     }
    //
    //     ~UserSetting() {
    //         All.Remove(this);
    //     }
    //     
    //     public abstract void Reset(bool save = true);
    // }


    public abstract class UserSetting {

        public static readonly List<UserSetting> All = new();

        protected UserSetting() {
            All.Add(this);
        }

        ~UserSetting() {
            All.Remove(this);
        }

        public abstract void Reset(bool save = true);
    }

    public abstract class UserSetting<T> : UserSetting where T : struct {

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
                OnValueChanged(value, oldValue);
                SaveValue();
            }
        }

        protected UserSetting(string name, T defaultValue) {
            Name = name;

            _defaultValue = defaultValue;
            
            All.Add(this);
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
        
        public override void Reset(bool save = true) {
            Value = _defaultValue;
            
            if ( save ) {
                SaveValue();
            }
        }
    }
}