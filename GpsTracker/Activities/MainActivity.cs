using System;
using Android.App;
using Android.Content.PM;
using Android.Gms.Common;
using Android.OS;
using Android.Widget;
using GpsTracker.Concrete;

namespace GpsTracker.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
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

        protected override void OnStart()
        {
            base.OnStart();

            var status = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(this);

            if (status != ConnectionResult.Success)
            {
                Alerts.ShowGooglePlayServicesErrorAlert(this, status);
            }
        }
    }
}