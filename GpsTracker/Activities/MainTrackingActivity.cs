using System;
using Android.App;
using Android.Content.PM;
using Android.Gms.Maps;
using Android.OS;
using Android.Widget;

namespace GpsTracker.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = false, ScreenOrientation = ScreenOrientation.Portrait)]
    internal class MainTrackingActivity : BaseActiveTrackActivity
    {
        private Button _fullScreenButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _fullScreenButton = FindViewById<Button>(Resource.Id.FullScreenButton);

            _fullScreenButton.Click += delegate
            {
                try
                {
                    StartActivity(typeof(ActiveTrackFullScreenMapActivity));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            };
        }

        protected override void SetView()
        {
            SetContentView(Resource.Layout.MainTrackingLayout);
        }

        protected override GoogleMap GetMap()
        {
            var mapFragment = (MapFragment) FragmentManager.FindFragmentById(Resource.Id.Map);

            return mapFragment.Map;
        }

        protected override void InitMap()
        {
            base.InitMap();

            Map.UiSettings.ZoomControlsEnabled = false;
            Map.UiSettings.CompassEnabled = false;
        }

        protected override void OnPause()
        {
            base.OnPause();

            GC.Collect();
        }
    }
}