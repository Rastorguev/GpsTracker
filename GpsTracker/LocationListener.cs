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
        public Location Location { get; private set; }
        public Location PreviousLocation { get; private set; }
        public DateTime? LastLocationUpDateTime { get; private set; }

        [Obsolete]
        public double Speed { get; private set; }

        public float? Bearing
        {
            get
            {
                return Location != null && PreviousLocation != null
                    ? (float?) PreviousLocation.BearingTo(Location)
                    : null;
            }
        }

        public void OnConnected(Bundle connectionHint)
        {
            StartListenLocationUpdates();

            var location = LocationServices.FusedLocationApi.GetLastLocation(App.LocationClient);

            TriggerConnected(location);
            OnLocationChanged(location);
        }

        public virtual void OnConnectionSuspended(int cause) {}

        public void OnLocationChanged(Location location)
        {
            var locationChanged = ChangeLocation(location);
            if (locationChanged)
            {
                if (IsSpeedAvailable())
                {
                    Speed = CalculateSpeed();
                }
                LastLocationUpDateTime = DateTime.Now;

                TriggerLocationChanged(Location);
            }
        }

        public event Action<Location> Connected;
        public event Action<Location> LocationChanged;

        private bool ChangeLocation(Location location)
        {
            if (location == null)
            {
                return false;
            }

            PreviousLocation = Location;
            Location = location;

            return true;
        }

        private void StartListenLocationUpdates()
        {
            var locationRequest = new LocationRequest();

            locationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            locationRequest.SetInterval(Constants.LocationUpdateInterval);
            locationRequest.SetFastestInterval(Constants.LocationUpdateFastestInterval);
            locationRequest.SetSmallestDisplacement(Constants.MinimalDisplacement);

            LocationServices.FusedLocationApi.RequestLocationUpdates(App.LocationClient, locationRequest, this);
        }

        private double CalculateSpeed()
        {
            if (!IsSpeedAvailable())
            {
                return 0;
            }

            var distance = PreviousLocation.DistanceTo(Location);
            var time = (DateTime.Now - LastLocationUpDateTime).Value.TotalSeconds;
            var speed = distance/time;

            return speed;
        }

        private bool IsSpeedAvailable()
        {
            return Location != null && PreviousLocation != null && LastLocationUpDateTime != null;
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