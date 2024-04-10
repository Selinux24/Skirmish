using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Find polygons around a shape query
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="vertices"></param>
    public class FindPolysAroundShapeQuery(Vector3[] vertices) : IFindPolysQuery
    {
        /// <summary>
        /// Shape vertices (polygon)
        /// </summary>
        private readonly Vector3[] vertices = vertices;

        /// <inheritdoc/>
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
