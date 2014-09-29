using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Graphics.Drawables;
using GpsTracker.Abstract;
using GpsTracker.Entities;
using GpsTracker.Managers.Abstract;
using GpsTracker.Tools;

namespace GpsTracker.Concrete
{
    internal class ActiveTrackDrawer : IActiveTrackDrawer
    {
        private const int SegmentMaxLength = 500;
        private const double MarkerDotHaloRatio = 2.9;
        private const int CurrentPositionMarkerIconResetDelay = 5000;
        public const string PolylineColor = "#AA3E97D1";

        private readonly Activity _activity;
        private readonly Timer _currentPositionMarkerIconResetTimer;
        private readonly ILocationManager _locationManager = ServiceLocator.Instance.Resolve<ILocationManager>();
        private readonly GoogleMap _map;
        private readonly List<Polyline> _polylines = new List<Polyline>();
        private Marker _currentPositionMarker;
        private Bitmap _currentPositionMarkerIconMoving;
        private Bitmap _currentPositionMarkerIconStatic;
        private bool _disposed;
        private Marker _startPositionMarker;
        private Bitmap _startPositionMarkerIcon;

        public ActiveTrackDrawer(GoogleMap map, Activity activity)
        {
            _map = map;
            _activity = activity;

            _currentPositionMarkerIconResetTimer = new Timer
            {
                AutoReset = false,
                Interval = CurrentPositionMarkerIconResetDelay
            };

            _currentPositionMarkerIconResetTimer.Elapsed += CurrentPositionMarkerIconResetHandler;
        }

        #region Path Display Methods

        public void DrawTrack(List<TrackPoint> trackPoints)
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

        public void DrawCurrentPositionMarker(TrackPoint trackPoint)
        {
            if (_currentPositionMarker == null)
            {
                _currentPositionMarker = CreateCurrentPositionMarker(trackPoint);
            }
            else
            {
                var latLng = trackPoint.ToLatLng();

                _currentPositionMarker.Position = latLng;
            }

            SetCurrentPositionMarkerIcon();
        }

        private void DrawStartPositionMarker(TrackPoint trackPoint)
        {
            if (_startPositionMarker == null)
            {
                _startPositionMarker = CreateStartPositionMarker(trackPoint);
            }
            else
            {
                var latLng = trackPoint.ToLatLng();

                _startPositionMarker.Position = latLng;
            }
        }

