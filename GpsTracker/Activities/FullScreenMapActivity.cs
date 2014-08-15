using System;
using System.Globalization;
using System.Linq;
using System.Timers;
using Android.App;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Views;
using Android.Widget;
using GpsTracker.Config.GpsTracker;
using GpsTracker.Managers;
using GpsTracker.Tools;
using ILocationListener = Android.Gms.Location.ILocationListener;

namespace GpsTracker.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = false)]
    internal class FullScreenMapActivity : Activity, IGoogleApiClientConnectionCallbacks,
        GoogleMap.IOnCameraChangeListener, ILocationListener
    {
        private IGoogleApiClient _locationClient;
        private GoogleMap _map;
        private float _zoom = 18;
        private const int AutoreturnDelay = 5000;
        private ITrackDrawer _trackDrawer;
        private static ActiveTrackManager _activeTrackManager;
        private TextView _trackPointsQuantityWidgetValue;
        private TextView _distanceWidgetValue;
        private TextView _currentSpeedWidgetValue;
        private TextView _currentSpeedWidgetUnit;
        private TextView _durationWidgetValue;

        private Timer _trackInfoUpdateTimer;
        private Timer _autoreturnTimer;

        #region Life Circle

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RestoreSavedState(savedInstanceState);

            SetContentView(Resource.Layout.FullScreenMap);

            InitMap();
            InitActiveTrackManager();
            InitLocationClient();
            InitTrackDrawer();
            InitTimers();
        }

        #region Initializtion

        private void InitMap()
        {
            var mapFragment = (MapFragment) FragmentManager.FindFragmentById(Resource.Id.Map);

            _map = mapFragment.Map;
            _map.SetOnCameraChangeListener(this);
        }

        private void InitActiveTrackManager()
        {
            if (_activeTrackManager == null)
            {
                _activeTrackManager = new ActiveTrackManager();
                _activeTrackManager.StartTrack();
            }

            if (!_activeTrackManager.IsStarted)
            {
                _activeTrackManager.StartTrack();
            }
        }

        private void InitLocationClient()
        {
            _locationClient = new GoogleApiClientBuilder(this)
                .AddApi(LocationServices.Api)
                .AddConnectionCallbacks(this)
                .Build();

            _locationClient.Connect();
        }

        private void InitTrackDrawer()
        {
            _trackDrawer = new TrackDrawer(_map);
        }

        private void InitTimers()
        {
            _trackInfoUpdateTimer = new Timer(1000);

            _autoreturnTimer = new Timer
            {
                AutoReset = false,
                Interval = AutoreturnDelay
            };
        }

        #endregion

        protected override void OnStart()
        {
            base.OnStart();

            _trackDrawer.DrawTrack(_activeTrackManager.TrackPoints);

            UpdateWidgets();

            _trackInfoUpdateTimer.Elapsed += UpdateTrackInfoEventHandler;
            _trackInfoUpdateTimer.Start();

            _autoreturnTimer.Elapsed += AutoreturnEventHandler;
        }

        protected override void OnPause()
        {
            base.OnPause();

            _trackDrawer.RemoveTrack();
            _trackInfoUpdateTimer.Elapsed -= UpdateTrackInfoEventHandler;
            _trackInfoUpdateTimer.Stop();

            _autoreturnTimer.Elapsed -= AutoreturnEventHandler;

            GC.Collect();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            outState.PutFloat("zoom", _zoom);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_locationClient != null)
            {
                if (_locationClient.IsConnected)
                {
                    LocationServices.FusedLocationApi.RemoveLocationUpdates(_locationClient, this);
                }

                _locationClient.UnregisterConnectionCallbacks(this);
                _locationClient.Disconnect();
                _locationClient.Dispose();
            }

            GC.Collect(GC.MaxGeneration);
        }

        #endregion

        #region Location Callbacks

        public void OnConnected(Bundle bundle)
        {
            var locationRequest = new LocationRequest();

            locationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            locationRequest.SetInterval(1000);
            locationRequest.SetFastestInterval(1000);

            LocationServices.FusedLocationApi.RequestLocationUpdates(_locationClient, locationRequest, this);

            var location = LocationServices.FusedLocationApi.GetLastLocation(_locationClient);

            if (location != null)
            {
                MoveCamera(location.ToLatLng());
                OnLocationChanged(location);
            }
        }

        public void OnConnectionSuspended(int cause)
        {
            Console.WriteLine(cause);
        }

        public void OnLocationChanged(Location location)
        {
            var currentSpeed = location.HasSpeed ? (float?) location.Speed : null;

            UpdateCurrentSpeedWidget(currentSpeed);

            var trackPoint = location.ToLatLng();
            var pointAdded = _activeTrackManager.TryAddTrackPoint(trackPoint);

            if (pointAdded)
            {
                var trackPoints = _activeTrackManager.TrackPoints;
                var distance = _activeTrackManager.Distance;

                _trackDrawer.DrawTrack(_activeTrackManager.TrackPoints);

                UpdateTrackPointsWidget(trackPoints.Count);
                UpdateDistanceWidget(distance.MetersToKilometers());

                if (UserConfig.RotateMapInAccordanceWithTheMovement && IsTrackPointVisible(trackPoint))
                {
                    MoveCamera(trackPoint);
                }
            }
        }

        public void OnCameraChange(CameraPosition position)
        {
            _zoom = position.Zoom;

            var currentPosition = _activeTrackManager.TrackPoints.Last();

            if (UserConfig.Autoreturn && !IsTrackPointVisible(currentPosition))
            {
                Autoreturn();
            }
        }

        #endregion

        #region Path Display Methods

        private void UpdateWidgets()
        {
            var trackPoints = _activeTrackManager.TrackPoints;
            var distance = _activeTrackManager.Distance.MetersToKilometers();
            var duration = _activeTrackManager.Duration;

            UpdateTrackPointsWidget(trackPoints.Count);
            UpdateDistanceWidget(distance);
            UpdateDurationWidget(duration);

            UpdateCurrentSpeedWidget(null);
        }

        private void MoveCamera(LatLng trackPoint)
        {
            var builder = CameraPosition.InvokeBuilder();

            builder.Target(trackPoint);
            builder.Zoom(_zoom);
            builder.Bearing(_activeTrackManager.Bearing);

            var cameraPosition = builder.Build();
            var cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);

            _map.AnimateCamera(cameraUpdate);
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

            _distanceWidgetValue.Text = String.Format("{0:0.000}", distance);
        }

        private void UpdateDurationWidget(TimeSpan duration)
        {
            if (_durationWidgetValue == null)
            {
                _durationWidgetValue = FindViewById<TextView>(Resource.Id.DurationWidget_Value);
            }

            _durationWidgetValue.Text = String.Format("{0:hh\\:mm\\:ss}", duration);
        }

        private void UpdateCurrentSpeedWidget(float? speed)
        {
            if (_currentSpeedWidgetValue == null)
            {
                _currentSpeedWidgetValue = FindViewById<TextView>(Resource.Id.CurrentSpeedWidget_Value);
            }

            if (_currentSpeedWidgetUnit == null)
            {
                _currentSpeedWidgetUnit = FindViewById<TextView>(Resource.Id.CurrentSpeedWidget_Unit);
            }

            if (speed != null)
            {
                _currentSpeedWidgetValue.Text = String.Format("{0:0.0}", speed);
                _currentSpeedWidgetUnit.Visibility = ViewStates.Visible;
            }
            else
            {
                _currentSpeedWidgetValue.Text = "-.-";
                _currentSpeedWidgetUnit.Visibility = ViewStates.Gone;
            }
        }

        private void Autoreturn()
        {
            _autoreturnTimer.Stop();
            _autoreturnTimer.Start();
        }

        private void UpdateTrackInfoEventHandler(object sender, EventArgs e)
        {
            RunOnUiThread(() => UpdateDurationWidget(_activeTrackManager.Duration));
        }

        private void AutoreturnEventHandler(object sender, EventArgs e)
        {
            var currentPosition = _activeTrackManager.TrackPoints.Last();

            RunOnUiThread(() => MoveCamera(currentPosition));
        }

        #endregion

        #region Helpers

        private void RestoreSavedState(Bundle savedInstanceState)
        {
            if (savedInstanceState != null)
            {
                _zoom = savedInstanceState.GetFloat("zoom");
            }
        }

        private bool IsTrackPointVisible(LatLng trackPoint)
        {
            var bounds = _map.Projection.VisibleRegion.LatLngBounds;
            return bounds.Contains(trackPoint);
        }

        #endregion

        
    }
}