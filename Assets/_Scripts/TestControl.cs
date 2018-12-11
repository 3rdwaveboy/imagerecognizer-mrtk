using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Core.Services;
using UnityEngine;
using _Scripts;

public class test : MonoBehaviour {
    
    public GameObject OutputText;
    private TextMesh OutputTextMesh;
    
    // Use this for initialization
    void Start() {
        OutputTextMesh = OutputText.GetComponent<TextMesh>();
        OutputTextMesh.text = string.Empty;
        MixedRealityToolkit.Instance.GetService<ImageRecognitionService>().OnRecognition += OnRecognition;
    }

    private void OnRecognition(RecognitionResult obj) {
        Debug.LogWarning(obj.Message);
        OutputTextMesh.text = obj.Message;
    }

    // Update is called once per frame
    void Update() { }
}