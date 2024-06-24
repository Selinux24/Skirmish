using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Collections
{
    /// <summary>
    /// Quad-tree node
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="quadTree">Quad-tree</param>
    /// <param name="parent">Parent node</param>
    public class QuadTreeNode(QuadTreeNode parent)
    {
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
                node.Children.AddRange(childList);
                node.TopLeftChild = topLeftChild;
                node.TopRightChild = topRightChild;
                node.BottomLeftChild = bottomLeftChild;
                node.BottomRightChild = bottomRightChild;
            }
        }

        /// <summary>
        /// Parent node
        /// </summary>
        public QuadTreeNode Parent { get; private set; } = parent;
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
        public List<QuadTreeNode> Children { get; private set; } = [];
        /// <summary>
        /// Gets whether the node is a leaf node
        /// </summary>
        public bool IsLeaf { get => Children.Count == 0; }

        /// <summary>
        /// Connect nodes in the grid
        /// </summary>
        private static void ConnectNodes(QuadTreeNode node)
        {
            if (node == null)
            {
                return;
            }

            node.TopNeighbor = FindNeighborNodeAtTop(node);
            node.BottomNeighbor = FindNeighborNodeAtBottom(node);
            node.LeftNeighbor = FindNeighborNodeAtLeft(node);
            node.RightNeighbor = FindNeighborNodeAtRight(node);

            node.TopLeftNeighbor = FindNeighborNodeAtLeft(node.TopNeighbor);
            node.TopRightNeighbor = FindNeighborNodeAtRight(node.TopNeighbor);
            node.BottomLeftNeighbor = FindNeighborNodeAtLeft(node.BottomNeighbor);
            node.BottomRightNeighbor = FindNeighborNodeAtRight(node.BottomNeighbor);

            if (node.Children.Count == 0)
            {
                return;
            }

            for (int i = 0; i < node.Children.Count; i++)
            {
                ConnectNodes(node.Children[i]);
            }
        }
        /// <summary>
        /// Searches for the neighbor node at top position (from above)
        /// </summary>
        /// <returns>Returns the neighbor node at top position if exists.</returns>
        private static QuadTreeNode FindNeighborNodeAtTop(QuadTreeNode node)
        {
            var parent = node?.Parent;

            if (parent == null)
            {
                return null;
            }

            if (node == parent.TopLeftChild)
            {
                return FindNeighborNodeAtTop(parent)?.BottomLeftChild;
            }
            else if (node == parent.TopRightChild)
            {
                return FindNeighborNodeAtTop(parent)?.BottomRightChild;
            }
            else if (node == parent.BottomLeftChild)
            {
                return parent.TopLeftChild;
            }
            else if (node == parent.BottomRightChild)
            {
                return parent.TopRightChild;
            }

            return null;
        }
        /// <summary>
        /// Searches for the neighbor node at bottom position (from above)
        /// </summary>
        /// <returns>Returns the neighbor node at bottom position if exists.</returns>
        private static QuadTreeNode FindNeighborNodeAtBottom(QuadTreeNode node)
        {
            var parent = node?.Parent;

            if (parent == null)
            {
                return null;
            }

            if (node == parent.TopLeftChild)
            {
                return parent.BottomLeftChild;
            }
            else if (node == parent.TopRightChild)
            {
                return parent.BottomRightChild;
            }
            else if (node == parent.BottomLeftChild)
            {
                return FindNeighborNodeAtBottom(parent)?.TopLeftChild;
            }
            else if (node == parent.BottomRightChild)
            {
                return FindNeighborNodeAtBottom(parent)?.TopRightChild;
            }

            return null;
        }
        /// <summary>
        /// Searches for the neighbor node at right position(from above)
        /// </summary>
        /// <returns>Returns the neighbor node at top position if exists.</returns>
        private static QuadTreeNode FindNeighborNodeAtRight(QuadTreeNode node)
        {
            var parent = node?.Parent;

            if (parent == null)
            {
                return null;
            }

            if (node == parent.TopLeftChild)
            {
                return parent.TopRightChild;
            }
            else if (node == parent.TopRightChild)
            {
                return FindNeighborNodeAtRight(parent)?.TopLeftChild;
            }
            else if (node == parent.BottomLeftChild)
            {
                return parent.BottomRightChild;
            }
            else if (node == parent.BottomRightChild)
            {
                return FindNeighborNodeAtRight(parent)?.BottomLeftChild;
            }

            return null;
        }
        /// <summary>
        /// Searches for the neighbor node at left position (from above)
        /// </summary>
        /// <returns>Returns the neighbor node at left position if exists.</returns>
        private static QuadTreeNode FindNeighborNodeAtLeft(QuadTreeNode node)
        {
            var parent = node?.Parent;

            if (parent == null)
            {
                return null;
            }

            if (node == parent.TopLeftChild)
            {
                return FindNeighborNodeAtLeft(parent)?.TopRightChild;
            }
            else if (node == parent.TopRightChild)
            {
                return parent.TopLeftChild;
            }
            else if (node == parent.BottomLeftChild)
            {
                return FindNeighborNodeAtLeft(parent)?.BottomRightChild;
            }
            else if (node == parent.BottomRightChild)
            {
                return parent.BottomLeftChild;
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
            if (maxDepth < 0)
            {
                return [];
            }

            if (Children.Count == 0)
            {
                return [BoundingBox];
            }

            if (maxDepth > 0 && Level == maxDepth)
            {
                return [BoundingBox];
            }

            return Children.SelectMany(c => c.GetBoundingBoxes(maxDepth)).ToArray();
        }
        /// <summary>
        /// Gets all leaf nodes
        /// </summary>
        /// <returns>Returns all leaf nodes</returns>
        public IEnumerable<QuadTreeNode> GetLeafNodes()
        {
            if (Children.Count == 0)
            {
                return [this];
            }

            return Children.SelectMany(c => c.GetLeafNodes());
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
        /// Gets node at position
        /// </summary>
        /// <param name="node">Node to test</param>
        /// <param name="position">Position</param>
        /// <returns>Returns the leaf node which contains the specified position</returns>
        public static QuadTreeNode GetNodeAtPosition(QuadTreeNode node, Vector3 position)
        {
            if (node.IsLeaf)
            {
                // Leaf node. Test against boundaries
                if (node.BoundingBox.Contains(position) != ContainmentType.Disjoint)
                {
                    return node;
                }

                return null;
            }

            for (int i = 0; i < node.Children.Count; i++)
            {
                var childNode = GetNodeAtPosition(node.Children[i], position);
                if (childNode != null)
                {
                    return childNode;
                }
            }

            return null;
        }
        /// <summary>
        /// Gets the closest node to the specified position
        /// </summary>
        /// <param name="node">Node to test</param>
        /// <param name="position">Position</param>
        /// <returns>Returns the closest leaf node to the specified position</returns>
        public static QuadTreeNode GetClosestNodeAtPosition(QuadTreeNode node, Vector3 position)
        {
            QuadTreeNode n = null;
            float dist = float.MaxValue;
            for (int i = 0; i < node.Children.Count; i++)
            {
                float d = Vector3.DistanceSquared(position, node.Children[i].Center);
                if (d < dist)
                {
                    dist = d;
                    n = node.Children[i];
                }
            }

            if (n?.Children.Count > 0)
            {
                return GetClosestNodeAtPosition(n, position);
            }

            return n;
        }

        /// <summary>
        /// Gets the leaf nodes contained into the specified frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the leaf nodes contained into the frustum</returns>
        public static IEnumerable<QuadTreeNode> GetNodesInVolume(QuadTreeNode node, BoundingFrustum frustum)
        {
            if (node.IsLeaf)
            {
                if (frustum.Contains(node.BoundingBox) != ContainmentType.Disjoint)
                {
                    yield return node;
                }

                yield break;
            }

            for (int i = 0; i < node.Children.Count; i++)
            {
                var childNodes = GetNodesInVolume(node.Children[i], frustum);
                foreach (var child in childNodes)
                {
                    yield return child;
                }
            }
        }
        /// <summary>
        /// Gets the leaf nodes contained into the specified bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the leaf nodes contained into the bounding box</returns>
        public static IEnumerable<QuadTreeNode> GetNodesInVolume(QuadTreeNode node, BoundingBox bbox)
        {
            if (node.IsLeaf)
            {
                if (bbox.Contains(node.BoundingBox) != ContainmentType.Disjoint)
                {
                    yield return node;
                }

                yield break;
            }

            for (int i = 0; i < node.Children.Count; i++)
            {
                var childNodes = GetNodesInVolume(node.Children[i], bbox);
                foreach (var child in childNodes)
                {
                    yield return child;
                }
            }
        }
        /// <summary>
        /// Gets the leaf nodes contained into the specified bounding sphere
        /// </summary>
        /// <param name="sphere">Bounding sphere</param>
        /// <returns>Returns the leaf nodes contained into the bounding sphere</returns>
        public static IEnumerable<QuadTreeNode> GetNodesInVolume(QuadTreeNode node, BoundingSphere sphere)
        {
            if (node.IsLeaf)
            {
                var bbox = node.BoundingBox;
                if (sphere.Contains(ref bbox) != ContainmentType.Disjoint)
                {
                    yield return node;
                }

                yield break;
            }

            for (int i = 0; i < node.Children.Count; i++)
            {
                var childNodes = GetNodesInVolume(node.Children[i], sphere);
                foreach (var child in childNodes)
                {
                    yield return child;
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Children == null)
            {
                //Leaf node
                return $"{nameof(QuadTreeNode)} {Id}; Depth {Level}";
            }
            else
            {
                //Node
                return $"{nameof(QuadTreeNode)} {Id}; Depth {Level}; Children {Children.Count}";
            }
        }
    }
}
