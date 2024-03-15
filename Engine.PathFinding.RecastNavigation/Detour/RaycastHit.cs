using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Provides information about raycast hit
    /// </summary>
    public class RaycastHit
    {
        private readonly List<int> path = [];

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
                return [.. path];
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
        /// Prepares the hit data
        /// </summary>
        /// <param name="n">Path counter</param>
        /// <param name="cur">Tile</param>
        /// <param name="tmax">Maximum distance</param>
        /// <param name="segMax">Maximum segment</param>
        public bool PrepareHitData(ref int n, TileRef cur, float tmax, int segMax)
        {
            HitEdgeIndex = segMax;

            // Keep track of furthest t so far.
            T = Math.Max(T, tmax);

            // Store visited polygons.
            if (n < MaxPath)
            {
                Add(cur.Ref);

                n++;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Adds a polygon reference to the ray-cast path
        /// </summary>
        /// <param name="r">Polygin reference</param>
        public void Add(int r)
        {
            path.Add(r);
        }
        /// <summary>
        /// Cuts the polygon reference list to the specified length
        /// </summary>
        /// <param name="length">Final length</param>
        public void Cut(int length)
        {
            if (path.Count > length)
            {
                var tmp = path.Take(length);

                path.Clear();
                path.AddRange(tmp);
            }
        }
        /// <summary>
        /// Creates a simple path
        /// </summary>
        /// <returns>Returns a simple path</returns>
        public SimplePath CreateSimplePath()
        {
            var res = new SimplePath(MaxPath);
            res.StartPath(Path);

            return res;
        }
    }
}
