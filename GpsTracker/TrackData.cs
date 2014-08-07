using System.Collections.Generic;
using Android.Gms.Maps.Model;

namespace GpsTracker
{
    internal class TrackData
    {
        private readonly List<LatLng> _trackPoints = new List<LatLng>();


        public List<LatLng> TrackPoints
        {
            get { return _trackPoints ?? new List<LatLng>(); }
        }

        public float TotalDistance { get; set; }
    }
}