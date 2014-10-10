using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using GpsTracker.Bindings.Android;
using GpsTracker.BL.Managers.Abstract;
using GpsTracker.Entities;
using GpsTracker.Services;
using Object = Java.Lang.Object;

namespace GpsTracker.Managers
{
    public class ActiveTrackManager : Object, IServiceConnection
    {
        private static volatile ActiveTrackManager _instance;
        private static readonly object Locker = new System.Object();

        private readonly ITrackHistoryManager _trackHistoryManager = DependencyResolver.Resolve<ITrackHistoryManager>();

        private ActiveTrackService _activeTrackService;
        private bool _isBound;
        private bool _isStarted;

        private ActiveTrackManager() {}

        public static ActiveTrackManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Locker)
                    {
                        if (_instance == null)
                            _instance = new ActiveTrackManager();
                    }
                }

                return _instance;
            }
        }

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
                    ? (DateTime.Now - _activeTrackService.ActiveTrack.StartTime)
                    : new TimeSpan();
            }
        }

        public List<TrackPoint> TrackPoints
        {
            get { return HasActiveTrack ? _activeTrackService.ActiveTrack.TrackPoints : new List<TrackPoint>(); }
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
            var track = _activeTrackService.ActiveTrack;
            track.EndTime = DateTime.Now;

            context.StopService(new Intent(context, typeof (ActiveTrackService)));

            _activeTrackService = null;

            if (_isBound)
            {
                context.UnbindService(this);
            }

            _isStarted = false;

            _trackHistoryManager.SaveTrack(track);
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