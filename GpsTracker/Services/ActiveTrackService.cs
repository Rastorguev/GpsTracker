using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using GpsTracker.Entities;
using GpsTracker.Tools;
using LocationManager = GpsTracker.Managers.LocationManager;

namespace GpsTracker.Services
{
    [Service]
    public class ActiveTrackService : Service
    {
        private const double MinValuableBearing = 0.5;
        private readonly LocationManager _locationManager = LocationManager.Instance;
        private DateTime _startTime;

        public Track ActiveTrack { get; private set; }

        public override IBinder OnBind(Intent intent)
        {
            return new ActiveTrackServiceBinder(this);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            _startTime = DateTime.Now;

            ActiveTrack = new Track(_startTime);

            var location = _locationManager.Location;

            //GeneratedFakeTrack(60000);

            UpdateTrackPoints(location);

            StartForeground((int) NotificationFlags.ForegroundService,
                Notifications.GetRecordStartedNotification(this));

            return StartCommandResult.Sticky;
        }

        public override void OnCreate()
        {
            _locationManager.LocationChanged += OnLocationChanged;
        }

        public override void OnDestroy()
        {
            ActiveTrack = null;

            var notificationManager = (NotificationManager) GetSystemService(NotificationService);

            notificationManager.Notify((int) NotificationFlags.ForegroundService,
                Notifications.GetRecordStopedNotification(this));

            _locationManager.LocationChanged -= OnLocationChanged;
        }

        public virtual void OnLocationChanged(Location location)
        {
            UpdateTrackPoints(location);
        }

        private void UpdateTrackPoints(Location location)
        {
            if (NeedToAddNewTrackPoint(location))
            {
                AddNewTrackPoint(location);
            }
            else
            {
                UpdateLastTrackPoint(location);
            }
        }

        private void AddNewTrackPoint(Location location)
        {
            if (ActiveTrack.TrackPoints.Any())
            {
                ActiveTrack.Distance += ActiveTrack.TrackPoints.Last().ToLatLng().DistanceTo(location.ToLatLng());
            }

            ActiveTrack.TrackPoints.Add(location.ToTrackPoint());
        }

        private void UpdateLastTrackPoint(Location location)
        {
            var lastTrackPoint = ActiveTrack.TrackPoints[ActiveTrack.TrackPoints.Count - 1];
            var lastButOneTrackPoint = ActiveTrack.TrackPoints[ActiveTrack.TrackPoints.Count - 2];

            ActiveTrack.Distance -= lastButOneTrackPoint.ToLatLng().DistanceTo(lastTrackPoint.ToLatLng());
            ActiveTrack.Distance += lastButOneTrackPoint.ToLatLng().DistanceTo(location.ToLatLng());

            ActiveTrack.TrackPoints[ActiveTrack.TrackPoints.Count - 1] = location.ToTrackPoint();
        }

        private bool NeedToAddNewTrackPoint(Location location)
        {
            return _locationManager.PreviousLocation == null ||
                   _locationManager.PreviousLocation.HasBearing == false ||
                   location.HasBearing == false ||
                   ActiveTrack.TrackPoints.Count < 2 ||
                   Math.Abs(_locationManager.PreviousLocation.Bearing - location.Bearing) > MinValuableBearing;
        }

        private void GeneratedFakeTrack(int n)
        {
            var random = new Random();
            var lat = 53.926193;
            var ts = new List<TrackPoint>(n);

            for (var i = 0; i < n; i++)
            {
                lat += 0.000008;

                var x = (double) 1/random.Next(-100000, 100000);

                ts.Add(new TrackPoint(lat, 27.689841 + x));
            }

            ActiveTrack.TrackPoints = ts;
        }
    }

    public class ActiveTrackServiceBinder : Binder
    {
        public ActiveTrackServiceBinder(ActiveTrackService service)
        {
            Service = service;
        }

        public ActiveTrackService Service { get; private set; }

        public bool IsBound { get; set; }
    }
}