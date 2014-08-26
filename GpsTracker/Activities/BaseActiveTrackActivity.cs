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
        GoogleMap.IOnCameraChangeListener, GoogleMap.IOnMapLoadedCallback, GoogleMap.ICancelableCallback
    {
        private GoogleMap _map;

        protected Timer AutoreturnTimer;
        protected ITrackDrawer TrackDrawer;
        protected const float DefaultMapZoom = Constants.DefaultMapZoom;
        protected static float Zoom = DefaultMapZoom;
        protected static LatLng Position;
        protected static float Bearing;
        protected bool MapIsLoaded;
        protected bool FirstOnCameraChangeEventOccured;
        protected LatLngBounds AutoSetMapBounds;

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

            DrawTrack();

            AdjustCamera(Zoom);

            AutoreturnTimer.Elapsed += AutoreturnHandler;
        }

        protected override void OnPause()
        {
            base.OnPause();

            TrackDrawer.RemoveTrack();

            UnsubscribeFromLocationListenerEvents();
            AutoreturnTimer.Elapsed -= AutoreturnHandler;

            GC.Collect();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            TrackDrawer.Dispose();

            GC.Collect(GC.MaxGeneration);
        }

        #endregion

        #region Initializtion

        protected abstract void SetView();
        protected abstract GoogleMap GetMap();

        protected virtual void InitMap()
        {
            Map.SetOnCameraChangeListener(this);
            Map.SetOnMapLoadedCallback(this);
        }

        protected virtual void InitTrackDrawer()
        {
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
            App.LocationListener.Connected += LocationListenerOnConnected;
            App.LocationListener.LocationChanged += LocationListenerOnLocationChanged;
        }

        #endregion

        #region CleanUp1

        protected void UnsubscribeFromLocationListenerEvents()
        {
            App.LocationListener.Connected -= LocationListenerOnConnected;
            App.LocationListener.LocationChanged -= LocationListenerOnLocationChanged;
        }

        #endregion

        #region LocationListener event handlers

        public virtual void LocationListenerOnConnected(Location location)
        {
            DrawTrack();

            AdjustCamera(DefaultMapZoom, true);
        }

        public virtual void LocationListenerOnLocationChanged(Location location)
        {
            DrawTrack();

            if (!AutoreturnTimer.Enabled)
            {
                AdjustCamera(Zoom, true);
            }
        }

        #endregion

        #region IOnCameraChangeListener implementation

        public virtual void OnCameraChange(CameraPosition position)
        {
            if (AutoSetMapBounds == null || !AutoSetMapBounds.Equals(Map.Projection.VisibleRegion.LatLngBounds))
            {
                Zoom = position.Zoom;
                Bearing = position.Bearing;
                Position = position.Target;

                var lastLocation = App.LocationListener.Location;

                if (UserConfig.Autoreturn && lastLocation != null)
                {
                    InitAutoreturn();
                }

                //TODO: Check
                if (UserConfig.FitTrackToScreen && !FirstOnCameraChangeEventOccured)
                {
                    FirstOnCameraChangeEventOccured = true;

                    FitTrackToScreen();
                }
            }
        }

        #endregion

        #region IOnMapLoadedCallback implementation

        public void OnMapLoaded()
        {
            MapIsLoaded = true;
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
            var location = App.LocationListener.Location;

            if (UserConfig.FitTrackToScreen)
            {
                if (MapIsLoaded)
                {
                    FitTrackToScreen(animate);
                }
            }
            else if (location != null)
            {
                MoveCamera(location.ToLatLng(), zoom, animate);
            }
        }

        protected void MoveCamera(LatLng trackPoint, float zoom, bool animate = false)
        {
            var builder = CameraPosition.InvokeBuilder();

            builder.Target(trackPoint);
            builder.Zoom(zoom);
            builder.Bearing(Bearing);

            var cameraPosition = builder.Build();
            var cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);

            SetCameraView(cameraUpdate, animate);
        }

        protected void FitTrackToScreen(bool animate = false)
        {
            var builder = new LatLngBounds.Builder();

            if (App.ActiveTrackManager.HasActiveTrack)
            {
                App.ActiveTrackManager.TrackPoints.ForEach(p => builder.Include(p));
            }
            else if (App.LocationListener.Location != null)
            {
                builder.Include(App.LocationListener.Location.ToLatLng());
            }

            var bounds = builder.Build();
            var cameraUpdate = CameraUpdateFactory.NewLatLngBounds(bounds, Constants.FitTrackToScreenPadding);

            SetCameraView(cameraUpdate, animate);
        }

        protected void SetCameraView(CameraUpdate cameraUpdate, bool animate = false)
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

        protected void InitAutoreturn()
        {
            AutoreturnTimer.Stop();
            AutoreturnTimer.Start();
        }

        protected void AutoreturnHandler(object sender, EventArgs e)
        {
            RunOnUiThread(() => AdjustCamera(Zoom, true));
        }

        #endregion

        #region Track Display

        public void DrawTrack()
        {
            if (App.ActiveTrackManager.HasActiveTrack && App.ActiveTrackManager.TrackPoints.Any())
            {
                TrackDrawer.DrawTrack(App.ActiveTrackManager.TrackPoints);
            }
            else if (App.LocationListener.Location != null)
            {
                TrackDrawer.DrawCurrentPositionMarker(App.LocationListener.Location.ToLatLng());
            }
        }

        #endregion

        #region Helpers

        protected bool IsTrackPointVisible(LatLng trackPoint)
        {
            var bounds = Map.Projection.VisibleRegion.LatLngBounds;
            return bounds.Contains(trackPoint);
        }

        protected void DisableMapControl()
        {
            Map.UiSettings.ZoomControlsEnabled = false;
            Map.UiSettings.ZoomGesturesEnabled = false;
            Map.UiSettings.ScrollGesturesEnabled = false;
            Map.UiSettings.RotateGesturesEnabled = false;
            Map.UiSettings.TiltGesturesEnabled = false;
        }

        #endregion
    }
}