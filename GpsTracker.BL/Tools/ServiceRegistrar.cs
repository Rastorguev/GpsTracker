using GpsTracker.Repositories.Abstract;
using GpsTracker.Repositories.Concrete;
using Ninject;

namespace GpsTracker.BL.Tools
{
    public static class ServiceRegistrar
    {
        public static void Register()
        {
            var kernel = new StandardKernel();

            kernel.Bind<ITrackRepository>().To<TrackRepository>();
        }
    }
}