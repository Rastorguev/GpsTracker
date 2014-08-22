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

        public Location Location { get; private set; }

        private bool ChangeLastLocation(Location location)
        {
            if (location == null ||
                (Location != null &&
                 (Location.Equals(location) || !(Location.DistanceTo(location) >= Constants.MinimalDisplacement))))
                return false;

            Location = location;

            return true;
        }

        public void OnConnected(Bundle connectionHint)
        {
            StartListenLocationUpdates();

            ChangeLastLocation(LocationServices.FusedLocationApi.GetLastLocation(App.LocationClient));

            UpdateActiveTrack(Location);

            TriggerConnected(Location);
        }

        public virtual void OnConnectionSuspended(int cause) {}

        public void OnLocationChanged(Location location)
        {
            var locationChanged = ChangeLastLocation(location);
            if (locationChanged)
            {
                UpdateActiveTrack(Location);
                TriggerLocationChanged(Location);
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

        public void UpdateActiveTrack(Location location)
        {
            if (location == null)
            {
                return;
            }

            var trackPoint = location.ToLatLng();

            if (App.ActiveTrackManager.HasActiveTrack)
            {
                App.ActiveTrackManager.AddTrackPoint(trackPoint);

                TriggerLocationChanged(location);
            }
        }
    }
}