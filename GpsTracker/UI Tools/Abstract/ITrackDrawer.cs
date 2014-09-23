using System;
using System.Collections.Generic;
using GpsTracker.Entities;

namespace GpsTracker.Abstract
{
    internal interface ITrackDrawer : IDisposable
    {
        void DrawTrack(List<TrackPoint> trackPoints);
        void DrawStartPositionMarker(TrackPoint trackPoint);
        void DrawCurrentPositionMarker(TrackPoint trackPoint);
        void RemoveTrack();
    }
}