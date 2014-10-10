using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Net;
using Android.Provider;
using GpsTracker.Activities;
using GpsTracker.Tools;

namespace GpsTracker
{
    public static class Utils
    {
        public static void ShowLocationDisabledAlert(Context context)
        {
            var alert = new AlertDialog.Builder(context);

            alert.SetMessage(Resource.String.locationDisabledMessage);

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
                ? context.Resources.GetString(Resource.String.gpServicesOutdated)
                : context.Resources.GetString(Resource.String.gpServicesNotInstalled);

            var alert = new AlertDialog.Builder(context);

            alert.SetMessage(alertMessage);

            alert.SetPositiveButton(context.Resources.GetString(Resource.String.getLatestVersion).CapitalizeFirst(),
                (s, e) => RedirectToGooglePlayServicesDownloadLink(context));

            alert.SetCancelable(false);
            alert.Show();
        }

        public static ProgressDialog CreateProgressDialog(Context context)
        {
            var progressDialog = new ProgressDialog(context);

            progressDialog.SetCancelable(false);
            progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
            progressDialog.SetMessage(Application.Context.Resources.GetString(Resource.String.progressDialogMessage));

            return progressDialog;
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