using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using GpsTracker.Config;

namespace GpsTracker.Tools
{
    internal class TrackDrawer : ITrackDrawer
    {
        private const int SegmentMaxLength = 500;
        private const double MarkerDotHaloRatio = 2.9;
        private const int CurrentPositionMarkerIconResetDelay = 5000;

        private Marker _currentPositionMarker;
        private Marker _startPositionMarker;
        private readonly List<Polyline> _polylines = new List<Polyline>();
        private readonly GoogleMap _map;
        private readonly Activity _context;
        private bool _disposed;

        private Bitmap _currentPositionMarkerIconStatic;
        private Bitmap _currentPositionMarkerIconMoving;
        private Bitmap _startPositionMarkerIcon;

        private Bitmap CurrentPositionMarkerIconStatic
        {
            get
            {
                return _currentPositionMarkerIconStatic ??
                       (_currentPositionMarkerIconStatic = GetCurrentPositionMarkerIconStatic());
            }
        }

        private Bitmap CurrentPositionMarkerIconMoving
        {
            get
            {
                return _currentPositionMarkerIconMoving ??
                       (_currentPositionMarkerIconMoving = GetCurrentPositionMarkerIconMoving());
            }
        }

        private Bitmap StartPositionMarkerIcon
        {
            get { return _startPositionMarkerIcon ?? (_startPositionMarkerIcon = GetStartPositionMarkerIcon()); }
        }

        private readonly Timer _currentPositionMarkerIconResetTimer;

        public TrackDrawer(GoogleMap map, Activity context)
        {
            _map = map;
            _context = context;

            _currentPositionMarkerIconResetTimer = new Timer
            {
                AutoReset = false,
                Interval = CurrentPositionMarkerIconResetDelay
            };

            _currentPositionMarkerIconResetTimer.Elapsed += CurrentPositionMarkerIconResetHandler;
        }

        #region Path Display Methods

        protected GoogleMap GetMap()
        {
            return _map;
        }

        public virtual void DrawTrack(List<LatLng> trackPoints)
        {
            if (trackPoints.Count > 1)
            {
                DrawStartPositionMarker(trackPoints.First());
                DrawTrackLine(trackPoints);
            }

            if (trackPoints.Any())
            {
                DrawCurrentPositionMarker(trackPoints.Last());
            }
        }

        public void RemoveTrack()
        {
            if (_startPositionMarker != null)
            {
                _startPositionMarker.Remove();
                _startPositionMarker.Dispose();
                _startPositionMarker = null;
            }
            if (_currentPositionMarker != null)
            {
                _currentPositionMarker.Remove();
                _currentPositionMarker.Dispose();
                _currentPositionMarker = null;
            }

            _polylines.ForEach(p =>
            {
                p.Remove();
                p.Dispose();
            });
            _polylines.Clear();
        }

        public virtual void DrawCurrentPositionMarker(LatLng trackPoint)
        {
            if (_currentPositionMarker == null)
            {
                _currentPositionMarker = CreateCurrentPositionMarker(trackPoint);
            }
            else
            {
                _currentPositionMarker.Position = trackPoint;
            }

            SetCurrentPositionMarkerIcon();
        }

        public virtual void DrawStartPositionMarker(LatLng trackPoint)
        {
            if (_startPositionMarker == null)
            {
                _startPositionMarker = CreateStartPositionMarker(trackPoint);
            }
            else
            {
                _startPositionMarker.Position = trackPoint;
            }
        }

        private void SetCurrentPositionMarkerIcon()
        {
            var bearing = App.LocationListener.Bearing;

            if (bearing != null)
            {
                _currentPositionMarker.SetIcon(BitmapDescriptorFactory.FromBitmap(CurrentPositionMarkerIconMoving));
                _currentPositionMarker.Rotation = bearing.Value;

                InitCurrentPositionMarkerIconReset();
            }
            else
            {
                _currentPositionMarker.SetIcon(BitmapDescriptorFactory.FromBitmap(CurrentPositionMarkerIconStatic));
            }
        }

        private void InitCurrentPositionMarkerIconReset()
        {
            _currentPositionMarkerIconResetTimer.Stop();
            _currentPositionMarkerIconResetTimer.Start();
        }

        private void CurrentPositionMarkerIconResetHandler(object sender, EventArgs e)
        {
            _context.RunOnUiThread(
                () =>
                {
                    if (_currentPositionMarker != null)
                    {
                        _currentPositionMarker.SetIcon(
                            BitmapDescriptorFactory.FromBitmap(CurrentPositionMarkerIconStatic));
                    }
                    else
                    {
                        Console.WriteLine();
                    }
                });
        }

        private void DrawTrackLine(List<LatLng> trackPoints)
        {
            var segments = SplitTrackOnSegments(trackPoints);

            if (!_polylines.Any())
            {
                var polylines = segments.Select(CreatePolyline);

                _polylines.AddRange(polylines);
            }

            else if (segments.Count == _polylines.Count)
            {
                var lastPolyline = _polylines.Last();
                var newSegment = segments.Last();

                lastPolyline.Points = newSegment;
            }

            if (segments.Count > _polylines.Count)
            {
                for (var i = _polylines.Count - 1; i <= segments.Count - 1; i++)
                {
                    if (_polylines.ElementAtOrDefault(i) != null)
                    {
                        _polylines[i].Points = segments[i];
                    }
                    else
                    {
                        var polyline = CreatePolyline(segments[i]);

                        _polylines.Add(polyline);
                    }
                }
            }
        }

