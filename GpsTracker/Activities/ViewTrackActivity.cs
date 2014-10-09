using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Widget;
using GpsTracker.Abstract;
using GpsTracker.Bindings.Android;
using GpsTracker.BL.Managers.Abstract;
using GpsTracker.Concrete;
using GpsTracker.Config;
using GpsTracker.Entities;
using GpsTracker.Tools;

namespace GpsTracker.Activities
{
    [Activity(Label = "@string/app_name")]
    internal class ViewTrackActivity : Activity, GoogleMap.IOnCameraChangeListener
    {
        protected ITrackDrawer TrackDrawer;
        private TextView _distanceDefinition;
        private TextView _distanceUnit;
        private TextView _distanceValue;

        private TextView _durationDefinition;
        private TextView _durationValue;

        private TextView _avgSpeedDefinition;
        private TextView _avgSpeedValue;
        private TextView _avgSpeedUnit;

        private Button _deleteButton;
        private Button _folowRouteButton;

        private GoogleMap _map;
        private MapFragment _mapFragment;
        private Track _track;

        private readonly ITrackHistoryManager _trackHistoryManager = DependencyResolver.Resolve<ITrackHistoryManager>();

        private bool _firstOnCameraChangeEventOccured;

        protected GoogleMap Map
        {
            get { return _map ?? (_map = GetMap()); }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _track = GlobalStorage.Track;

            SetView();
            InitMap();
            InitTrackDrawer();

            _durationDefinition = FindViewById<TextView>(Resource.Id.DurationDefinition);
            _durationValue = FindViewById<TextView>(Resource.Id.DurationValue);

            _distanceDefinition = FindViewById<TextView>(Resource.Id.DistanceDefinition);
            _distanceValue = FindViewById<TextView>(Resource.Id.DistanceValue);
            _distanceUnit = FindViewById<TextView>(Resource.Id.DistanceUnit);

            _avgSpeedDefinition = FindViewById<TextView>(Resource.Id.AvgSpeedDefinition);
            _avgSpeedValue = FindViewById<TextView>(Resource.Id.AvgSpeedValue);
            _avgSpeedUnit = FindViewById<TextView>(Resource.Id.AvgSpeedUnit);

            _deleteButton = FindViewById<Button>(Resource.Id.DeleteButton);
            _folowRouteButton = FindViewById<Button>(Resource.Id.FollowRouteButton);

            var duration = _track.Duration;
            var distance = UnitsPersonalizer.GetDistanceValue(_track.Distance);
            var avgSpeed = UnitsPersonalizer.GetSpeedValue(_track.AvgSpeed);

            _durationDefinition.Text = Resources.GetString(Resource.String.duration).CapitalizeFirst();
            _durationValue.Text = String.Format(GetString(Resource.String.durationFormat), duration);

            _distanceDefinition.Text = Resources.GetString(Resource.String.distance).CapitalizeFirst();
            _distanceValue.Text = String.Format(GetString(Resource.String.distanceFormat), distance);
            _distanceUnit.Text = UnitsPersonalizer.GetDistanceUnit();

            _avgSpeedDefinition.Text = Resources.GetString(Resource.String.avgSpeed).CapitalizeFirst();
            _avgSpeedValue.Text = String.Format(GetString(Resource.String.speedFormat), avgSpeed);
            _avgSpeedUnit.Text = UnitsPersonalizer.GetSpeedUnit();

            _deleteButton.Text = Resources.GetString(Resource.String.delete).CapitalizeFirst();
            _deleteButton.Click += DeleteButtonClickHandler;

            _folowRouteButton.Text = Resources.GetString(Resource.String.followRoute).CapitalizeFirst();
            _folowRouteButton.Click += FollowRouteButtonClickHandler;
        }

        protected override void OnResume()
        {
            base.OnResume();
            TrackDrawer.DrawTrack(_track.TrackPoints);

            if (_firstOnCameraChangeEventOccured)
            {
                FitTrackToScreen(_track.TrackPoints);
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            TrackDrawer.RemoveTrack();
            GC.Collect();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            TrackDrawer.Dispose();

            GC.Collect(GC.MaxGeneration);
            _deleteButton.Click += DeleteButtonClickHandler;
        }

        public void OnCameraChange(CameraPosition position)
        {
            if (!_firstOnCameraChangeEventOccured)
            {
                _firstOnCameraChangeEventOccured = true;

                FitTrackToScreen(_track.TrackPoints);
            }
        }

        protected virtual void InitMap()
        {
            Map.SetOnCameraChangeListener(this);
        }

        private void InitTrackDrawer()
        {
            TrackDrawer = new TrackDrawer(Map, this);
        }

        private void SetView()
        {
            SetContentView(Resource.Layout.ViewTrackLayout);
        }

        private GoogleMap GetMap()
        {
            if (_mapFragment == null)
            {
                _mapFragment = (MapFragment) FragmentManager.FindFragmentById(Resource.Id.Map);
            }

            return _mapFragment.Map;
        }

        private void FitTrackToScreen(List<TrackPoint> trackPoints, bool animate = false)
        {
            Task.Run(() =>
            {
                var bounds = MapUtils.CalculateMapBounds(trackPoints);
                var cameraUpdate = CameraUpdateFactory.NewLatLngBounds(bounds, Constants.FitTrackToScreenPadding);

                RunOnUiThread(() => SetCameraView(cameraUpdate, animate));
            });
        }

        private void SetCameraView(CameraUpdate cameraUpdate, bool animate = false)
        {
            if (animate)
            {
                Map.AnimateCamera(cameraUpdate);
            }
            else
            {
                Map.MoveCamera(cameraUpdate);
            }
        }

        private void DeleteButtonClickHandler(object sender, EventArgs e)
        {
            _trackHistoryManager.DeleteTrack(_track);
            StartActivity(typeof (MainActivity));
        }

        private void FollowRouteButtonClickHandler(object sender, EventArgs e)
        {
            StartActivity(typeof (MainTrackingActivity));
        }
    }
}