using System;
using System.Collections.Generic;
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
        protected const float DefaultMapZoom = Constants.DefaultMapZoom;
        protected static float Zoom = DefaultMapZoom;

        protected LatLng CurrentPosition
        {
            get
            {
                var lastLocation = App.LocationListener.LastLocation;

                var currentPosition = lastLocation != null
                    ? lastLocation.ToLatLng()
                    : null;

                return currentPosition;
            }
        }

        protected static float _bearing = 0;

        public float Bearing
        {
            get
            {
                var bearing = _bearing;

                if (App.ActiveTrackManager.TrackPoints.Count > 1 && UserConfig.RotateMapInAccordanceWithTheMovement)
                {
                    var lastButOneLocation =
                        App.ActiveTrackManager.TrackPoints[App.ActiveTrackManager.TrackPoints.Count - 2].ToLocation();
                    var lastLocation = App.ActiveTrackManager.TrackPoints.Last().ToLocation();

                    bearing = lastButOneLocation.BearingTo(lastLocation);
                }

                return bearing;
            }

            set { _bearing = value; }
        }

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

            if (CurrentPosition != null)
            {
                DrawTrack();
                MoveCamera(CurrentPosition, Zoom);
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
            Zoom = DefaultMapZoom;

            if (CurrentPosition != null)
            {
                DrawTrack();
                MoveCamera(CurrentPosition, Zoom, true);
            }
        }

        public virtual void LocationListenerOnLocationChanged(Location location)
        {
            if (CurrentPosition != null)
            {
                DrawTrack();

                if (IsTrackPointVisible(CurrentPosition))
                {
                    MoveCamera(CurrentPosition, Zoom, true);
                }
            }
        }

        #endregion

        #region IOnCameraChangeListener implementation

        public void OnCameraChange(CameraPosition position)
        {
            Zoom = position.Zoom;
            Bearing = position.Bearing;

            if (UserConfig.Autoreturn && !IsTrackPointVisible(CurrentPosition))
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
            RunOnUiThread(() => MoveCamera(CurrentPosition, Zoom, true));
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
                trackPoints = CurrentPosition != null ? new List<LatLng> {CurrentPosition} : new List<LatLng>();
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