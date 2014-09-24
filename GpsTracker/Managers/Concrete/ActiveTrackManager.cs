using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using GpsTracker.Entities;
using GpsTracker.Managers.Abstract;
using GpsTracker.Repositories.Abstract;
using GpsTracker.Services;
using GpsTracker.Tools;
using Object = Java.Lang.Object;

namespace GpsTracker.Managers.Concrete
{
    public class ActiveTrackManager : Object, IServiceConnection, IActiveTrackManager
    {
        private readonly ITrackRepository _trackRepository = ServiceLocator.Instance.Resolve<ITrackRepository>();
        private ActiveTrackService _activeTrackService;
        private bool _isBound;
        private bool _isStarted;

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

            var s1 = DateTime.Now;
            _trackRepository.Save(track);
            var e1 = DateTime.Now;
            var r1 = e1 - s1;
            Console.WriteLine(String.Format("!!!###!!! Save Time: {0} !!!###!!! ", r1.TotalMilliseconds));

            var s2 = DateTime.Now;
            var savedTracks = _trackRepository.GetAll();
            var e2 = DateTime.Now;
            var r2 = e2 - s2;
            Console.WriteLine(String.Format("!!!###!!! Get Time: {0} !!!###!!! ", r2.TotalMilliseconds));

            var firstTrack = savedTracks.First();
            var s3 = DateTime.Now;
            firstTrack.DecodeTrackPoints();
            var e3 = DateTime.Now;
            var r3 = e3 - s3;
            Console.WriteLine(String.Format("!!!###!!! Deserialize Time: {0} !!!###!!! ", r3.TotalMilliseconds));

            var s = "";
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