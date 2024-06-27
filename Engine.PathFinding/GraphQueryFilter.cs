using System;
using System.Collections.Generic;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph query filter
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    [Serializable]
    public abstract class GraphQueryFilter(int defaultWalkableArea) : IGraphQueryFilter
    {
        /// <summary>
        /// Area - costs dictionary
        /// </summary>
        private readonly Dictionary<int, float> areaCosts = [];
        /// <summary>
        /// Default walkable area value
        /// </summary>
        private readonly int defaultWalkableArea = defaultWalkableArea;
        /// <summary>
        /// Flags for polygons that can be visited. (Used by default implementation.)
        /// </summary>
        private int includeFlags = int.MaxValue;
        /// <summary>
        /// Flags for polygons that should not be visted. (Used by default implementation.)
        /// </summary>
        private int excludeFlags = 0;

        /// <inheritdoc/>
        public bool PassFilter(int polyFlags)
        {
            return
                (polyFlags & GetIncludeFlags()) != 0 &&
                (polyFlags & GetExcludeFlags()) == 0;
        }
        /// <inheritdoc/>
        public bool PassFilter<T>(T polyFlags) where T : Enum
        {
            return PassFilter((int)(object)polyFlags);
        }

        /// <inheritdoc/>
        public float GetAreaCost(int i)
        {
            if (!areaCosts.TryGetValue(i, out float cost))
            {
                return 1;
            }

            return cost;
        }
        /// <inheritdoc/>
        public float GetAreaCost<T>(T i) where T : Enum
        {
            return GetAreaCost((int)(object)i);
        }
        /// <inheritdoc/>
        public void SetAreaCost(int i, float cost)
        {
            if (areaCosts.TryAdd(i, cost))
            {
                return;
            }

            areaCosts[i] = cost;
        }
        /// <inheritdoc/>
        public void SetAreaCost<T>(T i, float cost) where T : Enum
        {
            SetAreaCost((int)(object)i, cost);
        }
        /// <inheritdoc/>
        public void ClearCosts()
        {
            areaCosts.Clear();
        }

        /// <inheritdoc/>
        public int GetIncludeFlags()
        {
            return includeFlags;
        }
        /// <inheritdoc/>
        public T GetIncludeFlags<T>() where T : Enum
        {
            return (T)(object)includeFlags;
        }
        /// <inheritdoc/>
        public void SetIncludeFlags(int flags)
        {
            includeFlags = flags;
        }
        /// <inheritdoc/>
        public void SetIncludeFlags<T>(T flags) where T : Enum
        {
            SetIncludeFlags((int)(object)flags);
        }

        /// <inheritdoc/>
        public int GetExcludeFlags()
        {
            return excludeFlags;
        }
        /// <inheritdoc/>
        public T GetExcludeFlags<T>() where T : Enum
        {
            return (T)(object)excludeFlags;
        }
        /// <inheritdoc/>
        public void SetExcludeFlags(int flags)
        {
            excludeFlags = flags;
        }
        /// <inheritdoc/>
        public void SetExcludeFlags<T>(T flags) where T : Enum
        {
            SetExcludeFlags((int)(object)flags);
        }

        /// <inheritdoc/>
        public int GetDefaultWalkableAreaType()
        {
            return defaultWalkableArea;
        }
        /// <inheritdoc/>
        public T GetDefaultWalkableAreaType<T>() where T : Enum
        {
            return (T)(object)defaultWalkableArea;
        }
        /// <inheritdoc/>
        public abstract int EvaluateArea(int area);
        /// <inheritdoc/>
        public abstract TAction EvaluateArea<TArea, TAction>(TArea area) where TArea : Enum where TAction : Enum;
    }
}
