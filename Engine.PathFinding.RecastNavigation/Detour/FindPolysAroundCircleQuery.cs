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

        /// <inheritdoc/>
        public bool Contains(Vector3 va, Vector3 vb)
        {
            // If the circle is not touching the next polygon, skip it.
            float distSqr = Utils.DistancePtSegSqr2D(Center, va, vb, out _);
            if (distSqr > (Radius * Radius))
            {
                return false;
            }

            return true;
        }
    }
}
