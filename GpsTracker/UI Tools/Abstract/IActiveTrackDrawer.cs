using System;
using System.Collections.Generic;
using GpsTracker.Entities;

namespace GpsTracker.Abstract
{
    internal interface IActiveTrackDrawer : IDisposable
    {
        void DrawTrack(List<TrackPoint> trackPoints);
        void DrawCurrentPositionMarker(TrackPoint trackPoint);
        void RemoveTrack();
    }
}