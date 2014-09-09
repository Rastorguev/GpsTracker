using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Support.V4.App;
using GpsTracker.Activities;
using GpsTracker.Entities;

namespace GpsTracker.Services
{
    [Service]
    public class ActiveTrackService : Service
    {
        private readonly NotificationManager _notificationManager;
        private DateTime _startTime;

        public TrackData ActiveTrack { get; private set; }

        public void AddTrackPoint(LatLng trackPoint)
        {
            if (ActiveTrack.TrackPoints.Any())
            {
                ActiveTrack.Distance += ActiveTrack.TrackPoints.Last().DistanceTo(trackPoint);
            }

            ActiveTrack.TrackPoints.Add(trackPoint);
        }

        public override IBinder OnBind(Intent intent)
        {
            return new ActiveTrackServiceBinder(this);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            //_notificationManager.Notify((int) NotificationFlags.ForegroundService, GetNotification());

            //var notification = new Notification(Resource.Drawable.Icon, "DemoService in foreground");
            //var pendingIntent = PendingIntent.GetActivity(this, 0, new Intent(this, typeof(MainTrackingActivity)), 0);
            //notification.SetLatestEventInfo(this, "DemoService", "DemoService is running in the foreground", pendingIntent);

            _startTime = DateTime.Now;

            ActiveTrack = new TrackData(_startTime);

            var location = App.LocationListener.Location;
            var startPosition = location != null ? location.ToLatLng() : null;

            AddTrackPoint(startPosition);

            StartForeground((int) NotificationFlags.ForegroundService, GetNotification());

            return StartCommandResult.Sticky;
        }

        public override void OnCreate()
        {
            App.LocationListener.LocationChanged += OnLocationChanged;
        }

        public override void OnDestroy()
        {
            ActiveTrack = null;
            App.LocationListener.LocationChanged -= OnLocationChanged;
        }

        private Notification GetNotification()
        {
            return new NotificationCompat.Builder(this)
                .SetContentTitle("Service started")
                .SetSmallIcon(Resource.Drawable.Icon)
                .SetContentText("!!!Service started!!!")
                .SetContentIntent(PendingIntent.GetActivity(this, 0, new Intent(this, typeof (MainTrackingActivity)), 0))
                .Build();
        }

        public virtual void OnLocationChanged(Location location)
        {
            AddTrackPoint(location.ToLatLng());
        }
    }

    public class ActiveTrackServiceBinder : Binder
    {
        public ActiveTrackService Service { get; private set; }

        public bool IsBound { get; set; }

        public ActiveTrackServiceBinder(ActiveTrackService service)
        {
            Service = service;
        }
    }
}