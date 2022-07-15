using FusionFps.Core;
using UnityEngine;
using UnityEngine.Rendering;

public class StartGameGrayscaler : MonoBehaviour {
    
    [SerializeField] private float _secondsUntilStartFade = 3f;
    [SerializeField] private Volume _volume;

    private void Update() {
        var matchManager = MatchManager.Instance;
        if ( matchManager == null ) return;
        
        var timeUntilStart = matchManager.WaitForPlayersTimeLeft;
        if ( timeUntilStart == null ) return;

        var alpha = Remap(timeUntilStart.Value, 0f, _secondsUntilStartFade, 0f, 1f);
        
        _volume.weight = Mathf.Lerp(0f, 1f, alpha);
    }

    private static float Remap (float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
