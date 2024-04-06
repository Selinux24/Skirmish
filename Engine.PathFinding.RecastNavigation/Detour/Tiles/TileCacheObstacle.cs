using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile-cache obstacle processor
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="obstacle">Obstacle</param>
    /// <param name="salt">Salt value</param>
    /// <param name="state">Initial state</param>
    public class TileCacheObstacle(IObstacle obstacle, int salt, ObstacleState state = ObstacleState.DT_OBSTACLE_EMPTY)
    {
        /// <summary>
        /// Obstacle descriptor
        /// </summary>
        private readonly IObstacle obstacle = obstacle;

        /// <summary>
        /// Salt
        /// </summary>
        public int Salt { get; set; } = salt;
        /// <summary>
        /// State
        /// </summary>
        public ObstacleState State { get; set; } = state;
        /// <summary>
        /// Next obstacle in the queue
        /// </summary>
        public int Next { get; set; }
        /// <summary>
        /// Touched tile list
        /// </summary>
        public List<CompressedTile> Touched { get; set; } = [];
        /// <summary>
        /// Pending tile list
        /// </summary>
        public List<CompressedTile> Pending { get; set; } = [];

        /// <summary>
        /// Gets the obstacle descriptor bounds
        /// </summary>
        /// <returns></returns>
        public BoundingBox GetObstacleBounds()
        {
            return obstacle.GetBounds();
        }
        /// <summary>
        /// Rasterizes the obstacle descriptor
        /// </summary>
        /// <param name="tlayer">Layer</param>
        /// <param name="orig">Origin</param>
        /// <param name="cellSize">Cell size</param>
        /// <param name="cellHeight">Cell height</param>
        public void Rasterize(ref TileCacheLayer tlayer, Vector3 orig, float cellSize, float cellHeight)
        {
            if (State == ObstacleState.DT_OBSTACLE_EMPTY || State == ObstacleState.DT_OBSTACLE_REMOVING)
            {
                return;
            }

            obstacle.MarkArea(ref tlayer, orig, cellSize, cellHeight, 0);
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
