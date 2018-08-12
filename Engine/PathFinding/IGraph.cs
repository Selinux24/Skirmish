using SharpDX;
using System;

namespace Engine.PathFinding
{
    /// <summary>
    /// Pathfinding graph interface
    /// </summary>
    public interface IGraph : IDisposable
    {
        /// <summary>
        /// Gets the node collection of the graph for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <returns>Returns the node collection for the agent type</returns>
        IGraphNode[] GetNodes(AgentType agent);
        /// <summary>
        /// Find path from point to point for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <returns>Return path if exists</returns>
        Vector3[] FindPath(AgentType agent, Vector3 from, Vector3 to);
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
        /// Adds a cylinder obstacle
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="radius">Radius</param>
        /// <param name="height">Height</param>
        /// <returns>Returns the obstacle id</returns>
        int AddObstacle(Vector3 position, float radius, float height);
        /// <summary>
        /// Adds a oriented bounding box obstacle
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="halfExtents">half extent vectors</param>
        /// <param name="yRotation">Rotation in the y axis</param>
        /// <returns>Returns the obstacle id</returns>
        int AddObstacle(Vector3 position, Vector3 halfExtents, float yRotation);
        /// <summary>
        /// Adds a bounding box obstacle
        /// </summary>
        /// <param name="minimum">Minimum corner</param>
        /// <param name="maximum">Maximum corner</param>
        /// <returns>Returns the obstacle id</returns>
        int AddObstacle(Vector3 minimum, Vector3 maximum);
        /// <summary>
        /// Removes an obstacle by obstacle id
        /// </summary>
        /// <param name="obstacle">Obstacle id</param>
        void RemoveObstacle(int obstacle);

        /// <summary>
        /// Adds a new connection
        /// </summary>
        /// <param name="from">From point</param>
        /// <param name="to">To point</param>
        /// <returns>Returns the connection id</returns>
        int AddConnection(Vector3 from, Vector3 to);
        /// <summary>
        /// Removes a connection by id
        /// </summary>
        /// <param name="id">Connection id</param>
        void RemoveConnection(int id);

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
