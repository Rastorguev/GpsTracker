using System;
using System.Collections.Generic;
using System.Linq;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;

namespace GpsTracker
{
    internal class TrackDrawer : ITrackDrawer
    {
        private Marker _currentPositionMarker;
        private Marker _startPositionMarker;
        private readonly List<Polyline> _polylines = new List<Polyline>();
        private readonly GoogleMap _map;
        private const int SegmentMaxLength = 500;
        private const string PolylineColor = "#AA3E97D1";

        #region Path Display Methods

        protected GoogleMap GetMap()
        {
            return _map;
        }

        public TrackDrawer(GoogleMap map)
        {
            _map = map;
        }

        public virtual void DrawTrack(List<LatLng> trackPoints)
        {
            if (trackPoints.Any())
            {
                DrawStartPositionMarker(trackPoints.First());
            }

            if (trackPoints.Count > 1)
            {
                DrawTrackLine(trackPoints);

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

        protected virtual void DrawStartPositionMarker(LatLng trackPoint)
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

        protected virtual void DrawCurrentPositionMarker(LatLng trackPoint)
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

        protected virtual void DrawTrackLine(List<LatLng> trackPoints)
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
            var map = GetMap();
            var options = new MarkerOptions();
            var color = BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueGreen);

            options.SetPosition(trackPoint);
            options.InvokeIcon(color);

            var marker = map.AddMarker(options);
            return marker;
        }

        private Polyline CreatePolyline(List<LatLng> trackPoints)
        {
            var map = GetMap();
            var polylineOptions = new PolylineOptions();

            polylineOptions.InvokeColor(GetPolylineColor());
            polylineOptions.InvokeWidth(6);

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

        #region Helpers

        private static List<List<LatLng>> SplitTrackOnSegments(List<LatLng> trackPoints)
        {
            const int overlay = 1;
            var expectedSegmentsNumber =
                Math.Ceiling(((decimal)trackPoints.Count / SegmentMaxLength));

            var segments = new List<List<LatLng>>();
            var n = 0;

            while (segments.Count < expectedSegmentsNumber)
            {
                if (trackPoints.Count >= SegmentMaxLength * (n + 1))
                {
                    var index = n != 0 ? n * SegmentMaxLength - overlay : n * SegmentMaxLength;
                    var count = n != 0 ? SegmentMaxLength + overlay : SegmentMaxLength;

                    segments.Add(trackPoints.GetRange(index, count));
                    n++;
                }
                else
                {
                    var index = n != 0 ? n * SegmentMaxLength - overlay : n * SegmentMaxLength;
                    var count = n != 0
                        ? trackPoints.Count - n * SegmentMaxLength + overlay
                        : trackPoints.Count - n * SegmentMaxLength;

                    segments.Add(trackPoints.GetRange(index, count));
                }
            }

            return segments;
        }

        #endregion
    }
}