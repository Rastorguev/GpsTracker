using System.Collections.Generic;
using System.Linq;
using Android.Gms.Maps.Model;
using MapsTest_1;

namespace GpsTracker
{
    public class TrackDataStorage
    {
        private readonly List<LatLng> _trackPoints = new List<LatLng>();


        public List<LatLng> TrackPoints
        {
            get { return _trackPoints ?? new List<LatLng>(); }
        }

        public float TotalDistance { get; private set; }
        public double CurrentSpeed { get; private set; }

        public void AddTrackPoint(LatLng trackPoint)
        {
            TrackPoints.Add(trackPoint);

            if (TrackPoints.Count > 1)
            {
                var distance = CalculateDistanceBetweenTwoLastPoints();
                TotalDistance += distance;
            }
        }


        private float CalculateDistanceBetweenTwoLastPoints()
        {
            if (TrackPoints.Count > 1)
            {
                var distance = (_trackPoints.Last()).DistanceTo(_trackPoints[TrackPoints.Count - 2]);

                return distance;
            }

            return 0;
        }
    }
}