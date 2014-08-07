using System;
using System.Collections.Generic;
using System.Linq;
using Android.Gms.Maps.Model;

namespace GpsTracker
{
    internal static class TrackOperations
    {
        public static TrackData AddTrackPoint(TrackData trackData, LatLng trackPoint)
        {
            trackData.TrackPoints.Add(trackPoint);

            if (trackData.TrackPoints.Count > 1)
            {
                var distance = CalculateDistanceBetweenTwoLastPoints(trackData);
                trackData.TotalDistance += distance;
            }

            return trackData;
        }

        private static float CalculateDistanceBetweenTwoLastPoints(TrackData trackData)
        {
            if (trackData.TrackPoints.Count > 1)
            {
                var distance =
                    (trackData.TrackPoints.Last()).DistanceTo(trackData.TrackPoints[trackData.TrackPoints.Count - 2]);

                return distance;
            }

            return 0;
        }

        public static List<List<LatLng>> SplitTrackOnSegments(List<LatLng> trackPoints, int segmentMaxLength)
        {
            const int overlay = 1;
            var expectedSegmentsNumber =
                Math.Ceiling(((decimal) trackPoints.Count/segmentMaxLength));

            var segments = new List<List<LatLng>>();
            var n = 0;

            while (segments.Count < expectedSegmentsNumber)
            {
                if (trackPoints.Count >= segmentMaxLength*(n + 1))
                {
                    var index = n != 0 ? n*segmentMaxLength - overlay : n*segmentMaxLength;
                    var count = n != 0 ? segmentMaxLength + overlay : segmentMaxLength;

                    segments.Add(trackPoints.GetRange(index, count));
                    n++;
                }
                else
                {
                    var index = n != 0 ? n*segmentMaxLength - overlay : n*segmentMaxLength;
                    var count = n != 0
                        ? trackPoints.Count - n*segmentMaxLength + overlay
                        : trackPoints.Count - n*segmentMaxLength;

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