using System;
using System.Collections.Generic;
using Android.Gms.Maps.Model;

namespace GpsTracker.Entities
{
    public class TrackData
    {
        public TrackData(DateTime starTime)
        {
            StartTime = starTime;
            TrackPoints=new List<LatLng>();
        }

        public DateTime StartTime { get; set; }
        public List<LatLng> TrackPoints { get; set; }
        public float Distance { get; set; }
        public TimeSpan Duration { get; set; }
    }
}