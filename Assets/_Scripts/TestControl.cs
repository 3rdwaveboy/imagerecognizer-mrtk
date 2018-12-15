using Microsoft.MixedReality.Toolkit.Core.Services;
using Microsoft.MixedReality.Toolkit.Core.Utilities;
using UnityEngine;

namespace _Scripts {
    public class TestControl : MonoBehaviour {
    
        private TextMesh _outputTextMesh;
    
        // Use this for initialization
        void Start() {
            _outputTextMesh = CameraCache.Main.GetComponentInChildren<TextMesh>();
            _outputTextMesh.text = "Start Recognizing";
            MixedRealityToolkit.Instance.GetService<ImageRecognitionService>().OnRecognition += OnRecognition;
        }

        private void OnRecognition(RecognitionResult obj) {
            Debug.LogWarning(obj.Message);
            _outputTextMesh.text = obj.Message;
        }

    }
}