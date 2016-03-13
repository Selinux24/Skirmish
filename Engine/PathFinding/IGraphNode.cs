using SharpDX;

namespace Engine.PathFinding
{
    public interface IGraphNode
    {
        /// <summary>
        /// Gets the connected node list
        /// </summary>
        IGraphNode[] Connections { get; }
        /// <summary>
        /// Gets a connected node by index
        /// </summary>
        /// <param name="index">Node index</param>
        /// <returns>Returns the connected node by index</returns>
        IGraphNode this[int index] { get; }

        /// <summary>
        /// Node state
        /// </summary>
        GraphNodeStates State { get; }
        /// <summary>
        /// Node passing cost
        /// </summary>
        float Cost { get; }
        /// <summary>
        /// Center position
        /// </summary>
        Vector3 Center { get; }

        /// <summary>
        /// Gets whether this node contains specified point
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <returns>Returns whether this node contains specified point</returns>
        bool Contains(Vector3 point, out float distance);
        /// <summary>
        /// Gets the point list of this node perimeter
        /// </summary>
        /// <returns>Returns the point list of this node perimeter</returns>
        Vector3[] GetPoints();
    }
}
