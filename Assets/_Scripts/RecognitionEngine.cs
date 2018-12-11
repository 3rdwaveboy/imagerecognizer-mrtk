#if UNITY_WSA && !UNITY_EDITOR
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;

namespace _Scripts {
    public class RecognitionEngine {
        public TimeSpan PredictionFrequency = TimeSpan.FromMilliseconds(400);

        private MediaCapture CameraCapture;
        private MediaFrameReader CameraFrameReader;

        private Int64 FramesCaptured;
        private OnnxModelProcessor ModelProcessor;

        public async Task Inititalize(ImageRecognitionService.IRecognitionResultCallback callback) {
            ModelProcessor = new OnnxModelProcessor(callback);
            await ModelProcessor.LoadModelAsync();

            await InitializeCameraCapture();
            await InitializeCameraFrameReader();
        }

        private async Task InitializeCameraCapture() {
            CameraCapture = new MediaCapture();
            MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings();
            settings.StreamingCaptureMode = StreamingCaptureMode.Video;
            await CameraCapture.InitializeAsync(settings);
        }

        private async Task InitializeCameraFrameReader() {
            var frameSourceGroups = await MediaFrameSourceGroup.FindAllAsync();
            MediaFrameSourceGroup selectedGroup = null;
            MediaFrameSourceInfo colorSourceInfo = null;

            foreach (var sourceGroup in frameSourceGroups) {
                foreach (var sourceInfo in sourceGroup.SourceInfos) {
                    if (sourceInfo.MediaStreamType == MediaStreamType.VideoPreview
                        && sourceInfo.SourceKind == MediaFrameSourceKind.Color) {
                        colorSourceInfo = sourceInfo;
                        break;
                    }
                }

                if (colorSourceInfo != null) {
                    selectedGroup = sourceGroup;
                    break;
                }
            }

            var colorFrameSource = CameraCapture.FrameSources[colorSourceInfo.Id];
            var preferredFormat = colorFrameSource.SupportedFormats.Where(format => {
                return format.Subtype == MediaEncodingSubtypes.Argb32;
            }).FirstOrDefault();

            CameraFrameReader = await CameraCapture.CreateFrameReaderAsync(colorFrameSource);
            await CameraFrameReader.StartAsync();
        }

        private bool IsRunning = false;
        private CancellationTokenSource Cts;

        /// <summary>
        /// Stops the camera
        /// </summary>
        public void StopPullCameraFrames() {
            Cts.Cancel();
            CameraFrameReader.StopAsync();
        }

        /// <summary>
        /// Starts the camera and evaluates video frames every <paramref name="PredictionFrequency"/>
        /// </summary>
        public void StartPullCameraFrames() {
            Cts = new CancellationTokenSource();
            Task.Run(async () => {
                while (!Cts.Token.IsCancellationRequested) {
                    FramesCaptured++;
                    await Task.Delay(PredictionFrequency);
                    using (var frameReference = CameraFrameReader.TryAcquireLatestFrame())
                    using (var videoFrame = frameReference?.VideoMediaFrame?.GetVideoFrame()) {

                        if (videoFrame == null) {
                            continue; //ignoring frame
                        }

                        if (videoFrame.Direct3DSurface == null) {
                            videoFrame.Dispose();
                            continue; //ignoring frame
                        }

                        try {
                            await ModelProcessor.EvaluateVideoFrameAsync(videoFrame).ConfigureAwait(false);
                        } catch (Exception ex) {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                        }
                    }
                }
            });
        }

    }
}
#endif
