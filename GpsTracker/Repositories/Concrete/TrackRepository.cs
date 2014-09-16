using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Gms.Maps.Model;
using GpsTracker.Entities;
using GpsTracker.Repositories.Abstract;
using Newtonsoft.Json;

namespace GpsTracker.Repositories.Concrete
{
    public class TrackRepository : ITrackRepository
    {
        private static readonly string FilesDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        private static readonly string TracksFile = Path.Combine(FilesDirectory, "tracks.txt");
        private static readonly string TracksBackupFile = Path.Combine(FilesDirectory, "tracks_backup.txt");
        private static readonly object FsLocker = new object();

        public void Save(TrackData track)
        {
            lock (FsLocker)
            {
                var savedTracks = GetAll();
                savedTracks.Add(track);

                savedTracks.ForEach(t =>
                {
                    t.TrackPointsSerializable =
                        t.TrackPoints.Select(p => new LatLngSerializable(p.Latitude, p.Longitude)).ToList();
                });

                var serializedTracks = JsonConvert.SerializeObject(savedTracks);

                File.Copy(TracksFile, TracksBackupFile, true);

                using (var sw = new StreamWriter(new FileStream(TracksFile, FileMode.OpenOrCreate)))
                {
                    sw.Write(serializedTracks);
                }
            }
        }

        public List<TrackData> GetAll()
        {
            lock (FsLocker)
            {
                string serializedTracks;

                using (var sr = new StreamReader(new FileStream(TracksFile, FileMode.OpenOrCreate)))
                {
                    serializedTracks = sr.ReadToEnd();
                }

                var savedTracks = JsonConvert.DeserializeObject<List<TrackData>>(serializedTracks) ??
                                  new List<TrackData>();
                savedTracks.ForEach(
                    t =>
                    {
                        t.TrackPoints =
                            t.TrackPointsSerializable.Select(p => new LatLng(p.Latitude, p.Longitude)).ToList();
                    });

                return savedTracks;
            }
        }
    }
}