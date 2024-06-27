using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Collections.Generic
{
    using Engine.Collections.Helpers;

    /// <summary>
    /// Quad-tree node
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="parent">Parent node</param>
    public class QuadTreeNode<T>(QuadTreeNode<T> parent) : IQuadTreeNode<QuadTreeNode<T>>
    {
        const string cName = nameof(QuadTreeNode<T>);

        /// <summary>
        /// Recursive partition creation
        /// </summary>
        /// <param name="parent">Parent node</param>
        /// <param name="bbox">Parent bounding box</param>
        /// <param name="items">All items</param>
        /// <param name="maxDepth">Maximum depth</param>
        /// <param name="treeDepth">Current depth</param>
        /// <param name="nodeCount">Node count</param>
        /// <returns>Returns new node</returns>
        public static QuadTreeNode<T> CreatePartitions(
            QuadTreeNode<T> parent,
            BoundingBox bbox, IEnumerable<(BoundingBox Box, T Item)> items,
            int maxDepth,
            int treeDepth,
            ref int nodeCount)
        {
            if (treeDepth > maxDepth)
            {
                return null;
            }

            // Creates a new node
            QuadTreeNode<T> node = new(parent)
            {
                Id = -1,
                Level = treeDepth,
                BoundingBox = bbox,
            };

            //Find items into the bounding box
            var nodeItems = items
                .Where(i => bbox.Contains(i.Box) != ContainmentType.Disjoint)
                .ToList(); //Break the reference

            bool haltByDepth = treeDepth == maxDepth;
            if (haltByDepth)
            {
                // Maximum tree depth reached. Stop the process
                node.Id = nodeCount++;
                node.items.AddRange(nodeItems.Select(i => i.Item));
            }
            else
            {
                // Initialize node partitions
                IntializeNode(node, bbox, nodeItems, maxDepth, treeDepth + 1, ref nodeCount);
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
            QuadTreeNode<T> node,
            BoundingBox bbox, IEnumerable<(BoundingBox Box, T Item)> items,
            int maxDepth,
            int nextTreeDepth,
            ref int nodeCount)
        {
            BoundingBox[] boxes = [.. bbox.SubdivideQuadtree()];

            var topLeftChild = CreatePartitions(node, boxes[0], items, maxDepth, nextTreeDepth, ref nodeCount);
            var topRightChild = CreatePartitions(node, boxes[1], items, maxDepth, nextTreeDepth, ref nodeCount);
            var bottomLeftChild = CreatePartitions(node, boxes[2], items, maxDepth, nextTreeDepth, ref nodeCount);
            var bottomRightChild = CreatePartitions(node, boxes[3], items, maxDepth, nextTreeDepth, ref nodeCount);

            List<QuadTreeNode<T>> childList = [];

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
        private static void ConnectNodes(QuadTreeNode<T> node)
        {
            if (node == null)
            {
                return;
            }

            node.TopNeighbor = QuadTreeNodeHelper<QuadTreeNode<T>>.FindNeighborNodeAtTop(node);
            node.BottomNeighbor = QuadTreeNodeHelper<QuadTreeNode<T>>.FindNeighborNodeAtBottom(node);
            node.LeftNeighbor = QuadTreeNodeHelper<QuadTreeNode<T>>.FindNeighborNodeAtLeft(node);
            node.RightNeighbor = QuadTreeNodeHelper<QuadTreeNode<T>>.FindNeighborNodeAtRight(node);

            node.TopLeftNeighbor = QuadTreeNodeHelper<QuadTreeNode<T>>.FindNeighborNodeAtLeft(node.TopNeighbor);
            node.TopRightNeighbor = QuadTreeNodeHelper<QuadTreeNode<T>>.FindNeighborNodeAtRight(node.TopNeighbor);
            node.BottomLeftNeighbor = QuadTreeNodeHelper<QuadTreeNode<T>>.FindNeighborNodeAtLeft(node.BottomNeighbor);
            node.BottomRightNeighbor = QuadTreeNodeHelper<QuadTreeNode<T>>.FindNeighborNodeAtRight(node.BottomNeighbor);

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
        private readonly List<QuadTreeNode<T>> children = [];
        /// <summary>
        /// Items list
        /// </summary>
        private readonly List<T> items = [];

        /// <inheritdoc/>
        public QuadTreeNode<T> Parent { get; private set; } = parent;
        /// <inheritdoc/>
        public QuadTreeNode<T> TopLeftChild { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode<T> TopRightChild { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode<T> BottomLeftChild { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode<T> BottomRightChild { get; private set; }

        /// <inheritdoc/>
        public QuadTreeNode<T> TopNeighbor { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode<T> BottomNeighbor { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode<T> LeftNeighbor { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode<T> RightNeighbor { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode<T> TopLeftNeighbor { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode<T> TopRightNeighbor { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode<T> BottomLeftNeighbor { get; private set; }
        /// <inheritdoc/>
        public QuadTreeNode<T> BottomRightNeighbor { get; private set; }

        /// <inheritdoc/>
        public int Id { get; private set; }
        /// <inheritdoc/>
        public int Level { get; private set; }
        /// <inheritdoc/>
        public BoundingBox BoundingBox { get; private set; }
        /// <inheritdoc/>
        public Vector3 Center { get => BoundingBox.Center; }
        /// <inheritdoc/>
        public IEnumerable<QuadTreeNode<T>> Children { get => children.AsReadOnly(); }
        /// <inheritdoc/>
        public bool IsLeaf { get => children.Count == 0; }
        /// <summary>
        /// Node item collection
        /// </summary>
        public IEnumerable<T> Items { get => items.AsReadOnly(); }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (IsLeaf)
            {
                //Leaf node
                return $"{cName} {Id}; Depth {Level}; Items {items.Count}";
            }
            else
            {
                //Node
                return $"{cName} {Id}; Depth {Level}; Children {children.Count}";
            }
        }
    }
}
