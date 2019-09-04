using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Views.Accessibility;
using System.Runtime.Remoting.Contexts;

//using Org.Tensorflow.Contrib.Android;
//using System.IO;
//using System.Linq;
//using Android.Graphics;
//using System.Collections.Generic;
//using Plugin.CurrentActivity;

// XBarker: Resource - https://blog.xamarin.com/android-apps-tensorflow/
// Install NuGet package: Xam.Android.Tensorflow
// https://xamarinhelp.com/use-camera-take-photo-xamarin-forms/
// Install NuGet package: Xam.Plugin.Media

// XBarker: Add these:
//[assembly: UsesFeature("android.hardware.camera", Required = false)]
//[assembly: UsesFeature("android.hardware.camera.autofocus", Required = false)]


namespace Xa11ytaire.Droid
{
    //[Activity(Label = "Xa11ytaire", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
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

            accessibilityManager = (AccessibilityManager)GetSystemService("accessibility"); // Context.AccessibilityService);

            //CrossCurrentActivity.Current.Init(this, savedInstanceState);
        }

        //// XBarker: Add this.
        //public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        //{
        //    Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        //}
    }
}