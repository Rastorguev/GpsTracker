using GpsTracker.BL.Managers.Abstract;
using GpsTracker.BL.Managers.Concrete;
using GpsTracker.DAL.Abstract.Repositories;
using GpsTracker.DAL.Android.Repositories;
using Ninject.Modules;

namespace GpsTracker.Bindings.Android
{
    internal class ServiceRegistrar : NinjectModule
    {
        public override void Load()
        {
            Bind<ITrackRepository>().To<TrackRepository>();
            Bind<ITrackHistoryManager>().To<TrackHistoryManager>();
        }
    }
}