using System.Collections.Generic;
using GpsTracker.Entities;

namespace GpsTracker.DAL.Abstract.Repositories
{
    public interface ITrackRepository
    {
        void Save(Track track);
        List<Track> GetAll();
        void Delete(Track track);
    }
}