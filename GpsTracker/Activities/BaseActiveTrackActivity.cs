using System;
using System.Linq;
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
        private const float DefaultMapZoom = Constants.DefaultMapZoom;
        protected static float Zoom = DefaultMapZoom;

        protected GoogleMap Map
        {
            get { return _map ?? (_map = GetMap()); }
        }

        #region Life Circle

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //RestoreSavedState(savedInstanceState);

            SetView();
            InitMap();

            InitTrackDrawer();
            InitAutoreturnTimer();

            SubscribeOnLocationListenerEvents();
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (App.ActiveTrackManager.HasActiveTrack)
            {
                TrackDrawer.DrawTrack(App.ActiveTrackManager.TrackPoints);
                AutoreturnTimer.Elapsed += AutoreturnEventHandler;

                if (App.ActiveTrackManager.TrackPoints.Any())
                {
                    var trackPoint = App.ActiveTrackManager.TrackPoints.Last();

                    MoveCamera(trackPoint, Zoom);
                }
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            TrackDrawer.RemoveTrack();

            AutoreturnTimer.Elapsed -= AutoreturnEventHandler;

            GC.Collect();
        }

        //protected override void OnSaveInstanceState(Bundle outState)
        //{
        //    base.OnSaveInstanceState(outState);

        //    outState.PutFloat("zoom", Zoom);
        //}

        protected override void OnDestroy()
        {
            base.OnDestroy();

            UnsubscribeFromLocationListenerEvents();

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
            if (location != null)
            {
                Zoom = DefaultMapZoom;
                MoveCamera(location.ToLatLng(), DefaultMapZoom, true);
                LocationListenerOnLocationChanged(location);
            }
        }

        public virtual void LocationListenerOnLocationChanged(Location location)
        {
            var trackPoint = App.ActiveTrackManager != null && App.ActiveTrackManager.TrackPoints.Any()
                ? App.ActiveTrackManager.TrackPoints.Last()
                : null;

            if (trackPoint != null)
            {
                TrackDrawer.DrawTrack(App.ActiveTrackManager.TrackPoints);

                if (UserConfig.RotateMapInAccordanceWithTheMovement && IsTrackPointVisible(trackPoint))
                {
                    MoveCamera(trackPoint, Zoom, true);
                }
            }
        }

        #endregion

        #region IOnCameraChangeListener implementation

        public void OnCameraChange(CameraPosition position)
        {
            Zoom = position.Zoom;

            if (App.ActiveTrackManager.HasActiveTrack && App.ActiveTrackManager.TrackPoints.Any())
            {
                var currentPosition = App.ActiveTrackManager.TrackPoints.Last();

                if (UserConfig.Autoreturn && !IsTrackPointVisible(currentPosition))
                {
                    Autoreturn();
                }
            }
        }

        #endregion

        #region Camera position methods

        private void MoveCamera(LatLng trackPoint, float zoom, bool animate = false)
        {
            var builder = CameraPosition.InvokeBuilder();

            builder.Target(trackPoint);
            builder.Zoom(zoom);
            builder.Bearing(App.ActiveTrackManager.Bearing);

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
            var currentPosition = App.ActiveTrackManager.TrackPoints.Last();

            RunOnUiThread(() => MoveCamera(currentPosition, Zoom, true));
        }

        #endregion

        #region Helpers

        //protected virtual void RestoreSavedState(Bundle savedInstanceState)
        //{
        //    if (savedInstanceState != null)
        //    {
        //        Zoom = savedInstanceState.GetFloat("zoom");
        //    }
        //}

        private bool IsTrackPointVisible(LatLng trackPoint)
        {
            var bounds = Map.Projection.VisibleRegion.LatLngBounds;
            return bounds.Contains(trackPoint);
        }

        #endregion
    }
}