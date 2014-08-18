using System;
using System.Linq;
using System.Timers;
using Android.App;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using GpsTracker.Config;
using GpsTracker.Managers;
using GpsTracker.Tools;
using ILocationListener = Android.Gms.Location.ILocationListener;

namespace GpsTracker.Activities
{
    internal abstract class BaseActiveTrackActivity : Activity, IGoogleApiClientConnectionCallbacks,
        GoogleMap.IOnCameraChangeListener, ILocationListener
    {
        protected IGoogleApiClient LocationClient;
        protected float Zoom = Constants.DefaultZoom;
        protected ITrackDrawer TrackDrawer;
        protected static ActiveTrackManager ActiveTrackManager;
        protected Timer AutoreturnTimer;

        protected GoogleMap Map
        {
            get { return _map ?? (_map = GetMap()); }
        }

        private GoogleMap _map;

        #region Life Circle

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RestoreSavedState(savedInstanceState);

            SetView();

            InitMap();
            InitActiveTrackManager();
            InitLocationClient();
            InitTrackDrawer();
            InitAutoreturnTimer();
        }

        #region Initializtion

        protected abstract void SetView();
        protected abstract GoogleMap GetMap();

        protected virtual void InitMap()
        {
            Map.SetOnCameraChangeListener(this);
        }

        protected virtual void InitActiveTrackManager()
        {
            ActiveTrackManager = ActiveTrackManager.Instance;

            if (!ActiveTrackManager.IsStarted)
            {
                ActiveTrackManager.StartTrack();
            }
        }

        protected virtual void InitLocationClient()
        {
            LocationClient = new GoogleApiClientBuilder(this)
                .AddApi(LocationServices.Api)
                .AddConnectionCallbacks(this)
                .Build();

            LocationClient.Connect();
        }

        protected virtual void InitTrackDrawer()
        {
            TrackDrawer = new TrackDrawer(Map);
        }

        protected virtual void InitAutoreturnTimer()
        {
            AutoreturnTimer = new Timer
            {
                AutoReset = false,
                Interval = Constants.AutoreturnDelay
            };
        }

        #endregion

        protected override void OnStart()
        {
            base.OnStart();

            TrackDrawer.DrawTrack(ActiveTrackManager.TrackPoints);

            AutoreturnTimer.Elapsed += AutoreturnEventHandler;
        }

        protected override void OnPause()
        {
            base.OnPause();

            TrackDrawer.RemoveTrack();

            AutoreturnTimer.Elapsed -= AutoreturnEventHandler;

            GC.Collect();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            outState.PutFloat("zoom", Zoom);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            CleanUpLocationClient();

            GC.Collect(GC.MaxGeneration);
        }

        private void CleanUpLocationClient()
        {
            if (LocationClient != null)
            {
                if (LocationClient.IsConnected)
                {
                    LocationServices.FusedLocationApi.RemoveLocationUpdates(LocationClient, this);
                }

                LocationClient.UnregisterConnectionCallbacks(this);
                LocationClient.Disconnect();
                LocationClient.Dispose();
            }
        }

        #endregion

        #region Location Callbacks

        public virtual void OnConnected(Bundle bundle)
        {
            var locationRequest = new LocationRequest();

            locationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            locationRequest.SetInterval(Constants.LocationUpdateInterval);
            locationRequest.SetFastestInterval(Constants.LocationUpdateFastestInterval);

            LocationServices.FusedLocationApi.RequestLocationUpdates(LocationClient, locationRequest, this);

            var location = LocationServices.FusedLocationApi.GetLastLocation(LocationClient);

            if (location != null)
            {
                MoveCamera(location.ToLatLng());
                OnLocationChanged(location);
            }
        }

        public virtual void OnConnectionSuspended(int cause)
        {
            //Console.WriteLine(cause);
        }

        public virtual void OnLocationChanged(Location location)
        {
            var trackPoint = location.ToLatLng();
            var pointAdded = ActiveTrackManager.TryAddTrackPoint(trackPoint);

            if (pointAdded)
            {
                TrackDrawer.DrawTrack(ActiveTrackManager.TrackPoints);

                if (UserConfig.RotateMapInAccordanceWithTheMovement && IsTrackPointVisible(trackPoint))
                {
                    MoveCamera(trackPoint);
                }
            }
        }

        public void OnCameraChange(CameraPosition position)
        {
            Zoom = position.Zoom;

            var currentPosition = ActiveTrackManager.TrackPoints.Last();

            if (UserConfig.Autoreturn && !IsTrackPointVisible(currentPosition))
            {
                Autoreturn();
            }
        }

        #endregion

        #region Path Display Methods

        private void MoveCamera(LatLng trackPoint)
        {
            var builder = CameraPosition.InvokeBuilder();

            builder.Target(trackPoint);
            builder.Zoom(Zoom);
            builder.Bearing(ActiveTrackManager.Bearing);

            var cameraPosition = builder.Build();
            var cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);

            Map.AnimateCamera(cameraUpdate);
        }

        private void Autoreturn()
        {
            AutoreturnTimer.Stop();
            AutoreturnTimer.Start();
        }

        private void AutoreturnEventHandler(object sender, EventArgs e)
        {
            var currentPosition = ActiveTrackManager.TrackPoints.Last();

            RunOnUiThread(() => MoveCamera(currentPosition));
        }

        #endregion

        #region Helpers

        protected virtual void RestoreSavedState(Bundle savedInstanceState)
        {
            if (savedInstanceState != null)
            {
                Zoom = savedInstanceState.GetFloat("zoom");
            }
        }

        private bool IsTrackPointVisible(LatLng trackPoint)
        {
            var bounds = Map.Projection.VisibleRegion.LatLngBounds;
            return bounds.Contains(trackPoint);
        }

        #endregion
    }
}