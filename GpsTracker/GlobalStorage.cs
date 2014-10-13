using GpsTracker.Entities;

namespace GpsTracker
{
    public static class GlobalStorage
    {
        public static Track Route { get; set; }
        public static Track ActiveTrack { get; set; }
    }
}