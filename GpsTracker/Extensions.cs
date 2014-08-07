using Android.Gms.Maps.Model;
using Android.Locations;

namespace GpsTracker
{
    internal static class Extensions
    {
        public static LatLng ToLatLng(this Location location)
        {
            return new LatLng(location.Latitude, location.Longitude);
        }

        public static Location ToLocation(this LatLng latLng)
        {
            return new Location("") { Latitude = latLng.Latitude, Longitude = latLng.Longitude };
        }

        public static float DistanceTo(this LatLng startPoint, LatLng destination)
        {
            var distance = (startPoint.ToLocation()).DistanceTo(destination.ToLocation());

            return distance;
        }

        public static float MetersToKilometers(this float meters)
        {
            return meters / 1000;
        }

        public static float MetersPerSecondToKilometersPerHour(this float speed)
        {
            return speed * (float)3.6;
        }
    }
}