        private void SetCurrentPositionMarkerIcon()
        {
            var bearing = _locationManager.Bearing ?? 0;

            if (IsNeedToShowMovingIcon())
            {
                _currentPositionMarker.SetIcon(BitmapDescriptorFactory.FromBitmap(CurrentPositionMarkerIconMoving));
                _currentPositionMarker.Rotation = bearing;

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
            _activity.RunOnUiThread(
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

        private void DrawTrackLine(List<TrackPoint> trackPoints)
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

                var latLngs = newSegment.Select(p => p.ToLatLng()).ToList();

                lastPolyline.Points = latLngs;
            }

            if (segments.Count > _polylines.Count)
            {
                for (var i = _polylines.Count - 1; i <= segments.Count - 1; i++)
                {
                    if (_polylines.ElementAtOrDefault(i) != null)
                    {
                        var latLngs = segments[i].Select(p => p.ToLatLng()).ToList();

                        _polylines[i].Points = latLngs;
                    }
                    else
                    {
                        var polyline = CreatePolyline(segments[i]);

                        _polylines.Add(polyline);
                    }
                }
            }
        }

        private Marker CreateStartPositionMarker(TrackPoint trackPoint)
        {
            var options = new MarkerOptions();
            var latLng = trackPoint.ToLatLng();

            options.SetPosition(latLng);
            options.InvokeIcon(BitmapDescriptorFactory.FromBitmap(StartPositionMarkerIcon));
            options.Anchor(.5f, .5f);
            options.Flat(true);

            var marker = _map.AddMarker(options);
            return marker;
        }

        private Marker CreateCurrentPositionMarker(TrackPoint trackPoint)
        {
            var options = new MarkerOptions();
            var latLng = trackPoint.ToLatLng();

            options.SetPosition(latLng);
            options.InvokeIcon(BitmapDescriptorFactory.FromBitmap(CurrentPositionMarkerIconStatic));
            options.Anchor(.5f, .5f);
            options.Flat(true);

            var marker = _map.AddMarker(options);

            return marker;
        }

        private bool IsNeedToShowMovingIcon()
        {
            var lastLocationUpDateTime = _locationManager.LastLocationUpDateTime;
            var bearing = _locationManager.Bearing;

            return
                lastLocationUpDateTime != null &&
                lastLocationUpDateTime.Value.AddMilliseconds(CurrentPositionMarkerIconResetDelay) > DateTime.Now &&
                bearing != null;
        }

        private Polyline CreatePolyline(List<TrackPoint> trackPoints)
        {
            var polylineOptions = new PolylineOptions();

            polylineOptions.InvokeColor(GetPolylineColor());
            polylineOptions.InvokeWidth(14);

            trackPoints.ForEach(p =>
            {
                var latLng = p.ToLatLng();

                polylineOptions.Add(latLng);
                latLng.Dispose();
            });

            var polyline = _map.AddPolyline(polylineOptions);

            return polyline;
        }

        protected virtual Color GetPolylineColor()
        {
            var color = Color.ParseColor(PolylineColor);
            return color;
        }

        #endregion

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

        private Bitmap GetCurrentPositionMarkerIconStatic()
        {
            var size = _activity.Resources.GetDimensionPixelSize(Resource.Dimension.marker_size);
            var markerIcon = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888);
            var canvas = new Canvas(markerIcon);
            var dot = _activity.Resources.GetDrawable(Resource.Drawable.CurrentPositionMarkerDot);
            var halo = _activity.Resources.GetDrawable(Resource.Drawable.CurrentPositionMarkerHalo);

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
            var size = _activity.Resources.GetDimensionPixelSize(Resource.Dimension.marker_size);
            var bitmap = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888);
            var canvas = new Canvas(bitmap);
            var arrow = _activity.Resources.GetDrawable(Resource.Drawable.Arrow);
            var halo = _activity.Resources.GetDrawable(Resource.Drawable.CurrentPositionMarkerHalo);

            halo.SetBounds(0, 0, bitmap.Width, bitmap.Width);
            SetIconBounds(arrow, bitmap.Width, bitmap.Height, MarkerDotHaloRatio);

            halo.Draw(canvas);
            arrow.Draw(canvas);

            return bitmap;
        }

        private Bitmap GetStartPositionMarkerIcon()
        {
            var size = _activity.Resources.GetDimensionPixelSize(Resource.Dimension.marker_size);
            var bitmap = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888);
            var canvas = new Canvas(bitmap);

            var dot = _activity.Resources.GetDrawable(Resource.Drawable.StartPositionMarkerDot);

            SetIconBounds(dot, bitmap.Width, bitmap.Height, MarkerDotHaloRatio);

            dot.Draw(canvas);

            return bitmap;
        }

        private void SetIconBounds(Drawable drawable, int width, int height, double ratio)
        {
            drawable.SetBounds((int) (
                width/ratio),
                (int) (height/ratio),
                width - (int) (width/ratio),
                height - (int) (height/ratio));
        }

        #region Helpers

        private static List<List<TrackPoint>> SplitTrackOnSegments(List<TrackPoint> trackPoints)
        {
            const int overlay = 1;
            var expectedSegmentsNumber =
                Math.Ceiling(((decimal) trackPoints.Count/SegmentMaxLength));

            var segments = new List<List<TrackPoint>>();
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ActiveTrackDrawer()
        {
            Dispose(false);
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