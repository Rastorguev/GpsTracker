using System;
using System.Collections.Generic;
using GpsTracker.Entities;

namespace GpsTracker.Managers.Abstract
{
    public interface IActiveTrackManager
    {
        bool HasActiveTrack { get; }
        float Distance { get; }
        TimeSpan Duration { get; }
        List<TrackPoint> TrackPoints { get; }
        void Start();
        void Stop();
    }
}