
namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Provides custom polygon query behavior.
    /// </summary>
    public interface IPolyQuery
    {
        /// <summary>
        /// Called for each batch of unique polygons touched by the search area in queryPolygons.
        /// This can be called multiple times for a single query.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="polys"></param>
        /// <param name="refs"></param>
        /// <param name="count"></param>
        void Process(MeshTile tile, Poly[] polys, int[] refs, int count);
    }
}
