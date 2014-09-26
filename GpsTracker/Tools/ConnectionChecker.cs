using Android.Content;
using Android.Locations;
using Android.Provider;

namespace GpsTracker.Tools
{
    public static class ConnectionChecker
    {
        public static bool IsLocationEnabled(Context context)
        {
            return IsGpsEnabled(context) || (IsNetworkConnectionsEnabled(context) && !IsAirplaneModeOn(context));
        }

        public static bool IsGpsEnabled(Context context)
        {
            var locationManager = (LocationManager) context.GetSystemService(Context.LocationService);
            return locationManager.IsProviderEnabled(LocationManager.GpsProvider);
        }

        public static bool IsNetworkConnectionsEnabled(Context context)
        {
            var locationManager = (LocationManager) context.GetSystemService(Context.LocationService);
            return locationManager.IsProviderEnabled(LocationManager.NetworkProvider);
        }

        public static bool IsAirplaneModeOn(Context context)
        {
            var airplaneSetting =
                Settings.System.GetInt(context.ContentResolver, Settings.Global.AirplaneModeOn, 0);
            return airplaneSetting != 0;
        }
    }
}