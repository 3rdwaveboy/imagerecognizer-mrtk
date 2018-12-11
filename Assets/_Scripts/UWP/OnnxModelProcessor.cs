#if UNITY_WSA && !UNITY_EDITOR
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
namespace _Scripts {
    public class OnnxModelProcessor {
        private ONNXModel Model = null;
        private string ModelFilename = "ONNXModel.onnx";

        private ImageRecognitionService.IRecognitionResultCallback Callback;
        
        public OnnxModelProcessor(ImageRecognitionService.IRecognitionResultCallback callback) {
            Callback = callback;
        }

        public async Task LoadModelAsync(string filename = null) {
            if (!string.IsNullOrEmpty(filename)) {
                ModelFilename = filename;
            }
            
            try
            {
                var modelFile = await StorageFile.GetFileFromApplicationUriAsync(
                    new Uri($"ms-appx:///Data/StreamingAssets/{ModelFilename}"));
                Model = await ONNXModel.CreateOnnxModel(modelFile);
            }
            catch (Exception ex)
            {
                InvokeNullsafe(new RecognitionResult(ResultType.Failed, ex.Message, "", -1)); 
                Model = null;
            }
        }

        public async Task EvaluateVideoFrameAsync(VideoFrame frame)
        {
            if (frame != null)
            {
                try
                {
                    ONNXModelInput inputData = new ONNXModelInput();
                    inputData.Data = frame;
                    var output = await Model.EvaluateAsync(inputData).ConfigureAwait(false);

                    var product = output.ClassLabel.GetAsVectorView()[0];
                    var loss = output.Loss[0][product];

                    var lossStr = string.Join(product, " " + (loss * 100.0f).ToString("#0.00") + "%");
                    string message = $"({DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second})\n";

                    string prediction = $"Prediction: {product} {lossStr}";
                    if (loss > 0.5f)
                    {
                        message += prediction;
                    }

                    message = message.Replace("\\n", "\n");

                    InvokeNullsafe(new RecognitionResult(ResultType.Success, message, product, loss));
                }
                catch (Exception ex)
                {
                    InvokeNullsafe(new RecognitionResult(ResultType.Failed, $"error: {ex.Message}", "", -1));
                }
            }
        }
        
        private void InvokeNullsafe(RecognitionResult result) {
            if (Callback != null) {
                Callback.OnRecognitionResult(result);
            }
        }
    }
    
}
#endif