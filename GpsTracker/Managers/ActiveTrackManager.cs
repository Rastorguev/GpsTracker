using System;
using System.Collections.Generic;
using System.Linq;
using Android.Gms.Maps.Model;
using GpsTracker.Entities;

namespace GpsTracker.Managers
{
    public class ActiveTrackManager
    {
        private const double MinimalDisplacement = 1;
        private static TrackData _activeTrack;
        private DateTime _startTime;

        private static ActiveTrackManager _instance;
        private static readonly object Locker = new Object();

        private ActiveTrackManager() {}

        public static ActiveTrackManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Locker)
                    {
                        if (_instance == null)
                            _instance = new ActiveTrackManager();
                    }
                }
                return _instance;
            }
        }

        //public TrackData ActiveTrack
        //{
        //    get
        //    {
        //        return new TrackData
        //        {
        //            Distance = _activeTrack.Distance,
        //            Duration = new TimeSpan(_activeTrack.Duration.Ticks) + (DateTime.Now - _startTime),
        //            StartTime = new DateTime(_activeTrack.Duration.Ticks),
        //            TrackPoints = _activeTrack.TrackPoints.Select(p => new LatLng(p.Latitude, p.Longitude)).ToList()
        //        };
        //    }
        //}

        public bool IsStarted { get; private set; }

        public bool HasActiveTrack
        {
            get { return _activeTrack != null; }
        }

        public float Distance
        {
            get { return _activeTrack.Distance; }
        }

        public TimeSpan Duration
        {
            get { return _activeTrack.Duration + (DateTime.Now - _startTime); }
        }

        public List<LatLng> TrackPoints
        {
            get { return _activeTrack.TrackPoints; }
        }

        public float Bearing
        {
            get
            {
                return TrackPoints.Count > 1
                    ? TrackPoints[TrackPoints.Count - 2].ToLocation().BearingTo(TrackPoints.Last().ToLocation())
                    : 0;
            }
        }

        public void StartTrack()
        {
            _startTime = DateTime.Now;

            if (_activeTrack == null)
            {
                _activeTrack = new TrackData(_startTime);
                //GeneratedFakeTrack(100);
            }

            IsStarted = true;
        }

        public void PauseTrack()
        {
            IsStarted = false;
            _activeTrack.Duration += DateTime.Now - _startTime;
        }

        public bool TryAddTrackPoint(LatLng trackPoint)
        {
            var isTrackPointAdded = false;

            if (_activeTrack.TrackPoints.Any())
            {
                var displacement = _activeTrack.TrackPoints.Last().DistanceTo(trackPoint);

                if (displacement >= MinimalDisplacement)
                {
                    _activeTrack.TrackPoints.Add(trackPoint);
                    _activeTrack.Distance += displacement;

                    isTrackPointAdded = true;
                }
            }
            else
            {
                _activeTrack.TrackPoints.Add(trackPoint);

                isTrackPointAdded = true;
            }

            return isTrackPointAdded;
        }

        private void GeneratedFakeTrack(int n)
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

            _activeTrack.TrackPoints = trackPoints;
        }
    }
}