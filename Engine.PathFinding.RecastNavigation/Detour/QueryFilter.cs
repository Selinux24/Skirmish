using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Defines polygon filtering and traversal costs for navigation mesh query operations.
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
            SetIncludeFlags(SamplePolyFlagTypes.Walk);
            SetExcludeFlags(SamplePolyFlagTypes.None);

            SetAreaCost(SamplePolyAreas.Ground, 1.0f);
            SetAreaCost(SamplePolyAreas.Water, 10.0f);
            SetAreaCost(SamplePolyAreas.Road, 1.0f);
            SetAreaCost(SamplePolyAreas.Door, 1.0f);
            SetAreaCost(SamplePolyAreas.Grass, 2.0f);
            SetAreaCost(SamplePolyAreas.Jump, 1.5f);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filter">Path filter</param>
        public QueryFilter(GraphPathFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            foreach (var (area, cost) in filter.GetAreaCosts())
            {
                SetAreaCost((int)area, cost);
            }
        }

        /// <summary>
        /// Returns true if the polygon can be visited. (I.e. Is traversable.)
        /// </summary>
        /// <param name="polyFlags">Sample polygon flags.</param>
        /// <returns>Returns true if the filter pass</returns>
        public virtual bool PassFilter(int polyFlags)
        {
            return
                (polyFlags & includeFlags) != 0 &&
                (polyFlags & excludeFlags) == 0;
        }
        /// <summary>
        /// Returns true if the polygon can be visited. (I.e. Is traversable.)
        /// </summary>
        /// <param name="polyFlags">Sample polygon flags.</param>
        /// <returns>Returns true if the filter pass</returns>
        public virtual bool PassFilter<T>(T polyFlags) where T : Enum
        {
            return PassFilter((int)(object)polyFlags);
        }
        /// <summary>
        /// Returns cost to move from the beginning to the end of a line segment that is fully contained within a polygon.
        /// </summary>
        /// <param name="pa">The start position on the edge of the previous and current polygon. [(x, y, z)]</param>
        /// <param name="pb">The end position on the edge of the current and next polygon. [(x, y, z)]</param>
        /// <param name="prev">The tile containing the previous polygon. [opt]</param>
        /// <param name="cur">The tile containing the current polygon.</param>
        /// <param name="next">The tile containing the next polygon. [opt]</param>
        /// <returns></returns>
        public virtual float GetCost(Vector3 pa, Vector3 pb, TileRef prev, TileRef cur, TileRef next)
        {
            float dist = Vector3.Distance(pa, pb);

            if (!areaCosts.TryGetValue((int)cur.Poly.Area, out float cost))
            {
                return dist;
            }

            return dist * cost;
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
