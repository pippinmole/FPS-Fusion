using UnityEngine;
using UnityEngine.Rendering.Universal;
using ShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution;

public class TestQualityChanger : MonoBehaviour {
    private UniversalRenderPipelineAsset _asset;
    
    private void Update() {

        CheckKey(KeyCode.Alpha1, 0); // low
        CheckKey(KeyCode.Alpha2, 1); // medium
        CheckKey(KeyCode.Alpha3, 2); // high

    }

    private void CheckKey(KeyCode keyCode, int level) {
        if ( Input.GetKeyDown(keyCode) ) {
            // QualitySettings.streamingMipmapsAddAllCameras = true;
            // QualitySettings.masterTextureLimit = level;

            // QualitySettings.shadowResolution = (ShadowResolution)level;


            // ((UniversalRenderPipelineAsset)QualitySettings.renderPipeline).shadowCascadeCount = level;

            UniversalCameraSettings.Antialiasing = (AntialiasingMode)level;
            
            Debug.Log($"Set mipmap level to: {level}");
        }
    }
}
