using Ninject;

namespace GpsTracker.Bindings.Android
{
    public static class DependencyResolver
    {
        private static readonly IKernel Kernel = new StandardKernel(new ServiceRegistrar());

        public static T Resolve<T>()
        {
            return Kernel.Get<T>();
        }
    }
}