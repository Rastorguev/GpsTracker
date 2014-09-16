using System;
using System.Collections.Generic;
using Android.Gms.Maps.Model;
using Newtonsoft.Json;

namespace GpsTracker.Entities
{
    public class TrackData
    {
        public TrackData(DateTime starTime)
        {
            StartTime = starTime;
            TrackPoints = new List<LatLng>();
            TrackPointsSerializable=new List<LatLngSerializable>();
        }

        public DateTime StartTime { get; set; }
        public float Distance { get; set; }
        public TimeSpan Duration { get; set; }

        [JsonIgnore]
        public List<LatLng> TrackPoints { get; set; }

        public List<LatLngSerializable> TrackPointsSerializable { get; set; }
    }

    public class LatLngSerializable
    {
        public LatLngSerializable(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}