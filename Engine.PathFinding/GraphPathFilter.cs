using System;
using System.Collections.Generic;

namespace Engine.PathFinding
{
    /// <summary>
    /// Path filter
    /// </summary>
    public class GraphPathFilter
    {
        /// <summary>
        /// Area cost list
        /// </summary>
        private readonly Dictionary<int, float> areaCosts = [];

        /// <summary>
        /// Gets the cost of the specified area
        /// </summary>
        /// <param name="area">Area</param>
        /// <returns>Returns the cost</returns>
        public float GetCost(int area)
        {
            if (areaCosts.TryGetValue(area, out float cost))
            {
                return cost;
            }

            return 1f;
        }
        /// <summary>
        /// Gets the cost of the specified area
        /// </summary>
        /// <param name="area">Area</param>
        /// <returns>Returns the cost</returns>
        public float GetCost<T>(T area) where T : Enum
        {
            return GetCost((int)(object)area);
        }
        /// <summary>
        /// Sets the cost for the specified area
        /// </summary>
        /// <param name="area">Area</param>
        /// <param name="cost">Cost</param>
        public void SetCost(int area, float cost)
        {
            if (areaCosts.TryAdd(area, cost))
            {
                return;
            }

            areaCosts[area] = cost;
        }
        /// <summary>
        /// Sets the cost for the specified area
        /// </summary>
        /// <param name="area">Area</param>
        /// <param name="cost">Cost</param>
        public void SetCost<T>(T area, float cost) where T : Enum
        {
            SetCost((int)(object)area, cost);
        }

        /// <summary>
        /// Gets the area cost list
        /// </summary>
        public IEnumerable<(int Area, float Cost)> GetAreaCosts()
        {
            foreach (var item in areaCosts)
            {
                yield return (item.Key, item.Value);
            }
        }
        /// <summary>
        /// Gets the area cost list
        /// </summary>
        public IEnumerable<(T Area, float Cost)> GetAreaCosts<T>() where T : Enum
        {
            foreach (var item in areaCosts)
            {
                yield return ((T)(object)item.Key, item.Value);
            }
        }
        /// <summary>
        /// Clears the area cost list
        /// </summary>
        public void Clear()
        {
            areaCosts.Clear();
        }
    }
}
