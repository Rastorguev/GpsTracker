﻿using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using DropboxSync.Android;
using GpsTracker.Bindings.Android;
using GpsTracker.BL.Managers.Abstract;
using GpsTracker.Entities;
using GpsTracker.Tools;

namespace GpsTracker.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    internal class MainActivity : Activity
    {
        private const string DropboxSyncKey = "wwnmyaoj0v0608p";
        private const string DropboxSyncSecret = "gom3h89jeb2cuax";
        private const int LinkToDropboxRequest = 1111;
        private readonly ITrackHistoryManager _trackHistoryManager = DependencyResolver.Resolve<ITrackHistoryManager>();
        private List<Track> _savedTracks;
        private ListView _tracksListView;
        private ProgressDialog _progressDialog;
        private Button _startButton;
        private Button _linkDropboxButton;

        private readonly DBAccountManager _accountManager = DBAccountManager.GetInstance(Application.Context,
            DropboxSyncKey, DropboxSyncSecret);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.MainLayout);

            _tracksListView = FindViewById<ListView>(Resource.Id.TrackList);
            _startButton = FindViewById<Button>(Resource.Id.StartButton);
            _linkDropboxButton = FindViewById<Button>(Resource.Id.LinkDropboxButton);

            _tracksListView.ItemClick += OnListItemClick;

            _startButton.Click += OnStartButtonClick;
            _linkDropboxButton.Click += OnLinkDropboxButtonClick;

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

        protected override void OnResume()
        {
            base.OnResume();

            _linkDropboxButton.Visibility = _accountManager.HasLinkedAccount ? ViewStates.Gone : ViewStates.Visible;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == LinkToDropboxRequest && resultCode != Result.Canceled)
            {
                Toast.MakeText(this, "Dropbox Linked", ToastLength.Short).Show();

                _trackHistoryManager.UploadToDropbox();
            }
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

            GlobalStorage.Route = track;
            StartActivity(typeof (ViewTrackActivity));
        }

        private void OnStartButtonClick(object sender, EventArgs e)
        {
            GlobalStorage.Route = null;
            StartActivity(typeof (MainTrackingActivity));
        }

        private void OnLinkDropboxButtonClick(object sender, EventArgs e)
        {
            _accountManager.StartLink(this, LinkToDropboxRequest);
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