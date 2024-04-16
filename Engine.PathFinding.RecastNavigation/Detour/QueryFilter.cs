using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Defines polygon filtering and traversal costs for navigation mesh query operations.
    /// </summary>
    public static class QueryFilterExtensions
    {
        /// <summary>
        /// Gets the default query filter
        /// </summary>
        public static QueryFilter Default
        {
            get
            {
                QueryFilter filter = new();

                filter.SetIncludeFlags(SamplePolyFlagTypes.Walk);
                filter.SetExcludeFlags(SamplePolyFlagTypes.None);

                filter.SetAreaCost(SamplePolyAreas.Ground, 1.0f);
                filter.SetAreaCost(SamplePolyAreas.Water, 10.0f);
                filter.SetAreaCost(SamplePolyAreas.Road, 1.0f);
                filter.SetAreaCost(SamplePolyAreas.Door, 1.0f);
                filter.SetAreaCost(SamplePolyAreas.Grass, 2.0f);
                filter.SetAreaCost(SamplePolyAreas.Jump, 1.5f);

                return filter;
            }
        }

        /// <summary>
        /// Returns cost to move from the beginning to the end of a line segment that is fully contained within a polygon.
        /// </summary>
        /// <param name="pa">The start position on the edge of the previous and current polygon. [(x, y, z)]</param>
        /// <param name="pb">The end position on the edge of the current and next polygon. [(x, y, z)]</param>
        /// <param name="prev">The tile containing the previous polygon. [opt]</param>
        /// <param name="cur">The tile containing the current polygon.</param>
        /// <param name="next">The tile containing the next polygon. [opt]</param>
        /// <returns></returns>
        public static float GetCost(this QueryFilter filter, Vector3 pa, Vector3 pb, TileRef prev, TileRef cur, TileRef next)
        {
            float dist = Vector3.Distance(pa, pb);

            float cost = filter.GetAreaCost(cur.Poly.Area);

            return dist * cost;
        }
    }
}
