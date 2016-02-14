using SharpDX;

namespace Engine.PathFinding
{
    public interface IGraph
    {
        /// <summary>
        /// Graph node list
        /// </summary>
        IGraphNode[] Nodes { get; set; }
        /// <summary>
        /// Gets node wich contains specified point
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Returns the node wich contains the specified point if exists</returns>
        IGraphNode FindNode(Vector3 point);
    }
}
