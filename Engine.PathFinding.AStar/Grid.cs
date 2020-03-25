using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engine.PathFinding.AStar
{
    /// <summary>
    /// Grid
    /// </summary>
    public class Grid : IGraph
    {
        /// <summary>
        /// On graph updating event
        /// </summary>
        public event EventHandler Updating;
        /// <summary>
        /// On graph updated event
        /// </summary>
        public event EventHandler Updated;

        /// <summary>
        /// Gets whether the graph is initialized
        /// </summary>
        public bool Initialized { get; set; }
        /// <summary>
        /// Geometry input
        /// </summary>
        public PathFinderInput Input { get; set; }
        /// <summary>
        /// Build settings
        /// </summary>
        public GridGenerationSettings BuildSettings { get; set; }
        /// <summary>
        /// Graph node list
        /// </summary>
        public GridNode[] Nodes { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Grid()
        {

        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~Grid()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Input = null;
                this.BuildSettings = null;
                this.Nodes = null;
            }
        }

        /// <summary>
        /// Gets node wich contains specified point
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Returns the node wich contains the specified point if exists</returns>
        public GridNode FindNode(Vector3 point)
        {
            float minDistance = float.MaxValue;
            GridNode bestNode = null;

            for (int i = 0; i < this.Nodes.Length; i++)
            {
                if (this.Nodes[i].Contains(point, out float distance) && distance < minDistance)
                {
                    minDistance = distance;
                    bestNode = this.Nodes[i];
                }
            }

            return bestNode;
        }

        /// <summary>
        /// Gets the node collection of the grid
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <returns>Returns the node collection of the grid</returns>
        public IEnumerable<IGraphNode> GetNodes(AgentType agent)
        {
            return Array.ConvertAll(this.Nodes, (n) => { return (IGraphNode)n; });
        }
        /// <summary>
        /// Find path from point to point
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <returns>Return path if exists</returns>
        public IEnumerable<Vector3> FindPath(AgentType agent, Vector3 from, Vector3 to)
        {
            return AStarQuery.FindPath(this, from, to);
        }
        /// <summary>
        /// Find path from point to point
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <returns>Return path if exists</returns>
        public async Task<IEnumerable<Vector3>> FindPathAsync(AgentType agent, Vector3 from, Vector3 to)
        {
            Vector3[] result = new Vector3[] { };

            await Task.Run(() =>
            {
                result = AStarQuery.FindPath(this, from, to);
            });

            return result;
        }
        /// <summary>
        /// Gets wether the specified position is walkable
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="position">Position</param>
        /// <returns>Returns true if the specified position is walkable</returns>
        public bool IsWalkable(AgentType agent, Vector3 position)
        {
            for (int i = 0; i < this.Nodes.Length; i++)
            {
                if (this.Nodes[i].Contains(position))
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets wether the specified position is walkable
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="position">Position</param>
        /// <param name="nearest">Gets the nearest walkable position</param>
        /// <returns>Returns true if the specified position is walkable</returns>
        public bool IsWalkable(AgentType agent, Vector3 position, out Vector3? nearest)
        {
            bool contains = false;
            nearest = null;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < this.Nodes.Length; i++)
            {
                if (this.Nodes[i].Contains(position, out float distance))
                {
                    contains = true;

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = this.Nodes[i].Center;
                    }
                }
            }

            return contains;
        }

        /// <summary>
        /// Updates the graph at specified position
        /// </summary>
        /// <param name="position">Position</param>
        public void UpdateAt(Vector3 position)
        {
            //TODO: update grid state around position
        }
        /// <summary>
        /// Updates the graph at specified position list
        /// </summary>
        /// <param name="positions">Position list</param>
        public void UpdateAt(IEnumerable<Vector3> positions)
        {
            //TODO: update grid state around position list
        }
        /// <summary>
        /// Removes the graph node at specified position
        /// </summary>
        /// <param name="position">Position</param>
        public void RemoveAt(Vector3 position)
        {
            //TODO: update grid state around position
        }
        /// <summary>
        /// Removes the graph node at specified position list
        /// </summary>
        /// <param name="positions">Position list</param>
        public void RemoveAt(IEnumerable<Vector3> positions)
        {
            //TODO: update grid state around position
        }

        /// <summary>
        /// Adds a cylinder obstacle
        /// </summary>
        /// <param name="cylinder">Bounding Cylinder</param>
        /// <returns>Returns the obstacle id</returns>
        public int AddObstacle(BoundingCylinder cylinder)
        {
            return -1;
        }
        /// <summary>
        /// Adds a bounding box obstacle
        /// </summary>
        /// <param name="bbox">Bounding Box</param>
        /// <returns>Returns the obstacle id</returns>
        public int AddObstacle(BoundingBox bbox)
        {
            return -1;
        }
        /// <summary>
        /// Adds a oriented bounding box obstacle
        /// </summary>
        /// <param name="obb">Oriented Bounding Box</param>
        /// <returns>Returns the obstacle id</returns>
        /// <remarks>Only applies rotation if the obb's transform has rotation in the Y axis</remarks>
        public int AddObstacle(OrientedBoundingBox obb)
        {
            return -1;
        }
        /// <summary>
        /// Removes an obstacle by obstacle id
        /// </summary>
        /// <param name="obstacleId">Obstacle id</param>
        public void RemoveObstacle(int obstacleId)
        {
            //TODO: update grid state using obstacle
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            //TODO: update grid state
        }
        /// <summary>
        /// Fires the updated event
        /// </summary>
        protected void FireUpdated()
        {
            this.Updated?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Fires the updating event
        /// </summary>
        protected void FireUpdating()
        {
            this.Updating?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation</returns>
        public override string ToString()
        {
            return string.Format("Nodes {0}; Side {1:0.00};", this.Nodes.Length, this.BuildSettings.NodeSize);
        }
    }
}
