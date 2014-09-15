using Android.Gms.Maps.Model;
using Android.Locations;

namespace GpsTracker
{
    public static class Extensions
    {
        public static LatLng ToLatLng(this Location location)
        {
            return new LatLng(location.Latitude, location.Longitude);
        }

        public static Location ToLocation(this LatLng latLng)
        {
            return new Location("") {Latitude = latLng.Latitude, Longitude = latLng.Longitude};
        }

        public static float DistanceTo(this LatLng point1, LatLng point2)
        {
            var location1 = point1.ToLocation();
            var location2 = point2.ToLocation();

            var distance = location1.DistanceTo(location2);

            return distance;
        }

        public static float MetersToKilometers(this float meters)
        {
            return meters/1000;
        }

        public static double MetersPerSecondToKilometersPerHour(this float speed)
        {
            return speed*3.6;
        }

        public static string CapitalizeFirst(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            var a = s.ToCharArray();

            a[0] = char.ToUpper(a[0]);

            return new string(a);
        }
    }
}