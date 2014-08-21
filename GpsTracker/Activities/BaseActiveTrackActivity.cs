using System;
using System.Collections.Generic;
using System.Timers;
using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using GpsTracker.Config;
using GpsTracker.Tools;

namespace GpsTracker.Activities
{
    internal abstract class BaseActiveTrackActivity : Activity,
        GoogleMap.IOnCameraChangeListener
    {
        private GoogleMap _map;

        protected Timer AutoreturnTimer;
        protected ITrackDrawer TrackDrawer;
        protected const float DefaultMapZoom = Constants.DefaultMapZoom;
        protected static float Zoom = DefaultMapZoom;
        protected static LatLng Position;
        protected static float Bearing;

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

        protected override void OnStart()
        {
            base.OnStart();

            SubscribeOnLocationListenerEvents();

            if (!App.LocationClient.IsConnected)
            {
                App.LocationClient.Connect();
            }

            var lastLocation = App.LocationListener.LastLocation;

            if (lastLocation != null)
            {
                DrawTrack();
                MoveCamera(lastLocation.ToLatLng(), Zoom);
            }

            AutoreturnTimer.Elapsed += AutoreturnEventHandler;
        }

        protected override void OnPause()
        {
            base.OnPause();

            TrackDrawer.RemoveTrack();

            UnsubscribeFromLocationListenerEvents();
            AutoreturnTimer.Elapsed -= AutoreturnEventHandler;

            GC.Collect();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            GC.Collect(GC.MaxGeneration);
        }

        #endregion

        #region Initializtion

        protected abstract void SetView();
        protected abstract GoogleMap GetMap();

        protected virtual void InitMap()
        {
            Map.SetOnCameraChangeListener(this);
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

        private void SubscribeOnLocationListenerEvents()
        {
            App.LocationListener.Connected += LocationListenerOnConnected;
            App.LocationListener.LocationChanged += LocationListenerOnLocationChanged;
        }

        #endregion

        #region CleanUp

        private void UnsubscribeFromLocationListenerEvents()
        {
            App.LocationListener.Connected -= LocationListenerOnConnected;
            App.LocationListener.LocationChanged -= LocationListenerOnLocationChanged;
        }

        #endregion

        #region LocationListener event handlers

        public virtual void LocationListenerOnConnected(Location location)
        {
            var lastLocation = App.LocationListener.LastLocation;

            if (lastLocation != null)
            {
                DrawTrack();
                MoveCamera(lastLocation.ToLatLng(), DefaultMapZoom, true);
            }
        }

        public virtual void LocationListenerOnLocationChanged(Location location)
        {
            var lastLocation = App.LocationListener.LastLocation;
            if (lastLocation != null)
            {
                DrawTrack();
            }
        }

        #endregion

        #region IOnCameraChangeListener implementation

        public void OnCameraChange(CameraPosition position)
        {
            Zoom = position.Zoom;
            Bearing = position.Bearing;
            Position = position.Target;

            var lastLocation = App.LocationListener.LastLocation;

            if (UserConfig.Autoreturn && lastLocation != null && !IsTrackPointVisible(lastLocation.ToLatLng()))
            {
                Autoreturn();
            }
        }

        #endregion

        #region Camera position methods

        private void MoveCamera(LatLng trackPoint, float zoom, bool animate = false)
        {
            var builder = CameraPosition.InvokeBuilder();

            builder.Target(trackPoint);
            builder.Zoom(zoom);
            builder.Bearing(Bearing);

            var cameraPosition = builder.Build();
            var cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);

            if (animate)
            {
                Map.AnimateCamera(cameraUpdate);
            }
            else
            {
                Map.MoveCamera(cameraUpdate);
            }
        }

        private void Autoreturn()
        {
            AutoreturnTimer.Stop();
            AutoreturnTimer.Start();
        }

        private void AutoreturnEventHandler(object sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {
                var lastLocation = App.LocationListener.LastLocation;
                if (lastLocation != null)
                {
                    MoveCamera(lastLocation.ToLatLng(), Zoom, true);
                }
            });
        }

        #endregion

        #region Helpers

        private void DrawTrack()
        {
            List<LatLng> trackPoints;

            if (App.ActiveTrackManager.HasActiveTrack)
            {
                trackPoints = App.ActiveTrackManager.TrackPoints;
            }
            else
            {
                var lastLocation = App.LocationListener.LastLocation;

                trackPoints = lastLocation != null ? new List<LatLng> { lastLocation.ToLatLng() } : new List<LatLng>();
            }

            TrackDrawer.DrawTrack(trackPoints);
        }

        private bool IsTrackPointVisible(LatLng trackPoint)
        {
            var bounds = Map.Projection.VisibleRegion.LatLngBounds;
            return bounds.Contains(trackPoint);
        }

        #endregion
    }
}