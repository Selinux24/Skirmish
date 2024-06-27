using SharpDX;
using System.Collections.Generic;

namespace Engine.Collections
{
    /// <summary>
    /// Quad-tree interface
    /// </summary>
    /// <typeparam name="T">Node type</typeparam>
    public interface IQuadTree<T> where T : class, IQuadTreeNode<T>
    {
        /// <summary>
        /// Root node
        /// </summary>
        T Root { get; }
        /// <summary>
        /// Global bounding box
        /// </summary>
        BoundingBox BoundingBox { get; }

        /// <summary>
        /// Gets bounding boxes of specified depth
        /// </summary>
        /// <param name="maxDepth">Maximum depth (if zero there is no limit)</param>
        /// <returns>Returns bounding boxes of specified depth</returns>
        IEnumerable<BoundingBox> GetBoundingBoxes(int maxDepth = 0);
        /// <summary>
        /// Gets all leaf nodes
        /// </summary>
        /// <returns>Returns all leaf nodel</returns>
        IEnumerable<T> GetLeafNodes();

        /// <summary>
        /// Gets the closest node to the specified position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns the closest node to the specified position</returns>
        T FindClosestNode(Vector3 position);
        /// <summary>
        /// Gets the nodes contained into the specified volume
        /// </summary>
        /// <param name="volume">Bounding volume</param>
        /// <returns>Returns the nodes contained into the volume</returns>
        IEnumerable<T> FindNodesInVolume(ICullingVolume volume);
    }
}
