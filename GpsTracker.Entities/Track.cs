using System;
using System.Collections.Generic;
//using GpsTracker.Tools;
using GpsTracker.Entities.Tools;
using Newtonsoft.Json;

namespace GpsTracker.Entities
{
    public class Track
    {
        public Track() : this(DateTime.Now) { }

        public Track(DateTime starTime)
        {
            StartTime = starTime;
            TrackPoints = new List<TrackPoint>();
        }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public float Distance { get; set; }

        [JsonIgnore]
        public TimeSpan Duration
        {
            get { return EndTime - StartTime; }
        }

        [JsonIgnore]
        public float AvgSpeed
        {
            get { return (float)(Distance / Duration.TotalSeconds); }
        }

        [JsonIgnore]
        public List<TrackPoint> TrackPoints { get; set; }

        public string TrackPointsEncoded { get; set; }

        public void DecodeTrackPoints()
        {
            TrackPoints = TrackPointsUtils.Decode(TrackPointsEncoded);
        }

        public void EncodeTrackPoints()
        {
            TrackPointsEncoded = TrackPointsUtils.Encode(TrackPoints);
        }
    }

    public class TrackPoint
    {
        public TrackPoint(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}