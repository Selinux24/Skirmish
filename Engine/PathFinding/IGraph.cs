using SharpDX;

namespace Engine.PathFinding
{
    public interface IGraph
    {
        /// <summary>
        /// Gets the node collection of the graph
        /// </summary>
        /// <returns></returns>
        IGraphNode[] GetNodes();
        /// <summary>
        /// Find path from point to point
        /// </summary>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <returns>Return path if exists</returns>
        PathFindingPath FindPath(Vector3 from, Vector3 to);
        /// <summary>
        /// Gets wether the specified position is walkable
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns true if the specified position is walkable</returns>
        bool IsWalkable(Vector3 position);
    }
}
