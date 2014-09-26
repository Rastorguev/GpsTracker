using System;
using Android.App;
using Android.Content.PM;
using Android.Gms.Maps;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace GpsTracker.Activities
{
    [Activity(Label = "@string/app_name",  ScreenOrientation = ScreenOrientation.Portrait)]
    internal class MainTrackingActivity : BaseActiveTrackActivity
    {
        private Button _fullScreenButton;
        private MapFragment _mapFragment;
        private Button _startButton;
        private Button _stopButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _fullScreenButton = FindViewById<Button>(Resource.Id.FullScreenButton);
            _startButton = FindViewById<Button>(Resource.Id.StartButton);
            _stopButton = FindViewById<Button>(Resource.Id.StopButton);

            _fullScreenButton.Click += FullScreenButtonClickEventHandler;
            _startButton.Click += StartButtonClickEventHandler;
            _stopButton.Click += StopButtonClickEventHandler;
        }

        protected override void SetView()
        {
            SetContentView(Resource.Layout.MainTrackingLayout);
        }

        protected override GoogleMap GetMap()
        {
            if (_mapFragment == null)
            {
                _mapFragment = (MapFragment) FragmentManager.FindFragmentById(Resource.Id.Map);
            }

            return _mapFragment.Map;
        }

        protected override void InitMap()
        {
            base.InitMap();

            Map.UiSettings.ZoomControlsEnabled = false;
            Map.UiSettings.CompassEnabled = false;
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (ActiveTrackManager.HasActiveTrack)
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

        private void FullScreenButtonClickEventHandler(object sender, EventArgs e)
        {
            StartActivity(typeof (FullScreenMapTrackingActivity));
        }

        private void StartButtonClickEventHandler(object sender, EventArgs e)
        {
            _startButton.Visibility = ViewStates.Gone;
            _stopButton.Visibility = ViewStates.Visible;

            var location = LocationManager.Location;

            if (location != null)
            {
                ActiveTrackManager.Start();
                ShowLocationChanges();
            }
        }

        private void StopButtonClickEventHandler(object sender, EventArgs e)
        {
            _startButton.Visibility = ViewStates.Visible;
            _stopButton.Visibility = ViewStates.Gone;

            ActiveTrackManager.Stop();

            TrackDrawer.RemoveTrack();
            ShowLocationChanges();

            if (LocationManager.Location != null)
            {
                MoveCamera(LocationManager.Location, Zoom);
            }
        }
    }
}