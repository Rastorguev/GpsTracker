using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.OS;
using Android.Views;
using Android.Widget;
using GpsTracker.Concrete;
using GpsTracker.Entities;
using GpsTracker.Repositories.Abstract;
using GpsTracker.Tools;

namespace GpsTracker.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    internal class MainActivity : Activity
    {
        private readonly ITrackRepository _trackRepository = ServiceLocator.Instance.Resolve<ITrackRepository>();
        private Button _startButton;
        private ListView _tracksListView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.MainLayout);

            var savedTracks = _trackRepository.GetAll();

            _tracksListView = FindViewById<ListView>(Resource.Id.TrackList);
            _tracksListView.Adapter = new TrackListAdapter(this, Resource.Layout.TrackListItem, savedTracks);

            _startButton = FindViewById<Button>(Resource.Id.StartButton);
            _startButton.Click += delegate { StartActivity(typeof (MainTrackingActivity)); };
        }

        protected override void OnStart()
        {
            base.OnStart();

            var status = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(this);

            if (status != ConnectionResult.Success)
            {
                Alerts.ShowGooglePlayServicesErrorAlert(this, status);
            }
        }
    }

    public class TrackListAdapter : ArrayAdapter<Track>
    {
        private readonly LayoutInflater _inflater;
        private readonly int _viewResourceId;
        private readonly Context _context;

        public TrackListAdapter(Context context, int viewResourceId, IList<Track> tracks)
            : base(context, viewResourceId, tracks)
        {
            _context = context;
            _inflater = (LayoutInflater) _context.GetSystemService(Context.LayoutInflaterService);
            _viewResourceId = viewResourceId;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ?? _inflater.Inflate(_viewResourceId, null);
            var track = GetItem(position);

            if (track != null)
            {
                var startTimeDateView = (TextView) view.FindViewById(Resource.Id.StartTimeDate);
                var startTimeTimeView = (TextView) view.FindViewById(Resource.Id.StartTimeTime);
                var distanceView = view.FindViewById<TextView>(Resource.Id.Distance);
                var durationView = view.FindViewById<TextView>(Resource.Id.Duration);

                startTimeDateView.Text = track.StartTime.ToShortDateString();
                startTimeTimeView.Text = track.StartTime.ToLongTimeString();
                distanceView.Text = String.Format(_context.GetString(Resource.String.distance_format), track.Distance.MetersToKilometers());
                durationView.Text = String.Format(_context.GetString(Resource.String.duration_format), track.Duration);
            }

            return view;
        }
    }
}