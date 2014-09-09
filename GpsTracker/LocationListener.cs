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
        public Location PreviousLocation { get; private set; }

        public float? Bearing
        {
            get
            {
                return Location != null && PreviousLocation != null
                    ? (float?) PreviousLocation.BearingTo(Location)
                    : null;
            }
        }

        private bool ChangeLocation(Location location)
        {
            if (location == null ||
                (Location != null &&
                 (Location.Equals(location) || !(Location.DistanceTo(location) >= Constants.MinimalDisplacement))))
                return false;

            PreviousLocation = Location;
            Location = location;

            return true;
        }

        public void OnConnected(Bundle connectionHint)
        {
            StartListenLocationUpdates();

            ChangeLocation(LocationServices.FusedLocationApi.GetLastLocation(App.LocationClient));

            TriggerConnected(Location);
            TriggerLocationChanged(Location);
        }

        public virtual void OnConnectionSuspended(int cause) {}

        public void OnLocationChanged(Location location)
        {
            var locationChanged = ChangeLocation(location);
            if (locationChanged)
            {
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
    }
}