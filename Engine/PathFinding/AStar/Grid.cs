using SharpDX;
using System;

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
                Helper.Dispose(this.BuildSettings);
                Helper.Dispose(this.Input);
                Helper.Dispose(this.Nodes);
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
                if (this.Nodes[i].Contains(point, out float distance))
                {
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestNode = this.Nodes[i];
                    }
                }
            }

            return bestNode;
        }

        /// <summary>
        /// Gets the node collection of the grid
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <returns>Returns the node collection of the grid</returns>
        public IGraphNode[] GetNodes(AgentType agent)
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
        public Vector3[] FindPath(AgentType agent, Vector3 from, Vector3 to)
        {
            return AStarQuery.FindPath(this, from, to);
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
            nearest = null;

            for (int i = 0; i < this.Nodes.Length; i++)
            {
                float distance;
                if (this.Nodes[i].Contains(position, out distance))
                {
                    nearest = this.Nodes[i].Center;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Updates the graph at specified position
        /// </summary>
        /// <param name="position">Position</param>
        public void UpdateAt(Vector3 position)
        {

        }

        /// <summary>
        /// Adds a cylinder obstacle
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="radius">Radius</param>
        /// <param name="height">Height</param>
        /// <returns>Returns the obstacle id</returns>
        public int AddObstacle(Vector3 position, float radius, float height)
        {
            return -1;
        }
        /// <summary>
        /// Adds a oriented bounding box obstacle
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="halfExtents">half extent vectors</param>
        /// <param name="yRotation">Rotation in the y axis</param>
        /// <returns>Returns the obstacle id</returns>
        public int AddObstacle(Vector3 position, Vector3 halfExtents, float yRotation)
        {
            return -1;
        }
        /// <summary>
        /// Adds a bounding box obstacle
        /// </summary>
        /// <param name="minimum">Minimum corner</param>
        /// <param name="maximum">Maximum corner</param>
        /// <returns>Returns the obstacle id</returns>
        public int AddObstacle(Vector3 minimum, Vector3 maximum)
        {
            return -1;
        }
        /// <summary>
        /// Removes an obstacle by obstacle id
        /// </summary>
        /// <param name="obstacle">Obstacle id</param>
        public void RemoveObstacle(int obstacle)
        {

        }

        /// <summary>
        /// Adds a new connection
        /// </summary>
        /// <param name="from">From point</param>
        /// <param name="to">To point</param>
        /// <returns>Returns the connection id</returns>
        public int AddConnection(Vector3 from, Vector3 to)
        {
            return -1;
        }
        /// <summary>
        /// Removes a connection by id
        /// </summary>
        /// <param name="id">Connection id</param>
        public void RemoveConnection(int id)
        {

        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {

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
