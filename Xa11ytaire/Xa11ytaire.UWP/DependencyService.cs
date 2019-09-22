using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Xa11ytaire.UWP;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(Xa11ytairePlatformAction))]
namespace Xa11ytaire.UWP
{
    public class Xa11ytairePlatformAction : IXa11ytairePlatformAction
    {
        // The ObjectDetection class is defined in the sample ObjectDetection.cs 
        // file exported with the custom object detection model whe exporting the
        // model from http://customvision.ai.

        // Barker: Local image reco not currently included:
        // private ObjectDetection objectDetection;

        public Settings LoadSettings()
        {
            var settings = new Settings();

            settings.ShowSuggestionsButton = false;
            settings.TurnOverOneCard = false;
            settings.IncludeRowNumber = false;

            if (Application.Current.Properties.ContainsKey("ShowSuggestionsButton"))
            {
                settings.ShowSuggestionsButton = (bool)Application.Current.Properties["ShowSuggestionsButton"];
            }

            if (Application.Current.Properties.ContainsKey("TurnOverOneCard"))
            {
                settings.TurnOverOneCard = (bool)Application.Current.Properties["TurnOverOneCard"];
            }

            if (Application.Current.Properties.ContainsKey("IncludeRowNumber"))
            {
                settings.IncludeRowNumber = (bool)Application.Current.Properties["IncludeRowNumber"];
            }

            return settings;
        }

        public void SaveSettings(Settings settings)
        {
            Application.Current.Properties["ShowSuggestionsButton"] =
                settings.ShowSuggestionsButton;

            Application.Current.Properties["TurnOverOneCard"] =
                settings.TurnOverOneCard;

            Application.Current.Properties["IncludeRowNumber"] =
                settings.IncludeRowNumber;
        }

        public void ScreenReaderAnnouncement(string notification)
        {

        }

