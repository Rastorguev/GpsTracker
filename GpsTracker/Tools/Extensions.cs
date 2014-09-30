using Android.Gms.Maps.Model;
using Android.Locations;
using GpsTracker.Entities;

namespace GpsTracker.Tools
{
    public static class Extensions
    {
        public static Location ToLocation(this LatLng source)
        {
            return new Location("") {Latitude = source.Latitude, Longitude = source.Longitude};
        }

        public static Location ToLocation(this TrackPoint source)
        {
            return new Location("") { Latitude = source.Latitude, Longitude = source.Longitude };
        }

        public static TrackPoint ToTrackPoint(this Location source)
        {
            return new TrackPoint(source.Latitude, source.Longitude);
        }

        public static TrackPoint ToTrackPoint(this LatLng source)
        {
            return new TrackPoint(source.Latitude, source.Longitude);
        }

        public static LatLng ToLatLng(this Location source)
        {
            return new LatLng(source.Latitude, source.Longitude);
        }

        public static LatLng ToLatLng(this TrackPoint source)
        {
            return new LatLng(source.Latitude, source.Longitude);
        }

        public static float DistanceTo(this LatLng point1, LatLng point2)
        {
            var location1 = point1.ToLocation();
            var location2 = point2.ToLocation();

            var distance = location1.DistanceTo(location2);

            return distance;
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