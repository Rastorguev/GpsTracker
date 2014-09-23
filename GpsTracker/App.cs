using System;
using Android.App;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Runtime;
using GpsTracker.Config;

namespace GpsTracker
{
    [Application]
    public class App : Application
    {
        private static Application _app;
        private static IGoogleApiClient _locationClient;
        //private static IActiveTrackManager _activeTrackManager;
        private static LocationListener _locationListener;

        public App(IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
        {
            _app = this;
        }

        //public static IActiveTrackManager ActiveTrackManager
        //{
        //    get { return ServiceLocator.Instance.Resolve<IActiveTrackManager>(); }
        //}

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

        public override void OnCreate()
        {
            base.OnCreate();
            ServiceRegistrar.Startup();
        }
    }
}