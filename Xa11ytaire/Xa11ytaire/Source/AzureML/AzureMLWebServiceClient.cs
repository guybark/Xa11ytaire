using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Xa11ytaire
{
    public class AzureMLWebServiceClient
    {
        // Call a deployed ML model in the cloud to recognize the
        // numberic value shown in an image of a handwritten number.
        
        public async Task<string> GetAzureMLPrediction()
        {
            string cardNumber = "";

            var service = DependencyService.Get<IXa11ytairePlatformAction>();

            try
            {
                // Get the byte array to be supplied as input to the model.
                var pixels = await service.GetPixelValuesForMLReco();
                if (pixels != null)
                {
                    // Set the scoring URI. No authentication key is required 
                    // in this experiment, given that the service was deployed 
                    // to Azure Container Instances rather than Azue Kubernetes.

                    string scoringUri = "<your web service URI>";

                    // Manually convert the input byte array to a
                    // text string to supply to the web service.

                    string input_data = "{\"data\": [[";

                    for (int i = 0; i < pixels.Length; i++)
                    {
                        // "0.00" mean a white pixel, "1.00" mean black.
                        input_data += ((255.0 - pixels[i]) / 255.0).ToString("F");

                        if (i < pixels.Length - 1)
                        {
                            input_data += ", ";
                        }
                    }

                    input_data += "]]}";

                    var request = new HttpRequestMessage(HttpMethod.Post,
                                        new Uri(scoringUri));

                    request.Content = new StringContent(input_data);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue(
                        "application/json");

                    HttpClient client = new HttpClient();

                    var response = client.SendAsync(request).Result;

                    var result = response.Content.ReadAsStringAsync().Result;

                    Debug.WriteLine("Azure ML service result is: " + result);

                    // Pull the actual numberic value from the result,
                    // and later find a card with that value in the game
                    // and either select it or move it.

                    cardNumber = result.Substring(1, 1);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return cardNumber;
        }
    }
}
