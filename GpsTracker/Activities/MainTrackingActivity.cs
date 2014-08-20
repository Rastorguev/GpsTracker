using System;
using Android.App;
using Android.Content.PM;
using Android.Gms.Maps;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace GpsTracker.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    internal class MainTrackingActivity : BaseActiveTrackActivity
    {
        private Button _fullScreenButton;
        private Button _startButton;
        private Button _stopButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _fullScreenButton = FindViewById<Button>(Resource.Id.FullScreenButton);
            _startButton = FindViewById<Button>(Resource.Id.StartButton);
            _stopButton = FindViewById<Button>(Resource.Id.StopButton);

            _fullScreenButton.Click += delegate
            {
                try
                {
                    StartActivity(typeof (ActiveTrackFullScreenMapActivity));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            };

            _startButton.Click += StartButtonClickEventHandler;
            _stopButton.Click += StopButtonClickEventHandler;
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

        protected override void OnStart()
        {
            base.OnStart();

            if (App.ActiveTrackManager.HasActiveTrack)
            {
                _startButton.Visibility = ViewStates.Gone;
                _stopButton.Visibility = ViewStates.Visible;
            }
            else
            {
                _startButton.Visibility = ViewStates.Visible;
                _stopButton.Visibility = ViewStates.Gone;
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            GC.Collect();
        }

        private void StartButtonClickEventHandler(object sender, EventArgs e)
        {
            //App.LocationClient.Connect();

            App.ActiveTrackManager.StartTrack();

            _startButton.Visibility = ViewStates.Gone;
            _stopButton.Visibility = ViewStates.Visible;
        }

        private void StopButtonClickEventHandler(object sender, EventArgs e)
        {
            //App.LocationClient.Disconnect();

            _startButton.Visibility = ViewStates.Visible;
            _stopButton.Visibility = ViewStates.Gone;

            App.ActiveTrackManager.StopTrack();

            TrackDrawer.RemoveTrack();
        }
    }
}