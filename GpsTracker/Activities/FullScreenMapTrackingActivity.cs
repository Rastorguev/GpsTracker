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
    internal class FullScreenMapTrackingActivity : BaseTrackingActivity
    {
        private TextView _trackPointsValue;
        private TextView _currentSpeedValue;
        private TextView _currentSpeedUnit;
        private TextView _distanceValue;
        private TextView _distanceUnit;
        private TextView _durationValue;
        private MapFragment _mapFragment;
        private Timer _trackInfoUpdateTimer;

        #region Life Circle

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            InitTrackInfoUpdateTimer();
        }

        protected override void SetView()
        {
            SetContentView(Resource.Layout.FullScreenMapTrackingLayout);
        }

        protected override GoogleMap GetMap()
        {
            if (_mapFragment == null)
            {
                _mapFragment = (MapFragment) FragmentManager.FindFragmentById(Resource.Id.Map);
            }

            return _mapFragment.Map;
        }

        private void InitTrackInfoUpdateTimer()
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

        protected override void LocationListenerOnLocationChanged(Location location)
        {
            base.LocationListenerOnLocationChanged(location);

            UpdateWidgets();
        }

        #endregion

        #region Path Display Methods

        private void UpdateWidgets()
        {
            var location = LocationManager.Location;
            var currentSpeed = location != null ? UnitsPersonalizer.GetSpeedValue(location.Speed) : 0;
            var trackPoints = ActiveTrackManager.TrackPoints;
            var distance = UnitsPersonalizer.GetDistanceValue(ActiveTrackManager.Distance);
            var duration = ActiveTrackManager.Duration;

            SetUnits();
            UpdateTrackPointsWidget(trackPoints.Count);
            UpdateDistanceWidget(distance);
            UpdateDurationWidget(duration);
            UpdateCurrentSpeedWidget(currentSpeed);
        }

        private void SetUnits()
        {
            if (_currentSpeedUnit == null)
            {
                _currentSpeedUnit = FindViewById<TextView>(Resource.Id.CurrentSpeedUnit);
            }

            if (_distanceUnit == null)
            {
                _distanceUnit = FindViewById<TextView>(Resource.Id.DistanceUnit);
            }

            _currentSpeedUnit.Text = UnitsPersonalizer.GetSpeedUnit();
            _distanceUnit.Text = UnitsPersonalizer.GetDistanceUnit();
        }

        private void UpdateTrackPointsWidget(int trackPointsQuantity)
        {
            if (_trackPointsValue == null)
            {
                _trackPointsValue = FindViewById<TextView>(Resource.Id.TrackPointsValue);
            }

            _trackPointsValue.Text = trackPointsQuantity.ToString(CultureInfo.InvariantCulture);
        }

        private void UpdateDistanceWidget(float distance)
        {
            if (_distanceValue == null)
            {
                _distanceValue = FindViewById<TextView>(Resource.Id.DistanceValue);
            }

            _distanceValue.Text = String.Format(GetString(Resource.String.distanceFormat), distance);
        }

        private void UpdateDurationWidget(TimeSpan duration)
        {
            if (_durationValue == null)
            {
                _durationValue = FindViewById<TextView>(Resource.Id.DurationValue);
            }

            _durationValue.Text = String.Format(GetString(Resource.String.durationFormat), duration);
        }

        private void UpdateCurrentSpeedWidget(double speed)
        {
            if (_currentSpeedValue == null)
            {
                _currentSpeedValue = FindViewById<TextView>(Resource.Id.CurrentSpeedValue);
            }

            _currentSpeedValue.Text = String.Format(GetString(Resource.String.speedFormat), speed);
        }

        private void UpdateTrackInfoEventHandler(object sender, EventArgs e)
        {
            RunOnUiThread(() => UpdateDurationWidget(ActiveTrackManager.Duration));
        }

        #endregion
    }
}