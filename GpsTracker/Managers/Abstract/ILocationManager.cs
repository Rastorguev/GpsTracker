using System;
using Android.Gms.Common.Apis;
using Android.Locations;

namespace GpsTracker.Managers.Abstract
{
    public interface ILocationManager: IGoogleApiClientConnectionCallbacks
    {
        Location Location { get; }
        Location PreviousLocation { get; }
        DateTime? LastLocationUpDateTime { get; }
        float? Bearing { get; }
        event Action<Location> Connected;
        event Action<Location> LocationChanged;
    }
}