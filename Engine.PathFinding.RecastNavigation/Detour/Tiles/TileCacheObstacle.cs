using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile-cache obstacle processor
    /// </summary>
    public class TileCacheObstacle
    {
        /// <summary>
        /// Touched tile list
        /// </summary>
        public List<CompressedTile> Touched { get; set; } = new List<CompressedTile>();
        /// <summary>
        /// Pending tile list
        /// </summary>
        public List<CompressedTile> Pending { get; set; } = new List<CompressedTile>();
        /// <summary>
        /// Salt
        /// </summary>
        public int Salt { get; set; }
        /// <summary>
        /// State
        /// </summary>
        public ObstacleState State { get; set; }
        /// <summary>
        /// Next obstacle in the queue
        /// </summary>
        public int Next { get; set; }
        /// <summary>
        /// Obstacle descriptor
        /// </summary>
        public IObstacle Obstacle { get; set; }

        /// <summary>
        /// Gets the obstacle descriptor bounds
        /// </summary>
        /// <returns></returns>
        public BoundingBox GetObstacleBounds()
        {
            return Obstacle.GetBounds();
        }
        /// <summary>
        /// Rasterizes the obstacle descriptor
        /// </summary>
        /// <param name="bc">Build context</param>
        /// <param name="tile">Tile</param>
        /// <param name="cellSize">Cell size</param>
        /// <param name="cellHeight">Cell height</param>
        public void Rasterize(NavMeshTileBuildContext bc, CompressedTile tile, float cellSize, float cellHeight)
        {
            if (State == ObstacleState.Empty || State == ObstacleState.Removing)
            {
                return;
            }

            if (!Touched.Contains(tile))
            {
                return;
            }

            Obstacle.MarkArea(bc, tile.Header.BBox.Minimum, cellSize, cellHeight, 0);
        }
    }
}
