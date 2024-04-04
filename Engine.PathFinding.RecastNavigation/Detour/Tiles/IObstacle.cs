using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Onstacle interface
    /// </summary>
    public interface IObstacle
    {
        /// <summary>
        /// Gets the obstacle bounds
        /// </summary>
        /// <returns>Returns a bounding box</returns>
        BoundingBox GetBounds();
        /// <summary>
        /// Marks the build context area with the specified area type
        /// </summary>
        /// <param name="tc">Build context</param>
        /// <param name="orig">Origin</param>
        /// <param name="cs">Cell size</param>
        /// <param name="ch">Cell height</param>
        /// <param name="area">Area type</param>
        /// <returns>Returns true if all layer areas were marked</returns>
        bool MarkArea(TileCacheBuildContext tc, Vector3 orig, float cs, float ch, AreaTypes area);
    }
}
