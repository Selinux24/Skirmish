using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Collections.Generic
{
    using Engine.Collections.Helpers;

    /// <summary>
    /// Picking quad tree node
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="quadTree">Quadtree</param>
    /// <param name="parent">Parent node</param>
    public class PickingQuadTreeNode<T>(PickingQuadTreeNode<T> parent) : IQuadTreeNode<PickingQuadTreeNode<T>> where T : IVertexList, IRayIntersectable
    {
        const string cName = nameof(PickingQuadTreeNode<T>);

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
        public static PickingQuadTreeNode<T> CreatePartitions(
            PickingQuadTreeNode<T> parent,
            BoundingBox bbox, IEnumerable<T> items,
            int maxDepth,
            int treeDepth,
            ref int nodeCount)
        {
            if (treeDepth > maxDepth)
            {
                return null;
            }

            // Creates a new node
            PickingQuadTreeNode<T> node = new(parent)
            {
                Id = -1,
                Level = treeDepth,
                BoundingBox = bbox,
            };

            //Find triangles into the bounding box
            var nodeItems = items
                .Where(t =>
                {
                    var tbox = SharpDXExtensions.BoundingBoxFromPoints(t.GetVertices());

                    return bbox.Contains(tbox) != ContainmentType.Disjoint;
                })
                .ToList(); //Break the reference

            bool haltByDepth = treeDepth == maxDepth;
            if (haltByDepth)
            {
                // Maximum tree depth reached. Stop the process
                node.Id = nodeCount++;
                node.items.AddRange(nodeItems);
            }
            else
            {
                // Initialize node partitions
                InitializeNode(node, bbox, nodeItems, maxDepth, treeDepth + 1, ref nodeCount);
            }

            if (parent == null)
            {
                ConnectNodes(node);
            }

            return node;
        }
        /// <summary>
        /// Initializes node partitinos
        /// </summary>
        /// <param name="node">Current node</param>
        /// <param name="bbox">Bounding box</param>
        /// <param name="items">Items into the node</param>
        /// <param name="maxDepth">Maximum depth</param>
        /// <param name="nextTreeDepth">Next depth</param>
        /// <param name="nodeCount">Node count</param>
        private static void InitializeNode(
            PickingQuadTreeNode<T> node,
            BoundingBox bbox, IEnumerable<T> items,
            int maxDepth,
            int nextTreeDepth,
            ref int nodeCount)
        {
            BoundingBox[] boxes = [.. bbox.SubdivideQuadtree()];

            var topLeftChild = CreatePartitions(node, boxes[0], items, maxDepth, nextTreeDepth, ref nodeCount);
            var topRightChild = CreatePartitions(node, boxes[1], items, maxDepth, nextTreeDepth, ref nodeCount);
            var bottomLeftChild = CreatePartitions(node, boxes[2], items, maxDepth, nextTreeDepth, ref nodeCount);
            var bottomRightChild = CreatePartitions(node, boxes[3], items, maxDepth, nextTreeDepth, ref nodeCount);

            List<PickingQuadTreeNode<T>> childList = [];

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
        private static void ConnectNodes(PickingQuadTreeNode<T> node)
        {
            if (node == null)
            {
                return;
            }

            node.TopNeighbor = QuadTreeNodeHelper<PickingQuadTreeNode<T>>.FindNeighborNodeAtTop(node);
            node.BottomNeighbor = QuadTreeNodeHelper<PickingQuadTreeNode<T>>.FindNeighborNodeAtBottom(node);
            node.LeftNeighbor = QuadTreeNodeHelper<PickingQuadTreeNode<T>>.FindNeighborNodeAtLeft(node);
            node.RightNeighbor = QuadTreeNodeHelper<PickingQuadTreeNode<T>>.FindNeighborNodeAtRight(node);

            node.TopLeftNeighbor = QuadTreeNodeHelper<PickingQuadTreeNode<T>>.FindNeighborNodeAtLeft(node.TopNeighbor);
            node.TopRightNeighbor = QuadTreeNodeHelper<PickingQuadTreeNode<T>>.FindNeighborNodeAtRight(node.TopNeighbor);
            node.BottomLeftNeighbor = QuadTreeNodeHelper<PickingQuadTreeNode<T>>.FindNeighborNodeAtLeft(node.BottomNeighbor);
            node.BottomRightNeighbor = QuadTreeNodeHelper<PickingQuadTreeNode<T>>.FindNeighborNodeAtRight(node.BottomNeighbor);

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
        private readonly List<PickingQuadTreeNode<T>> children = [];
        /// <summary>
        /// Items list
        /// </summary>
        private readonly List<T> items = [];

        /// <inheritdoc/>
        public PickingQuadTreeNode<T> Parent { get; private set; } = parent;
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> TopLeftChild { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> TopRightChild { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> BottomLeftChild { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> BottomRightChild { get; private set; }

        /// <inheritdoc/>
        public PickingQuadTreeNode<T> TopNeighbor { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> BottomNeighbor { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> LeftNeighbor { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> RightNeighbor { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> TopLeftNeighbor { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> TopRightNeighbor { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> BottomLeftNeighbor { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> BottomRightNeighbor { get; private set; }

        /// <inheritdoc/>
        public int Id { get; private set; }
        /// <inheritdoc/>
        public int Level { get; private set; }
        /// <inheritdoc/>
        public BoundingBox BoundingBox { get; private set; }
        /// <inheritdoc/>
        public Vector3 Center { get => BoundingBox.Center; }
        /// <inheritdoc/>
        public IEnumerable<PickingQuadTreeNode<T>> Children { get => children.AsReadOnly(); }
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
