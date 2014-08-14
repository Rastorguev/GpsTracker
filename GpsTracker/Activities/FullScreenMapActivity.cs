using System;
using System.Timers;
using Android.App;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Widget;
using GpsTracker.Managers;
using ILocationListener = Android.Gms.Location.ILocationListener;

namespace GpsTracker
{
    [Activity(Label = "@string/app_name", MainLauncher = false)]
    internal class FullScreenMapActivity : Activity, IGooglePlayServicesClientConnectionCallbacks,
        IGooglePlayServicesClientOnConnectionFailedListener, GoogleMap.IOnCameraChangeListener, ILocationListener
    {
        private LocationClient _locationClient;
        private GoogleMap _map;
        private float _zoom = 18;
        private Marker _currentPositionMarker;
        private Marker _startPositionMarker;
        private ITrackDrawer _trackDrawer;
        private static ActiveTrackManager _activeTrackManager;
        private TextView _trackPointsQuantityWidget;
        private TextView _distanceWidget;
        private TextView _speedWidget;
        private Timer _trackInfoUpdateTimer;
        private TextView _durationWidget;

        #region Life Circle

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RestoreSavedState(savedInstanceState);

            SetContentView(Resource.Layout.FullScreenMap);

            if (_activeTrackManager == null)
            {
                _activeTrackManager = new ActiveTrackManager();
                _activeTrackManager.StartTrack();
            }

            if (!_activeTrackManager.IsStarted)
            {
                _activeTrackManager.StartTrack();
            }

            var mapFragment = (MapFragment)FragmentManager.FindFragmentById(Resource.Id.Map);

            _map = mapFragment.Map;
            _map.SetOnCameraChangeListener(this);

            _map.UiSettings.MyLocationButtonEnabled = true;
            _map.UiSettings.CompassEnabled = true;

            _trackDrawer = new TrackDrawer(_map);

            _locationClient = new LocationClient(this, this, this);
            _locationClient.Connect();

            _trackInfoUpdateTimer = new Timer(1000);
        }

        protected override void OnStart()
        {
            base.OnStart();
            UpdateTrackInfo();

            _trackInfoUpdateTimer.Elapsed += UpdateTrackInfoEventHandler;
            _trackInfoUpdateTimer.Start();
        }

        protected override void OnPause()
        {
            base.OnPause();

            _trackDrawer.RemoveTrack();
            _trackInfoUpdateTimer.Elapsed -= UpdateTrackInfoEventHandler;
            _trackInfoUpdateTimer.Stop();

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
                    _locationClient.RemoveLocationUpdates(this);
                }

                _locationClient.UnregisterConnectionCallbacks(this);
                _locationClient.UnregisterConnectionFailedListener(this);
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
           
            _locationClient.RequestLocationUpdates(locationRequest, this);

            var location = _locationClient.LastLocation;

            if (location != null)
            {
                MoveCamera(_locationClient.LastLocation.ToLatLng());
                OnLocationChanged(location);
            }
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            Toast.MakeText(this, String.Format("Connection Failed"), ToastLength.Long).Show();
        }

        public void OnLocationChanged(Location location)
        {
            var trackPoint = location.ToLatLng();
            var pointAdded = _activeTrackManager.TryAddTrackPoint(trackPoint);

            if (pointAdded)
            {
                UpdateTrackInfo();
            }
        }

        public void OnCameraChange(CameraPosition position)
        {
            _zoom = position.Zoom;
        }

        public void OnDisconnected()
        {
            Toast.MakeText(this, String.Format("Disconnected"), ToastLength.Long).Show();
        }

        #endregion

        #region Path Display Methods

        private void UpdateTrackInfo()
        {
            var trackPoints = _activeTrackManager.TrackPoints;
            var distance = _activeTrackManager.Distance;

            _trackDrawer.DrawTrack(trackPoints);
            UpdateTrackPointsWidget(trackPoints.Count);
            UpdateDistanceWidget(distance.MetersToKilometers());
        }

        private void MoveCamera(LatLng trackPoint)
        {
            var builder = CameraPosition.InvokeBuilder();

            builder.Target(trackPoint);
            builder.Zoom(_zoom);

            var cameraPosition = builder.Build();
            var cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);

            _map.MoveCamera(cameraUpdate);
        }

        private void UpdateTrackPointsWidget(int trackPointsQuantity)
        {
            if (_trackPointsQuantityWidget == null)
            {
                _trackPointsQuantityWidget = FindViewById<TextView>(Resource.Id.TrackPointsQuantityWidget);
            }

            _trackPointsQuantityWidget.Text = trackPointsQuantity.ToString();
        }

        private void UpdateDistanceWidget(float distance)
        {
            if (_distanceWidget == null)
            {
                _distanceWidget = FindViewById<TextView>(Resource.Id.DistanceWidget);
            }

            _distanceWidget.Text = String.Format("{0:0.000}", distance);
        }

        private void UpdateSpeedWidget(double speed)
        {
            if (_speedWidget == null)
            {
                _speedWidget = FindViewById<TextView>(Resource.Id.SpeedWidget);
            }

            _speedWidget.Text = String.Format("{0:0.00}", speed);
        }


        private void UpdateDurationWidget(TimeSpan duration)
        {
            if (_durationWidget == null)
            {
                _durationWidget = FindViewById<TextView>(Resource.Id.DurationWidget);
            }

            _durationWidget.Text = String.Format("{0:hh\\:mm\\:ss}", duration);
        }

        private void UpdateTrackInfoEventHandler(object sender, EventArgs e)
        {
            RunOnUiThread(() => UpdateDurationWidget(_activeTrackManager.Duration));
            RunOnUiThread(() => UpdateSpeedWidget(_activeTrackManager.Speed.MetersPerSecondToKilometersPerHour()));
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

        #endregion
    }
}