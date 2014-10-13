using System;
using Android.App;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Runtime;
using CrittercismAndroid;
using Mindscape.Raygun4Net;
using Xamarin;

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
                var locationListener = LocationListener.Instance;

                return _locationClient ?? (_locationClient = new GoogleApiClientBuilder(_app)
                    .AddApi(LocationServices.Api)
                    .AddConnectionCallbacks(locationListener)
                    .Build());
            }
        }

        public override void OnCreate()
        {
            base.OnCreate();

            //register bug tracking tools
            Insights.Initialize("595cf24ee422382fe108ce17a21a32d19d31cb28", ApplicationContext);
            RaygunClient.Attach("Veutcv+XL4iSn2ND6EgrdA==");
            Crittercism.Init(ApplicationContext, "54229b3383fb797ba9000007");
        }
    }
}