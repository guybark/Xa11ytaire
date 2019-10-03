using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views.Accessibility;

namespace Xa11ytaire.Droid
{
    [Activity(Label = "Xa11ytaire", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,
        ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static AccessibilityManager accessibilityManager = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            accessibilityManager = (AccessibilityManager)GetSystemService("accessibility");
        }
    }
}