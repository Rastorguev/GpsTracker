using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Android.App;
using DropboxSync.Android;
using GpsTracker.DAL.Abstract.Repositories;
using GpsTracker.Entities;
using Newtonsoft.Json;
using Environment = Android.OS.Environment;

namespace GpsTracker.DAL.Android.Repositories
{
    public class TrackRepository : ITrackRepository
    {
        private const string DropboxSyncKey = "wwnmyaoj0v0608p";
        private const string DropboxSyncSecret = "gom3h89jeb2cuax";

        //private static readonly string FilesDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        private static readonly string FilesDirectory = Environment.ExternalStorageDirectory.AbsolutePath;
        private static readonly string TracksDirectory = Path.Combine(FilesDirectory, "tracks");
        private static readonly object FsLocker = new object();

        private readonly DBAccountManager _accountManager = DBAccountManager.GetInstance(Application.Context,
            DropboxSyncKey, DropboxSyncSecret);

        #region ITrackRepository implementation

        public void Save(Track track)
        {
            track.EncodeTrackPoints();

            if (_accountManager.HasLinkedAccount)
            {
                SaveOnDropbox(track);
            }
            else
            {
                SaveOnLocalStorage(track);
            }
        }

        public List<Track> GetAll()
        {
            List<Track> tracks;

            if (_accountManager.HasLinkedAccount)
            {
                tracks = GetAllFromDropbox().ToList();
            }
            else
            {
                tracks = GetAllFromLocalStorage().ToList();
            }

            return tracks;
        }

        public void Delete(Track track)
        {
            DeleteFromLocalStorage(track);

            if (_accountManager.HasLinkedAccount)
            {
                DeleteFromDropbox(track);
            }
        }

        public void UploadToDropbox()
        {
            if (_accountManager.HasLinkedAccount)
            {
                var tracks = GetAllFromLocalStorage();

                foreach (var track in tracks)
                {
                    SaveOnDropbox(track);
                }
            }
        }

        #endregion

        #region Helpers

        private IEnumerable<Track> GetAllFromLocalStorage()
        {
            var tracks = new List<Track>();

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
                                string serializedTrack;
                                using (var sr = new StreamReader(new FileStream(trackFile, FileMode.Open)))
                                {
                                    serializedTrack = sr.ReadToEnd();
                                }

                                var track = JsonConvert.DeserializeObject<Track>(serializedTrack);

                                if (track != null)
                                {
                                    tracks.Add(track);
                                }
                            }
                            catch (Exception) { }
                        }
                    }
                }
            }
            return tracks;
        }

        private IEnumerable<Track> GetAllFromDropbox()
        {
            var tracks = new List<Track>();
            var fileSystem = DBFileSystem.ForAccount(_accountManager.LinkedAccount);

            lock (FsLocker)
            {
                var trackFiles =
                    fileSystem.ListFolder(new DBPath(DBPath.Root.Name)).Where(i => IsTrackFile(i.Path.Name)).ToList();

                if (trackFiles.Any())
                {
                    foreach (var trackFile in trackFiles)
                    {
                        DBFile dbFile = null;
                        try
                        {
                            dbFile = fileSystem.Open(new DBPath(trackFile.Path.Name));
                            var serializedTrack = dbFile.ReadString();
                            var track = JsonConvert.DeserializeObject<Track>(serializedTrack);

                            if (track != null)
                            {
                                tracks.Add(track);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        finally
                        {
                            if (dbFile != null)
                            {
                                dbFile.Close();
                            }
                        }
                    }
                }
            }

            return tracks;
        }

        private void SaveOnLocalStorage(Track track)
        {
            var fileName = GetFileName(track);
            var filePath = GetFilePath(fileName);

            lock (FsLocker)
            {
                if (!Directory.Exists(TracksDirectory))
                {
                    Directory.CreateDirectory(TracksDirectory);
                }
                using (var sw = new StreamWriter(new FileStream(filePath, FileMode.OpenOrCreate)))
                {
                    var serializedTrack = JsonConvert.SerializeObject(track);
                    sw.Write(serializedTrack);
                }
            }
        }

        private void SaveOnDropbox(Track track)
        {
            var fileSystem = DBFileSystem.ForAccount(_accountManager.LinkedAccount);

            lock (FsLocker)
            {
                var dbPath = new DBPath(GetFileName(track));

                if (!fileSystem.Exists(dbPath))
                {
                    DBFile dbFile = null;

                    try
                    {
                        dbFile = fileSystem.Create(dbPath);
                        var serializedTrack = JsonConvert.SerializeObject(track);
                        dbFile.WriteString(serializedTrack);
                    }
                    catch (Exception) { }
                    finally
                    {
                        if (dbFile != null)
                        {
                            dbFile.Close();
                        }
                    }
                }
            }
        }

        private void DeleteFromLocalStorage(Track track)
        {
            lock (FsLocker)
            {
                var fileName = GetFileName(track);
                var filePath = GetFilePath(fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        private void DeleteFromDropbox(Track track)
        {
            var fileName = GetFileName(track);
            var fileSystem = DBFileSystem.ForAccount(_accountManager.LinkedAccount);

            lock (FsLocker)
            {
                try
                {
                    var dbPath = new DBPath(fileName);

                    if (fileSystem.Exists(new DBPath(fileName)))
                    {
                        fileSystem.Delete(dbPath);
                    }
                }
                catch (Exception) { }
            }
        }

        private bool IsTrackFile(string path)
        {
            var filename = Path.GetFileName(path);

            var pattern = @"^" + GetTrackFilePrefix() + @"\[\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}-\d{2}\].txt\z";
            var regex = new Regex(pattern);
            var match = regex.Match(filename);

            return match.Success;
        }

        private string GetFilePath(string fileName)
        {
            return Path.Combine(TracksDirectory, fileName);
        }

        private string GetFileName(Track track)
        {
            return String.Format("{0}[{1}].txt",
                GetTrackFilePrefix(),
                track.StartTime.ToString("yyyy-M-d_hh-mm-ss-ff"));
        }

        private string GetTrackFilePrefix()
        {
            return "GpsTracker";
            //return Application.Context.Resources.GetString(Resource.String.app_name);
        }

        #endregion
    }
}