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

        public bool IsStarted { get; private set; }

        public bool HasActiveTrack
        {
            get { return _activeTrack != null; }
        }

        public float Distance
        {
            get { return HasActiveTrack ? _activeTrack.Distance : 0; }
        }

        public TimeSpan Duration
        {
            get { return HasActiveTrack ? _activeTrack.Duration + (DateTime.Now - _startTime) : new TimeSpan(); }
        }

        public List<LatLng> TrackPoints
        {
            get { return HasActiveTrack ? _activeTrack.TrackPoints : new List<LatLng>(); }
        }

        public void StartTrack()
        {
            _startTime = DateTime.Now;

            if (_activeTrack == null)
            {
                _activeTrack = new TrackData(_startTime);
            }
            //GeneratedFakeTrack(5000);

            IsStarted = true;
        }

        public void PauseTrack()
        {
            IsStarted = false;
            _activeTrack.Duration += DateTime.Now - _startTime;
        }

        public void StopTrack()
        {
            _activeTrack = null;
        }

        public bool TryAddTrackPoint(LatLng trackPoint)
        {
            var isTrackPointAdded = false;

            if (HasActiveTrack)
            {
                if (_activeTrack.TrackPoints.Any())
                {
                    if (!trackPoint.Equals(_activeTrack.TrackPoints.Last()))
                    {
                        if (_activeTrack.TrackPoints.Count > 1)
                        {
                            var lastButOneTrackPoint = _activeTrack.TrackPoints[_activeTrack.TrackPoints.Count - 2];
                            var displacement = lastButOneTrackPoint.DistanceTo(_activeTrack.TrackPoints.Last());

                            if (displacement < MinimalDisplacement)
                            {
                                _activeTrack.Distance -= displacement;
                                _activeTrack.TrackPoints.Remove(_activeTrack.TrackPoints.Last());
                            }

                            if (_activeTrack.Distance < 0)
                            {
                                Console.WriteLine();
                            }
                          
                        }

                        _activeTrack.Distance += _activeTrack.TrackPoints.Last().DistanceTo(trackPoint);
                        _activeTrack.TrackPoints.Add(trackPoint);

                        isTrackPointAdded = true;
  
                    }
                }
                else
                {
                    _activeTrack.TrackPoints.Add(trackPoint);
                    isTrackPointAdded = true;
                }
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