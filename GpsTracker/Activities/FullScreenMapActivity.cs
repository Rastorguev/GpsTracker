using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Widget;
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
        private readonly List<Polyline> _polylines = new List<Polyline>();
        private static TrackData _trackData;

        //Constants
        private const int MinimalDisplacement = 3;
        private const int MaxPointsInPolylineQuantity = 500;

        #region Life Circle

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RestoreSavedState(savedInstanceState);

            SetContentView(Resource.Layout.FullScreenMap);

            if (_trackData == null)
            {
                _trackData = new TrackData();

                //var trackPoints = TrackOperations.GeneratedFakeTrack(1000);

                //trackPoints.ForEach(p => TrackOperations.AddTrackPoint(_trackData, p));
            }

            var mapFragment = (MapFragment) FragmentManager.FindFragmentById(Resource.Id.Map);

            _map = mapFragment.Map;
            _map.SetOnCameraChangeListener(this);
            //_map.MyLocationEnabled = true;

            _map.UiSettings.MyLocationButtonEnabled = true;
            _map.UiSettings.CompassEnabled = true;
        }

        protected override void OnStart()
        {
            base.OnStart();

            _locationClient = new LocationClient(this, this, this);
            _locationClient.Connect();

            UpdateTrackPointsWidget(_trackData.TrackPoints.Count);
            UpdateDistanceWidget(_trackData.TotalDistance.MetersToKilometers());
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

            GC.Collect();
        }

        #endregion

        #region Location Callbacks

        public void OnConnected(Bundle bundle)
        {
            var locationRequest = new LocationRequest();

            locationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            locationRequest.SetFastestInterval(1000);
            locationRequest.SetInterval(2000);
            //locationRequest.SetSmallestDisplacement(1);

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
            if (location.HasSpeed)
            {
                var speed = location.Speed;
                UpdateSpeedWidget(speed.MetersPerSecondToKilometersPerHour());
            }

            var trackPoint = location.ToLatLng();

            if (_trackData.TrackPoints.Any())
            {
                var displacement = _trackData.TrackPoints.Last().DistanceTo(trackPoint);

                if (displacement >= MinimalDisplacement)
                {
                    TrackOperations.AddTrackPoint(_trackData, trackPoint);
                }
            }
            else
            {
                TrackOperations.AddTrackPoint(_trackData, trackPoint);
            }

            UpdateTrackPointsWidget(_trackData.TrackPoints.Count);
            UpdateDistanceWidget(_trackData.TotalDistance.MetersToKilometers());
            ShowTrack();
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

        private void ShowTrack()
        {
            var st = DateTime.Now;

            if (_trackData.TrackPoints.Any())
            {
                if (_startPositionMarker == null)
                {
                    _startPositionMarker = CreateStartPositionMarker(_map, _trackData.TrackPoints.First());
                }
                else
                {
                    _startPositionMarker.Position = _trackData.TrackPoints.First();
                }
            }

            if (_trackData.TrackPoints.Count > 1)
            {
                var segments = TrackOperations.SplitTrackOnSegments(_trackData.TrackPoints, MaxPointsInPolylineQuantity);

                if (!_polylines.Any())
                {
                    var polylines = segments.Select(s => CreatePolyline(_map, s, new PolylineOptions()));

                    _polylines.AddRange(polylines);
                }

                else if (segments.Count == _polylines.Count)
                {
                    var lastPolyline = _polylines.Last();
                    var newSegment = segments.Last();

                    lastPolyline.Points = newSegment;
                }

                if (segments.Count > _polylines.Count)
                {
                    var newSegment = segments.Last();
                    var polyline = CreatePolyline(_map, newSegment, new PolylineOptions());

                    _polylines.Add(polyline);
                }

                if (_currentPositionMarker == null)
                {
                    _currentPositionMarker = CreateCurrentPositionMarker(_map, _trackData.TrackPoints.Last());
                }
                else
                {
                    _currentPositionMarker.Position = _trackData.TrackPoints.Last();
                }
            }

            var et = DateTime.Now;
            Console.WriteLine("@@@@@@@@@@@@ " + (et - st).TotalMilliseconds + " @@@@@@@@@@@@@@");
        }

        private Marker CreateStartPositionMarker(GoogleMap map, LatLng trackPoint)
        {
            var options = new MarkerOptions();
            var color = BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed);

            options.InvokeIcon(color);
            options.SetPosition(trackPoint);

            var marker = map.AddMarker(options);
            return marker;
        }

        private Marker CreateCurrentPositionMarker(GoogleMap map, LatLng trackPoint)
        {
            var options = new MarkerOptions();
            var color = BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueGreen);

            options.SetPosition(trackPoint);
            options.InvokeIcon(color);

            var marker = map.AddMarker(options);
            return marker;
        }

        private Polyline CreatePolyline(GoogleMap map, List<LatLng> trackPoints, PolylineOptions polylineOptions)
        {
            var color = Resources.GetColor(Resource.Color.track_color);
            //var random = new Random();
            //var red = random.Next(0, 255);
            //var green = random.Next(0, 255);
            //var blue = random.Next(0, 255);
            //polylineOptions.InvokeColor(Color.Argb(255, red, green, blue));
            polylineOptions.InvokeColor(color);
            polylineOptions.InvokeWidth(6);

            trackPoints.ForEach(p => polylineOptions.Add(p));
            var polyline = map.AddPolyline(polylineOptions);

            return polyline;
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
            var trackPointsQuantityWidget = FindViewById<TextView>(Resource.Id.TrackPointsQuantityWidget);
            trackPointsQuantityWidget.Text = trackPointsQuantity.ToString();
        }

        private void UpdateDistanceWidget(float distance)
        {
            var distanceWidget = FindViewById<TextView>(Resource.Id.DistanceWidget);
            distanceWidget.Text = String.Format("{0:0.00}", distance);
        }

        private void UpdateSpeedWidget(double speed)
        {
            var speedWidget = FindViewById<TextView>(Resource.Id.SpeedWidget);
            speedWidget.Text = String.Format("{0:0.00}", speed);
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