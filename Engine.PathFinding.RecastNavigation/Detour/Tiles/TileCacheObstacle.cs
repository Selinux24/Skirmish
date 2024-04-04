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
        public void Rasterize(TileCacheBuildContext bc, CompressedTile tile, float cellSize, float cellHeight)
        {
            if (State == ObstacleState.DT_OBSTACLE_EMPTY || State == ObstacleState.DT_OBSTACLE_REMOVING)
            {
                return;
            }

            if (!Touched.Contains(tile))
            {
                return;
            }

            Obstacle.MarkArea(bc, tile.Header.Bounds.Minimum, cellSize, cellHeight, 0);
        }
        /// <summary>
        /// Process the obstacle
        /// </summary>
        /// <param name="r">Tile</param>
        /// <param name="next">Next free obstacle index</param>
        public bool ProcessUpdate(CompressedTile r, int next)
        {
            // Remove handled tile from pending list.
            Pending.Remove(r);

            // If all pending tiles processed, change state.
            if (Pending.Count == 0)
            {
                if (State == ObstacleState.DT_OBSTACLE_PROCESSING)
                {
                    State = ObstacleState.DT_OBSTACLE_PROCESSED;
                }
                else if (State == ObstacleState.DT_OBSTACLE_REMOVING)
                {
                    State = ObstacleState.DT_OBSTACLE_EMPTY;
                    // Update salt, salt should never be zero.
                    Salt = (Salt + 1) & ((1 << 16) - 1);
                    if (Salt == 0)
                    {
                        Salt++;
                    }

                    // Return obstacle to free list.
                    Next = next;

                    return true;
                }
            }

            return false;
        }
    }
}
