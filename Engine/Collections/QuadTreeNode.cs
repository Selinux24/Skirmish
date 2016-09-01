using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.Collections
{
    /// <summary>
    /// Quadtree node
    /// </summary>
    public class QuadTreeNode
    {
        /// <summary>
        /// Static node count
        /// </summary>
        private static int NodeCount = 0;

        /// <summary>
        /// Recursive partition creation
        /// </summary>
        /// <param name="quadTree">Quadtree</param>
        /// <param name="parent">Parent node</param>
        /// <param name="bbox">Parent bounding box</param>
        /// <param name="maxDepth">Maximum depth</param>
        /// <param name="treeDepth">Current depth</param>
        /// <returns>Returns new node</returns>
        public static QuadTreeNode CreatePartitions(
            QuadTree quadTree, QuadTreeNode parent,
            BoundingBox bbox,
            int maxDepth,
            int treeDepth)
        {
            if (parent == null) NodeCount = 0;

            if (treeDepth < maxDepth)
            {
                QuadTreeNode node = new QuadTreeNode(quadTree, parent)
                {
                    Id = NodeCount++,
                    Level = treeDepth,
                    BoundingBox = bbox,
                };

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

                QuadTreeNode topLeftChild = CreatePartitions(quadTree, node, topLeftBox, maxDepth, treeDepth + 1);
                QuadTreeNode topRightChild = CreatePartitions(quadTree, node, topRightBox, maxDepth, treeDepth + 1);
                QuadTreeNode bottomLeftChild = CreatePartitions(quadTree, node, bottomLeftBox, maxDepth, treeDepth + 1);
                QuadTreeNode bottomRightChild = CreatePartitions(quadTree, node, bottomRightBox, maxDepth, treeDepth + 1);

                List<QuadTreeNode> childList = new List<QuadTreeNode>();

                if (topLeftChild != null) childList.Add(topLeftChild);
                if (topRightChild != null) childList.Add(topRightChild);
                if (bottomLeftChild != null) childList.Add(bottomLeftChild);
                if (bottomRightChild != null) childList.Add(bottomRightChild);

                if (childList.Count > 0)
                {
                    node.Children = childList.ToArray();
                    node.TopLeftChild = topLeftChild;
                    node.TopRightChild = topRightChild;
                    node.BottomLeftChild = bottomLeftChild;
                    node.BottomRightChild = bottomRightChild;
                }

                return node;
            }

            return null;
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
        /// Gets the child node al top lef position (from above)
        /// </summary>
        public QuadTreeNode TopLeftChild { get; private set; }
        /// <summary>
        /// Gets the child node al top right position (from above)
        /// </summary>
        public QuadTreeNode TopRightChild { get; private set; }
        /// <summary>
        /// Gets the child node al bottom lef position (from above)
        /// </summary>
        public QuadTreeNode BottomLeftChild { get; private set; }
        /// <summary>
        /// Gets the child node al bottom right position (from above)
        /// </summary>
        public QuadTreeNode BottomRightChild { get; private set; }
        /// <summary>
        /// Gets the neighbour at top position (from above)
        /// </summary>
        public QuadTreeNode TopNeighbour { get; private set; }
        /// <summary>
        /// Gets the neighbour at bottom position (from above)
        /// </summary>
        public QuadTreeNode BottomNeighbour { get; private set; }
        /// <summary>
        /// Gets the neighbour at left position (from above)
        /// </summary>
        public QuadTreeNode LeftNeighbour { get; private set; }
        /// <summary>
        /// Gets the neighbour at right position (from above)
        /// </summary>
        public QuadTreeNode RightNeighbour { get; private set; }
        /// <summary>
        /// Node Id
        /// </summary>
        public int Id;
        /// <summary>
        /// Depth level
        /// </summary>
        public int Level;
        /// <summary>
        /// Bounding box
        /// </summary>
        public BoundingBox BoundingBox;
        /// <summary>
        /// Gets the node center position
        /// </summary>
        public Vector3 Center
        {
            get
            {
                return (this.BoundingBox.Maximum + this.BoundingBox.Minimum) * 0.5f;
            }
        }
        /// <summary>
        /// Children list
        /// </summary>
        public QuadTreeNode[] Children;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="quadTree">Quadtree</param>
        /// <param name="parent">Parent node</param>
        public QuadTreeNode(QuadTree quadTree, QuadTreeNode parent)
        {
            this.QuadTree = quadTree;
            this.Parent = parent;
        }
        /// <summary>
        /// Connect nodes in the grid
        /// </summary>
        public void ConnectNodes()
        {
            this.TopNeighbour = this.FindNeighbourNodeAtTop();
            this.BottomNeighbour = this.FindNeighbourNodeAtBottom();
            this.LeftNeighbour = this.FindNeighbourNodeAtLeft();
            this.RightNeighbour = this.FindNeighbourNodeAtRight();

            if (this.Children != null && this.Children.Length > 0)
            {
                for (int i = 0; i < this.Children.Length; i++)
                {
                    this.Children[i].ConnectNodes();
                }
            }
        }
        /// <summary>
        /// Searchs for the neighbour node at top position (from above)
        /// </summary>
        /// <returns>Returns the neighbour node at top position if exists.</returns>
        private QuadTreeNode FindNeighbourNodeAtTop()
        {
            if (this.Parent != null)
            {
                if (this == this.Parent.TopLeftChild)
                {
                    var node = this.Parent.FindNeighbourNodeAtTop();
                    if (node != null)
                    {
                        return node.BottomLeftChild;
                    }
                }
                else if (this == this.Parent.TopRightChild)
                {
                    var node = this.Parent.FindNeighbourNodeAtTop();
                    if (node != null)
                    {
                        return node.BottomRightChild;
                    }
                }
                else if (this == this.Parent.BottomLeftChild)
                {
                    return this.Parent.TopLeftChild;
                }
                else if (this == this.Parent.BottomRightChild)
                {
                    return this.Parent.TopRightChild;
                }
            }

            return null;
        }
        /// <summary>
        /// Searchs for the neighbour node at bottom position (from above)
        /// </summary>
        /// <returns>Returns the neighbour node at bottom position if exists.</returns>
        private QuadTreeNode FindNeighbourNodeAtBottom()
        {
            if (this.Parent != null)
            {
                if (this == this.Parent.TopLeftChild)
                {
                    return this.Parent.BottomLeftChild;
                }
                else if (this == this.Parent.TopRightChild)
                {
                    return this.Parent.BottomRightChild;
                }
                else if (this == this.Parent.BottomLeftChild)
                {
                    var node = this.Parent.FindNeighbourNodeAtBottom();
                    if (node != null)
                    {
                        return node.TopLeftChild;
                    }
                }
                else if (this == this.Parent.BottomRightChild)
                {
                    var node = this.Parent.FindNeighbourNodeAtBottom();
                    if (node != null)
                    {
                        return node.TopRightChild;
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Searchs for the neighbour node at right position(from above)
        /// </summary>
        /// <returns>Returns the neighbour node at top position if exists.</returns>
        private QuadTreeNode FindNeighbourNodeAtRight()
        {
            if (this.Parent != null)
            {
                if (this == this.Parent.TopLeftChild)
                {
                    return this.Parent.TopRightChild;
                }
                else if (this == this.Parent.TopRightChild)
                {
                    var node = this.Parent.FindNeighbourNodeAtRight();
                    if (node != null)
                    {
                        return node.TopLeftChild;
                    }
                }
                else if (this == this.Parent.BottomLeftChild)
                {
                    return this.Parent.BottomRightChild;
                }
                else if (this == this.Parent.BottomRightChild)
                {
                    var node = this.Parent.FindNeighbourNodeAtRight();
                    if (node != null)
                    {
                        return node.BottomLeftChild;
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Searchs for the neighbour node at left position (from above)
        /// </summary>
        /// <returns>Returns the neighbour node at left position if exists.</returns>
        private QuadTreeNode FindNeighbourNodeAtLeft()
        {
            if (this.Parent != null)
            {
                if (this == this.Parent.TopLeftChild)
                {
                    var node = this.Parent.FindNeighbourNodeAtLeft();
                    if (node != null)
                    {
                        return node.TopRightChild;
                    }
                }
                else if (this == this.Parent.TopRightChild)
                {
                    return this.Parent.TopLeftChild;
                }
                else if (this == this.Parent.BottomLeftChild)
                {
                    var node = this.Parent.FindNeighbourNodeAtLeft();
                    if (node != null)
                    {
                        return node.BottomRightChild;
                    }
                }
                else if (this == this.Parent.BottomRightChild)
                {
                    return this.Parent.BottomLeftChild;
                }
            }

            return null;
        }
        /// <summary>
        /// Get bounding boxes of specified level
        /// </summary>
        /// <param name="maxDepth">Maximum depth (if zero there is no limit)</param>
        /// <returns>Returns bounding boxes of specified depth</returns>
        public BoundingBox[] GetBoundingBoxes(int maxDepth = 0)
        {
            List<BoundingBox> bboxes = new List<BoundingBox>();

            if (this.Children != null)
            {
                bool haltByDepth = maxDepth > 0 ? this.Level == maxDepth : false;
                if (haltByDepth)
                {
                    Array.ForEach(this.Children, (c) =>
                    {
                        bboxes.Add(c.BoundingBox);
                    });
                }
                else
                {
                    Array.ForEach(this.Children, (c) =>
                    {
                        bboxes.AddRange(c.GetBoundingBoxes(maxDepth));
                    });
                }
            }
            else
            {
                bboxes.Add(this.BoundingBox);
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

            if (this.Children != null)
            {
                for (int i = 0; i < this.Children.Length; i++)
                {
                    int cLevel = this.Children[i].GetMaxLevel();

                    if (cLevel > level) level = cLevel;
                }
            }
            else
            {
                level = this.Level;
            }

            return level;
        }

        /// <summary>
        /// Gets the tail nodes contained into the specified frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the tail nodes contained into the frustum</returns>
        public QuadTreeNode[] GetNodesInVolume(ref BoundingFrustum frustum)
        {
            List<QuadTreeNode> nodes = new List<QuadTreeNode>();

            if (this.Children == null)
            {
                if (frustum.Contains(this.BoundingBox) != ContainmentType.Disjoint)
                {
                    nodes.Add(this);
                }
            }
            else
            {
                for (int i = 0; i < this.Children.Length; i++)
                {
                    var childNodes = this.Children[i].GetNodesInVolume(ref frustum);
                    if (childNodes.Length > 0)
                    {
                        nodes.AddRange(childNodes);
                    }
                }
            }

            return nodes.ToArray();
        }
        /// <summary>
        /// Gets all tail nodes
        /// </summary>
        /// <returns>Returns all tail nodes</returns>
        public QuadTreeNode[] GetTailNodes()
        {
            List<QuadTreeNode> nodes = new List<QuadTreeNode>();

            if (this.Children == null)
            {
                nodes.Add(this);
            }
            else
            {
                for (int i = 0; i < this.Children.Length; i++)
                {
                    var childNodes = this.Children[i].GetTailNodes();
                    if (childNodes.Length > 0)
                    {
                        nodes.AddRange(childNodes);
                    }
                }
            }

            return nodes.ToArray();
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            if (this.Children == null)
            {
                //Tail node
                return string.Format("QuadTreeNode {0}; Depth {1}", this.Id, this.Level);
            }
            else
            {
                //Node
                return string.Format("QuadTreeNode {0}; Depth {1}; Childs {2}", this.Id, this.Level, this.Children.Length);
            }
        }
    }
}
