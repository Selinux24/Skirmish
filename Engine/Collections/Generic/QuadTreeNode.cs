using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Collections.Generic
{
    using Engine.Common;

    /// <summary>
    /// Quad-tree node
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="quadTree">Quad-tree</param>
    /// <param name="parent">Parent node</param>
    public class QuadTreeNode<T>(QuadTree<T> quadTree, QuadTreeNode<T> parent)
    {
        /// <summary>
        /// Recursive partition creation
        /// </summary>
        /// <param name="quadTree">Quad-tree</param>
        /// <param name="parent">Parent node</param>
        /// <param name="bbox">Parent bounding box</param>
        /// <param name="items">All items</param>
        /// <param name="maxDepth">Maximum depth</param>
        /// <param name="treeDepth">Current depth</param>
        /// <param name="nodeCount">Node count</param>
        /// <returns>Returns new node</returns>
        public static QuadTreeNode<T> CreatePartitions(
            QuadTree<T> quadTree, QuadTreeNode<T> parent,
            BoundingBox bbox, IEnumerable<(BoundingBox Box, T Item)> items,
            int maxDepth,
            int treeDepth,
            ref int nodeCount)
        {
            if (treeDepth > maxDepth)
            {
                return null;
            }

            var nodeItems = items.Where(i => Intersection.BoxContainsBox(bbox, i.Box) != ContainmentType.Disjoint);
            if (!nodeItems.Any())
            {
                return null;
            }

            var node = new QuadTreeNode<T>(quadTree, parent)
            {
                Id = -1,
                Level = treeDepth,
                BoundingBox = bbox,
            };

            bool haltByDepth = treeDepth == maxDepth;
            if (haltByDepth)
            {
                node.Id = nodeCount++;
                node.Items.AddRange(nodeItems.Select(i => i.Item));
            }
            else
            {
                // Initialize node partitions
                IntializeNode(quadTree, node, bbox, nodeItems, maxDepth, treeDepth + 1, ref nodeCount);
            }

            return node;
        }
        /// <summary>
        /// Initializes node partitions
        /// </summary>
        /// <param name="quadTree">Quad-tree</param>
        /// <param name="node">Current node</param>
        /// <param name="bbox">Bounding box</param>
        /// <param name="items">Items into the node</param>
        /// <param name="maxDepth">Maximum depth</param>
        /// <param name="nextTreeDepth">Next depth</param>
        /// <param name="nodeCount">Node count</param>
        private static void IntializeNode(
            QuadTree<T> quadTree, QuadTreeNode<T> node,
            BoundingBox bbox, IEnumerable<(BoundingBox Box, T Item)> items,
            int maxDepth,
            int nextTreeDepth,
            ref int nodeCount)
        {
            var boxes = bbox.QuadTree();

            var topLeftChild = CreatePartitions(quadTree, node, boxes.ElementAt(0), items, maxDepth, nextTreeDepth, ref nodeCount);
            var topRightChild = CreatePartitions(quadTree, node, boxes.ElementAt(1), items, maxDepth, nextTreeDepth, ref nodeCount);
            var bottomLeftChild = CreatePartitions(quadTree, node, boxes.ElementAt(2), items, maxDepth, nextTreeDepth, ref nodeCount);
            var bottomRightChild = CreatePartitions(quadTree, node, boxes.ElementAt(3), items, maxDepth, nextTreeDepth, ref nodeCount);

            var childList = new List<QuadTreeNode<T>>();

            if (topLeftChild != null) childList.Add(topLeftChild);
            if (topRightChild != null) childList.Add(topRightChild);
            if (bottomLeftChild != null) childList.Add(bottomLeftChild);
            if (bottomRightChild != null) childList.Add(bottomRightChild);

            if (childList.Count > 0)
            {
                node.Children.AddRange(childList);
                node.TopLeftChild = topLeftChild;
                node.TopRightChild = topRightChild;
                node.BottomLeftChild = bottomLeftChild;
                node.BottomRightChild = bottomRightChild;
            }
        }

        /// <summary>
        /// Parent
        /// </summary>
        public QuadTree<T> QuadTree { get; private set; } = quadTree;
        /// <summary>
        /// Parent node
        /// </summary>
        public QuadTreeNode<T> Parent { get; private set; } = parent;
        /// <summary>
        /// Gets the child node at top left position (from above)
        /// </summary>
        public QuadTreeNode<T> TopLeftChild { get; private set; }
        /// <summary>
        /// Gets the child node at top right position (from above)
        /// </summary>
        public QuadTreeNode<T> TopRightChild { get; private set; }
        /// <summary>
        /// Gets the child node at bottom left position (from above)
        /// </summary>
        public QuadTreeNode<T> BottomLeftChild { get; private set; }
        /// <summary>
        /// Gets the child node at bottom right position (from above)
        /// </summary>
        public QuadTreeNode<T> BottomRightChild { get; private set; }

        /// <summary>
        /// Gets the neighbor at top position (from above)
        /// </summary>
        public QuadTreeNode<T> TopNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at bottom position (from above)
        /// </summary>
        public QuadTreeNode<T> BottomNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at left position (from above)
        /// </summary>
        public QuadTreeNode<T> LeftNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at right position (from above)
        /// </summary>
        public QuadTreeNode<T> RightNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at top left position (from above)
        /// </summary>
        public QuadTreeNode<T> TopLeftNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at top right position (from above)
        /// </summary>
        public QuadTreeNode<T> TopRightNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at bottom left position (from above)
        /// </summary>
        public QuadTreeNode<T> BottomLeftNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at bottom right position (from above)
        /// </summary>
        public QuadTreeNode<T> BottomRightNeighbor { get; private set; }

        /// <summary>
        /// Node Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Depth level
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// Bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; set; }
        /// <summary>
        /// Gets the node center position
        /// </summary>
        public Vector3 Center
        {
            get
            {
                return BoundingBox.GetCenter();
            }
        }
        /// <summary>
        /// Children list
        /// </summary>
        public List<QuadTreeNode<T>> Children { get; private set; } = [];
        /// <summary>
        /// Node triangles
        /// </summary>
        public List<T> Items { get; private set; } = [];

        /// <summary>
        /// Connect nodes in the grid
        /// </summary>
        public void ConnectNodes()
        {
            TopNeighbor = FindNeighborNodeAtTop();
            BottomNeighbor = FindNeighborNodeAtBottom();
            LeftNeighbor = FindNeighborNodeAtLeft();
            RightNeighbor = FindNeighborNodeAtRight();

            TopLeftNeighbor = TopNeighbor?.FindNeighborNodeAtLeft();
            TopRightNeighbor = TopNeighbor?.FindNeighborNodeAtRight();
            BottomLeftNeighbor = BottomNeighbor?.FindNeighborNodeAtLeft();
            BottomRightNeighbor = BottomNeighbor?.FindNeighborNodeAtRight();

            if (Children.Count == 0)
            {
                return;
            }

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].ConnectNodes();
            }
        }
        /// <summary>
        /// Searches for the neighbor node at top position (from above)
        /// </summary>
        /// <returns>Returns the neighbor node at top position if exists.</returns>
        private QuadTreeNode<T> FindNeighborNodeAtTop()
        {
            if (Parent == null)
            {
                return null;
            }

            if (this == Parent.TopLeftChild)
            {
                var node = Parent.FindNeighborNodeAtTop();
                if (node != null)
                {
                    return node.BottomLeftChild;
                }
            }
            else if (this == Parent.TopRightChild)
            {
                var node = Parent.FindNeighborNodeAtTop();
                if (node != null)
                {
                    return node.BottomRightChild;
                }
            }
            else if (this == Parent.BottomLeftChild)
            {
                return Parent.TopLeftChild;
            }
            else if (this == Parent.BottomRightChild)
            {
                return Parent.TopRightChild;
            }

            return null;
        }
        /// <summary>
        /// Searches for the neighbor node at bottom position (from above)
        /// </summary>
        /// <returns>Returns the neighbor node at bottom position if exists.</returns>
        private QuadTreeNode<T> FindNeighborNodeAtBottom()
        {
            if (Parent == null)
            {
                return null;
            }

            if (this == Parent.TopLeftChild)
            {
                return Parent.BottomLeftChild;
            }
            else if (this == Parent.TopRightChild)
            {
                return Parent.BottomRightChild;
            }
            else if (this == Parent.BottomLeftChild)
            {
                var node = Parent.FindNeighborNodeAtBottom();
                if (node != null)
                {
                    return node.TopLeftChild;
                }
            }
            else if (this == Parent.BottomRightChild)
            {
                var node = Parent.FindNeighborNodeAtBottom();
                if (node != null)
                {
                    return node.TopRightChild;
                }
            }

            return null;
        }
        /// <summary>
        /// Searches for the neighbor node at right position(from above)
        /// </summary>
        /// <returns>Returns the neighbor node at top position if exists.</returns>
        private QuadTreeNode<T> FindNeighborNodeAtRight()
        {
            if (Parent == null)
            {
                return null;
            }

            if (this == Parent.TopLeftChild)
            {
                return Parent.TopRightChild;
            }
            else if (this == Parent.TopRightChild)
            {
                var node = Parent.FindNeighborNodeAtRight();
                if (node != null)
                {
                    return node.TopLeftChild;
                }
            }
            else if (this == Parent.BottomLeftChild)
            {
                return Parent.BottomRightChild;
            }
            else if (this == Parent.BottomRightChild)
            {
                var node = Parent.FindNeighborNodeAtRight();
                if (node != null)
                {
                    return node.BottomLeftChild;
                }
            }

            return null;
        }
        /// <summary>
        /// Searches for the neighbor node at left position (from above)
        /// </summary>
        /// <returns>Returns the neighbor node at left position if exists.</returns>
        private QuadTreeNode<T> FindNeighborNodeAtLeft()
        {
            if (Parent == null)
            {
                return null;
            }

            if (this == Parent.TopLeftChild)
            {
                var node = Parent.FindNeighborNodeAtLeft();
                if (node != null)
                {
                    return node.TopRightChild;
                }
            }
            else if (this == Parent.TopRightChild)
            {
                return Parent.TopLeftChild;
            }
            else if (this == Parent.BottomLeftChild)
            {
                var node = Parent.FindNeighborNodeAtLeft();
                if (node != null)
                {
                    return node.BottomRightChild;
                }
            }
            else if (this == Parent.BottomRightChild)
            {
                return Parent.BottomLeftChild;
            }

            return null;
        }

        /// <summary>
        /// Get bounding boxes of specified level
        /// </summary>
        /// <param name="maxDepth">Maximum depth (if zero there is no limit)</param>
        /// <returns>Returns bounding boxes of specified depth</returns>
        public IEnumerable<BoundingBox> GetBoundingBoxes(int maxDepth = 0)
        {
            if (Children.Count == 0)
            {
                return [BoundingBox];
            }

            List<BoundingBox> bboxes = [];

            if (maxDepth > 0 && Level == maxDepth)
            {
                Children.ForEach((c) =>
                {
                    bboxes.Add(c.BoundingBox);
                });
            }
            else
            {
                Children.ForEach((c) =>
                {
                    bboxes.AddRange(c.GetBoundingBoxes(maxDepth));
                });
            }

            return bboxes;
        }
        /// <summary>
        /// Gets maximum level value
        /// </summary>
        /// <returns></returns>
        public int GetMaxLevel()
        {
            if (Children.Count == 0)
            {
                return Level;
            }

            int level = 0;

            for (int i = 0; i < Children.Count; i++)
            {
                int cLevel = Children[i].GetMaxLevel();

                if (cLevel > level) level = cLevel;
            }

            return level;
        }

        /// <summary>
        /// Gets the leaf nodes contained into the specified frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the leaf nodes contained into the frustum</returns>
        public IEnumerable<QuadTreeNode<T>> GetNodesInVolume(ref BoundingFrustum frustum)
        {
            List<QuadTreeNode<T>> nodes = [];

            if (Children.Count != 0)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    var childNodes = Children[i].GetNodesInVolume(ref frustum);
                    if (childNodes.Any())
                    {
                        nodes.AddRange(childNodes);
                    }
                }
            }
            else
            {
                if (frustum.Contains(BoundingBox) != ContainmentType.Disjoint)
                {
                    nodes.Add(this);
                }
            }

            return nodes;
        }
        /// <summary>
        /// Gets the leaf nodes contained into the specified bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the leaf nodes contained into the bounding box</returns>
        public IEnumerable<QuadTreeNode<T>> GetNodesInVolume(ref BoundingBox bbox)
        {
            List<QuadTreeNode<T>> nodes = [];

            if (Children.Count != 0)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    var childNodes = Children[i].GetNodesInVolume(ref bbox);
                    if (childNodes.Any())
                    {
                        nodes.AddRange(childNodes);
                    }
                }
            }
            else
            {
                if (bbox.Contains(BoundingBox) != ContainmentType.Disjoint)
                {
                    nodes.Add(this);
                }
            }

            return nodes;
        }
        /// <summary>
        /// Gets the leaf nodes contained into the specified bounding sphere
        /// </summary>
        /// <param name="sphere">Bounding sphere</param>
        /// <returns>Returns the leaf nodes contained into the bounding sphere</returns>
        public IEnumerable<QuadTreeNode<T>> GetNodesInVolume(ref BoundingSphere sphere)
        {
            List<QuadTreeNode<T>> nodes = [];

            if (Children.Count != 0)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    var childNodes = Children[i].GetNodesInVolume(ref sphere);
                    if (childNodes.Any())
                    {
                        nodes.AddRange(childNodes);
                    }
                }
            }
            else
            {
                var bbox = BoundingBox;
                if (sphere.Contains(ref bbox) != ContainmentType.Disjoint)
                {
                    nodes.Add(this);
                }
            }

            return nodes;
        }
        /// <summary>
        /// Gets all leaf nodes
        /// </summary>
        /// <returns>Returns all leaf nodes</returns>
        public IEnumerable<QuadTreeNode<T>> GetLeafNodes()
        {
            if (Children.Count == 0)
            {
                return [this];
            }

            return Children.SelectMany(c => c.GetLeafNodes());
        }
        /// <summary>
        /// Gets node at position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns the leaf node which contains the specified position</returns>
        public QuadTreeNode<T> GetNode(Vector3 position)
        {
            if (Children.Count == 0)
            {
                if (BoundingBox.Contains(position) != ContainmentType.Disjoint)
                {
                    return this;
                }
            }
            else
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    var childNode = Children[i].GetNode(position);
                    if (childNode != null)
                    {
                        return childNode;
                    }
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Children == null)
            {
                //Leaf node
                return $"{nameof(QuadTreeNode<T>)} {Id}; Depth {Level}; Items {Items.Count}";
            }
            else
            {
                //Node
                return $"{nameof(QuadTreeNode<T>)} {Id}; Depth {Level}; Children {Children.Count}";
            }
        }
    }
}
