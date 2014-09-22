using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using GpsTracker.Entities;

namespace GpsTracker.Services
{
    [Service]
    public class ActiveTrackService : Service
    {
        private DateTime _startTime;

        public TrackData ActiveTrack { get; private set; }

        public void AddTrackPoint(LatLng trackPoint)
        {
            if (ActiveTrack.TrackPoints.Any())
            {
                ActiveTrack.Distance += ActiveTrack.TrackPoints.Last().ToLatLng().DistanceTo(trackPoint);
            }

            ActiveTrack.TrackPoints.Add(trackPoint.ToTrackPoint());
        }

        public override IBinder OnBind(Intent intent)
        {
            return new ActiveTrackServiceBinder(this);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            _startTime = DateTime.Now;

            ActiveTrack = new TrackData(_startTime);

            var location = App.LocationListener.Location;
            var startPosition = location != null ? location.ToLatLng() : null;

            //GeneratedFakeTrack(60000);

            AddTrackPoint(startPosition);
            
            StartForeground((int) NotificationFlags.ForegroundService,
                TrackRecordingNotifications.GetRecordStartedNotification(this));

            return StartCommandResult.Sticky;
        }

        public override void OnCreate()
        {
            App.LocationListener.LocationChanged += OnLocationChanged;
        }

        public override void OnDestroy()
        {
            ActiveTrack = null;

            var notificationManager = (NotificationManager) GetSystemService(NotificationService);

            notificationManager.Notify((int) NotificationFlags.ForegroundService,
                TrackRecordingNotifications.GetRecordStopedNotification(this));

            App.LocationListener.LocationChanged -= OnLocationChanged;
        }

        public virtual void OnLocationChanged(Location location)
        {
            AddTrackPoint(location.ToLatLng());
        }

        private void GeneratedFakeTrack(int n)
        {
            var random = new Random();
            var lat = 53.926193;
            var ts = new List<TrackPoint>(n);

            for (var i = 0; i < n; i++)
            {
                lat += 0.000008;

                var x = (double)1 / random.Next(-100000, 100000);

                ts.Add(new TrackPoint(lat, 27.689841 + x));
            }

            ActiveTrack.TrackPoints = ts;
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