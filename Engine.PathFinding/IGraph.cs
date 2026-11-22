using Engine.Mathematics;
using System;
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
        /// Enables debug information
        /// </summary>
        bool EnableDebug { get; }

        /// <summary>
        /// Finds a random point over the graph
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <returns>Returns a valid random position over the graph</returns>
        Vector3? FindRandomPoint(AgentType agent);
        /// <summary>
        /// Finds a random point around a circle
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="position">Position</param>
        /// <param name="radius">Radius</param>
        /// <returns>Returns a valid random position over the graph</returns>
        Vector3? FindRandomPoint(AgentType agent, ref Vector3 position, float radius);
        /// <summary>
        /// Gets the node collection of the graph for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <returns>Returns the node collection for the agent type</returns>
        IGraphNode[] GetNodes(AgentType agent);
        /// <summary>
        /// Gets the node collection of the graph for the specified agent type
        /// </summary>
        /// <typeparam name="T">Node type</typeparam>
        /// <param name="agent">Agent type</param>
        /// <returns>Returns the node collection for the agent type</returns>
        T[] GetNodes<T>(AgentType agent) where T : IGraphNode;
        /// <summary>
        /// Gets node which contains specified point
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="point">Point</param>
        /// <returns>Returns the node which contains the specified point if exists</returns>
        IGraphNode FindNode(AgentType agent, ref Vector3 point);
        /// <summary>
        /// Gets node which contains specified point
        /// </summary>
        /// <typeparam name="T">Node type</typeparam>
        /// <param name="agent">Agent</param>
        /// <param name="point">Point</param>
        /// <returns>Returns the node which contains the specified point if exists</returns>
        T FindNode<T>(AgentType agent, ref Vector3 point) where T : IGraphNode;
        /// <summary>
        /// Find path from point to point for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <returns>Return path if exists</returns>
        Vector3[] FindPath(AgentType agent, ref Vector3 from, ref Vector3 to);
        /// <summary>
        /// Find path from point to point for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <returns>Return path if exists</returns>
        Task<Vector3[]> FindPathAsync(AgentType agent, Vector3 from, Vector3 to);
        /// <summary>
        /// Gets whether the specified position is walkable for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="position">Position</param>
        /// <param name="distanceThreshold">Distance threshold</param>
        /// <returns>Returns true if the specified position is walkable, and found nearest position is within the distance threshold with the specified position.</returns>
        bool IsWalkable(AgentType agent, ref Vector3 position, float distanceThreshold);
        /// <summary>
        /// Gets whether the specified position is walkable for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="position">Position</param>
        /// <param name="distanceThreshold">Distance threshold</param>
        /// <param name="nearest">Gets the nearest walkable position</param>
        /// <returns>Returns true if the specified position is walkable, and found nearest position is within the distance threshold with the specified position.</returns>
        bool IsWalkable(AgentType agent, ref Vector3 position, float distanceThreshold, out Vector3? nearest);

        /// <summary>
        /// Creates the graph at specified position
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="callback">Updating callback</param>
        void CreateAt(ref Vector3 position, Action<GraphUpdateStates> callback = null);
        /// <summary>
        /// Creates the graph at specified box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="callback">Updating callback</param>
        void CreateAt(ref BoundingBox bbox, Action<GraphUpdateStates> callback = null);
        /// <summary>
        /// Creates the graph at specified position list
        /// </summary>
        /// <param name="positions">Position list</param>
        /// <param name="callback">Updating callback</param>
        void CreateAt(Vector3[] positions, Action<GraphUpdateStates> callback = null);
        /// <summary>
        /// Updates the graph at specified position
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="callback">Updating callback</param>
        void UpdateAt(ref Vector3 position, Action<GraphUpdateStates> callback = null);
        /// <summary>
        /// Updates the graph at specified box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="callback">Updating callback</param>
        void UpdateAt(ref BoundingBox bbox, Action<GraphUpdateStates> callback = null);
        /// <summary>
        /// Updates the graph at specified position list
        /// </summary>
        /// <param name="positions">Position list</param>
        /// <param name="callback">Updating callback</param>
        void UpdateAt(Vector3[] positions, Action<GraphUpdateStates> callback = null);
        /// <summary>
        /// Removes the graph node at specified position
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="callback">Updating callback</param>
        void RemoveAt(ref Vector3 position, Action<GraphUpdateStates> callback = null);
        /// <summary>
        /// Removes the graph node at specified box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="callback">Updating callback</param>
        void RemoveAt(ref BoundingBox bbox, Action<GraphUpdateStates> callback = null);
        /// <summary>
        /// Removes the graph node at specified position list
        /// </summary>
        /// <param name="positions">Position list</param>
        /// <param name="callback">Updating callback</param>
        void RemoveAt(Vector3[] positions, Action<GraphUpdateStates> callback = null);

        /// <summary>
        /// Adds a cylinder obstacle
        /// </summary>
        /// <param name="cylinder">Bounding Cylinder</param>
        /// <returns>Returns the obstacle id</returns>
        int AddObstacle(ref BoundingCylinder cylinder);
        /// <summary>
        /// Adds a bounding box obstacle
        /// </summary>
        /// <param name="bbox">Bounding Box</param>
        /// <returns>Returns the obstacle id</returns>
        int AddObstacle(ref BoundingBox bbox);
        /// <summary>
        /// Adds a oriented bounding box obstacle
        /// </summary>
        /// <param name="obb">Oriented Bounding Box</param>
        /// <returns>Returns the obstacle id</returns>
        /// <remarks>Only applies rotation if the obb's transform has rotation in the Y axis</remarks>
        int AddObstacle(ref OrientedBoundingBox obb);
        /// <summary>
        /// Removes an obstacle by obstacle id
        /// </summary>
        /// <param name="obstacleId">Obstacle id</param>
        void RemoveObstacle(int obstacleId);
        /// <summary>
        /// Updates obstacles state
        /// </summary>
        /// <param name="callback">Updating callback</param>
        void UpdateObstacles(Action<GraphUpdateStates> callback = null);

        /// <summary>
        /// Gets the debug information helper
        /// </summary>
        /// <param name="agent">Agent</param>
        IGraphDebug GetDebugInfo(AgentType agent);
    }
}
