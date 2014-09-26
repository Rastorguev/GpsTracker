using System.Collections.Generic;
using GpsTracker.Entities;
using Java.Lang;
using StringBuilder = System.Text.StringBuilder;

namespace GpsTracker.Tools
{
    public static class TrackPointsUtils
    {
        public static List<TrackPoint> Decode(string encodedPath)
        {
            var len = encodedPath.Length;

            // For speed we preallocate to an upper bound on the final length, then
            // truncate the array before returning.
            var path = new List<TrackPoint>();
            var index = 0;
            var lat = 0;
            var lng = 0;

            while (index < len)
            {
                var result = 1;
                var shift = 0;
                int b;
                do
                {
                    b = encodedPath[index++] - 63 - 1;
                    result += b << shift;
                    shift += 5;
                } while (b >= 0x1f);
                lat += (result & 1) != 0 ? ~(result >> 1) : (result >> 1);

                result = 1;
                shift = 0;
                do
                {
                    b = encodedPath[index++] - 63 - 1;
                    result += b << shift;
                    shift += 5;
                } while (b >= 0x1f);
                lng += (result & 1) != 0 ? ~(result >> 1) : (result >> 1);

                path.Add(new TrackPoint(lat*1e-5, lng*1e-5));
            }

            return path;
        }

        public static string Encode(List<TrackPoint> path)
        {
            long lastLat = 0;
            long lastLng = 0;

            var result = new StringBuilder();

            foreach (var point in path)
            {
                var lat = Math.Round(point.Latitude*1e5);
                var lng = Math.Round(point.Longitude*1e5);

                var dLat = lat - lastLat;
                var dLng = lng - lastLng;

                Encode(dLat, result);
                Encode(dLng, result);

                lastLat = lat;
                lastLng = lng;
            }

            return result.ToString();
        }

        private static void Encode(long v, StringBuilder result)
        {
            v = v < 0 ? ~(v << 1) : v << 1;
            while (v >= 0x20)
            {
                result.Append(Character.ToChars((int) ((0x20 | (v & 0x1f)) + 63)));
                v >>= 5;
            }
            result.Append(Character.ToChars((int) (v + 63)));
        }
    }
}