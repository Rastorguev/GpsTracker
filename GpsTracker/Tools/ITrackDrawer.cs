using System.Collections.Generic;
using Android.Gms.Maps.Model;

namespace GpsTracker
{
    internal interface ITrackDrawer
    {
        void DrawTrack(List<LatLng> trackPoints);
        void RemoveTrack();
    }
}