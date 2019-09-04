//using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace Xa11ytaire
{
    public partial class MainPage : ContentPage
    {
        private const string subscriptionKeyVision = "<Your subscription key.>";
        private string projectIdVision = "<Your project ID.>";
        private const string southcentralusEndpoint = "https://southcentralus.api.cognitive.microsoft.com";

        private bool useAzureML = false;

        private async void MoveByImageReco()
        {
            //if (useAzureML)
            //{
            //    var azureMLWebServiceClient = new AzureMLWebServiceClient();
            //    string cardNumber = await azureMLWebServiceClient.GetAzureMLPrediction();
            //    if (!string.IsNullOrEmpty(cardNumber))
            //    {
            //        //CardRecoStatus.Text = "ML handwritten number reco result: " + cardNumber;

            //        string[] suitNames = new string[]
            //        {
            //            "clubs",
            //            "diamonds",
            //            "hearts",
            //            "spades"
            //        };

            //        for (int i = 0; i < suitNames.Length; ++i)
            //        {
            //            string cardFullName = cardNumber + " of " + suitNames[i];

            //            var cardHasBeenSelected = SelectCardByIntent(cardFullName);
            //            if (cardHasBeenSelected)
            //            {
            //                break;
            //            }
            //        }

            //    }
            //}
            //else
            //{
            //    // As stated at:
            //    // https://docs.microsoft.com/en-us/azure/cognitive-services/custom-vision-service/limits-and-quotas
            //    // currently images supplied to the vision service cannot be larger than 4MB.
            //    // The phone camera may by default generate images larger than 4MB. As such
            //    // adjust the compression quality to reduce the size of the files. Note that
            //    // given such action may potentially affect the image recognition results, a
            //    // shipping app would put more thought into what compression would be 
            //    // appropriate here.
            //    var options = new Plugin.Media.Abstractions.StoreCameraMediaOptions()
            //    {
            //        CompressionQuality = 50 // <- Some value to enable the test.
            //    };

            //    var photo = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(options);
            //    if (photo != null)
            //    {
            //        Stream imageStream = photo.GetStream();

            //        // Important: The stream must be back at the beginning before calling 
            //        // PredictImage() below!

            //        // Seek back to the start of the data before processing.
            //        imageStream.Seek(0, SeekOrigin.Begin);

            //        string cardName = "";

            //        //try
            //        //{
            //        //    // Supply whatever endpoint, key and project ID is appropriate for your service.
            //        //    var endpoint = new CustomVisionPredictionClient()
            //        //    {
            //        //        ApiKey = subscriptionKeyVision,
            //        //        Endpoint = southcentralusEndpoint
            //        //    };

            //        //    var result = endpoint.PredictImage(
            //        //        new Guid(projectIdVision),
            //        //        imageStream);

            //        //    double maxProbability = 0;

            //        //    foreach (var c in result.Predictions)
            //        //    {
            //        //        if (c.Probability > maxProbability)
            //        //        {
            //        //            maxProbability = c.Probability;

            //        //            cardName = c.TagName;

            //        //            Debug.WriteLine(cardName + ": " + maxProbability);
            //        //        }
            //        //    }

            //        //    Debug.WriteLine(cardName + ": " + maxProbability);

            //        //    // We're only interested for this test in predictions with a probability greater than 0.5.
            //        //    if (maxProbability < 0.5)
            //        //    {
            //        //        cardName = "";
            //        //    }
            //        //}
            //        //catch (Exception ex)
            //        {
            //            //Debug.WriteLine("Attempt to recognize image failed: " + ex.Message);

            //            // A shipping app would pay more attention as to exactly what exception
            //            // has been hit. For this test, assume it's related to there being no
            //            // connection available.

            //            //bool answerIsNo = await DisplayAlert(
            //            //    "Xa11ytaire",
            //            //    "Sorry, the image recognition service in the cloud couldn't be reached. " +
            //            //        "Would you care for some local image reco?",
            //            //    "No", "Yes");

            //            //if (!answerIsNo)
            //            {
            //                var service = DependencyService.Get<IXa11ytairePlatformAction>();

            //                cardName = await service.LocalRecognizeImage(imageStream);
            //            }
            //        }

            //        //CardRecoStatus.Text = "Card reco result: " + cardName;

            //        if (!string.IsNullOrEmpty(cardName))
            //        {
            //            SelectCardByIntent(cardName);
            //        }
            //    }
            //}
        }
    }
}
