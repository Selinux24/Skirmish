using SharpDX;
using System.Collections.Generic;

namespace Engine.Collections
{
    using Engine.Collections.Helpers;

    /// <summary>
    /// Quad tree
    /// </summary>
    public class QuadTree : IQuadTree<QuadTreeNode>
    {
        const string cName = nameof(QuadTree);

        /// <inheritdoc/>
        public QuadTreeNode Root { get; private set; }
        /// <inheritdoc/>
        public BoundingBox BoundingBox { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Partitioning items</param>
        /// <param name="maxDepth">Maximum depth</param>
        public QuadTree(BoundingBox bbox, int maxDepth)
        {
            BoundingBox = bbox;

            int nodeCount = 0;
            Root = QuadTreeNode.CreatePartitions(
                null,
                bbox,
                maxDepth,
                0,
                ref nodeCount);
        }

        /// <inheritdoc/>
        public IEnumerable<BoundingBox> GetBoundingBoxes(int maxDepth = 0)
        {
            return QuadTreeNodeHelper<QuadTreeNode>.GetBoundingBoxes(Root, maxDepth);
        }
        /// <inheritdoc/>
        public IEnumerable<QuadTreeNode> GetLeafNodes()
        {
            return QuadTreeNodeHelper<QuadTreeNode>.GetLeafNodes(Root);
        }

        /// <inheritdoc/>
        public QuadTreeNode FindClosestNode(Vector3 position)
        {
            var node = QuadTreeNodeHelper<QuadTreeNode>.GetNodeAtPosition(Root, position);
            if (node != null)
            {
                // Position is into a node
                return node;
            }

            //Look for the closest node
            return QuadTreeNodeHelper<QuadTreeNode>.GetClosestNodeAtPosition(Root, position);
        }
        /// <inheritdoc/>
        public IEnumerable<QuadTreeNode> FindNodesInVolume(ICullingVolume volume)
        {
            return QuadTreeNodeHelper<QuadTreeNode>.GetNodesInVolume(Root, volume);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Root != null)
            {
                return $"{cName} Levels {QuadTreeNodeHelper<QuadTreeNode>.GetMaxLevel(Root) + 1}";
            }
            else
            {
                return $"{cName} Empty";
            }
        }
    }
}

