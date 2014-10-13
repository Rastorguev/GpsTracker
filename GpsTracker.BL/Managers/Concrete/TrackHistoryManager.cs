using System.Collections.Generic;
using GpsTracker.BL.Managers.Abstract;
using GpsTracker.DAL.Abstract.Repositories;
using GpsTracker.Entities;

namespace GpsTracker.BL.Managers.Concrete
{
    public class TrackHistoryManager : ITrackHistoryManager
    {
        private readonly ITrackRepository _trackRepository;

        public TrackHistoryManager(ITrackRepository trackRepository)
        {
            _trackRepository = trackRepository;
        }

        public void SaveTrack(Track track)
        {
            _trackRepository.Save(track);
        }

        public List<Track> GetSavedTracks()
        {
            return _trackRepository.GetAll();
        }

        public void DeleteTrack(Track track)
        {
            _trackRepository.Delete(track);
        }
    }
}