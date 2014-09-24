using System;
using Android.App;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Runtime;
using CrittercismAndroid;
using GpsTracker.Config;
using GpsTracker.Managers.Abstract;
using GpsTracker.Tools;
using Mindscape.Raygun4Net;

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

            //register bug tracking tools
            RaygunClient.Attach("Veutcv+XL4iSn2ND6EgrdA==");
            Crittercism.Init(ApplicationContext, "54229b3383fb797ba9000007");
        }
    }
}