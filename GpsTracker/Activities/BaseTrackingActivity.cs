using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using GpsTracker.Abstract;
using GpsTracker.Concrete;
using GpsTracker.Config;
using GpsTracker.Entities;
using GpsTracker.Managers.Abstract;
using GpsTracker.Tools;

namespace GpsTracker.Activities
{
    internal abstract class BaseTrackingActivity : Activity,
        GoogleMap.IOnCameraChangeListener, GoogleMap.ICancelableCallback
    {
        protected const float DefaultMapZoom = Constants.DefaultMapZoom;
        protected static float Zoom = DefaultMapZoom;
        protected static float Bearing;

        protected readonly IActiveTrackManager ActiveTrackManager =
            ServiceLocator.Instance.Resolve<IActiveTrackManager>();

        protected readonly ILocationManager LocationManager = ServiceLocator.Instance.Resolve<ILocationManager>();
        private readonly Track _route = GlobalStorage.Track;
        protected IActiveTrackDrawer ActiveTrackDrawer;

        protected LatLngBounds AutoSetMapBounds;
        protected Timer AutoreturnTimer;
        protected bool FirstOnCameraChangeEventOccured;
        protected ITrackDrawer TrackDrawer;
        private GoogleMap _map;

        protected GoogleMap Map
        {
            get { return _map ?? (_map = GetMap()); }
        }

        #region Life Circle

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetView();
            InitMap();
            InitTrackDrawer();
            InitAutoreturnTimer();
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (!ConnectionChecker.IsLocationEnabled(this))
            {
                Utils.ShowLocationDisabledAlert(this);

                return;
            }

            if (!App.LocationClient.IsConnected)
            {
                App.LocationClient.Connect();
            }

            SubscribeOnLocationListenerEvents();

            ShowRoute();
            ShowActiveTrack();

            if (FirstOnCameraChangeEventOccured)
            {
                AdjustCamera(Zoom);
            }

            AutoreturnTimer.Elapsed += AutoreturnHandler;
        }

        protected override void OnPause()
        {
            base.OnPause();

            ActiveTrackDrawer.RemoveTrack();

            UnsubscribeFromLocationListenerEvents();
            AutoreturnTimer.Elapsed -= AutoreturnHandler;

            GC.Collect();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ActiveTrackDrawer.Dispose();

            GC.Collect(GC.MaxGeneration);
        }

        protected abstract void SetView();
        protected abstract GoogleMap GetMap();

        protected virtual void InitMap()
        {
            Map.SetOnCameraChangeListener(this);
        }

        protected virtual void InitTrackDrawer()
        {
            ActiveTrackDrawer = new ActiveTrackDrawer(Map, this);
            TrackDrawer = new TrackDrawer(Map, this);
        }

        protected virtual void InitAutoreturnTimer()
        {
            AutoreturnTimer = new Timer
            {
                AutoReset = false,
                Interval = Constants.AutoreturnDelay
            };
        }

        protected void SubscribeOnLocationListenerEvents()
        {
            LocationManager.Connected += LocationListenerOnConnected;
            LocationManager.LocationChanged += LocationListenerOnLocationChanged;
        }

        protected void UnsubscribeFromLocationListenerEvents()
        {
            LocationManager.Connected -= LocationListenerOnConnected;
            LocationManager.LocationChanged -= LocationListenerOnLocationChanged;
        }

        #endregion

        #region LocationManager event handlers

        protected virtual void LocationListenerOnConnected(Location location)
        {
            ShowActiveTrack();

            AdjustCamera(DefaultMapZoom, true);
        }

        protected virtual void LocationListenerOnLocationChanged(Location location)
        {
            ShowActiveTrack();

            if (!AutoreturnTimer.Enabled && FirstOnCameraChangeEventOccured)
            {
                AdjustCamera(Zoom, true);
            }
        }

        #endregion

        #region IOnCameraChangeListener implementation

        public virtual void OnCameraChange(CameraPosition position)
        {
            if (FirstOnCameraChangeEventOccured &&
                (AutoSetMapBounds == null || !AutoSetMapBounds.Equals(Map.Projection.VisibleRegion.LatLngBounds)))
            {
                Zoom = position.Zoom;
                Bearing = position.Bearing;

                var location = LocationManager.Location;

                if (UserConfig.Autoreturn && location != null)
                {
                    InitAutoreturn();
                }
            }

            if (!FirstOnCameraChangeEventOccured)
            {
                FirstOnCameraChangeEventOccured = true;

                AdjustCamera(Zoom);
            }
        }

        #endregion

        #region ICancelableCallback implementation

        public void OnCancel()
        {
            AutoSetMapBounds = Map.Projection.VisibleRegion.LatLngBounds;
        }

        public void OnFinish()
        {
            AutoSetMapBounds = Map.Projection.VisibleRegion.LatLngBounds;
        }

        #endregion

        #region Camera position methods

        protected void AdjustCamera(float zoom, bool animate = false)
        {
            var location = LocationManager.Location;

            if (UserConfig.FitTrackToScreen && (ActiveTrackManager.HasActiveTrack || _route != null))
            {
                FitTrackToScreen(animate);
            }
            else if (location != null)
            {
                MoveCamera(location, zoom, animate);
            }
        }

        private void MoveCamera(Location location, float zoom, bool animate = false)
        {
            var builder = CameraPosition.InvokeBuilder();

            builder.Target(location.ToLatLng());
            builder.Zoom(zoom);
            builder.Bearing(Bearing);

            var cameraPosition = builder.Build();
            var cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);

            SetCameraView(cameraUpdate, animate);
        }

        private void FitTrackToScreen(bool animate = false)
        {
            Task.Run(() =>
            {
                var points = new List<TrackPoint>();

                if (ActiveTrackManager.HasActiveTrack)
                {
                    points.AddRange(ActiveTrackManager.TrackPoints);
                }

                if (_route != null)
                {
                    points.AddRange(_route.TrackPoints);
                }

                var bounds = MapUtils.CalculateMapBounds(points);
                var cameraUpdate = CameraUpdateFactory.NewLatLngBounds(bounds, Constants.FitTrackToScreenPadding);

                RunOnUiThread(() => SetCameraView(cameraUpdate, animate));
            });
        }

        private void SetCameraView(CameraUpdate cameraUpdate, bool animate = false)
        {
            if (animate)
            {
                Map.AnimateCamera(cameraUpdate, this);
            }
            else
            {
                Map.MoveCamera(cameraUpdate);
                AutoSetMapBounds = Map.Projection.VisibleRegion.LatLngBounds;
            }
        }

        private void InitAutoreturn()
        {
            AutoreturnTimer.Stop();
            AutoreturnTimer.Start();
        }

        private void AutoreturnHandler(object sender, EventArgs e)
        {
            RunOnUiThread(() => AdjustCamera(Zoom, true));
        }

        #endregion

        #region Location Display

        protected void ShowActiveTrack()
        {
            if (ActiveTrackManager.HasActiveTrack && ActiveTrackManager.TrackPoints.Any())
            {
                ActiveTrackDrawer.DrawTrack(ActiveTrackManager.TrackPoints);
            }
            else if (LocationManager.Location != null)
            {
                ActiveTrackDrawer.DrawCurrentPositionMarker(LocationManager.Location.ToTrackPoint());
            }
        }

        protected void ShowRoute()
        {
            if (_route != null)
            {
                TrackDrawer.DrawTrack(_route.TrackPoints);
            }
        }

        #endregion
    }
}