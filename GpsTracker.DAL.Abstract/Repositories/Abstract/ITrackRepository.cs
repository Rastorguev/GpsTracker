using System.Collections.Generic;
using GpsTracker.Entities;

namespace GpsTracker.Repositories.Abstract
{
    public interface ITrackRepository
    {
        void Save(Track track);
        List<Track> GetAll();
        void Delete(Track track);
    }
}