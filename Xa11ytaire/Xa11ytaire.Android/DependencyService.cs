using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views.Accessibility;

using Java.Lang;

using Xa11ytaire.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(Xa11ytairePlatformAction))]
namespace Xa11ytaire.Droid
{ 
    public class Xa11ytairePlatformAction : 
        IXa11ytairePlatformAction,
        ILocalize
    {
        public Settings LoadSettings()
        {
            var settings = new Settings();

            var preferences = Application.Context.GetSharedPreferences("Xa11ytaire", 
                FileCreationMode.Private);

            settings.ShowSuggestionsButton =
                (preferences.GetString("ShowSuggestionsButton", "off") == "on");

            settings.TurnOverOneCard =
                (preferences.GetString("TurnOverOneCard", "off") == "on");

            settings.IncludeRowNumber =
                (preferences.GetString("IncludeRowNumber", "off") == "on");

            Settings.HideUI =
                (preferences.GetString("HideUI", "off") == "on");

            return settings;
        }

        public void SaveSettings(Settings settings)
        {
            var preferences = Application.Context.GetSharedPreferences("Xa11ytaire",
                FileCreationMode.Private);

            var preferencesEditor = preferences.Edit();

            preferencesEditor.PutString("ShowSuggestionsButton",
                settings.ShowSuggestionsButton ? "on" : "off");

            preferencesEditor.PutString("TurnOverOneCard",
                settings.TurnOverOneCard ? "on" : "off");

            preferencesEditor.PutString("IncludeRowNumber",
                settings.IncludeRowNumber ? "on" : "off");

            preferencesEditor.PutString("HideUI",
                Settings.HideUI ? "on" : "off");

            preferencesEditor.Commit();
        }

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
