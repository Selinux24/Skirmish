using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Shader stage state helper
    /// </summary>
    /// <typeparam name="T">Type of resource</typeparam>
    public class EngineShaderStageState<T>
    {
        /// <summary>
        /// Start slot of the last call
        /// </summary>
        public int StartSlot { get; set; }
        /// <summary>
        /// Resource list of the last call
        /// </summary>
        public IEnumerable<T> Resources { get; set; }
        /// <summary>
        /// Number of resources of the last call
        /// </summary>
        public int Count
        {
            get
            {
                return Resources?.Count() ?? 0;
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
            int index = Resources?.ToList()?.IndexOf(resource) ?? -1;
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
            if (resourceList?.Any() != true)
            {
                //Nothing to compare
                return true;
            }

            if (Resources?.Any() != true)
            {
                //Resources is empty
                return false;
            }

            if (StartSlot == startSlot && Helper.CompareEnumerables(Resources, resourceList))
            {
                //Same data
                return true;
            }

            //Look up coincidences
            int currentMaxSlot = StartSlot + Resources.Count();
            int newMaxSlot = startSlot + resourceList.Count();
            if (newMaxSlot > currentMaxSlot)
            {
                return false;
            }

            //Get range
            var range = Resources.Skip(startSlot).Take(resourceList.Count());
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
            if (resource == null)
            {
                return false;
            }

            if (LookupResource(slot, resource))
            {
                return false;
            }

            if (Resources?.Any() != true)
            {
                //Empty resource state
                StartSlot = slot;
                Resources = new[] { resource };

                return true;
            }

            int setSlot = slot + StartSlot;
            if (setSlot < Resources.Count())
            {
                //Update the slot
                var array = Resources.ToArray();
                array[setSlot] = resource;
                Resources = array;

                return true;
            }

            //Add space to the new resource
            var list = Resources.ToList();
            list.Add(resource);
            Resources = list;

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
                return false;
            }

            if (LookupResource(startSlot, resourceList))
            {
                return false;
            }

            if (resourceList?.Any() != true)
            {
                //Nothing to do
                return false;
            }

            if (Resources?.Any() != true)
            {
                StartSlot = startSlot;
                Resources = resourceList;

                return true;
            }

            //Get the range to update
            int currentCount = StartSlot + Resources.Count();
            int newCount = startSlot + resourceList.Count();
            int newLength = Math.Max(currentCount, newCount);
            T[] list = new T[newLength];

            for (int i = 0; i < Resources.Count(); i++)
            {
                list[i + StartSlot] = Resources.ElementAt(i);
            }

            for (int i = 0; i < resourceList.Count(); i++)
            {
                list[i + startSlot] = resourceList.ElementAt(i);
            }

            Resources = list;

            return true;
        }

        /// <summary>
        /// Clears the state
        /// </summary>
        protected void Clear()
        {
            StartSlot = 0;
            Resources = Enumerable.Empty<T>();
        }
    }
}
