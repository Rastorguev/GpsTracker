using System;
using Android.App;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Runtime;
using GpsTracker.Managers;

namespace GpsTracker
{
    [Application]
    public class App : Application
    {
        private static Application _app;
        private static IGoogleApiClient _locationClient;
        private static ActiveTrackManager _activeTrackManager;
        private static LocationListener _locationListener;

        public static ActiveTrackManager ActiveTrackManager
        {
            get { return _activeTrackManager ?? (_activeTrackManager = new ActiveTrackManager()); }
        }

        public static IGoogleApiClient LocationClient
        {
            get
            {
                return _locationClient ?? (_locationClient = new GoogleApiClientBuilder(_app)
                    .AddApi(LocationServices.Api)
                    .AddConnectionCallbacks(LocationListener)
                    .Build());
            }
        }

        public static LocationListener LocationListener
        {
            get { return _locationListener ?? (_locationListener = new LocationListener()); }
        }

        public App(IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
        {
            _app = this;
        }

        public override void OnCreate()
        {
            base.OnCreate();
        }
    }
}