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

        public Location LastLocation
        {
            get { return LocationServices.FusedLocationApi.GetLastLocation(App.LocationClient); }
        }

        public void OnConnected(Bundle connectionHint)
        {
            StartListenLocationUpdates();

            var location = LocationServices.FusedLocationApi.GetLastLocation(App.LocationClient);
            var trackPoint = location.ToLatLng();

            App.ActiveTrackManager.TryAddTrackPoint(trackPoint);
            TriggerConnected(location);
        }
        
        public virtual void OnConnectionSuspended(int cause) {}

        public void OnLocationChanged(Location location)
        {
            var trackPoint = location.ToLatLng();

            if (App.ActiveTrackManager.HasActiveTrack)
            {
                var tracPointAdded = App.ActiveTrackManager.TryAddTrackPoint(trackPoint);

                if (tracPointAdded)
                {
                    TriggerLocationChanged(location);
                }
            }
            else
            {
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