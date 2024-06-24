using SharpDX;
using System.Collections.Generic;

namespace Engine.Collections
{
    using Engine.Collections.Helpers;

    /// <summary>
    /// Quad-tree node
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="quadTree">Quad-tree</param>
    /// <param name="parent">Parent node</param>
    public class QuadTreeNode(QuadTreeNode parent) : IQuadTreeNode<QuadTreeNode>
    {
        const string cName = nameof(QuadTreeNode);

        /// <summary>
        /// Recursive partition creation
        /// </summary>
        /// <param name="parent">Parent node</param>
        /// <param name="bbox">Parent bounding box</param>
        /// <param name="maxDepth">Maximum depth</param>
        /// <param name="treeDepth">Current depth</param>
        /// <param name="nodeCount">Node count</param>
        /// <returns>Returns new node</returns>
        public static QuadTreeNode CreatePartitions(
            QuadTreeNode parent,
            BoundingBox bbox,
            int maxDepth,
            int treeDepth,
            ref int nodeCount)
        {
            if (treeDepth > maxDepth)
            {
                return null;
            }

            QuadTreeNode node = new(parent)
            {
                Id = -1,
                Level = treeDepth,
                BoundingBox = bbox,
            };

            bool haltByDepth = treeDepth == maxDepth;
            if (haltByDepth)
            {
                node.Id = nodeCount++;
            }
            else
            {
                // Initialize node partitions
                IntializeNode(node, bbox, maxDepth, treeDepth + 1, ref nodeCount);
            }

            if (parent == null)
            {
                ConnectNodes(node);
            }

            return node;
        }
        /// <summary>
        /// Initializes node partitions
        /// </summary>
        /// <param name="node">Current node</param>
        /// <param name="bbox">Bounding box</param>
        /// <param name="items">Items into the node</param>
        /// <param name="maxDepth">Maximum depth</param>
        /// <param name="nextTreeDepth">Next depth</param>
        /// <param name="nodeCount">Node count</param>
        private static void IntializeNode(
            QuadTreeNode node,
            BoundingBox bbox,
            int maxDepth,
            int nextTreeDepth,
            ref int nodeCount)
        {
            BoundingBox[] boxes = [.. bbox.SubdivideQuadtree()];

            var topLeftChild = CreatePartitions(node, boxes[0], maxDepth, nextTreeDepth, ref nodeCount);
            var topRightChild = CreatePartitions(node, boxes[1], maxDepth, nextTreeDepth, ref nodeCount);
            var bottomLeftChild = CreatePartitions(node, boxes[2], maxDepth, nextTreeDepth, ref nodeCount);
            var bottomRightChild = CreatePartitions(node, boxes[3], maxDepth, nextTreeDepth, ref nodeCount);

            List<QuadTreeNode> childList = [];

            if (topLeftChild != null) childList.Add(topLeftChild);
            if (topRightChild != null) childList.Add(topRightChild);
            if (bottomLeftChild != null) childList.Add(bottomLeftChild);
            if (bottomRightChild != null) childList.Add(bottomRightChild);

            if (childList.Count > 0)
            {
                node.children.AddRange(childList);
                node.TopLeftChild = topLeftChild;
                node.TopRightChild = topRightChild;
                node.BottomLeftChild = bottomLeftChild;
                node.BottomRightChild = bottomRightChild;
            }
        }
        /// <summary>
        /// Connect nodes in the grid
        /// </summary>
        /// <param name="node">Parent node</param>
        private static void ConnectNodes(QuadTreeNode node)
        {
            if (node == null)
            {
                return;
            }

            node.TopNeighbor = QuadTreeNodeHelper<QuadTreeNode>.FindNeighborNodeAtTop(node);
            node.BottomNeighbor = QuadTreeNodeHelper<QuadTreeNode>.FindNeighborNodeAtBottom(node);
            node.LeftNeighbor = QuadTreeNodeHelper<QuadTreeNode>.FindNeighborNodeAtLeft(node);
            node.RightNeighbor = QuadTreeNodeHelper<QuadTreeNode>.FindNeighborNodeAtRight(node);

            node.TopLeftNeighbor = QuadTreeNodeHelper<QuadTreeNode>.FindNeighborNodeAtLeft(node.TopNeighbor);
            node.TopRightNeighbor = QuadTreeNodeHelper<QuadTreeNode>.FindNeighborNodeAtRight(node.TopNeighbor);
            node.BottomLeftNeighbor = QuadTreeNodeHelper<QuadTreeNode>.FindNeighborNodeAtLeft(node.BottomNeighbor);
            node.BottomRightNeighbor = QuadTreeNodeHelper<QuadTreeNode>.FindNeighborNodeAtRight(node.BottomNeighbor);

            if (node.IsLeaf)
            {
                return;
            }

            for (int i = 0; i < node.children.Count; i++)
            {
                ConnectNodes(node.children[i]);
            }
        }

        /// <summary>
        /// Children list
        /// </summary>
        private readonly List<QuadTreeNode> children = [];

        /// <inheritdoc/>
        public QuadTreeNode Parent { get; private set; } = parent;
        /// <inheritdoc/>
        public QuadTreeNode TopLeftChild { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode TopRightChild { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode BottomLeftChild { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode BottomRightChild { get; private set; }

        /// <inheritdoc/>
        public QuadTreeNode TopNeighbor { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode BottomNeighbor { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode LeftNeighbor { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode RightNeighbor { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode TopLeftNeighbor { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode TopRightNeighbor { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode BottomLeftNeighbor { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode BottomRightNeighbor { get; private set; }

        /// <inheritdoc/>
        public int Id { get; set; }
        /// <inheritdoc/>
        public int Level { get; set; }
        /// <inheritdoc/>
        public BoundingBox BoundingBox { get; set; }
        /// <inheritdoc/>
        public Vector3 Center
        {
            get
            {
                return BoundingBox.GetCenter();
            }
        }
        /// <inheritdoc/>
        public IEnumerable<QuadTreeNode> Children { get => children.AsReadOnly(); }
        /// <inheritdoc/>
        public bool IsLeaf { get => children.Count == 0; }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (IsLeaf)
            {
                //Leaf node
                return $"{cName} {Id}; Depth {Level}";
            }
            else
            {
                //Node
                return $"{cName} {Id}; Depth {Level}; Children {children.Count}";
            }
        }
    }
}
