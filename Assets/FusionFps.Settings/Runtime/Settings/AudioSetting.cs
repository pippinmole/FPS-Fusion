using UnityEngine;
using UnityEngine.Audio;
using Zenvin.Settings.Framework;

namespace FusionFps.Settings {
    public class AudioSetting : FloatSetting {

        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioMixerGroup _group;

        protected override void OnValueChanged(ValueChangeMode mode) {
            base.OnValueChanged(mode);

            Debug.Log($"ValueChanged on mode {mode} to cached:{CachedValue}|current{CurrentValue}");
            
            // We have to make the number really small because log_10(0) does not exist.
            var value = CachedValue == 0f ? 0.00001f : CachedValue;
            var db = Mathf.Log10(value / 100f) * 20f;

            Debug.Log($"Setting mixer dB to {db}");
            
            _mixer.SetFloat(_group.name, db);
        }
    }
}