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
        GoogleMap.IOnCameraChangeListener, GoogleMap.IOnMapLoadedCallback
    {
        private GoogleMap _map;

        protected Timer AutoreturnTimer;
        protected ITrackDrawer TrackDrawer;
        protected const float DefaultMapZoom = Constants.DefaultMapZoom;
        protected static float Zoom = DefaultMapZoom;
        protected static LatLng Position;
        protected static float Bearing;
        protected bool MapIsLoaded;

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

            var location = App.LocationListener.Location;

            if (UserConfig.FitTrackToScreen)
            {
                if (MapIsLoaded)
                {
                    FitTrackToScreen();
                }
            }
            else if (location != null)
            {
                MoveCamera(location.ToLatLng(), Zoom);
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

            TrackDrawer.CleanUp();

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

            if (UserConfig.FitTrackToScreen)
            {
                DisableMapControl();
            }
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
            var lastLocation = App.LocationListener.Location;

            if (lastLocation != null)
            {
                DrawTrack();

                if (UserConfig.FitTrackToScreen)
                {
                    FitTrackToScreen(true);
                }
                else
                {
                    MoveCamera(App.LocationListener.Location.ToLatLng(), DefaultMapZoom, true);
                }
            }
        }

        public virtual void LocationListenerOnLocationChanged(Location location)
        {
            var lastLocation = App.LocationListener.Location;

            if (lastLocation != null)
            {
                DrawTrack();

                if (UserConfig.FitTrackToScreen)
                {
                    FitTrackToScreen(true);
                }
            }
        }

        #endregion

        #region IOnCameraChangeListener implementation

        public virtual void OnCameraChange(CameraPosition position)
        {
            Zoom = position.Zoom;
            Bearing = position.Bearing;
            Position = position.Target;

            var lastLocation = App.LocationListener.Location;

            if (UserConfig.Autoreturn && lastLocation != null && !IsTrackPointVisible(lastLocation.ToLatLng()))
            {
                Autoreturn();
            }

            if (UserConfig.FitTrackToScreen)
            {
                FitTrackToScreen();
            }
        }

        #endregion

        #region IOnMapLoadedCallback implementation

        public void OnMapLoaded()
        {
            MapIsLoaded = true;
        }

        #endregion

        #region Camera position methods

        protected void MoveCamera(LatLng trackPoint, float zoom, bool animate = false)
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

        private void FitTrackToScreen(bool animate = false)
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
                var lastLocation = App.LocationListener.Location;
                if (lastLocation != null)
                {
                    MoveCamera(lastLocation.ToLatLng(), Zoom, true);
                }
            });
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

        private bool IsTrackPointVisible(LatLng trackPoint)
        {
            var bounds = Map.Projection.VisibleRegion.LatLngBounds;
            return bounds.Contains(trackPoint);
        }

        #endregion

        private void DisableMapControl()
        {
            Map.UiSettings.ZoomControlsEnabled = false;
            Map.UiSettings.ZoomGesturesEnabled = false;
            Map.UiSettings.ScrollGesturesEnabled = false;
            Map.UiSettings.RotateGesturesEnabled = false;
            Map.UiSettings.TiltGesturesEnabled = false;
        }
    }
}