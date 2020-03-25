using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engine.PathFinding
{
    /// <summary>
    /// Pathfinding graph interface
    /// </summary>
    public interface IGraph : IDisposable
    {
        /// <summary>
        /// Gets whether the graph is initialized
        /// </summary>
        bool Initialized { get; }
        /// <summary>
        /// Gets the node collection of the graph for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <returns>Returns the node collection for the agent type</returns>
        IEnumerable<IGraphNode> GetNodes(AgentType agent);
        /// <summary>
        /// Find path from point to point for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <returns>Return path if exists</returns>
        IEnumerable<Vector3> FindPath(AgentType agent, Vector3 from, Vector3 to);
        /// <summary>
        /// Find path from point to point for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <returns>Return path if exists</returns>
        Task<IEnumerable<Vector3>> FindPathAsync(AgentType agent, Vector3 from, Vector3 to);
        /// <summary>
        /// Gets wether the specified position is walkable for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="position">Position</param>
        /// <returns>Returns true if the specified position is walkable</returns>
        bool IsWalkable(AgentType agent, Vector3 position);
        /// <summary>
        /// Gets wether the specified position is walkable for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="position">Position</param>
        /// <param name="nearest">Gets the nearest walkable position</param>
        /// <returns>Returns true if the specified position is walkable</returns>
        bool IsWalkable(AgentType agent, Vector3 position, out Vector3? nearest);

        /// <summary>
        /// Updates the graph at specified position
        /// </summary>
        /// <param name="position">Position</param>
        void UpdateAt(Vector3 position);
        /// <summary>
        /// Updates the graph at specified position list
        /// </summary>
        /// <param name="positions">Position list</param>
        void UpdateAt(IEnumerable<Vector3> positions);
        /// <summary>
        /// Removes the graph node at specified position
        /// </summary>
        /// <param name="position">Position</param>
        void RemoveAt(Vector3 position);
        /// <summary>
        /// Removes the graph node at specified position list
        /// </summary>
        /// <param name="positions">Position list</param>
        void RemoveAt(IEnumerable<Vector3> positions);

        /// <summary>
        /// Adds a cylinder obstacle
        /// </summary>
        /// <param name="cylinder">Bounding Cylinder</param>
        /// <returns>Returns the obstacle id</returns>
        int AddObstacle(BoundingCylinder cylinder);
        /// <summary>
        /// Adds a bounding box obstacle
        /// </summary>
        /// <param name="bbox">Bounding Box</param>
        /// <returns>Returns the obstacle id</returns>
        int AddObstacle(BoundingBox bbox);
        /// <summary>
        /// Adds a oriented bounding box obstacle
        /// </summary>
        /// <param name="obb">Oriented Bounding Box</param>
        /// <returns>Returns the obstacle id</returns>
        /// <remarks>Only applies rotation if the obb's transform has rotation in the Y axis</remarks>
        int AddObstacle(OrientedBoundingBox obb);
        /// <summary>
        /// Removes an obstacle by obstacle id
        /// </summary>
        /// <param name="obstacleId">Obstacle id</param>
        void RemoveObstacle(int obstacleId);

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        void Update(GameTime gameTime);

        /// <summary>
        /// On graph updating event
        /// </summary>
        event EventHandler Updating;
        /// <summary>
        /// On graph updated event
        /// </summary>
        event EventHandler Updated;
    }
}
