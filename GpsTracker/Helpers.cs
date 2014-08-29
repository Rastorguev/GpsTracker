using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Locations;

namespace GpsTracker
{
    public static class Helpers
    {
        public static bool IsGpsEnabled(Context context)
        {
            var locationManager = (LocationManager) context.GetSystemService(Context.LocationService);
            return locationManager.IsProviderEnabled(LocationManager.GpsProvider);
        }

        public static bool IsGoogleMapsInstalled(Context context)
        {
            try
            {
                context.PackageManager.GetApplicationInfo("com.google.android.apps.maps", 0);
                return true;
            }
            catch (PackageManager.NameNotFoundException e)
            {
                return false;
            }
        }

        public static int GetGooglePlayServicesStatus(Context context)
        {
            return  GooglePlayServicesUtil.IsGooglePlayServicesAvailable(context);
        }
    }
}