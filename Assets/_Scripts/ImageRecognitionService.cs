using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Core.Interfaces;
using UnityEngine;
using _Scripts;

public class ImageRecognitionService : IMixedRealityExtensionService, ImageRecognitionService.IRecognitionResultCallback {

    public interface IRecognitionResultCallback {
        void OnRecognitionResult(RecognitionResult result);
    }

    /// <summary>
    /// Action is called when image recognition has a result of ResultType.Success or ResultType.Failed
    /// </summary>
    public event Action<RecognitionResult> OnRecognition;
    
#if UNITY_WSA && !UNITY_EDITOR
    private RecognitionEngine Engine;
#endif

    public ImageRecognitionService(string name, uint priority) {
        Name = name;
        Priority = priority;
    }

    // Update is called once per frame
    public string Name { get; }
    public uint Priority { get; }

    public async void Initialize() {
#if UNITY_WSA && !UNITY_EDITOR // RUNNING ON WINDOWS
        Engine = new RecognitionEngine();
        await Engine.Inititalize(this);
#endif
    }

    public void Reset() {
    }

    public void Enable() {
#if UNITY_WSA && !UNITY_EDITOR
        Engine.StartPullCameraFrames();
#endif
    }

    public void Update() {
    }

    public void Disable() {
    }

    public void Destroy() {}

    public void OnRecognitionResult(RecognitionResult result) {
        Debug.LogWarning("result");
        if (OnRecognition != null) {
            OnRecognition.Invoke(result);
        }
    }
}