using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;

namespace GpsTracker.Tools
{
    internal class TrackDrawer : ITrackDrawer
    {
        private Marker _currentPositionMarker;
        private Marker _startPositionMarker;
        private readonly List<Polyline> _polylines = new List<Polyline>();
        private readonly GoogleMap _map;
        private readonly Activity _context;
        private const int SegmentMaxLength = 500;
        private const string PolylineColor = "#AA3E97D1";

        private Bitmap _currentPositionMarkerIcon;

        private Bitmap CurrentPositionMarkerIcon
        {
            get { return _currentPositionMarkerIcon ?? (_currentPositionMarkerIcon = GetCurrentPositionMarkerIcon()); }
        }

        #region Path Display Methods

        protected GoogleMap GetMap()
        {
            return _map;
        }

        public TrackDrawer(GoogleMap map, Activity context)
        {
            _map = map;
            _context = context;
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
            var color = BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed);

            options.InvokeIcon(color);
            options.SetPosition(trackPoint);

            var marker = map.AddMarker(options);
            return marker;
        }

        private Marker CreateCurrentPositionMarker(LatLng trackPoint)
        {
            //var size = _context.Resources.GetDimensionPixelSize(Resource.Dimension.map_dot_marker_size);
            //var dotMarkerBitmap = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888);
            //var canvas = new Canvas(dotMarkerBitmap);

            //var dot = _context.Resources.GetDrawable(Resource.Drawable.CurrentPositionMarkerDot);
            //var halo = _context.Resources.GetDrawable(Resource.Drawable.CurrentPositionMarkerHalo);

            //halo.SetBounds(0, 0, dotMarkerBitmap.Width, dotMarkerBitmap.Width);

            //const double x = 2.9;
            //dot.SetBounds((int) (
            //    dotMarkerBitmap.Width/x),
            //    (int) (dotMarkerBitmap.Height/x),
            //    dotMarkerBitmap.Width - (int) (dotMarkerBitmap.Width/x),
            //    dotMarkerBitmap.Height - (int) (dotMarkerBitmap.Height/x));

            //halo.Draw(canvas);
            //dot.Draw(canvas);

            var map = GetMap();
            var options = new MarkerOptions();

            options.SetPosition(trackPoint);
            options.InvokeIcon(BitmapDescriptorFactory.FromBitmap(CurrentPositionMarkerIcon));
            options.Anchor(.5f, .5f);

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
            var color = Color.ParseColor(PolylineColor);
            return color;
        }

        #endregion

        private Bitmap GetCurrentPositionMarkerIcon()
        {
            const double ratio = 2.9;
            var size = _context.Resources.GetDimensionPixelSize(Resource.Dimension.marker_size);
            var markerIcon = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888);
            var canvas = new Canvas(markerIcon);
            var dot = _context.Resources.GetDrawable(Resource.Drawable.CurrentPositionMarkerDot);
            var halo = _context.Resources.GetDrawable(Resource.Drawable.CurrentPositionMarkerHalo);

            halo.SetBounds(0, 0, markerIcon.Width, markerIcon.Width);

            dot.SetBounds((int) (
                markerIcon.Width/ratio),
                (int) (markerIcon.Height/ratio),
                markerIcon.Width - (int) (markerIcon.Width/ratio),
                markerIcon.Height - (int) (markerIcon.Height/ratio));

            halo.Draw(canvas);
            dot.Draw(canvas);

            return markerIcon;
        }

        public void CleanUp()
        {
            CurrentPositionMarkerIcon.Recycle();
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
    }
}