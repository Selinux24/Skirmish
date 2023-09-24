using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Collect polygon guery
    /// </summary>
    public class CollectPolysQuery : IPolyQuery
    {
        /// <summary>
        /// Polygon list
        /// </summary>
        public int[] Polys { get; protected set; }
        /// <summary>
        /// Maximum number of polygons
        /// </summary>
        public int MaxPolys { get; protected set; }
        /// <summary>
        /// Number of collected polygons
        /// </summary>
        public int NumCollected { get; protected set; }
        /// <summary>
        /// Overflow flag
        /// </summary>
        public bool Overflow { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="polys">Polygon list</param>
        /// <param name="maxPolys">Maximum polygon count</param>
        public CollectPolysQuery(int[] polys, int maxPolys)
        {
            Polys = polys;
            MaxPolys = maxPolys;
            NumCollected = 0;
            Overflow = false;
        }

        /// <inheritdoc/>
        public void Process(MeshTile tile, IEnumerable<int> refs)
        {
            if (refs?.Any() != true)
            {
                return;
            }

            int numLeft = MaxPolys - NumCollected;
            int toCopy = refs.Count();
            if (toCopy > numLeft)
            {
                Overflow = true;
                toCopy = numLeft;
            }

            Array.Copy(refs.ToArray(), 0, Polys, NumCollected, toCopy);

            NumCollected += toCopy;
        }
    }
}
