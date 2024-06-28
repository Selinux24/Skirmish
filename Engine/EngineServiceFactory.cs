using System;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Engine service factory
    /// </summary>
    public static class EngineServiceFactory
    {
        /// <summary>
        /// Services
        /// </summary>
        private static readonly Dictionary<Type, IGameServiceFactory> serviceFactories = [];

        /// <summary>
        /// Register a service in the factory
        /// </summary>
        /// <typeparam name="I">Interface type</typeparam>
        /// <typeparam name="T">Service type</typeparam>
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
        /// <summary>
        /// Instances a service
        /// </summary>
        /// <typeparam name="I">Interface type</typeparam>
        /// <returns>Returns the registered service</returns>
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
