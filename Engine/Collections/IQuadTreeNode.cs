using SharpDX;
using System.Collections.Generic;

namespace Engine.Collections
{
    /// <summary>
    /// Quad-tree node interface
    /// </summary>
    /// <typeparam name="T">Node type</typeparam>
    public interface IQuadTreeNode<T> where T : class, IQuadTreeNode<T>
    {
        /// <summary>
        /// Parent node
        /// </summary>
        T Parent { get; }
        /// <summary>
        /// Gets the child node at top left position (from above)
        /// </summary>
        T TopLeftChild { get; }
        /// <summary>
        /// Gets the child node at top right position (from above)
        /// </summary>
        T TopRightChild { get; }
        /// <summary>
        /// Gets the child node at bottom left position (from above)
        /// </summary>
        T BottomLeftChild { get; }
        /// <summary>
        /// Gets the child node at bottom right position (from above)
        /// </summary>
        T BottomRightChild { get; }

        /// <summary>
        /// Gets the neighbor at top position (from above)
        /// </summary>
        T TopNeighbor { get; }
        /// <summary>
        /// Gets the neighbor at bottom position (from above)
        /// </summary>
        T BottomNeighbor { get; }
        /// <summary>
        /// Gets the neighbor at left position (from above)
        /// </summary>
        T LeftNeighbor { get; }
        /// <summary>
        /// Gets the neighbor at right position (from above)
        /// </summary>
        T RightNeighbor { get; }
        /// <summary>
        /// Gets the neighbor at top left position (from above)
        /// </summary>
        T TopLeftNeighbor { get; }
        /// <summary>
        /// Gets the neighbor at top right position (from above)
        /// </summary>
        T TopRightNeighbor { get; }
        /// <summary>
        /// Gets the neighbor at bottom left position (from above)
        /// </summary>
        T BottomLeftNeighbor { get; }
        /// <summary>
        /// Gets the neighbor at bottom right position (from above)
        /// </summary>
        T BottomRightNeighbor { get; }

        /// <summary>
        /// Node Id
        /// </summary>
        int Id { get; }
        /// <summary>
        /// Depth level
        /// </summary>
        int Level { get; }
        /// <summary>
        /// Bounding box
        /// </summary>
        BoundingBox BoundingBox { get; }
        /// <summary>
        /// Gets the node center position
        /// </summary>
        Vector3 Center { get; }
        /// <summary>
        /// Children list
        /// </summary>
        IEnumerable<T> Children { get; }
        /// <summary>
        /// Gets whether the node is a leaf node
        /// </summary>
        bool IsLeaf { get; }
    }
}
