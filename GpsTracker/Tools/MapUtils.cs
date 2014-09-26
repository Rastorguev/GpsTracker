using System.Collections.Generic;
using System.Linq;
using Android.Gms.Maps.Model;
using GpsTracker.Entities;

namespace GpsTracker.Tools
{
    public static class MapUtils
    {
        public static LatLngBounds CalculateMapBounds(List<TrackPoint> trackPoints)
        {
            if (trackPoints == null || !trackPoints.Any())
            {
                return null;
            }

            var builder = new LatLngBounds.Builder();
            var orderByLatitude = trackPoints.OrderBy(p => p.Latitude);
            var orderByLongitude = trackPoints.OrderBy(p => p.Longitude);

            var latLngs = new List<LatLng>
            {
                orderByLatitude.First().ToLatLng(),
                orderByLatitude.Last().ToLatLng(),
                orderByLongitude.First().ToLatLng(),
                orderByLongitude.Last().ToLatLng()
            };

            latLngs.ForEach(l =>
            {
                builder.Include(l);
                l.Dispose();
            });

            return builder.Build();
        }
    }
}