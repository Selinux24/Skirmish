using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Provides information about raycast hit
    /// </summary>
    public class RaycastHit
    {
        private readonly List<int> path = new List<int>();

        /// <summary>
        /// The hit parameter. (FLT_MAX if no wall hit.)
        /// </summary>
        public float T { get; set; }
        /// <summary>
        /// The normal of the nearest wall hit. [(x, y, z)]
        /// </summary>
        public Vector3 HitNormal { get; set; }
        /// <summary>
        /// The index of the edge on the final polygon where the wall was hit.
        /// </summary>
        public int HitEdgeIndex { get; set; }
        /// <summary>
        /// Pointer to an array of reference ids of the visited polygons. [opt]
        /// </summary>
        public int[] Path
        {
            get
            {
                return path.ToArray();
            }
        }
        /// <summary>
        /// The number of visited polygons. [opt]
        /// </summary>
        public int PathCount
        {
            get
            {
                return path.Count;
            }
        }
        /// <summary>
        /// The maximum number of polygons the @p path array can hold.
        /// </summary>
        public int MaxPath { get; set; }
        /// <summary>
        /// The cost of the path until hit.
        /// </summary>
        public float PathCost { get; set; }
        /// <summary>
        /// Previous polygon reference
        /// </summary>
        public int PrevReference { get; set; }

        /// <summary>
        /// Adds a polygon reference to the ray-cast path
        /// </summary>
        /// <param name="r">Polygin reference</param>
        public void Add(int r)
        {
            this.path.Add(r);
        }
        /// <summary>
        /// Cuts the polygon reference list to the specified length
        /// </summary>
        /// <param name="length">Final length</param>
        public void Cut(int length)
        {
            if (this.path.Count > length)
            {
                var tmp = this.path.Take(length);

                this.path.Clear();
                this.path.AddRange(tmp);
            }
        }
        /// <summary>
        /// Creates a simple path
        /// </summary>
        /// <returns>Returns a simple path</returns>
        public SimplePath CreateSimplePath()
        {
            SimplePath res = new SimplePath(MaxPath);
            res.StartPath(Path);

            return res;
        }
    }
}
