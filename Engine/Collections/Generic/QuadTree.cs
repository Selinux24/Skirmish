using SharpDX;
using System.Collections.Generic;

namespace Engine.Collections.Generic
{
    using Engine.Collections.Helpers;

    /// <summary>
    /// Quad-tree
    /// </summary>
    public class QuadTree<T> : IQuadTree<QuadTreeNode<T>>
    {
        const string cName = nameof(QuadTree<T>);

        /// <inheritdoc/>
        public QuadTreeNode<T> Root { get; private set; }
        /// <inheritdoc/>
        public BoundingBox BoundingBox { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Partitioning items</param>
        /// <param name="maxDepth">Maximum depth</param>
        public QuadTree(BoundingBox bbox, IEnumerable<(BoundingBox Box, T Item)> items, int maxDepth)
        {
            BoundingBox = bbox;

            int nodeCount = 0;
            Root = QuadTreeNode<T>.CreatePartitions(
                null,
                bbox, items,
                maxDepth,
                0,
                ref nodeCount);
        }

        /// <inheritdoc/>
        public IEnumerable<BoundingBox> GetBoundingBoxes(int maxDepth = 0)
        {
            return QuadTreeNodeHelper<QuadTreeNode<T>>.GetBoundingBoxes(Root, maxDepth);
        }
        /// <inheritdoc/>
        public IEnumerable<QuadTreeNode<T>> GetLeafNodes()
        {
            return QuadTreeNodeHelper<QuadTreeNode<T>>.GetLeafNodes(Root);
        }

        /// <inheritdoc/>
        public QuadTreeNode<T> FindClosestNode(Vector3 position)
        {
            var node = QuadTreeNodeHelper<QuadTreeNode<T>>.GetNodeAtPosition(Root, position);
            if (node != null)
            {
                // Position is into a node
                return node;
            }

            //Look for the closest node
            return QuadTreeNodeHelper<QuadTreeNode<T>>.GetClosestNodeAtPosition(Root, position);
        }
        /// <inheritdoc/>
        public IEnumerable<QuadTreeNode<T>> FindNodesInVolume(ICullingVolume volume)
        {
            return QuadTreeNodeHelper<QuadTreeNode<T>>.GetNodesInVolume(Root, volume);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Root != null)
            {
                return $"{cName} Levels {QuadTreeNodeHelper<QuadTreeNode<T>>.GetMaxLevel(Root) + 1}";
            }
            else
            {
                return $"{cName} Empty";
            }
        }
    }
}
