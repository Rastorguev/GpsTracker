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
using GpsTracker.Tools;

namespace GpsTracker.Activities
{
    internal abstract class BaseActiveTrackActivity : Activity,
        GoogleMap.IOnCameraChangeListener, GoogleMap.ICancelableCallback
    {
        protected const float DefaultMapZoom = Constants.DefaultMapZoom;
        protected static float Zoom = DefaultMapZoom;
        protected static float Bearing;
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

            if (!Helpers.IsLocationEnabled(this))
            {
                Alerts.ShowLocationDisabledAlert(this);

                return;
            }

            if (!App.LocationClient.IsConnected)
            {
                App.LocationClient.Connect();
            }

            SubscribeOnLocationListenerEvents();

            ShowLocationChanges();

            if (FirstOnCameraChangeEventOccured)
            {
                AdjustCamera(Zoom);
            }

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

        protected abstract void SetView();
        protected abstract GoogleMap GetMap();

        protected virtual void InitMap()
        {
            Map.SetOnCameraChangeListener(this);
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

        protected void UnsubscribeFromLocationListenerEvents()
        {
            App.LocationListener.Connected -= LocationListenerOnConnected;
            App.LocationListener.LocationChanged -= LocationListenerOnLocationChanged;
        }

        #endregion

        #region LocationListener event handlers

        public virtual void LocationListenerOnConnected(Location location)
        {
            ShowLocationChanges();

            AdjustCamera(DefaultMapZoom, true);
        }

        public virtual void LocationListenerOnLocationChanged(Location location)
        {
            ShowLocationChanges();

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

                var location = App.LocationListener.Location;

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
            var location = App.LocationListener.Location;

            if (UserConfig.FitTrackToScreen)
            {
                FitTrackToScreen(animate);
            }
            else if (location != null)
            {
                MoveCamera(location, zoom, animate);
            }
        }

        protected void MoveCamera(Location location, float zoom, bool animate = false)
        {
            var builder = CameraPosition.InvokeBuilder();

            builder.Target(location.ToLatLng());
            builder.Zoom(zoom);
            builder.Bearing(Bearing);

            var cameraPosition = builder.Build();
            var cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);

            SetCameraView(cameraUpdate, animate);
        }

        protected void FitTrackToScreen(bool animate = false)
        {
            Task.Run(() =>
            {
                if (App.LocationListener.Location == null)
                {
                    return;
                }

                var builder = new LatLngBounds.Builder();

                if (App.ActiveTrackManager.HasActiveTrack)
                {
                    var orderByLatitude = App.ActiveTrackManager.TrackPoints.OrderBy(p => p.Latitude);
                    var orderByLongitude = App.ActiveTrackManager.TrackPoints.OrderBy(p => p.Longitude);

                    var latLngs = new List<LatLng>
                    {
                        orderByLatitude.First().ToLatLng(),
                        orderByLatitude.Last().ToLatLng(),
                        orderByLongitude.First().ToLatLng(),
                        orderByLongitude.Last().ToLatLng()
                    };

                    latLngs.ForEach(l =>
                    {
                        builder.Include(l);
                        l.Dispose();
                    });
                }
                else
                {
                    var latLng = App.LocationListener.Location.ToLatLng();
                    builder.Include(latLng);
                    latLng.Dispose();
                }

                var bounds = builder.Build();
                var cameraUpdate = CameraUpdateFactory.NewLatLngBounds(bounds, Constants.FitTrackToScreenPadding);

                RunOnUiThread(() => SetCameraView(cameraUpdate, animate));
            });
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

        #region Location Display

        public void ShowLocationChanges()
        {
            if (App.ActiveTrackManager.HasActiveTrack && App.ActiveTrackManager.TrackPoints.Any())
            {
                TrackDrawer.DrawTrack(App.ActiveTrackManager.TrackPoints);
            }
            else if (App.LocationListener.Location != null)
            {
                TrackDrawer.DrawCurrentPositionMarker(App.LocationListener.Location.ToTrackPoint());
            }
        }

        #endregion
    }
}