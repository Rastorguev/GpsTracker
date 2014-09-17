using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using GpsTracker.Entities;
using GpsTracker.Repositories.Abstract;
using Newtonsoft.Json;

namespace GpsTracker.Repositories.Concrete
{
    public class TrackRepository : ITrackRepository
    {
        private static readonly string FilesDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        //private static readonly string FilesDirectory = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
        private static readonly string TracksDirectory = Path.Combine(FilesDirectory, "tracks");
        private static readonly object FsLocker = new object();

        public void Save(TrackData track)
        {
            track.SerializeTrackPoints();

            var serializedTrack = JsonConvert.SerializeObject(track);

            var fileName = String.Format("{0} [{1} - {2}].txt",
                GetTrackFilePrefix(),
                track.StartTime.ToString("u"),
                track.EndTime.ToString("u"));

            var filePath = Path.Combine(TracksDirectory, fileName);

            lock (FsLocker)
            {
                if (!Directory.Exists(TracksDirectory))
                {
                    Directory.CreateDirectory(TracksDirectory);
                }
                using (var sw = new StreamWriter(new FileStream(filePath, FileMode.OpenOrCreate)))
                {
                    sw.Write(serializedTrack);
                }
            }
        }

        public List<TrackData> GetAll()
        {
            var tracks = new List<TrackData>();
            var trackStrings = new List<string>();

            lock (FsLocker)
            {
                if (Directory.Exists(TracksDirectory))
                {
                    var files = Directory.GetFiles(TracksDirectory);
                    var trackFiles = files.Where(IsTrackFile).ToList();

                    if (trackFiles.Any())
                    {
                        foreach (var trackFile in trackFiles)
                        {
                            try
                            {
                                string trackString;
                                using (var sr = new StreamReader(new FileStream(trackFile, FileMode.Open)))
                                {
                                    trackString = sr.ReadToEnd();
                                }

                                trackStrings.Add(trackString);
                            }
                            catch (Exception) {}
                        }
                    }
                }
            }

            foreach (var trackString in trackStrings)
            {
                try
                {
                    var track = JsonConvert.DeserializeObject<TrackData>(trackString);

                    if (track != null)
                    {
                        tracks.Add(track);
                    }
                }
                catch (Exception) {}
            }

            return tracks;
        }

        private string GetTrackFilePrefix()
        {
            return Application.Context.Resources.GetString(Resource.String.app_name);
        }

        private bool IsTrackFile(string path)
        {
            var filename = Path.GetFileName(path);

            return filename.StartsWith(GetTrackFilePrefix());
        }
    }
}