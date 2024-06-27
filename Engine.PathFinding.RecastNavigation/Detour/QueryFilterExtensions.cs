using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Defines polygon filtering and traversal costs for navigation mesh query operations.
    /// </summary>
    public static class QueryFilterExtensions
    {
        /// <summary>
        /// Returns cost to move from the beginning to the end of a line segment that is fully contained within a polygon.
        /// </summary>
        /// <param name="pa">The start position on the edge of the previous and current polygon. [(x, y, z)]</param>
        /// <param name="pb">The end position on the edge of the current and next polygon. [(x, y, z)]</param>
        /// <param name="prev">The tile containing the previous polygon. [opt]</param>
        /// <param name="cur">The tile containing the current polygon.</param>
        /// <param name="next">The tile containing the next polygon. [opt]</param>
        /// <returns></returns>
        public static float GetCost(this IGraphQueryFilter filter, Vector3 pa, Vector3 pb, TileRef prev, TileRef cur, TileRef next)
        {
            if (prev.Ref == TileRef.Null.Ref)
            {
                Logger.WriteInformation(nameof(QueryFilterExtensions), "Getting first path node costs");
            }

            if (next.Ref == TileRef.Null.Ref)
            {
                Logger.WriteInformation(nameof(QueryFilterExtensions), "Getting last path node costs");
            }

            float dist = Vector3.Distance(pa, pb);

            float cost = filter.GetAreaCost(cur.Poly.Area);

            return dist * cost;
        }
    }
}
