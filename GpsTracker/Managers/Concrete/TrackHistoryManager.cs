using System.Collections.Generic;
using GpsTracker.Entities;
using GpsTracker.Managers.Abstract;
using GpsTracker.Repositories.Abstract;
using GpsTracker.Tools;

namespace GpsTracker.Managers.Concrete
{
    internal class TrackHistoryManager : ITrackHistoryManager
    {
        private readonly ITrackRepository _trackRepository = ServiceLocator.Instance.Resolve<ITrackRepository>();

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