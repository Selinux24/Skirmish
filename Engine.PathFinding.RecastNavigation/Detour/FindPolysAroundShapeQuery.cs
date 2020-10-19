using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Find polygons around a shape query
    /// </summary>
    public class FindPolysAroundShapeQuery : IFindPolysQuery
    {
        /// <summary>
        /// Shape vertices (polygon)
        /// </summary>
        public IEnumerable<Vector3> Vertices { get; set; }

        /// <summary>
        /// Performs the query against a segment
        /// </summary>
        /// <param name="va">Segment point A</param>
        /// <param name="vb">Segment point B</param>
        /// <returns>Returns true if the query passes</returns>
        public bool Contains(Vector3 va, Vector3 vb)
        {
            // If the poly is not touching the edge to the next polygon, skip the connection it.
            if (!DetourUtils.IntersectSegmentPoly2D(va, vb, Vertices, out float tmin, out float tmax, out _, out _))
            {
                return false;
            }
            if (tmin > 1.0f || tmax < 0.0f)
            {
                return false;
            }

            return true;
        }
    }
}
