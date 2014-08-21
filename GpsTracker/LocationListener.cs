using System;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Locations;
using Android.OS;
using GpsTracker.Config;
using ILocationListener = Android.Gms.Location.ILocationListener;
using Object = Java.Lang.Object;

namespace GpsTracker
{
    public class LocationListener : Object, ILocationListener, IGoogleApiClientConnectionCallbacks
    {
        public event Action<Location> Connected;
        public event Action<Location> LocationChanged;

        private Location _lastLocation;
        public Location LastLocation
        {
            get { return _lastLocation; }
            set
            {
                if (value == null) return;

                if (_lastLocation == null ||
                    (!_lastLocation.Equals(value) &&
                     _lastLocation.DistanceTo(value) >= Constants.MinimalDisplacement))
                {
                    _lastLocation = value;

                    LocationChangedHandler(_lastLocation);
                    TriggerLocationChanged(_lastLocation);
                }
            }
        }

        public void OnConnected(Bundle connectionHint)
        {
            StartListenLocationUpdates();

            LastLocation = LocationServices.FusedLocationApi.GetLastLocation(App.LocationClient);

            TriggerConnected(LastLocation);
        }

        public virtual void OnConnectionSuspended(int cause) {}

        public void OnLocationChanged(Location location)
        {
            LastLocation = location;
        }

        public void LocationChangedHandler(Location location)
        {
            var trackPoint = location.ToLatLng();

            if (App.ActiveTrackManager.HasActiveTrack)
            {
                App.ActiveTrackManager.AddTrackPoint(trackPoint);

                TriggerLocationChanged(location);
            }
        }

        private void StartListenLocationUpdates()
        {
            var locationRequest = new LocationRequest();

            locationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            locationRequest.SetInterval(Constants.LocationUpdateInterval);
            locationRequest.SetFastestInterval(Constants.LocationUpdateFastestInterval);

            LocationServices.FusedLocationApi.RequestLocationUpdates(App.LocationClient, locationRequest, this);
        }

        private void TriggerConnected(Location location)
        {
            if (Connected != null)
            {
                Connected(location);
            }
        }

        private void TriggerLocationChanged(Location location)
        {
            if (LocationChanged != null)
            {
                LocationChanged(location);
            }
        }
    }
}