using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Collections.Generic
{
    using Engine.Common;

    /// <summary>
    /// Quadtree node
    /// </summary>
    public class QuadTreeNode<T> where T : IVertexList
    {
        /// <summary>
        /// Recursive partition creation
        /// </summary>
        /// <param name="quadTree">Quadtree</param>
        /// <param name="parent">Parent node</param>
        /// <param name="bbox">Parent bounding box</param>
        /// <param name="items">All items</param>
        /// <param name="maxDepth">Maximum depth</param>
        /// <param name="treeDepth">Current depth</param>
        /// <param name="nodeCount">Node count</param>
        /// <returns>Returns new node</returns>
        public static QuadTreeNode<T> CreatePartitions(
            QuadTree<T> quadTree, QuadTreeNode<T> parent,
            BoundingBox bbox, IEnumerable<T> items,
            int maxDepth,
            int treeDepth,
            ref int nodeCount)
        {
            if (treeDepth <= maxDepth)
            {
                var nodeItems = items.Where(i =>
                {
                    var tbox = BoundingBox.FromPoints(i.GetVertices().ToArray());

                    return Intersection.BoxContainsBox(bbox, tbox) != ContainmentType.Disjoint;
                });

                if (nodeItems.Any())
                {
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
                        node.Items.AddRange(nodeItems);
                    }
                    else
                    {
                        // Initialize node partitions
                        IntializeNode(quadTree, node, bbox, nodeItems, maxDepth, treeDepth + 1, ref nodeCount);
                    }

                    return node;
                }
            }

            return null;
        }
        /// <summary>
        /// Initializes node partitinos
        /// </summary>
        /// <param name="quadTree">Quadtree</param>
        /// <param name="node">Current node</param>
        /// <param name="bbox">Bounding box</param>
        /// <param name="items">Items into the node</param>
        /// <param name="maxDepth">Maximum depth</param>
        /// <param name="nextTreeDepth">Next depth</param>
        /// <param name="nodeCount">Node count</param>
        private static void IntializeNode(
            QuadTree<T> quadTree, QuadTreeNode<T> node,
            BoundingBox bbox, IEnumerable<T> items,
            int maxDepth,
            int nextTreeDepth,
            ref int nodeCount)
        {
            Vector3 M = bbox.Maximum;
            Vector3 c = (bbox.Maximum + bbox.Minimum) * 0.5f;
            Vector3 m = bbox.Minimum;

            //-1-1-1   +0+1+0   -->   mmm    cMc
            BoundingBox topLeftBox = new BoundingBox(new Vector3(m.X, m.Y, m.Z), new Vector3(c.X, M.Y, c.Z));
            //-1-1+0   +0+1+1   -->   mmc    cMM
            BoundingBox topRightBox = new BoundingBox(new Vector3(m.X, m.Y, c.Z), new Vector3(c.X, M.Y, M.Z));
            //+0-1-1   +1+1+0   -->   cmm    MMc
            BoundingBox bottomLeftBox = new BoundingBox(new Vector3(c.X, m.Y, m.Z), new Vector3(M.X, M.Y, c.Z));
            //+0-1+0   +1+1+1   -->   cmc    MMM
            BoundingBox bottomRightBox = new BoundingBox(new Vector3(c.X, m.Y, c.Z), new Vector3(M.X, M.Y, M.Z));

            var topLeftChild = CreatePartitions(quadTree, node, topLeftBox, items, maxDepth, nextTreeDepth, ref nodeCount);
            var topRightChild = CreatePartitions(quadTree, node, topRightBox, items, maxDepth, nextTreeDepth, ref nodeCount);
            var bottomLeftChild = CreatePartitions(quadTree, node, bottomLeftBox, items, maxDepth, nextTreeDepth, ref nodeCount);
            var bottomRightChild = CreatePartitions(quadTree, node, bottomRightBox, items, maxDepth, nextTreeDepth, ref nodeCount);

            List<QuadTreeNode<T>> childList = new List<QuadTreeNode<T>>();

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
        public QuadTree<T> QuadTree { get; private set; }
        /// <summary>
        /// Parent node
        /// </summary>
        public QuadTreeNode<T> Parent { get; private set; }
        /// <summary>
        /// Gets the child node al top lef position (from above)
        /// </summary>
        public QuadTreeNode<T> TopLeftChild { get; private set; }
        /// <summary>
        /// Gets the child node al top right position (from above)
        /// </summary>
        public QuadTreeNode<T> TopRightChild { get; private set; }
        /// <summary>
        /// Gets the child node al bottom lef position (from above)
        /// </summary>
        public QuadTreeNode<T> BottomLeftChild { get; private set; }
        /// <summary>
        /// Gets the child node al bottom right position (from above)
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
        public List<QuadTreeNode<T>> Children { get; private set; } = new List<QuadTreeNode<T>>();
        /// <summary>
        /// Node triangles
        /// </summary>
        public List<T> Items { get; private set; } = new List<T>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="quadTree">Quadtree</param>
        /// <param name="parent">Parent node</param>
        public QuadTreeNode(QuadTree<T> quadTree, QuadTreeNode<T> parent) : base()
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
        /// Searchs for the neighbor node at top position (from above)
        /// </summary>
        /// <returns>Returns the neighbor node at top position if exists.</returns>
        private QuadTreeNode<T> FindNeighborNodeAtTop()
        {
            if (Parent != null)
            {
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
            }

            return null;
        }
        /// <summary>
        /// Searchs for the neighbor node at bottom position (from above)
        /// </summary>
        /// <returns>Returns the neighbor node at bottom position if exists.</returns>
        private QuadTreeNode<T> FindNeighborNodeAtBottom()
        {
            if (Parent != null)
            {
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
            }

            return null;
        }
        /// <summary>
        /// Searchs for the neighbor node at right position(from above)
        /// </summary>
        /// <returns>Returns the neighbor node at top position if exists.</returns>
        private QuadTreeNode<T> FindNeighborNodeAtRight()
        {
            if (Parent != null)
            {
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
            }

            return null;
        }
        /// <summary>
        /// Searchs for the neighbor node at left position (from above)
        /// </summary>
        /// <returns>Returns the neighbor node at left position if exists.</returns>
        private QuadTreeNode<T> FindNeighborNodeAtLeft()
        {
            if (Parent != null)
            {
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
                bool haltByDepth = (maxDepth > 0 && Level == maxDepth);
                if (haltByDepth)
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
        public IEnumerable<QuadTreeNode<T>> GetNodesInVolume(ref BoundingFrustum frustum)
        {
            List<QuadTreeNode<T>> nodes = new List<QuadTreeNode<T>>();

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
        public IEnumerable<QuadTreeNode<T>> GetNodesInVolume(ref BoundingBox bbox)
        {
            List<QuadTreeNode<T>> nodes = new List<QuadTreeNode<T>>();

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
        public IEnumerable<QuadTreeNode<T>> GetNodesInVolume(ref BoundingSphere sphere)
        {
            List<QuadTreeNode<T>> nodes = new List<QuadTreeNode<T>>();

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
        public IEnumerable<QuadTreeNode<T>> GetLeafNodes()
        {
            List<QuadTreeNode<T>> nodes = new List<QuadTreeNode<T>>();

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
        /// <returns>Returns the leaf node wich contains the specified position</returns>
        public QuadTreeNode<T> GetNode(Vector3 position)
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
                return string.Format("QuadTreeNode {0}; Depth {1}; Items {2}", Id, Level, Items.Count);
            }
            else
            {
                //Node
                return string.Format("QuadTreeNode {0}; Depth {1}; Childs {2}", Id, Level, Children.Count);
            }
        }
    }
}
