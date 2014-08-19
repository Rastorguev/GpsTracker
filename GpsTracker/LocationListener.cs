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

        public void OnConnected(Bundle connectionHint)
        {
            StartListenLocationUpdates();

            var location = LocationServices.FusedLocationApi.GetLastLocation(App.LocationClient);
            var trackPoint = location.ToLatLng();

            App.ActiveTrackManager.TryAddTrackPoint(trackPoint);

            if (Connected != null)
            {
                Connected(location);
            }
        }

        public virtual void OnConnectionSuspended(int cause) {}

        public void OnLocationChanged(Location location)
        {
            var trackPoint = location.ToLatLng();

            if (App.ActiveTrackManager.HasActiveTrack)
            {
                var pointAdded = App.ActiveTrackManager.TryAddTrackPoint(trackPoint);

                if (pointAdded && LocationChanged != null)
                {
                    LocationChanged(location);
                }
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
    }
}