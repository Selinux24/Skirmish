using System;
using System.Collections.Generic;

namespace Engine
{
    public static class EngineServiceFactory
    {
        private static readonly Dictionary<Type, IGameServiceFactory> serviceFactories = new();

        public static void Register<I, T>()
            where T : class, IGameServiceFactory<I>
        {
            Type type = typeof(I);
            var factory = Activator.CreateInstance<T>();

            if (serviceFactories.ContainsKey(type))
            {
                serviceFactories[type] = factory;
            }
            else
            {
                serviceFactories.Add(type, factory);
            }
        }

        public static I Instance<I>()
        {
            if (!serviceFactories.TryGetValue(typeof(I), out var factory))
            {
                return default;
            }

            return ((IGameServiceFactory<I>)factory).Instance();
        }
    }
}
