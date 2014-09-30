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
using GpsTracker.Managers.Abstract;
using GpsTracker.Tools;

namespace GpsTracker.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    internal class MainActivity : Activity
    {
        private readonly ITrackHistoryManager _trackHistoryManager =
            ServiceLocator.Instance.Resolve<ITrackHistoryManager>();

        private List<Track> _savedTracks;
        private Button _startButton;
        private ListView _tracksListView;
        private ProgressDialog _progressDialog;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.MainLayout);

            _tracksListView = FindViewById<ListView>(Resource.Id.TrackList);
            _startButton = FindViewById<Button>(Resource.Id.StartButton);

            _tracksListView.ItemClick += OnListItemClick;

            _startButton.Click += OnStartButtonClick;
            _progressDialog = Utils.CreateProgressDialog(this);
        }

        protected override void OnStart()
        {
            base.OnStart();

            _progressDialog.Show();
            _savedTracks = _trackHistoryManager.GetSavedTracks();
            _tracksListView.Adapter = new TrackListAdapter(this, Resource.Layout.TrackListItem, _savedTracks);
            _progressDialog.Hide();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _tracksListView.ItemClick -= OnListItemClick;
            _startButton.Click -= OnStartButtonClick;
        }

        private void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var track = _savedTracks[e.Position];
            track.DecodeTrackPoints();

            GlobalStorage.Track = track;
            StartActivity(typeof (ViewTrackActivity));
        }

        private void OnStartButtonClick(object sender, EventArgs e)
        {
            StartActivity(typeof (MainTrackingActivity));
        }
    }

    public class TrackListAdapter : ArrayAdapter<Track>
    {
        private readonly Context _context;
        private readonly LayoutInflater _inflater;
        private readonly int _viewResourceId;

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
                distanceView.Text = String.Format(_context.GetString(Resource.String.distanceFormat),
                    UnitsPersonalizer.GetDistanceValue(track.Distance));
                durationView.Text = String.Format(_context.GetString(Resource.String.durationFormat), track.Duration);
            }

            return view;
        }
    }
}