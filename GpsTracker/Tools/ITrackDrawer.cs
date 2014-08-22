using System.Collections.Generic;
using Android.Gms.Maps.Model;

namespace GpsTracker.Tools
{
    internal interface ITrackDrawer
    {
        void DrawTrack(List<LatLng> trackPoints);
        void DrawStartPositionMarker(LatLng trackPoint);
        void DrawCurrentPositionMarker(LatLng trackPoint);
        void RemoveTrack();
    }
}