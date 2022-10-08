using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Find polygons around a circle
    /// </summary>
    public class FindPolysAroundCircleQuery : IFindPolysQuery
    {
        /// <summary>
        /// Circle center
        /// </summary>
        public Vector3 Center { get; set; }
        /// <summary>
        /// Circle radius
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// Performs the query against a segment
        /// </summary>
        /// <param name="va">Segment point A</param>
        /// <param name="vb">Segment point B</param>
        /// <returns>Returns true if the query passes</returns>
        public bool Contains(Vector3 va, Vector3 vb)
        {
            // If the circle is not touching the next polygon, skip it.
            float distSqr = DetourUtils.DistancePtSegSqr2D(Center, va, vb, out _);
            if (distSqr > (Radius * Radius))
            {
                return false;
            }

            return true;
        }
    }
}
