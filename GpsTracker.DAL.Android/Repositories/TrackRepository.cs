using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GpsTracker.DAL.Abstract.Repositories;
using GpsTracker.Entities;
using Newtonsoft.Json;
using Environment = Android.OS.Environment;

namespace GpsTracker.DAL.Android.Repositories
{
    public class TrackRepository : ITrackRepository
    {
        //private static readonly string FilesDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        private static readonly string FilesDirectory = Environment.ExternalStorageDirectory.AbsolutePath;
        private static readonly string TracksDirectory = Path.Combine(FilesDirectory, "tracks");
        private static readonly object FsLocker = new object();

        public void Save(Track track)
        {
            track.EncodeTrackPoints();

            var serializedTrack = JsonConvert.SerializeObject(track);

            var filePath = GetFilePath(track);

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

        public List<Track> GetAll()
        {
            var tracks = new List<Track>();
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
                    var track = JsonConvert.DeserializeObject<Track>(trackString);

                    if (track != null)
                    {
                        tracks.Add(track);
                    }
                }
                catch (Exception) {}
            }

            return tracks;
        }

        public void Delete(Track track)
        {
            lock (FsLocker)
            {
                var filePath = GetFilePath(track);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        private string GetFilePath(Track track)
        {
            var fileName = GetFileName(track);
            return Path.Combine(TracksDirectory, fileName);
        }

        private string GetFileName(Track track)
        {
            return String.Format("{0} [{1}].txt",
                GetTrackFilePrefix(),
                track.StartTime.ToString("u"));
        }

        private string GetTrackFilePrefix()
        {

            return "Gps Tracker";
            //return Application.Context.Resources.GetString(Resource.String.app_name);
        }

        private bool IsTrackFile(string path)
        {
            var filename = Path.GetFileName(path);

            return filename.StartsWith(GetTrackFilePrefix());
        }
    }
}