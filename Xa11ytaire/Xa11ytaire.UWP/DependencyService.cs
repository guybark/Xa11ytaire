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
            // Barker: Do this when I know Xamarin ListViews are compatible with UIA.
        }
    }
}
