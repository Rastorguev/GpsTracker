using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;

namespace GpsTracker.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = false, ScreenOrientation = ScreenOrientation.Portrait)]
    internal class MainActivity : Activity
    {
        private Button _startButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.MainLayout);

            _startButton = FindViewById<Button>(Resource.Id.StartButton);

            _startButton.Click += delegate
            {
                try
                {
                    StartActivity(typeof(MainTrackingActivity));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            };
        }
    }
}