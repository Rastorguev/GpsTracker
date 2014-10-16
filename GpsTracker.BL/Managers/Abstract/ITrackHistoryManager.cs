using System.Collections.Generic;
using GpsTracker.Entities;

namespace GpsTracker.BL.Managers.Abstract
{
    public interface ITrackHistoryManager
    {
        void SaveTrack(Track track);
        List<Track> GetSavedTracks();
        void DeleteTrack(Track track);
        void UploadToDropbox();
    }
}