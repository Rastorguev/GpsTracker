using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Net;
using Android.Provider;
using GpsTracker.Activities;

namespace GpsTracker
{
    public static class Alerts
    {
        public static void ShowLocationDisabledAlert(Context context)
        {
            var alert = new AlertDialog.Builder(context);

            alert.SetMessage(Resource.String.location_disabled_message);

            alert.SetPositiveButton(context.Resources.GetString(Resource.String.settings).CapitalizeFirst(),
                (s, e) => context.StartActivity(new Intent(Settings.ActionLocationSourceSettings)));

            alert.SetNegativeButton(context.Resources.GetString(Resource.String.cancel).CapitalizeFirst(),
                (s, e) => context.StartActivity(typeof (MainActivity)));

            alert.SetCancelable(false);
            alert.Show();
        }

        public static void ShowGooglePlayServicesErrorAlert(Context context, int status)
        {
            var alertMessage = status == ConnectionResult.ServiceVersionUpdateRequired
                ? context.Resources.GetString(Resource.String.gp_services_outdated)
                : context.Resources.GetString(Resource.String.gp_services_not_installed);

            var alert = new AlertDialog.Builder(context);

            alert.SetMessage(alertMessage);

            alert.SetPositiveButton(context.Resources.GetString(Resource.String.get_latest_version).CapitalizeFirst(),
                (s, e) => RedirectToGooglePlayServicesDownloadLink(context));

            alert.SetCancelable(false);
            alert.Show();
        }

        private static void RedirectToGooglePlayServicesDownloadLink(Context context)
        {
            const string googlePlayServicesId = "com.google.android.gms";

            try
            {
                context.StartActivity(new Intent(Intent.ActionView,
                    Uri.Parse("market://details?id=" + googlePlayServicesId)));
            }
            catch (ActivityNotFoundException)
            {
                context.StartActivity(new Intent(Intent.ActionView,
                    Uri.Parse("http://play.google.com/store/apps/details?id=" + googlePlayServicesId)));
            }
        }
    }
}