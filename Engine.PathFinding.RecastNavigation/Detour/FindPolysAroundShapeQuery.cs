using SharpDX;

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
        private readonly Vector3[] vertices;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="vertices"></param>
        public FindPolysAroundShapeQuery(Vector3[] vertices)
        {
            this.vertices = vertices;
        }

        /// <summary>
        /// Performs the query against a segment
        /// </summary>
        /// <param name="va">Segment point A</param>
        /// <param name="vb">Segment point B</param>
        /// <returns>Returns true if the query passes</returns>
        public bool Contains(Vector3 va, Vector3 vb)
        {
            // If the poly is not touching the edge to the next polygon, skip the connection it.
            if (!Utils.IntersectSegmentPoly2D(va, vb, vertices, out float tmin, out float tmax, out _, out _))
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
