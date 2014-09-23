using System;
using System.Collections.Generic;

namespace GpsTracker.Tools
{
    public interface IServiceLocator
    {
        //Registration using derived type
        void Register<TBase, TDerived>(string name = "") where TDerived : new();

        //Registration using delegate
        void Register<TBase>(Func<TBase> theConstructor, string name = "");

        T Resolve<T>(string name = "");

        bool Contains<T>(string name = "");
    }

    public class ServiceLocator : IServiceLocator
    {
        private static readonly object TheLock = new Object();
        private static IServiceLocator _instance;
        private readonly IDictionary<string, object> _servicesConstructors;
        private readonly IDictionary<string, object> _servicesInstances;

        private ServiceLocator()
        {
            _servicesConstructors = new Dictionary<string, object>();
            _servicesInstances = new Dictionary<string, object>();
        }

        public static IServiceLocator Instance
        {
            get
            {
                lock (TheLock)
                {
                    if (_instance == null)
                    {
                        _instance = new ServiceLocator();
                    }
                }

                return _instance;
            }
        }

        public void Register<TBase, TDerived>(string name = "") where TDerived : new()
        {
            if (!typeof(TBase).IsAssignableFrom(typeof(TDerived)))
            {
                throw new ArgumentException(String.Format("{0} does not derive from {1}", typeof(TDerived), typeof(TBase)));
            }

            var baseType = typeof(TBase);
            var key = baseType + name;
            Func<TDerived> theConstructor = () => new TDerived();

            if (!_servicesConstructors.ContainsKey(key))
            {
                _servicesConstructors.Add(key, theConstructor);
            }
            else
            {
                _servicesConstructors[key] = theConstructor;
            }
        }

        public void Register<TBase>(Func<TBase> theConstructor, string name = "")
        {
            var baseType = typeof(TBase);
            var key = baseType + name;

            if (!_servicesConstructors.ContainsKey(key))
            {
                _servicesConstructors.Add(key, theConstructor);
            }
            else
            {
                _servicesConstructors[key] = theConstructor;
                _servicesInstances.Remove(key);
            }
        }

        public TBase Resolve<TBase>(string name = "")
        {
            var baseType = typeof(TBase);
            var key = baseType + name;
            if (_servicesInstances.ContainsKey(key))
            {
                return (TBase)_servicesInstances[key];
            }
            try
            {
                var instance = ((Func<TBase>)_servicesConstructors[key])();

                if (!_servicesInstances.ContainsKey(key))
                {
                    _servicesInstances.Add(key, instance);
                }
                else
                {
                    _servicesInstances[key] = instance;
                }

                return instance;
            }
            catch (KeyNotFoundException)
            {
                throw new ApplicationException("The requested service is not registered");
            }
        }

        public bool Contains<TBase>(string name = "")
        {
            var baseType = typeof(TBase);
            var key = baseType + name;

            return _servicesConstructors.ContainsKey(key);
        }
    }
}