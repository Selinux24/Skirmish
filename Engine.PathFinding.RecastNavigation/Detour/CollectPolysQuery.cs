using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Collect polygon guery
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="polys">Polygon list</param>
    /// <param name="maxPolys">Maximum polygon count</param>
    public class CollectPolysQuery(int[] polys, int maxPolys) : IPolyQuery
    {
        /// <summary>
        /// Polygon list
        /// </summary>
        private readonly int[] polys = polys;
        /// <summary>
        /// Maximum number of polygons
        /// </summary>
        private readonly int maxPolys = maxPolys;
        /// <summary>
        /// Number of collected polygons
        /// </summary>
        private int numCollected = 0;

        /// <inheritdoc/>
        public void Process(MeshTile tile, IEnumerable<int> refs)
        {
            if (refs?.Any() != true)
            {
                return;
            }

            int numLeft = maxPolys - numCollected;
            int toCopy = refs.Count();
            if (toCopy > numLeft)
            {
                toCopy = numLeft;
            }

            Array.Copy(refs.ToArray(), 0, polys, numCollected, toCopy);

            numCollected += toCopy;
        }
    }
}
