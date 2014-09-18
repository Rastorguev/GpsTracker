using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GpsTracker.Entities
{
    public class TrackData
    {
        public TrackData(DateTime starTime)
        {
            StartTime = starTime;
            TrackPoints = new List<TrackPoint>();
        }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public float Distance { get; set; }
        public TimeSpan Duration { get; set; }

        [JsonIgnore]
        public List<TrackPoint> TrackPoints { get; set; }

        public string TrackPointsSerialized { get; set; }

        public void DeserializeTrackPoints()
        {
            var serializableTrackPoints = JsonConvert.DeserializeObject<List<TrackPoint>>(TrackPointsSerialized);

            TrackPoints = serializableTrackPoints.Select(p => new TrackPoint(p.Latitude, p.Longitude)).ToList();
        }

        public void SerializeTrackPoints()
        {
            var trackPointsSerializable =
                TrackPoints.Select(p => new TrackPoint(p.Latitude, p.Longitude)).ToList();

            TrackPointsSerialized = JsonConvert.SerializeObject(trackPointsSerializable);
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