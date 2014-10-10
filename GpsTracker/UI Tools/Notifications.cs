using System;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using GpsTracker.Activities;
using GpsTracker.Tools;

namespace GpsTracker
{
    public class Notifications
    {
        private static readonly Type IntentPageType = typeof (MainTrackingActivity);

        public static Notification GetRecordStartedNotification(Context context)
        {
            return ConfigNotificationBasis(context)
                .SetContentText(context.Resources.GetString(Resource.String.recording).CapitalizeFirst())
                .SetTicker(context.Resources.GetString(Resource.String.recording).CapitalizeFirst())
                .SetOngoing(true)
                .Build();
        }

        public static Notification GetRecordStopedNotification(Context context)
        {
            return ConfigNotificationBasis(context)
                .SetContentText(context.Resources.GetString(Resource.String.ended).CapitalizeFirst())
                .SetTicker(context.Resources.GetString(Resource.String.ended).CapitalizeFirst())
                .Build();
        }

        private static NotificationCompat.Builder ConfigNotificationBasis(Context context)
        {
            return new NotificationCompat.Builder(context)
                .SetContentTitle(context.Resources.GetString(Resource.String.app_name).CapitalizeFirst())
                .SetSmallIcon(Resource.Drawable.Bear)
                .SetContentIntent(PendingIntent.GetActivity(context, 0,
                    new Intent(context, IntentPageType), PendingIntentFlags.UpdateCurrent));
        }
    }
}