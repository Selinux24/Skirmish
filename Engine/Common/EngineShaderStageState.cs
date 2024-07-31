using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Shader stage state helper
    /// </summary>
    /// <typeparam name="T">Type of resource</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="deviceContext">Device context</param>
    public class EngineShaderStageState<T>(EngineDeviceContext deviceContext)
    {
        /// <summary>
        /// Internal resource list
        /// </summary>
        private readonly Dictionary<int, T> resources = [];

        /// <summary>
        /// Device context
        /// </summary>
        protected EngineDeviceContext DeviceContext { get; private set; } = deviceContext;

        /// <summary>
        /// Gets the resource array
        /// </summary>
        public IEnumerable<T> GetResources()
        {
            int maxSlot = resources.Keys.Max();

            T[] values = new T[maxSlot + 1];
            for (int i = 0; i < values.Length; i++)
            {
                if (resources.TryGetValue(i, out T resource))
                {
                    values[i] = resource;
                }
            }
            return values;
        }

        /// <summary>
        /// Updates the resource state
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="resource">Resource</param>
        /// <returns>Returns true if the update must change the current resource state in the device</returns>
        public bool Update(int slot, T resource)
        {
            if (Equals(resource, default(T)))
            {
                //Nothing to do
                return false;
            }

            if (resources.TryGetValue(slot, out T currResource))
            {
                if (Equals(currResource, resource))
                {
                    //Resources already exists and it's in the same slot, nothing to do
                    return false;
                }

                resources[slot] = resource;

                return true;
            }

            resources.Add(slot, resource);

            return true;
        }
        /// <summary>
        /// Updates the resource state
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="resourceList">Resource list</param>
        /// <returns>Returns true if the update must change the current resource state in the device</returns>
        public bool Update(int startSlot, IEnumerable<T> resourceList)
        {
            if (resourceList?.Any() != true)
            {
                //Nothing to do
                return false;
            }

            bool updated = false;
            foreach (var resource in resourceList)
            {
                updated = Update(startSlot++, resource) || updated;
            }
            return updated;
        }

        /// <summary>
        /// Clears the state
        /// </summary>
        protected void Clear()
        {
            resources.Clear();
        }
    }
}
