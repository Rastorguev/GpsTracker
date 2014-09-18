using System;
using System.Collections.Generic;
using Android.Gms.Maps.Model;
using GpsTracker.Entities;

namespace GpsTracker.Tools
{
    internal interface ITrackDrawer: IDisposable
    {
        void DrawTrack(List<TrackPoint> trackPoints);
        void DrawStartPositionMarker(TrackPoint trackPoint);
        void DrawCurrentPositionMarker(TrackPoint trackPoint);
        void RemoveTrack();
    }
}