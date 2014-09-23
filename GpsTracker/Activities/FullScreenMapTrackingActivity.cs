using System;
using System.Globalization;
using System.Timers;
using Android.App;
using Android.Gms.Maps;
using Android.Locations;
using Android.OS;
using Android.Widget;
using GpsTracker.Tools;

namespace GpsTracker.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = false)]
    internal class FullScreenMapTrackingActivity : BaseActiveTrackActivity
    {
        private TextView _trackPointsQuantityWidgetValue;
        private TextView _distanceWidgetValue;
        private TextView _currentSpeedWidgetValue;
        private TextView _durationWidgetValue;
        private Timer _trackInfoUpdateTimer;
        private MapFragment _mapFragment;

        #region Life Circle

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            InitTrackInfoUpdateTimer();
        }

        protected override void SetView()
        {
            SetContentView(Resource.Layout.ActiveTrackFullScreenMapLayout);
        }

        protected override GoogleMap GetMap()
        {
            if (_mapFragment == null)
            {
                _mapFragment = (MapFragment) FragmentManager.FindFragmentById(Resource.Id.Map);
            }

            return _mapFragment.Map;
        }

        protected void InitTrackInfoUpdateTimer()
        {
            _trackInfoUpdateTimer = new Timer(1000);
        }

        protected override void OnResume()
        {
            base.OnResume();

            UpdateWidgets();

            _trackInfoUpdateTimer.Elapsed += UpdateTrackInfoEventHandler;
            _trackInfoUpdateTimer.Start();
        }

        protected override void OnPause()
        {
            base.OnPause();

            _trackInfoUpdateTimer.Elapsed -= UpdateTrackInfoEventHandler;
            _trackInfoUpdateTimer.Stop();

            GC.Collect();
        }

        #endregion

        #region Location Callbacks

        public override void LocationListenerOnLocationChanged(Location location)
        {
            base.LocationListenerOnLocationChanged(location);

            UpdateWidgets();
        }

        #endregion

        #region Path Display Methods

        private void UpdateWidgets()
        {
            var location = App.LocationListener.Location;
            var currentSpeed = location != null ? location.Speed.MetersPerSecondToKilometersPerHour() : 0;
            var trackPoints = App.ActiveTrackManager.TrackPoints;
            var distance = App.ActiveTrackManager.Distance.MetersToKilometers();
            var duration = App.ActiveTrackManager.Duration;

            UpdateTrackPointsWidget(trackPoints.Count);
            UpdateDistanceWidget(distance);
            UpdateDurationWidget(duration);
            UpdateCurrentSpeedWidget(currentSpeed);
        }

        private void UpdateTrackPointsWidget(int trackPointsQuantity)
        {
            if (_trackPointsQuantityWidgetValue == null)
            {
                _trackPointsQuantityWidgetValue = FindViewById<TextView>(Resource.Id.TrackPointsQuantityWidget_Value);
            }

            _trackPointsQuantityWidgetValue.Text = trackPointsQuantity.ToString(CultureInfo.InvariantCulture);
        }

        private void UpdateDistanceWidget(float distance)
        {
            if (_distanceWidgetValue == null)
            {
                _distanceWidgetValue = FindViewById<TextView>(Resource.Id.DistanceWidget_Value);
            }

            _distanceWidgetValue.Text = String.Format(GetString(Resource.String.distance_format), distance);
        }

        private void UpdateDurationWidget(TimeSpan duration)
        {
            if (_durationWidgetValue == null)
            {
                _durationWidgetValue = FindViewById<TextView>(Resource.Id.DurationWidget_Value);
            }

            _durationWidgetValue.Text = String.Format(GetString(Resource.String.duration_format), duration);
        }

        private void UpdateCurrentSpeedWidget(double speed)
        {
            if (_currentSpeedWidgetValue == null)
            {
                _currentSpeedWidgetValue = FindViewById<TextView>(Resource.Id.CurrentSpeedWidget_Value);
            }

            _currentSpeedWidgetValue.Text = String.Format(GetString(Resource.String.speed_format), speed);
        }

        private void UpdateTrackInfoEventHandler(object sender, EventArgs e)
        {
            RunOnUiThread(() => UpdateDurationWidget(App.ActiveTrackManager.Duration));
        }

        #endregion
    }
}