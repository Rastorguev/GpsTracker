using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.OS;
using GpsTracker.Services;
using Object = Java.Lang.Object;

namespace GpsTracker.Managers
{
    public class ActiveTrackManager : Object, IServiceConnection
    {
        private bool _isStarted;
        private ActiveTrackService _activeTrackService;
        private bool _isBound;

        public bool HasActiveTrack
        {
            get { return _isStarted && _activeTrackService != null && _activeTrackService.ActiveTrack != null; }
        }

        public float Distance
        {
            get { return HasActiveTrack ? _activeTrackService.ActiveTrack.Distance : 0; }
        }

        public TimeSpan Duration
        {
            get
            {
                return HasActiveTrack
                    ? _activeTrackService.ActiveTrack.Duration +
                      (DateTime.Now - _activeTrackService.ActiveTrack.StartTime)
                    : new TimeSpan();
            }
        }

        public List<LatLng> TrackPoints
        {
            get { return HasActiveTrack ? _activeTrackService.ActiveTrack.TrackPoints : new List<LatLng>(); }
        }

        public void Start()
        {
            var context = Application.Context;

            context.StartService(new Intent(context, typeof (ActiveTrackService)));
            context.BindService(new Intent(context, typeof (ActiveTrackService)), this, Bind.AutoCreate);

            _isStarted = true;
        }

        public void Stop()
        {
            var context = Application.Context;

            context.StopService(new Intent(context, typeof (ActiveTrackService)));
            _activeTrackService = null;

            if (_isBound)
            {
                context.UnbindService(this);
            }

            _isStarted = false;
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            var activeTrackServiceBinder = service as ActiveTrackServiceBinder;

            if (activeTrackServiceBinder != null)
            {
                _activeTrackService = activeTrackServiceBinder.Service;
                _isBound = true;
            }
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            _isBound = false;
        }
    }
}