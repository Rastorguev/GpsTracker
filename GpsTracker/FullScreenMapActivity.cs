using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Widget;
using MapsTest_1;
using ILocationListener = Android.Gms.Location.ILocationListener;

namespace GpsTracker
{
    [Activity(Label = "@string/app_name", MainLauncher = false)]
    public class FullScreenMapActivity : Activity, IGooglePlayServicesClientConnectionCallbacks,
        IGooglePlayServicesClientOnConnectionFailedListener, GoogleMap.IOnCameraChangeListener, ILocationListener
    {
        private LocationClient _locationClient;
        private GoogleMap _map;
        private float _zoom = 18;
        private Marker _currentPositionMarker;
        private Marker _startPositionMarker;
        private readonly List<Polyline> _polylines = new List<Polyline>();
        private static TrackDataStorage _trackDataStorage;

        //Constants
        private const int MinimalDisplacement = 3;
        private const int MaxPointsInPolylineQuantity = 500;

        #region Life Circle

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RestoreSavedState(savedInstanceState);

            SetContentView(Resource.Layout.FullScreenMap);

            if (_trackDataStorage == null)
            {
                _trackDataStorage = new TrackDataStorage();
            }

            _map = ((MapFragment)FragmentManager.FindFragmentById(Resource.Id.Map)).Map;
            _map.SetOnCameraChangeListener(this);
            //_map.MyLocationEnabled = true;

            _map.UiSettings.MyLocationButtonEnabled = true;
            _map.UiSettings.CompassEnabled = true;

            //GeneratedFakeTrack(10000);
        }

        protected override void OnStart()
        {
            base.OnStart();

            _locationClient = new LocationClient(this, this, this);
            _locationClient.Connect();

            UpdateTrackPointsWidget(_trackDataStorage.TrackPoints.Count);
            UpdateDistanceWidget(_trackDataStorage.TotalDistance.MetersToKilometers());
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

            if (_trackDataStorage.TrackPoints.Any())
            {
                var displacement = _trackDataStorage.TrackPoints.Last().DistanceTo(trackPoint);

                if (displacement >= MinimalDisplacement)
                {
                    _trackDataStorage.AddTrackPoint(trackPoint);
                }
            }
            else
            {
                _trackDataStorage.AddTrackPoint(trackPoint);
            }

            UpdateTrackPointsWidget(_trackDataStorage.TrackPoints.Count);
            UpdateDistanceWidget(_trackDataStorage.TotalDistance.MetersToKilometers());
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

            const int overlay = 1;

            if (_trackDataStorage.TrackPoints.Any())
            {
                if (_startPositionMarker == null)
                {
                    _startPositionMarker = CreateStartPositionMarker(_map, _trackDataStorage.TrackPoints.First());
                }
                else
                {
                    _startPositionMarker.Position = _trackDataStorage.TrackPoints.First();
                }
            }

            if (_trackDataStorage.TrackPoints.Count > 1)
            {
                var expectedWholePolylinesNumber =
                    Math.Ceiling(((decimal)_trackDataStorage.TrackPoints.Count / MaxPointsInPolylineQuantity));

                if (!_polylines.Any())
                {
                    var segments = new List<List<LatLng>>();
                    var n = 0;

                    while (segments.Count < expectedWholePolylinesNumber)
                    {
                        if (_trackDataStorage.TrackPoints.Count >= MaxPointsInPolylineQuantity * (n + 1))
                        {
                            var index = n != 0 ? n * MaxPointsInPolylineQuantity - overlay : n * MaxPointsInPolylineQuantity;
                            var count = n != 0 ? MaxPointsInPolylineQuantity + overlay : MaxPointsInPolylineQuantity;

                            segments.Add(_trackDataStorage.TrackPoints.GetRange(index, count));
                            n++;
                        }
                        else
                        {
                            var index = n != 0 ? n * MaxPointsInPolylineQuantity - overlay : n * MaxPointsInPolylineQuantity;
                            var count = n != 0
                                ? _trackDataStorage.TrackPoints.Count - n * MaxPointsInPolylineQuantity + overlay
                                : _trackDataStorage.TrackPoints.Count - n * MaxPointsInPolylineQuantity;

                            segments.Add(_trackDataStorage.TrackPoints.GetRange(index, count));
                        }
                    }

                    foreach (var segment in segments)
                    {
                        var polyline = CreatePolyline(_map, segment, new PolylineOptions());

                        _polylines.Add(polyline);
                    }
                }

                else if (expectedWholePolylinesNumber == _polylines.Count)
                {
                    var index = _polylines.Count > 1
                        ? (_polylines.Count - 1) * MaxPointsInPolylineQuantity - overlay
                        : (_polylines.Count - 1) * MaxPointsInPolylineQuantity;
                    var count = _polylines.Count > 1
                        ? _trackDataStorage.TrackPoints.Count - (_polylines.Count - 1) * MaxPointsInPolylineQuantity +
                          overlay
                        : _trackDataStorage.TrackPoints.Count - (_polylines.Count - 1) * MaxPointsInPolylineQuantity;

                    var lastPolyline = _polylines.Last();
                    var newSegment = _trackDataStorage.TrackPoints.GetRange(index, count);

                    lastPolyline.Points = newSegment;
                }

                if (expectedWholePolylinesNumber > _polylines.Count)
                {
                    var index = _polylines.Count * MaxPointsInPolylineQuantity - overlay;
                    var count = _trackDataStorage.TrackPoints.Count - _polylines.Count * MaxPointsInPolylineQuantity +
                                overlay;

                    var newSegment = _trackDataStorage.TrackPoints.GetRange(index, count);
                    var polyline = CreatePolyline(_map, newSegment, new PolylineOptions());

                    _polylines.Add(polyline);
                }

                if (_currentPositionMarker == null)
                {
                    _currentPositionMarker = CreateCurrentPositionMarker(_map, _trackDataStorage.TrackPoints.Last());
                }
                else
                {
                    _currentPositionMarker.Position = _trackDataStorage.TrackPoints.Last();
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

        private void GeneratedFakeTrack(int n)
        {
            var random = new Random();
            var lat = 53.926193;

            if (!_trackDataStorage.TrackPoints.Any())
            {
                for (var i = 0; i < n; i++)
                {
                    lat += 0.000008;

                    var x = (double)1 / random.Next(-100000, 100000);

                    _trackDataStorage.TrackPoints.Add(new LatLng(lat, 27.689841 + x));
                }
            }
        }

        #endregion
    }
}