        // LocalRecognizeImage() is called by the Xamarin app's cross-platform code.
        // The code makes the call, and through use of the DependencyService, the
        // appropriate platform-specific version of LocalRecognizeImage() gets called.
        public async Task<string> LocalRecognizeImage(Stream imageStream)
        {
            // We'll the name of the recognized card.
            string cardName = "";

            // Barker: Local image reco not currently included:
            //try
            //{
            //    // Does the custom vision model need to be initialized?
            //    if (objectDetection == null)
            //    {
            //        // First collect all the labels associated with the model.
            //        var labelsFile = await StorageFile.GetFileFromApplicationUriAsync(
            //            new Uri("ms-appx:///Assets/ML/labels.txt"));

            //        Stream fileStream = await labelsFile.OpenStreamForReadAsync();
            //        var sr = new StreamReader(fileStream);
            //        var labels = sr.ReadToEnd()
            //                        .Split('\n')
            //                        .Select(s => s.Trim())
            //                        .Where(s => !string.IsNullOrEmpty(s))
            //                        .ToList();

            //        // Create and initialize the ObjectDetection wrapper class.
            //        objectDetection = new ObjectDetection(labels);

            //        var modelFile = await StorageFile.GetFileFromApplicationUriAsync(
            //            new Uri("ms-appx:///Assets/ML/PlayingCardObjectDetection.onnx"));

            //        await objectDetection.Init(modelFile);
            //    }

            //    // The ObjectDetection wrapper expect a Frame object, so generate
            //    // this from the supplied image stream.
            //    var decoder = await 
            //        BitmapDecoder.CreateAsync(imageStream.AsRandomAccessStream());
            //    SoftwareBitmap bitmap = await decoder.GetSoftwareBitmapAsync();
            //    var frame = VideoFrame.CreateWithSoftwareBitmap(bitmap);

            //    // Attempt to perform the recognition now.
            //    var predictionResult = await objectDetection.PredictImageAsync(frame);

            //    // Return the prediction with the highest probability.
            //    // (And require that we have at least a 40% probability.)
            //    double maxProbability = 0;

            //    foreach (PredictionModel predictionModel in predictionResult)
            //    {
            //        Debug.WriteLine("Result: " + predictionModel.TagName + 
            //            ", " + predictionModel.Probability);

            //        if (predictionModel.Probability > maxProbability)
            //        {
            //            maxProbability = predictionModel.Probability;

            //            cardName = predictionModel.TagName;
            //        }
            //    }

            //    Debug.WriteLine("Max probability: " + cardName + ", " + maxProbability);

            //    if (maxProbability < 0.4)
            //    {
            //        cardName = "";
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine("Xamarin.UWP LocalRecognizeImage() failed: " + 
            //        ex.Message);
            //}

            return cardName;
        }

        public async Task<bool> InitializeMicrophone()
        {
            bool microphoneInitialized = false;

            try
            {
                // We need access to audio, and not video.
                var settings = new MediaCaptureInitializationSettings();
                settings.StreamingCaptureMode = StreamingCaptureMode.Audio;

                var mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync(settings);

                // If we got this far, we've initialized the microphone.
                microphoneInitialized = true;
            }
            catch (Exception micException)
            {
                Debug.WriteLine("Microphone access not initialized. " +
                    micException.Message);
            }

            return microphoneInitialized;
        }

        private const int resizedImageDimensions = 28;

        // Return a byte array representing a 1 byte per pixel 28x28 grayscale image. 
        public async Task<byte[]> GetPixelValuesForMLReco()
        {
            byte[] pixelDataGrayScale = null;

            StorageFolder picLib = KnownFolders.PicturesLibrary;
            var subfolder = await picLib.GetFolderAsync("AzureMLTest");

            // Get an image of interest manually selected by the player.

            var storageFile = await GetImageStreamAsync();
            if (storageFile != null)
            {
                // Generate a 28x28 version of the selected image.
                // For this experiment, write this resized image to 
                // disk, to enable a manual verification that the 
                // resized image was contains useful results.

                var resizedImageFilename = await CreateResizedVersionOfImage(
                    storageFile, subfolder);

                // Now generate the byte array containing the grayscale pixel data.

                pixelDataGrayScale = await GenerateGrayScaleByteArray(
                    subfolder, resizedImageFilename);
            }

            return pixelDataGrayScale;
        }

        private async Task<string> CreateResizedVersionOfImage(
            StorageFile storageFile,
            StorageFolder subfolder)
        {
            // Create a test file to manually examine the results later.
            string resizedImageFilename = storageFile.DisplayName +
                    "_Xa11ytaireResized.jpg";

            IRandomAccessStreamWithContentType raStream = await
                storageFile.OpenReadAsync();

            using (var stream = raStream.AsStreamForRead())
            {
                var bitmapDecoder = await BitmapDecoder.CreateAsync(
                    stream.AsRandomAccessStream());

                using (var randomAccessStream = new InMemoryRandomAccessStream())
                {
                    var bitmapEncoder = await
                        BitmapEncoder.CreateForTranscodingAsync(
                            randomAccessStream, bitmapDecoder);

                    bitmapEncoder.BitmapTransform.ScaledWidth = resizedImageDimensions;
                    bitmapEncoder.BitmapTransform.ScaledHeight = resizedImageDimensions;

                    // Use the Fant mode to maximize the anti-aliasing 
                    // leading to better results from the ML model.

                    bitmapEncoder.BitmapTransform.InterpolationMode =
                        BitmapInterpolationMode.Fant;

                    await bitmapEncoder.FlushAsync();

                    var buffer = new byte[randomAccessStream.Size];

                    await randomAccessStream.AsStream().ReadAsync(
                        buffer, 0, buffer.Length);

                    var resizedImageFile = await subfolder.CreateFileAsync(
                        resizedImageFilename,
                        CreationCollisionOption.ReplaceExisting);

                    await FileIO.WriteBytesAsync(resizedImageFile, buffer);
                }

            }

            return resizedImageFilename;
        }

        private async Task<byte[]> GenerateGrayScaleByteArray(
            StorageFolder subfolder,
            string resizedImageFilename)
        {
            byte[] pixelDataGrayScale = null;

            var imageFileResized = await subfolder.GetFileAsync(
                    resizedImageFilename);

            using (var stream = await imageFileResized.OpenAsync(
                FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(stream);

                var pixelDataProvider = await decoder.GetPixelDataAsync();
                var pixelData = pixelDataProvider.DetachPixelData();

                // Assume here for this experiement that the original
                // image has 4 bytes per pixel.

                int bytesPerPixel = 4;

                pixelDataGrayScale = new byte[decoder.PixelWidth * decoder.PixelHeight];

                for (int i = 0; i < pixelDataGrayScale.Length; ++i)
                {
                    int originalImageOffset = i * bytesPerPixel;

                    // This simple approach for averaging should be sufficient
                    // for this experiment. (And ignore the alpha byte.)

                    byte pixelAverage = (byte)(
                        (pixelData[originalImageOffset] +
                         pixelData[originalImageOffset + 1] +
                         pixelData[originalImageOffset + 2]) / 3);

                    pixelDataGrayScale[i] = pixelAverage;
                }
            }

            return pixelDataGrayScale;
        }

        public async Task<StorageFile> GetImageStreamAsync()
        {
            // Create and initialize the FileOpenPicker,
            FileOpenPicker openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail
            };

            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".bmp");

            // Get a file and return a Stream.
            return await openPicker.PickSingleFileAsync();
        }
    }
}
