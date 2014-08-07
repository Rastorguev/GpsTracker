using System;
using System.Collections.Generic;
using System.Linq;
using Android.Gms.Maps.Model;

namespace GpsTracker
{
    internal static class TrackOperations
    {
        private const int MinimalDisplacement = 3;
        private const int SegmentMaxLength = 500;

        public static bool TryAddTrackPoint(TrackData trackData, LatLng trackPoint)
        {
            var trackPoints = trackData.TrackPoints;

            if (trackPoints.Any())
            {
                var pointsAreEqual = trackPoints.Last().Equals(trackPoint);

                if (!pointsAreEqual)
                {
                    if (trackPoints.Count > 1)
                    {
                        var distanceBetweenTwoLastPoints =
                            trackPoints.Last().DistanceTo(trackPoints[trackPoints.Count - 2]);

                        if (distanceBetweenTwoLastPoints < MinimalDisplacement)
                        {
                            trackPoints.Remove(trackPoints.Last());
                            trackData.TotalDistance -= distanceBetweenTwoLastPoints;
                        }
                    }

                    var displacement = trackPoints.Last().DistanceTo(trackPoint);

                    trackData.TrackPoints.Add(trackPoint);
                    trackData.TotalDistance += displacement;

                    return true;
                }
            }
            else
            {
                trackData.TrackPoints.Add(trackPoint);

                return true;
            }

            return false;
        }

        public static List<List<LatLng>> SplitTrackOnSegments(List<LatLng> trackPoints)
        {
            const int overlay = 1;
            var expectedSegmentsNumber =
                Math.Ceiling(((decimal) trackPoints.Count/SegmentMaxLength));

            var segments = new List<List<LatLng>>();
            var n = 0;

            while (segments.Count < expectedSegmentsNumber)
            {
                if (trackPoints.Count >= SegmentMaxLength*(n + 1))
                {
                    var index = n != 0 ? n*SegmentMaxLength - overlay : n*SegmentMaxLength;
                    var count = n != 0 ? SegmentMaxLength + overlay : SegmentMaxLength;

                    segments.Add(trackPoints.GetRange(index, count));
                    n++;
                }
                else
                {
                    var index = n != 0 ? n*SegmentMaxLength - overlay : n*SegmentMaxLength;
                    var count = n != 0
                        ? trackPoints.Count - n*SegmentMaxLength + overlay
                        : trackPoints.Count - n*SegmentMaxLength;

                    segments.Add(trackPoints.GetRange(index, count));
                }
            }

            return segments;
        }

        public static List<LatLng> GeneratedFakeTrack(int n)
        {
            var random = new Random();
            var lat = 53.926193;
            var trackPoints = new List<LatLng>();

            for (var i = 0; i < n; i++)
            {
                lat += 0.000008;

                var x = (double) 1/random.Next(-100000, 100000);

                trackPoints.Add(new LatLng(lat, 27.689841 + x));
            }

            return trackPoints;
        }
    }
}