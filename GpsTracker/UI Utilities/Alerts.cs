using Android.App;
using Android.Content;
using Android.Provider;
using GpsTracker.Activities;

namespace GpsTracker
{
    public static class Alerts
    {
        public static void ShowGpsDisabledAlert(Context context)
        {
            var alert = new AlertDialog.Builder(context);

            alert.SetMessage(Resource.String.gps_disabled_message);

            alert.SetPositiveButton(context.Resources.GetString(Resource.String.settings).CapitalizeFirst(),
                (s, e) => context.StartActivity(new Intent(Settings.ActionLocationSourceSettings)));

            alert.SetNegativeButton(context.Resources.GetString(Resource.String.cancel).CapitalizeFirst(),
                (s, e) => context.StartActivity(typeof (MainActivity)));

            alert.SetCancelable(false);
            alert.Show();
        }
    }
}