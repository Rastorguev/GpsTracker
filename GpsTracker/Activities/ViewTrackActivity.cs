using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Gms.Maps;
using Android.OS;
using Android.Widget;
using GpsTracker.Abstract;
using GpsTracker.Concrete;
using GpsTracker.Config;
using GpsTracker.Entities;
using GpsTracker.Tools;

namespace GpsTracker.Activities
{
    [Activity(Label = "@string/app_name")]
    internal class ViewTrackActivity : Activity
    {
        protected ITrackDrawer TrackDrawer;
        private TextView _distanceDefinition;
        private TextView _distanceUnit;
        private TextView _distanceValue;

        private TextView _durationDefinition;
        private TextView _durationValue;

        private GoogleMap _map;
        private MapFragment _mapFragment;
        private Track _track;

        protected GoogleMap Map
        {
            get { return _map ?? (_map = GetMap()); }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _track = GlobalStorage.Track;

            SetView();
            InitTrackDrawer();

            _durationDefinition = FindViewById<TextView>(Resource.Id.DurationDefinition);
            _durationValue = FindViewById<TextView>(Resource.Id.DurationValue);

            _distanceDefinition = FindViewById<TextView>(Resource.Id.DistanceDefinition);
            _distanceValue = FindViewById<TextView>(Resource.Id.DistanceValue);
            _distanceUnit = FindViewById<TextView>(Resource.Id.DistanceUnit);

            var duration = _track.Duration;
            var distance = UnitsPersonalizer.GetDistanceValue(_track.Distance);

            _durationDefinition.Text = Resources.GetString(Resource.String.duration).CapitalizeFirst();
            _durationValue.Text = String.Format(GetString(Resource.String.duration_format), duration);

            _distanceDefinition.Text = Resources.GetString(Resource.String.distance).CapitalizeFirst();
            _distanceValue.Text = String.Format(GetString(Resource.String.distance_format), distance);
            _distanceUnit.Text = UnitsPersonalizer.GetDistanceUnit();
        }

        protected override void OnResume()
        {
            base.OnResume();
            TrackDrawer.DrawTrack(_track.TrackPoints);
            FitTrackToScreen(_track.TrackPoints);
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
        }

        protected virtual void InitTrackDrawer()
        {
            TrackDrawer = new TrackDrawer(Map, this);
        }

        protected void SetView()
        {
            SetContentView(Resource.Layout.ViewTrackLayout);
        }

        protected GoogleMap GetMap()
        {
            if (_mapFragment == null)
            {
                _mapFragment = (MapFragment) FragmentManager.FindFragmentById(Resource.Id.Map);
            }

            return _mapFragment.Map;
        }

        protected void FitTrackToScreen(List<TrackPoint> trackPoints, bool animate = false)
        {
            Task.Run(() =>
            {
                var bounds = MapUtils.CalculateMapBounds(trackPoints);
                var cameraUpdate = CameraUpdateFactory.NewLatLngBounds(bounds, Constants.FitTrackToScreenPadding);

                RunOnUiThread(() => SetCameraView(cameraUpdate, animate));
            });
        }

        protected void SetCameraView(CameraUpdate cameraUpdate, bool animate = false)
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
    }
}