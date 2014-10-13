using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using GpsTracker.Entities;
using GpsTracker.Tools;

namespace GpsTracker.Services
{
    [Service]
    public class ActiveTrackService : Service
    {
        private const double MinValuableBearing = 0.5;
        private readonly LocationListener _locationListener = LocationListener.Instance;

        private Track _activeTrack = GlobalStorage.ActiveTrack;

        public override IBinder OnBind(Intent intent)
        {
            return new ActiveTrackServiceBinder(this);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var location = _locationListener.Location;

            //GeneratedFakeTrack(60000);

            UpdateTrackPoints(location);

            StartForeground((int)NotificationFlags.ForegroundService,
                Notifications.GetRecordStartedNotification(this));

            return StartCommandResult.Sticky;
        }

        public override void OnCreate()
        {
            _locationListener.LocationChanged += OnLocationChanged;
        }

        public override void OnDestroy()
        {
            //_activeTrack = GlobalStorage.ActiveTrack = null;

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);

            notificationManager.Notify((int)NotificationFlags.ForegroundService,
                Notifications.GetRecordStopedNotification(this));

            _locationListener.LocationChanged -= OnLocationChanged;
        }

        public virtual void OnLocationChanged(Location location)
        {
            UpdateTrackPoints(location);
        }

        private void UpdateTrackPoints(Location location)
        {
            if (_activeTrack != null)
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
        }

        private void AddNewTrackPoint(Location location)
        {
            if (_activeTrack.TrackPoints.Any())
            {
                _activeTrack.Distance += _activeTrack.TrackPoints.Last().ToLatLng().DistanceTo(location.ToLatLng());
            }

            _activeTrack.TrackPoints.Add(location.ToTrackPoint());
        }

        private void UpdateLastTrackPoint(Location location)
        {
            var lastTrackPoint = _activeTrack.TrackPoints[_activeTrack.TrackPoints.Count - 1];
            var lastButOneTrackPoint = _activeTrack.TrackPoints[_activeTrack.TrackPoints.Count - 2];

            _activeTrack.Distance -= lastButOneTrackPoint.ToLatLng().DistanceTo(lastTrackPoint.ToLatLng());
            _activeTrack.Distance += lastButOneTrackPoint.ToLatLng().DistanceTo(location.ToLatLng());

            _activeTrack.TrackPoints[_activeTrack.TrackPoints.Count - 1] = location.ToTrackPoint();
        }

        private bool NeedToAddNewTrackPoint(Location location)
        {
            return _locationListener.PreviousLocation == null ||
                   _locationListener.PreviousLocation.HasBearing == false ||
                   location.HasBearing == false ||
                   _activeTrack.TrackPoints.Count < 2 ||
                   Math.Abs(_locationListener.PreviousLocation.Bearing - location.Bearing) > MinValuableBearing;
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

            _activeTrack.TrackPoints = ts;
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