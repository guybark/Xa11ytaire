using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.Accessibility;
using Java.Lang;
//using Org.Tensorflow;
//using Org.Tensorflow.Contrib.Android;
using Xa11ytaire.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(Xa11ytairePlatformAction))]
namespace Xa11ytaire.Droid
{ 
    public class Xa11ytairePlatformAction : 
        IXa11ytairePlatformAction,
        ILocalize
    {
        //private TensorFlowInferenceInterface inferenceInterface;
        //private List<string> labels;
        //private float[] floatValues;

        //private const string subscriptionKey = "<Your subscription key.>";
        //private string projectId = "<Your project ID.>";
        //private const string southcentralusEndpoint = "https://southcentralus.api.cognitive.microsoft.com";

        //private int inputSize;

        public void ScreenReaderAnnouncement(string notification)
        {
            if ((MainActivity.accessibilityManager != null) &&
                MainActivity.accessibilityManager.IsEnabled)
            {
                ICharSequence charSeqNotification = new Java.Lang.String(notification);

                AccessibilityEvent e = AccessibilityEvent.Obtain();

                e.EventType = (EventTypes)0x00004000; // This is the Android value.

                e.Text.Add(charSeqNotification);

                // NOTE: Announcements don't seem to interfer with other 
                // default announcements. Eg when game restarted, announcement on 
                // where focus is afterwards still gets announced. And note that
                // focus remains on card being moved from a delat card pile, and
                // so no announcement there gets announced by default anyway.

                MainActivity.accessibilityManager.SendAccessibilityEvent(e);
            }
        }

        public async Task<string> LocalRecognizeImage(Stream imageStream)
        {
            string result = "";

            //DateTime timeStart = DateTime.Now;

            //Debug.WriteLine("LocalRecognizeImage: Start");

            //PrepareForImageReco();

            //Debug.WriteLine("LocalRecognizeImage: Done PrepareForImageReco " + (DateTime.Now - timeStart).TotalMilliseconds);

            //Bitmap bm = BitmapFactory.DecodeStream(imageStream);

            //Debug.WriteLine("LocalRecognizeImage: Done DecodeStream " + (DateTime.Now - timeStart).TotalMilliseconds);

            //ResizePhoto(bm);

            //Debug.WriteLine("LocalRecognizeImage: Done ResizePhoto " + (DateTime.Now - timeStart).TotalMilliseconds);

            //result = RecognizeImage();

            //Debug.WriteLine("LocalRecognizeImage: Done RecognizeImage " + (DateTime.Now - timeStart).TotalMilliseconds);

            return result;
        }

        private void PrepareForImageReco()
        {
            //// Initialize the use of the local vision model if we're not already done so.
            //if (inferenceInterface == null)
            //{
            //    var assets = Application.Context.Assets;
            //    inferenceInterface = new TensorFlowInferenceInterface(assets, "model.pb");
            //    var sr = new StreamReader(assets.Open("labels.txt"));
            //    labels = sr.ReadToEnd()
            //                    .Split('\n')
            //                    .Select(s => s.Trim())
            //                    .Where(s => !string.IsNullOrEmpty(s))
            //                    .ToList();

            //    inputSize = (int)inferenceInterface.GraphOperation("Placeholder").Output(0).Shape().Size(1);

            //    Debug.WriteLine("PrepareForImageReco: inputSize " + inputSize.ToString());
            //}
        }

        private void ResizePhoto(Bitmap bitmap)
        {
            //// Previously we'd hard-code the image size to be 227x227. 
            //// Now we use the input size supplied by the model.

            //var resizedBitmap = Bitmap.CreateScaledBitmap(bitmap, inputSize, inputSize, false)
            //                          .Copy(Bitmap.Config.Argb8888, false);

            //floatValues = new float[inputSize * inputSize * 3];
            //var intValues = new int[inputSize * inputSize];

            //resizedBitmap.GetPixels(intValues, 0, inputSize, 0, 0, inputSize, inputSize);

            //for (int i = 0; i < intValues.Length; ++i)
            //{
            //    var val = intValues[i];
            //    floatValues[i * 3 + 0] = ((val & 0xFF) - 104);
            //    floatValues[i * 3 + 1] = (((val >> 8) & 0xFF) - 117);
            //    floatValues[i * 3 + 2] = (((val >> 16) & 0xFF) - 123);
            //}
        }

        private string RecognizeImage()
        {
            string result = "";

            //var outputs = new float[labels.Count];
            //inferenceInterface.Feed("Placeholder", floatValues, 1, inputSize, inputSize, 3);
            //inferenceInterface.Run(new[] { "loss" });
            //inferenceInterface.Fetch("loss", outputs);

            //// For this test, ignore confidences of less than 0.5.
            //float maxConfidence = 0.5f;
            //int maxConfidenceIndex = -1;

            //for (int i = 0; i < outputs.Length; ++i)
            //{
            //    if (outputs[i] > maxConfidence)
            //    {
            //        maxConfidence = outputs[i];
            //        maxConfidenceIndex = i;
            //    }
            //}

            //if (maxConfidenceIndex >= 0)
            //{
            //    result = labels[maxConfidenceIndex];    
            //}

            return result;
        }

        // The following taken from:
        // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/localization/text?tabs=windows
        public async Task<bool> InitializeMicrophone()
        {
            bool microphoneInitialized = false;

            return microphoneInitialized;
        }

        public async Task<byte[]> GetPixelValuesForMLReco()
        {
            return null;
        }

        // The following code was copied from:
        // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/localization/text?tabs=windows
        public CultureInfo GetCurrentCultureInfo()
        {
            var netLanguage = "en";
            var androidLocale = Java.Util.Locale.Default;
            netLanguage = AndroidToDotnetLanguage(androidLocale.ToString().Replace("_", "-"));
            // this gets called a lot - try/catch can be expensive so consider caching or something
            System.Globalization.CultureInfo ci = null;
            try
            {
                ci = new System.Globalization.CultureInfo(netLanguage);
            }
            catch (CultureNotFoundException e1)
            {
                // iOS locale not valid .NET culture (eg. "en-ES" : English in Spain)
                // fallback to first characters, in this case "en"
                try
                {
                    //var fallback = ToDotnetFallbackLanguage(new PlatformCulture(netLanguage));
                    //ci = new System.Globalization.CultureInfo(fallback);
                    ci = new System.Globalization.CultureInfo("en"); // Barker
                }
                catch (CultureNotFoundException e2)
                {
                    // iOS language not valid .NET culture, falling back to English
                    ci = new System.Globalization.CultureInfo("en");
                }
            }

            return ci;
        }

        public void SetLocale(CultureInfo ci)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
        }

        string AndroidToDotnetLanguage(string androidLanguage)
        {
            var netLanguage = androidLanguage;
            //certain languages need to be converted to CultureInfo equivalent
            switch (androidLanguage)
            {
                case "ms-BN":   // "Malaysian (Brunei)" not supported .NET culture
                case "ms-MY":   // "Malaysian (Malaysia)" not supported .NET culture
                case "ms-SG":   // "Malaysian (Singapore)" not supported .NET culture
                    netLanguage = "ms"; // closest supported
                    break;
                case "in-ID":  // "Indonesian (Indonesia)" has different code in  .NET
                    netLanguage = "id-ID"; // correct code for .NET
                    break;
                case "gsw-CH":  // "Schwiizertüütsch (Swiss German)" not supported .NET culture
                    netLanguage = "de-CH"; // closest supported
                    break;
                    // add more application-specific cases here (if required)
                    // ONLY use cultures that have been tested and known to work
            }
            return netLanguage;
        }
    }
}
