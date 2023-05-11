using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Collections
{
    /// <summary>
    /// Quad-tree node
    /// </summary>
    public class QuadTreeNode
    {
        /// <summary>
        /// Recursive partition creation
        /// </summary>
        /// <param name="quadTree">Quad-tree</param>
        /// <param name="parent">Parent node</param>
        /// <param name="bbox">Parent bounding box</param>
        /// <param name="maxDepth">Maximum depth</param>
        /// <param name="treeDepth">Current depth</param>
        /// <param name="nodeCount">Node count</param>
        /// <returns>Returns new node</returns>
        public static QuadTreeNode CreatePartitions(
            QuadTree quadTree, QuadTreeNode parent,
            BoundingBox bbox,
            int maxDepth,
            int treeDepth,
            ref int nodeCount)
        {
            if (treeDepth <= maxDepth)
            {
                var node = new QuadTreeNode(quadTree, parent)
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
                    IntializeNode(quadTree, node, bbox, maxDepth, treeDepth + 1, ref nodeCount);
                }

                return node;
            }

            return null;
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
            QuadTree quadTree, QuadTreeNode node,
            BoundingBox bbox,
            int maxDepth,
            int nextTreeDepth,
            ref int nodeCount)
        {
            var boxes = bbox.QuadTree();

            var topLeftChild = CreatePartitions(quadTree, node, boxes.ElementAt(0), maxDepth, nextTreeDepth, ref nodeCount);
            var topRightChild = CreatePartitions(quadTree, node, boxes.ElementAt(1), maxDepth, nextTreeDepth, ref nodeCount);
            var bottomLeftChild = CreatePartitions(quadTree, node, boxes.ElementAt(2), maxDepth, nextTreeDepth, ref nodeCount);
            var bottomRightChild = CreatePartitions(quadTree, node, boxes.ElementAt(3), maxDepth, nextTreeDepth, ref nodeCount);

            List<QuadTreeNode> childList = new List<QuadTreeNode>();

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
        public QuadTree QuadTree { get; private set; }
        /// <summary>
        /// Parent node
        /// </summary>
        public QuadTreeNode Parent { get; private set; }
        /// <summary>
        /// Gets the child node at top left position (from above)
        /// </summary>
        public QuadTreeNode TopLeftChild { get; private set; }
        /// <summary>
        /// Gets the child node at top right position (from above)
        /// </summary>
        public QuadTreeNode TopRightChild { get; private set; }
        /// <summary>
        /// Gets the child node at bottom left position (from above)
        /// </summary>
        public QuadTreeNode BottomLeftChild { get; private set; }
        /// <summary>
        /// Gets the child node at bottom right position (from above)
        /// </summary>
        public QuadTreeNode BottomRightChild { get; private set; }

        /// <summary>
        /// Gets the neighbor at top position (from above)
        /// </summary>
        public QuadTreeNode TopNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at bottom position (from above)
        /// </summary>
        public QuadTreeNode BottomNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at left position (from above)
        /// </summary>
        public QuadTreeNode LeftNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at right position (from above)
        /// </summary>
        public QuadTreeNode RightNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at top left position (from above)
        /// </summary>
        public QuadTreeNode TopLeftNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at top right position (from above)
        /// </summary>
        public QuadTreeNode TopRightNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at bottom left position (from above)
        /// </summary>
        public QuadTreeNode BottomLeftNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at bottom right position (from above)
        /// </summary>
        public QuadTreeNode BottomRightNeighbor { get; private set; }

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
        public List<QuadTreeNode> Children { get; private set; } = new List<QuadTreeNode>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="quadTree">Quad-tree</param>
        /// <param name="parent">Parent node</param>
        public QuadTreeNode(QuadTree quadTree, QuadTreeNode parent)
        {
            QuadTree = quadTree;
            Parent = parent;
        }

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

            if (Children?.Count > 0)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].ConnectNodes();
                }
            }
        }
        /// <summary>
        /// Searches for the neighbor node at top position (from above)
        /// </summary>
        /// <returns>Returns the neighbor node at top position if exists.</returns>
        private QuadTreeNode FindNeighborNodeAtTop()
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
        private QuadTreeNode FindNeighborNodeAtBottom()
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
        private QuadTreeNode FindNeighborNodeAtRight()
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
        private QuadTreeNode FindNeighborNodeAtLeft()
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
            List<BoundingBox> bboxes = new List<BoundingBox>();

            if (Children?.Any() == true)
            {
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
            }
            else
            {
                bboxes.Add(BoundingBox);
            }

            return bboxes.ToArray();
        }
        /// <summary>
        /// Gets maximum level value
        /// </summary>
        /// <returns></returns>
        public int GetMaxLevel()
        {
            int level = 0;

            if (Children?.Any() == true)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    int cLevel = Children[i].GetMaxLevel();

                    if (cLevel > level) level = cLevel;
                }
            }
            else
            {
                level = Level;
            }

            return level;
        }

        /// <summary>
        /// Gets the leaf nodes contained into the specified frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the leaf nodes contained into the frustum</returns>
        public IEnumerable<QuadTreeNode> GetNodesInVolume(ref BoundingFrustum frustum)
        {
            List<QuadTreeNode> nodes = new List<QuadTreeNode>();

            if (Children?.Any() == true)
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

            return nodes.ToArray();
        }
        /// <summary>
        /// Gets the leaf nodes contained into the specified bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the leaf nodes contained into the bounding box</returns>
        public IEnumerable<QuadTreeNode> GetNodesInVolume(ref BoundingBox bbox)
        {
            List<QuadTreeNode> nodes = new List<QuadTreeNode>();

            if (Children?.Any() == true)
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

            return nodes.ToArray();
        }
        /// <summary>
        /// Gets the leaf nodes contained into the specified bounding sphere
        /// </summary>
        /// <param name="sphere">Bounding sphere</param>
        /// <returns>Returns the leaf nodes contained into the bounding sphere</returns>
        public IEnumerable<QuadTreeNode> GetNodesInVolume(ref BoundingSphere sphere)
        {
            List<QuadTreeNode> nodes = new List<QuadTreeNode>();

            if (Children?.Any() == true)
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

            return nodes.ToArray();
        }
        /// <summary>
        /// Gets all leaf nodes
        /// </summary>
        /// <returns>Returns all leaf nodes</returns>
        public IEnumerable<QuadTreeNode> GetLeafNodes()
        {
            List<QuadTreeNode> nodes = new List<QuadTreeNode>();

            if (Children?.Any() == true)
            {
                var leafNodes = Children.SelectMany(c => c.GetLeafNodes());
                nodes.AddRange(leafNodes);
            }
            else
            {
                nodes.Add(this);
            }

            return nodes.ToArray();
        }
        /// <summary>
        /// Gets node at position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns the leaf node which contains the specified position</returns>
        public QuadTreeNode GetNode(Vector3 position)
        {
            if (Children == null)
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
                return $"QuadTreeNode {Id}; Depth {Level}";
            }
            else
            {
                //Node
                return $"QuadTreeNode {Id}; Depth {Level}; Children {Children.Count}";
            }
        }
    }
}
