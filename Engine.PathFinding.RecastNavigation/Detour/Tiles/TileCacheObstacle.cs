using SharpDX;
using System.Collections.Generic;
using System.Linq;

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
        /// Touched tile list
        /// </summary>
        private readonly List<CompressedTile> touched = [];
        /// <summary>
        /// Pending tile list
        /// </summary>
        private readonly List<CompressedTile> pending = [];

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
            pending.Remove(r);

            // If all pending tiles processed, change state.
            if (pending.Count == 0)
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

        /// <summary>
        /// Gets the touched tile list
        /// </summary>
        public CompressedTile[] GetTouched()
        {
            return [.. touched];
        }
        /// <summary>
        /// Adds the specified tile list to the touched tile list
        /// </summary>
        /// <param name="tiles">Tile list</param>
        public void AddTouchedTiles(IEnumerable<CompressedTile> tiles)
        {
            if (tiles?.Any() == false)
            {
                return;
            }

            touched.AddRange(tiles);
        }
        /// <summary>
        /// Gets whether the specified tile is in the touched tile list
        /// </summary>
        /// <param name="tile">Tile</param>
        public bool ContainsTouched(CompressedTile tile)
        {
            if (tile == null)
            {
                return false;
            }

            return touched.Contains(tile);
        }

        /// <summary>
        /// Adds the specified tile to the pending tile list
        /// </summary>
        /// <param name="tile">Tile</param>
        public void AddPendingTile(CompressedTile tile)
        {
            if (tile == null)
            {
                return;
            }

            pending.Add(tile);
        }

        /// <summary>
        /// Begins the process request of the specified tile list
        /// </summary>
        /// <param name="tiles">Tile list</param>
        public void BeginRequest(IEnumerable<CompressedTile> tiles)
        {
            pending.Clear();

            if (tiles?.Any() == false)
            {
                return;
            }

            AddTouchedTiles(tiles);
        }
        /// <summary>
        /// Begins the removal of the obstacle
        /// </summary>
        public void BeginRemove()
        {
            pending.Clear();

            State = ObstacleState.DT_OBSTACLE_REMOVING;
        }

        /// <summary>
        /// Gets whether the obstacle is untouched after its process
        /// </summary>
        public bool IsUntouched()
        {
            return State == ObstacleState.DT_OBSTACLE_PROCESSED && touched.Count == 0;
        }
        /// <summary>
        /// Gets whether the obstacle is process pending
        /// </summary>
        public bool IsPending()
        {
            return State == ObstacleState.DT_OBSTACLE_PROCESSING || State == ObstacleState.DT_OBSTACLE_REMOVING;
        }
    }
}
