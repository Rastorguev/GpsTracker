using System.Collections.Generic;
using GpsTracker.Entities;

namespace GpsTracker.Repositories.Abstract
{
    public interface ITrackRepository
    {
        void Save(TrackData track);
        List<TrackData> GetAll();
    }
}