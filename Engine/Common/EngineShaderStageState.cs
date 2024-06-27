using System;
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
        private readonly List<T> resources = [];

        /// <summary>
        /// Device context
        /// </summary>
        protected EngineDeviceContext DeviceContext { get; private set; } = deviceContext;

        /// <summary>
        /// Start slot of the last call
        /// </summary>
        public int StartSlot { get; set; }
        /// <summary>
        /// Resource list of the last call
        /// </summary>
        public IEnumerable<T> Resources { get => resources.AsReadOnly(); }
        /// <summary>
        /// Number of resources of the last call
        /// </summary>
        public int Count
        {
            get
            {
                return resources.Count;
            }
        }

        /// <summary>
        /// Finds out whether the specfied resource, was attached in the same slot in the las call
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="resource">Resource</param>
        /// <returns>Returns true if the resource is in the specified slot since the las call</returns>
        private bool LookupResource(int slot, T resource)
        {
            int index = resources.IndexOf(resource);
            if (index < 0)
            {
                //The resource is not into the collection
                return false;
            }

            int currentSlot = index + StartSlot;
            if (currentSlot != slot)
            {
                //The resource is in another slot
                return false;
            }

            //The resource is part of the current collection, and is assigned to the specified slot
            return true;
        }
        /// <summary>
        /// Finds out whether the specfied resource list, were attached in the same slot in the last call
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="resourceList">Resource list</param>
        /// <returns>Returns true if all the elements in the resource list are in the specified slot since the last call</returns>
        private bool LookupResource(int startSlot, IEnumerable<T> resourceList)
        {
            if (resources.Count == 0)
            {
                //Resources is empty
                return false;
            }

            if (StartSlot == startSlot && Helper.CompareEnumerables(resources, resourceList))
            {
                //Same data
                return true;
            }

            //Look up coincidences
            int currentMaxSlot = StartSlot + resources.Count;
            int newMaxSlot = startSlot + resourceList.Count();
            if (newMaxSlot > currentMaxSlot)
            {
                return false;
            }

            //Get range
            var range = resources.Skip(startSlot).Take(resourceList.Count());
            if (!Helper.CompareEnumerables(range, resourceList))
            {
                //The specified list is not into the current resource list
                return false;
            }

            return true;
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

            if (LookupResource(slot, resource))
            {
                //Resource already exists, nothing to do
                return false;
            }

            if (resources.Count == 0)
            {
                //Empty resource state, add the first resource
                StartSlot = slot;
                resources.Add(resource);

                return true;
            }

            int setSlot = slot + StartSlot;
            if (setSlot < resources.Count)
            {
                //Update the slot
                resources[setSlot] = resource;

                return true;
            }

            //Add space to the new resource
            resources.Add(resource);

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

            if (LookupResource(startSlot, resourceList))
            {
                //Resources already exists, nothing to do
                return false;
            }

            if (resources.Count == 0)
            {
                //Empty resource state, add the first resource
                StartSlot = startSlot;
                resources.AddRange(resourceList);

                return true;
            }

            int currCount = resources.Count;
            int newCount = resourceList.Count();

            //Get the range to update
            if (startSlot == StartSlot)
            {
                if (currCount == newCount)
                {
                    //Same size. Replace resouce list
                    resources.Clear();
                    resources.AddRange(resourceList);

                    return true;
                }

                //Resize the resource list (always bigger), copy current resources, then copy new resources
                T[] tmp = new T[Math.Max(currCount, newCount)];
                Array.Copy(resources.ToArray(), 0, tmp, 0, currCount);
                Array.Copy(resourceList.ToArray(), 0, tmp, 0, newCount);
                resources.Clear();
                resources.AddRange(tmp);

                return true;
            }
            else
            {
                //Resize the resource list (always bigger) relative to the minimum start slot value
                int minSlot = Math.Min(startSlot, StartSlot);
                int newSize = Math.Max(startSlot + newCount, StartSlot + currCount) - minSlot;
                T[] tmp = new T[newSize];

                //Copy current resources at current start slot
                Array.Copy(resources.ToArray(), 0, tmp, StartSlot - minSlot, currCount);
                //Then copy new resources at new start slot
                Array.Copy(resourceList.ToArray(), 0, tmp, startSlot - minSlot, newCount);
                resources.Clear();
                resources.AddRange(tmp);
                //Update start slot to the minimum value
                StartSlot = minSlot;

                return true;
            }
        }

        /// <summary>
        /// Clears the state
        /// </summary>
        protected void Clear()
        {
            StartSlot = 0;
            resources.Clear();
        }
    }
}
