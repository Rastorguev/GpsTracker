using System;
using Android.App;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Runtime;
using GpsTracker.Config;
using GpsTracker.Managers.Abstract;
using GpsTracker.Tools;

namespace GpsTracker
{
    [Application]
    public class App : Application
    {
        private static Application _app;
        private static IGoogleApiClient _locationClient;

        public App(IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
        {
            _app = this;
        }

        public static IGoogleApiClient LocationClient
        {
            get
            {
                var locationManager = ServiceLocator.Instance.Resolve<ILocationManager>();

                return _locationClient ?? (_locationClient = new GoogleApiClientBuilder(_app)
                    .AddApi(LocationServices.Api)
                    .AddConnectionCallbacks(locationManager)
                    .Build());
            }
        }

        public override void OnCreate()
        {
            base.OnCreate();
            ServiceRegistrar.Startup();
        }
    }
}