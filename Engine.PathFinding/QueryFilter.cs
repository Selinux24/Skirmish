using System;
using System.Collections.Generic;

namespace Engine.PathFinding
{
    /// <summary>
    /// Path filter
    /// </summary>
    public class QueryFilter
    {
        /// <summary>
        /// Area - costs dictionary
        /// </summary>
        private readonly Dictionary<int, float> areaCosts = [];
        /// <summary>
        /// Flags for polygons that can be visited. (Used by default implementation.)
        /// </summary>
        private int includeFlags = int.MaxValue;
        /// <summary>
        /// Flags for polygons that should not be visted. (Used by default implementation.)
        /// </summary>
        private int excludeFlags = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public QueryFilter()
        {

        }

        /// <summary>
        /// Returns true if the polygon can be visited. (I.e. Is traversable.)
        /// </summary>
        /// <param name="polyFlags">Sample polygon flags.</param>
        /// <returns>Returns true if the filter pass</returns>
        public bool PassFilter(int polyFlags)
        {
            return
                (polyFlags & GetIncludeFlags()) != 0 &&
                (polyFlags & GetExcludeFlags()) == 0;
        }
        /// <summary>
        /// Returns true if the polygon can be visited. (I.e. Is traversable.)
        /// </summary>
        /// <param name="polyFlags">Sample polygon flags.</param>
        /// <returns>Returns true if the filter pass</returns>
        public bool PassFilter<T>(T polyFlags) where T : Enum
        {
            return PassFilter((int)(object)polyFlags);
        }

        /// <summary>
        /// Returns the traversal cost of the area.
        /// </summary>
        /// <param name="i">The id of the area.</param>
        /// <returns>The traversal cost of the area.</returns>
        public float GetAreaCost(int i)
        {
            if (!areaCosts.TryGetValue(i, out float cost))
            {
                return 1;
            }

            return cost;
        }
        /// <summary>
        /// Returns the traversal cost of the area.
        /// </summary>
        /// <param name="i">The id of the area.</param>
        /// <returns>The traversal cost of the area.</returns>
        public float GetAreaCost<T>(T i) where T : Enum
        {
            return GetAreaCost((int)(object)i);
        }
        /// <summary>
        /// Sets the traversal cost of the area.
        /// </summary>
        /// <param name="i">The id of the area.</param>
        /// <param name="cost">The new cost of traversing the area.</param>
        public void SetAreaCost(int i, float cost)
        {
            if (areaCosts.TryAdd(i, cost))
            {
                return;
            }

            areaCosts[i] = cost;
        }
        /// <summary>
        /// Sets the traversal cost of the area.
        /// </summary>
        /// <param name="i">The id of the area.</param>
        /// <param name="cost">The new cost of traversing the area.</param>
        public void SetAreaCost<T>(T i, float cost) where T : Enum
        {
            SetAreaCost((int)(object)i, cost);
        }
        /// <summary>
        /// Clears the area-costs list
        /// </summary>
        public void ClearCosts()
        {
            areaCosts.Clear();
        }

        /// <summary>
        /// Returns the include flags for the filter.
        /// Any polygons that include one or more of these flags will be included in the operation.
        /// </summary>
        /// <returns></returns>
        public int GetIncludeFlags()
        {
            return includeFlags;
        }
        /// <summary>
        /// Returns the include flags for the filter.
        /// Any polygons that include one or more of these flags will be included in the operation.
        /// </summary>
        /// <returns></returns>
        public T GetIncludeFlags<T>() where T : Enum
        {
            return (T)(object)includeFlags;
        }
        /// <summary>
        /// Sets the include flags for the filter.
        /// </summary>
        /// <param name="flags">The new flags.</param>
        public void SetIncludeFlags(int flags)
        {
            includeFlags = flags;
        }
        /// <summary>
        /// Sets the include flags for the filter.
        /// </summary>
        /// <param name="flags">The new flags.</param>
        public void SetIncludeFlags<T>(T flags) where T : Enum
        {
            SetIncludeFlags((int)(object)flags);
        }

        /// <summary>
        /// Returns the exclude flags for the filter.
        /// Any polygons that include one ore more of these flags will be excluded from the operation.
        /// </summary>
        /// <returns></returns>
        public int GetExcludeFlags()
        {
            return excludeFlags;
        }
        /// <summary>
        /// Returns the exclude flags for the filter.
        /// Any polygons that include one ore more of these flags will be excluded from the operation.
        /// </summary>
        /// <returns></returns>
        public T GetExcludeFlags<T>() where T : Enum
        {
            return (T)(object)excludeFlags;
        }
        /// <summary>
        /// Sets the exclude flags for the filter.
        /// </summary>
        /// <param name="flags">The new flags.</param>
        public void SetExcludeFlags(int flags)
        {
            excludeFlags = flags;
        }
        /// <summary>
        /// Sets the exclude flags for the filter.
        /// </summary>
        /// <param name="flags">The new flags.</param>
        public void SetExcludeFlags<T>(T flags) where T : Enum
        {
            SetExcludeFlags((int)(object)flags);
        }
    }
}
