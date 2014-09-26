using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using GpsTracker.Abstract;
using GpsTracker.Config;
using GpsTracker.Entities;
using GpsTracker.Managers.Abstract;
using GpsTracker.Tools;

namespace GpsTracker.Concrete
{
    internal class TrackDrawer : ITrackDrawer
    {
        private const double MarkerDotHaloRatio = 2.9;
        private Polyline _polyline;
        private Marker _startMarker;
        private Marker _finishMarker;

        private readonly GoogleMap _map;
        private readonly Activity _activity;
        private bool _disposed;

        private Bitmap _finishMarkerIcon;
        private Bitmap _startMarkerIcon;

        private Bitmap StartMarkerIcon
        {
            get { return _startMarkerIcon ?? (_startMarkerIcon = GetStartMarkerIcon()); }
        }

        private Bitmap FinishMarkerIcon
        {
            get
            {
                return _finishMarkerIcon ??
                       (_finishMarkerIcon = GetFinishMarkerIcon());
            }
        }

        public TrackDrawer(GoogleMap map, Activity activity)
        {
            _map = map;
            _activity = activity;
        }

        #region Path Display Methods

        public void DrawTrack(List<TrackPoint> trackPoints)
        {
            if (trackPoints.Count > 1)
            {
                DrawStartMarker(trackPoints.First());
                DrawTrackLine(trackPoints);
            }

            if (trackPoints.Any())
            {
                DrawFinishMarker(trackPoints.Last());
            }
        }

        private void DrawStartMarker(TrackPoint trackPoint)
        {
            if (_startMarker == null)
            {
                _startMarker = CreateStartMarker(trackPoint);
            }
            else
            {
                var latLng = trackPoint.ToLatLng();

                _startMarker.Position = latLng;
            }
        }

        public void DrawFinishMarker(TrackPoint trackPoint)
        {
            if (_finishMarker == null)
            {
                _finishMarker = CreateFinishMarker(trackPoint);
            }
            else
            {
                var latLng = trackPoint.ToLatLng();

                _finishMarker.Position = latLng;
            }
        }

        public void RemoveTrack()
        {
            if (_startMarker != null)
            {
                _startMarker.Remove();
                _startMarker.Dispose();
                _startMarker = null;
            }
            if (_finishMarker != null)
            {
                _finishMarker.Remove();
                _finishMarker.Dispose();
                _finishMarker = null;
            }
            if (_polyline != null)
            {
                _polyline.Remove();
                _polyline.Dispose();
                _polyline = null;
            }
        }

        private void DrawTrackLine(List<TrackPoint> trackPoints)
        {
            _polyline = CreatePolyline(trackPoints);
        }

        private Marker CreateStartMarker(TrackPoint trackPoint)
        {
            var options = new MarkerOptions();
            var latLng = trackPoint.ToLatLng();

            options.SetPosition(latLng);
            options.InvokeIcon(BitmapDescriptorFactory.FromBitmap(StartMarkerIcon));
            options.Anchor(.5f, .5f);
            options.Flat(true);

            var marker = _map.AddMarker(options);
            return marker;
        }

        private Marker CreateFinishMarker(TrackPoint trackPoint)
        {
            var options = new MarkerOptions();
            var latLng = trackPoint.ToLatLng();

            options.SetPosition(latLng);
            options.InvokeIcon(BitmapDescriptorFactory.FromBitmap(FinishMarkerIcon));
            options.Anchor(.5f, .5f);
            options.Flat(true);

            var marker = _map.AddMarker(options);

            return marker;
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
            var color = Color.ParseColor(Constants.PolylineColor);
            return color;
        }

        #endregion

        private Bitmap GetFinishMarkerIcon()
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

        private Bitmap GetStartMarkerIcon()
        {
            var size = _activity.Resources.GetDimensionPixelSize(Resource.Dimension.marker_size);
            var markerIcon = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888);
            var canvas = new Canvas(markerIcon);
            var dot = _activity.Resources.GetDrawable(Resource.Drawable.StartPositionMarkerDot);

            dot.SetBounds((int) (
                markerIcon.Width/MarkerDotHaloRatio),
                (int) (markerIcon.Height/MarkerDotHaloRatio),
                markerIcon.Width - (int) (markerIcon.Width/MarkerDotHaloRatio),
                markerIcon.Height - (int) (markerIcon.Height/MarkerDotHaloRatio));

            dot.Draw(canvas);

            return markerIcon;
        }

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
                    FinishMarkerIcon.Recycle();
                    StartMarkerIcon.Recycle();
                }

                _disposed = true;
            }
        }

        #endregion
    }
}