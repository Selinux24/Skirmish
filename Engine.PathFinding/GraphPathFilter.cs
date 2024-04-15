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
        private readonly Dictionary<GraphConnectionAreaTypes, float> areaCosts = [];

        /// <summary>
        /// Sets the cost for the specified area
        /// </summary>
        /// <param name="area">Area</param>
        /// <param name="cost">Cost</param>
        public void SetCost(GraphConnectionAreaTypes area, float cost)
        {
            if (areaCosts.TryAdd(area, cost))
            {
                return;
            }

            areaCosts[area] = cost;
        }
        /// <summary>
        /// Gets the cost of the specified area
        /// </summary>
        /// <param name="area">Area</param>
        /// <returns>Returns the cost</returns>
        public float GetCost(GraphConnectionAreaTypes area)
        {
            if (areaCosts.TryGetValue(area, out float cost))
            {
                return cost;
            }

            return 1f;
        }

        /// <summary>
        /// Gets the area cost list
        /// </summary>
        public IEnumerable<(GraphConnectionAreaTypes Area, float Cost)> GetAreaCosts()
        {
            foreach (var item in areaCosts)
            {
                yield return (item.Key, item.Value);
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
