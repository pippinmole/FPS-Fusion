using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TestQualityChanger : MonoBehaviour {
    private UniversalRenderPipelineAsset _asset;
    
    private void Update() {

        CheckKey(KeyCode.Alpha1, 0); // low
        CheckKey(KeyCode.Alpha2, 1); // medium
        CheckKey(KeyCode.Alpha3, 2); // high
        CheckKey(KeyCode.Alpha4, 3); // high
        CheckKey(KeyCode.Alpha5, 4); // high
        CheckKey(KeyCode.Alpha6, 5); // high
        CheckKey(KeyCode.Alpha7, 9); // high

    }

    private void CheckKey(KeyCode keyCode, int level) {
        if ( Input.GetKeyDown(keyCode) ) {
            QualitySettings.streamingMipmapsAddAllCameras = true;
            QualitySettings.masterTextureLimit = level;

            Debug.Log($"Set mipmap level to: {level}");
        }
    }
}