        private Marker CreateStartPositionMarker(LatLng trackPoint)
        {
            var map = GetMap();
            var options = new MarkerOptions();

            options.SetPosition(trackPoint);
            options.InvokeIcon(BitmapDescriptorFactory.FromBitmap(StartPositionMarkerIcon));
            options.Anchor(.5f, .5f);
            options.Flat(true);

            var marker = map.AddMarker(options);
            return marker;
        }

        private Marker CreateCurrentPositionMarker(LatLng trackPoint)
        {
            var map = GetMap();
            var options = new MarkerOptions();

            options.SetPosition(trackPoint);
            options.InvokeIcon(BitmapDescriptorFactory.FromBitmap(CurrentPositionMarkerIconStatic));
            options.Anchor(.5f, .5f);
            options.Flat(true);

            var marker = map.AddMarker(options);

            return marker;
        }

        private Polyline CreatePolyline(List<LatLng> trackPoints)
        {
            var map = GetMap();
            var polylineOptions = new PolylineOptions();

            polylineOptions.InvokeColor(GetPolylineColor());
            polylineOptions.InvokeWidth(14);

            trackPoints.ForEach(p => polylineOptions.Add(p));
            var polyline = map.AddPolyline(polylineOptions);

            return polyline;
        }

        protected virtual Color GetPolylineColor()
        {
            var color = Color.ParseColor(Constants.PolylineColor);
            return color;
        }

        #endregion

        private Bitmap GetCurrentPositionMarkerIconStatic()
        {
            var size = _context.Resources.GetDimensionPixelSize(Resource.Dimension.marker_size);
            var markerIcon = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888);
            var canvas = new Canvas(markerIcon);
            var dot = _context.Resources.GetDrawable(Resource.Drawable.CurrentPositionMarkerDot);
            var halo = _context.Resources.GetDrawable(Resource.Drawable.CurrentPositionMarkerHalo);

            halo.SetBounds(0, 0, markerIcon.Width, markerIcon.Width);
            dot.SetBounds((int) (
                markerIcon.Width/MarkerDotHaloRatio),
                (int) (markerIcon.Height/MarkerDotHaloRatio),
                markerIcon.Width - (int) (markerIcon.Width/MarkerDotHaloRatio),
                markerIcon.Height - (int) (markerIcon.Height/MarkerDotHaloRatio));

            halo.Draw(canvas);
            dot.Draw(canvas);

            return markerIcon;
        }

        private Bitmap GetCurrentPositionMarkerIconMoving()
        {
            var size = _context.Resources.GetDimensionPixelSize(Resource.Dimension.marker_size);
            var markerIcon = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888);
            var canvas = new Canvas(markerIcon);
            var dot = _context.Resources.GetDrawable(Resource.Drawable.Arrow);
            var halo = _context.Resources.GetDrawable(Resource.Drawable.CurrentPositionMarkerHalo);

            halo.SetBounds(0, 0, markerIcon.Width, markerIcon.Width);
            dot.SetBounds((int) (
                markerIcon.Width/MarkerDotHaloRatio),
                (int) (markerIcon.Height/MarkerDotHaloRatio),
                markerIcon.Width - (int) (markerIcon.Width/MarkerDotHaloRatio),
                markerIcon.Height - (int) (markerIcon.Height/MarkerDotHaloRatio));

            halo.Draw(canvas);
            dot.Draw(canvas);

            return markerIcon;
        }

        private Bitmap GetStartPositionMarkerIcon()
        {
            var size = _context.Resources.GetDimensionPixelSize(Resource.Dimension.marker_size);
            var markerIcon = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888);
            var canvas = new Canvas(markerIcon);
            var dot = _context.Resources.GetDrawable(Resource.Drawable.StartPositionMarkerDot);

            dot.SetBounds((int) (
                markerIcon.Width/MarkerDotHaloRatio),
                (int) (markerIcon.Height/MarkerDotHaloRatio),
                markerIcon.Width - (int) (markerIcon.Width/MarkerDotHaloRatio),
                markerIcon.Height - (int) (markerIcon.Height/MarkerDotHaloRatio));

            dot.Draw(canvas);

            return markerIcon;
        }

        #region Helpers

        private static List<List<LatLng>> SplitTrackOnSegments(List<LatLng> trackPoints)
        {
            const int overlay = 1;
            var expectedSegmentsNumber =
                Math.Ceiling(((decimal) trackPoints.Count/SegmentMaxLength));

            var segments = new List<List<LatLng>>();
            var n = 0;

            while (segments.Count < expectedSegmentsNumber)
            {
                if (trackPoints.Count >= SegmentMaxLength*(n + 1))
                {
                    var index = n != 0 ? n*SegmentMaxLength - overlay : n*SegmentMaxLength;
                    var count = n != 0 ? SegmentMaxLength + overlay : SegmentMaxLength;

                    segments.Add(trackPoints.GetRange(index, count));
                    n++;
                }
                else
                {
                    var index = n != 0 ? n*SegmentMaxLength - overlay : n*SegmentMaxLength;
                    var count = n != 0
                        ? trackPoints.Count - n*SegmentMaxLength + overlay
                        : trackPoints.Count - n*SegmentMaxLength;

                    segments.Add(trackPoints.GetRange(index, count));
                }
            }

            return segments;
        }

        #endregion

        #region IDisposable impementation

        ~TrackDrawer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    CurrentPositionMarkerIconStatic.Recycle();
                    StartPositionMarkerIcon.Recycle();
                    _currentPositionMarkerIconResetTimer.Elapsed -= CurrentPositionMarkerIconResetHandler;
                }

                _disposed = true;
            }
        }

        #endregion
    }
}