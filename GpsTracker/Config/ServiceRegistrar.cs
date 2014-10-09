using GpsTracker.Managers.Abstract;
using GpsTracker.Managers.Concrete;
//using GpsTracker.Repositories.Abstract;
//using GpsTracker.Repositories.Concrete;
using GpsTracker.Tools;

namespace GpsTracker.Config
{
    public class ServiceRegistrar
    {
        public static void Startup()
        {
            ServiceLocator.Instance.Register<IActiveTrackManager, ActiveTrackManager>();
            ServiceLocator.Instance.Register<ILocationManager, LocationManager>();
            //ServiceLocator.Instance.Register<ITrackHistoryManager, TrackHistoryManager>();
            //ServiceLocator.Instance.Register<ITrackRepository, TrackRepository>();
        }
    }
}