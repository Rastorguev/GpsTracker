using GpsTracker.Managers.Abstract;
using GpsTracker.Managers.Concrete;
using GpsTracker.Tools;

namespace GpsTracker.Config
{
    public class ServiceRegistrar
    {
        public static void Startup()
        {
            ServiceLocator.Instance.Register<IActiveTrackManager, ActiveTrackManager>();
        }
    }
}