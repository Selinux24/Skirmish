using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.Collections
{
    using Engine.Common;

    /// <summary>
    /// Picking quad tree node
    /// </summary>
    public class PickingQuadTreeNode<T> : IRayPickable<T> where T : IVertexList, IRayIntersectable
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
        /// <param name="items">All items</param>
        /// <param name="maxDepth">Maximum depth</param>
        /// <param name="treeDepth">Current depth</param>
        /// <returns>Returns new node</returns>
        public static PickingQuadTreeNode<T> CreatePartitions(
            PickingQuadTree<T> quadTree, PickingQuadTreeNode<T> parent,
            BoundingBox bbox, T[] items,
            int maxDepth,
            int treeDepth)
        {
            if (parent == null) NodeCount = 0;

            if (treeDepth <= maxDepth)
            {
                var nodeItems = Array.FindAll(items, t =>
                {
                    var tbox = BoundingBox.FromPoints(t.GetVertices());

                    return Intersection.BoxContainsBox(ref bbox, ref tbox) != ContainmentType.Disjoint;
                });

                if (nodeItems.Length > 0)
                {
                    var node = new PickingQuadTreeNode<T>(quadTree, parent)
                    {
                        Id = -1,
                        Level = treeDepth,
                        BoundingBox = bbox,
                    };

                    bool haltByDepth = treeDepth == maxDepth;
                    if (haltByDepth)
                    {
                        node.Id = NodeCount++;
                        node.Items = nodeItems;
                    }
                    else
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

                        var topLeftChild = CreatePartitions(quadTree, node, topLeftBox, nodeItems, maxDepth, treeDepth + 1);
                        var topRightChild = CreatePartitions(quadTree, node, topRightBox, nodeItems, maxDepth, treeDepth + 1);
                        var bottomLeftChild = CreatePartitions(quadTree, node, bottomLeftBox, nodeItems, maxDepth, treeDepth + 1);
                        var bottomRightChild = CreatePartitions(quadTree, node, bottomRightBox, nodeItems, maxDepth, treeDepth + 1);

                        List<PickingQuadTreeNode<T>> childList = new List<PickingQuadTreeNode<T>>();

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
                    }

                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// Parent
        /// </summary>
        public PickingQuadTree<T> QuadTree { get; private set; }
        /// <summary>
        /// Parent node
        /// </summary>
        public PickingQuadTreeNode<T> Parent { get; private set; }
        /// <summary>
        /// Gets the child node al top lef position (from above)
        /// </summary>
        public PickingQuadTreeNode<T> TopLeftChild { get; private set; }
        /// <summary>
        /// Gets the child node al top right position (from above)
        /// </summary>
        public PickingQuadTreeNode<T> TopRightChild { get; private set; }
        /// <summary>
        /// Gets the child node al bottom lef position (from above)
        /// </summary>
        public PickingQuadTreeNode<T> BottomLeftChild { get; private set; }
        /// <summary>
        /// Gets the child node al bottom right position (from above)
        /// </summary>
        public PickingQuadTreeNode<T> BottomRightChild { get; private set; }

        /// <summary>
        /// Gets the neighbor at top position (from above)
        /// </summary>
        public PickingQuadTreeNode<T> TopNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at bottom position (from above)
        /// </summary>
        public PickingQuadTreeNode<T> BottomNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at left position (from above)
        /// </summary>
        public PickingQuadTreeNode<T> LeftNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at right position (from above)
        /// </summary>
        public PickingQuadTreeNode<T> RightNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at top left position (from above)
        /// </summary>
        public PickingQuadTreeNode<T> TopLeftNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at top right position (from above)
        /// </summary>
        public PickingQuadTreeNode<T> TopRightNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at bottom left position (from above)
        /// </summary>
        public PickingQuadTreeNode<T> BottomLeftNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at bottom right position (from above)
        /// </summary>
        public PickingQuadTreeNode<T> BottomRightNeighbor { get; private set; }

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
        public PickingQuadTreeNode<T>[] Children;
        /// <summary>
        /// Node items
        /// </summary>
        internal T[] Items;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="quadTree">Quadtree</param>
        /// <param name="parent">Parent node</param>
        public PickingQuadTreeNode(PickingQuadTree<T> quadTree, PickingQuadTreeNode<T> parent)
        {
            this.QuadTree = quadTree;
            this.Parent = parent;
        }
        /// <summary>
        /// Connect nodes in the grid
        /// </summary>
        public void ConnectNodes()
        {
            this.TopNeighbor = this.FindNeighborNodeAtTop();
            this.BottomNeighbor = this.FindNeighborNodeAtBottom();
            this.LeftNeighbor = this.FindNeighborNodeAtLeft();
            this.RightNeighbor = this.FindNeighborNodeAtRight();

            this.TopLeftNeighbor = this.TopNeighbor != null ? this.TopNeighbor.FindNeighborNodeAtLeft() : null;
            this.TopRightNeighbor = this.TopNeighbor != null ? this.TopNeighbor.FindNeighborNodeAtRight() : null;
            this.BottomLeftNeighbor = this.BottomNeighbor != null ? this.BottomNeighbor.FindNeighborNodeAtLeft() : null;
            this.BottomRightNeighbor = this.BottomNeighbor != null ? this.BottomNeighbor.FindNeighborNodeAtRight() : null;

            if (this.Children != null && this.Children.Length > 0)
            {
                for (int i = 0; i < this.Children.Length; i++)
                {
                    this.Children[i].ConnectNodes();
                }
            }
        }
        /// <summary>
        /// Searchs for the neighbor node at top position (from above)
        /// </summary>
        /// <returns>Returns the neighbor node at top position if exists.</returns>
        private PickingQuadTreeNode<T> FindNeighborNodeAtTop()
        {
            if (this.Parent != null)
            {
                if (this == this.Parent.TopLeftChild)
                {
                    var node = this.Parent.FindNeighborNodeAtTop();
                    if (node != null)
                    {
                        return node.BottomLeftChild;
                    }
                }
                else if (this == this.Parent.TopRightChild)
                {
                    var node = this.Parent.FindNeighborNodeAtTop();
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
        /// Searchs for the neighbor node at bottom position (from above)
        /// </summary>
        /// <returns>Returns the neighbor node at bottom position if exists.</returns>
        private PickingQuadTreeNode<T> FindNeighborNodeAtBottom()
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
                    var node = this.Parent.FindNeighborNodeAtBottom();
                    if (node != null)
                    {
                        return node.TopLeftChild;
                    }
                }
                else if (this == this.Parent.BottomRightChild)
                {
                    var node = this.Parent.FindNeighborNodeAtBottom();
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
        private PickingQuadTreeNode<T> FindNeighborNodeAtRight()
        {
            if (this.Parent != null)
            {
                if (this == this.Parent.TopLeftChild)
                {
                    return this.Parent.TopRightChild;
                }
                else if (this == this.Parent.TopRightChild)
                {
                    var node = this.Parent.FindNeighborNodeAtRight();
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
                    var node = this.Parent.FindNeighborNodeAtRight();
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
        private PickingQuadTreeNode<T> FindNeighborNodeAtLeft()
        {
            if (this.Parent != null)
            {
                if (this == this.Parent.TopLeftChild)
                {
                    var node = this.Parent.FindNeighborNodeAtLeft();
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
                    var node = this.Parent.FindNeighborNodeAtLeft();
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
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="position">Hit position</param>
        /// <param name="item">Hit item</param>
        /// <returns>Returns true if picked position found</returns>
        /// <remarks>By default, result is constrained to front faces only</remarks>
        public bool PickNearest(ref Ray ray, out Vector3 position, out T item)
        {
            return this.PickNearest(ref ray, true, out position, out item);
        }
        /// <summary>
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing items</param>
        /// <param name="position">Hit position</param>
        /// <param name="item">Hit item</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickNearest(ref Ray ray, bool facingOnly, out Vector3 position, out T item)
        {
            float distance;
            return this.PickNearest(ref ray, facingOnly, out position, out item, out distance);
        }
        /// <summary>
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing items</param>
        /// <param name="position">Hit position</param>
        /// <param name="item">Hit item</param>
        /// <param name="distance">Distance to hit</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickNearest(ref Ray ray, bool facingOnly, out Vector3 position, out T item, out float distance)
        {
            position = Vector3.Zero;
            item = default(T);
            distance = float.MaxValue;

            if (this.Children == null)
            {
                if (this.Items != null && this.Items.Length > 0)
                {
                    #region Per bound test

                    float d;
                    if (Intersection.RayIntersectsBox(ref ray, ref this.BoundingBox, out d))
                    {
                        #region Per item test

                        Vector3 pos;
                        T tri;
                        if (Intersection.IntersectNearest(ref ray, this.Items, facingOnly, out pos, out tri, out d))
                        {
                            position = pos;
                            item = tri;
                            distance = d;

                            return true;
                        }

                        #endregion
                    }

                    #endregion
                }
            }
            else
            {
                SortedDictionary<float, PickingQuadTreeNode<T>> boxHitsByDistance = new SortedDictionary<float, PickingQuadTreeNode<T>>();

                #region Find children contacts by distance to hit in bounding box

                foreach (var node in this.Children)
                {
                    float d;
                    if (Intersection.RayIntersectsBox(ref ray, ref node.BoundingBox, out d))
                    {
                        while (boxHitsByDistance.ContainsKey(d))
                        {
                            // avoid duplicate keys
                            d += 0.0001f;
                        }

                        boxHitsByDistance.Add(d, node);
                    }
                }

                #endregion

                if (boxHitsByDistance.Count > 0)
                {
                    bool intersect = false;

                    #region Find closest item node by node, from closest to farthest

                    Vector3 bestHit = Vector3.Zero;
                    T bestTri = default(T);
                    float bestD = float.MaxValue;

                    foreach (var node in boxHitsByDistance.Values)
                    {
                        Vector3 thisHit;
                        T thisTri;
                        float thisD;
                        if (node.PickNearest(ref ray, facingOnly, out thisHit, out thisTri, out thisD))
                        {
                            // check that the intersection is closer than the nearest intersection found thus far
                            if (thisD < bestD)
                            {
                                // if we have found a closer intersection store the new closest intersection
                                bestHit = thisHit;
                                bestTri = thisTri;
                                bestD = thisD;
                                intersect = true;
                            }
                        }
                    }

                    if (intersect)
                    {
                        position = bestHit;
                        item = bestTri;
                        distance = bestD;
                    }

                    #endregion

                    return intersect;
                }
            }

            return false;
        }
        /// <summary>
        /// Pick first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="position">Hit position</param>
        /// <param name="item">Hit item</param>
        /// <returns>Returns true if picked position found</returns>
        /// <remarks>By default, result is constrained to front faces only</remarks>
        public bool PickFirst(ref Ray ray, out Vector3 position, out T item)
        {
            return this.PickFirst(ref ray, true, out position, out item);
        }
        /// <summary>
        /// Pick first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing items</param>
        /// <param name="position">Hit position</param>
        /// <param name="item">Hit item</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickFirst(ref Ray ray, bool facingOnly, out Vector3 position, out T item)
        {
            float distance;
            return this.PickFirst(ref ray, facingOnly, out position, out item, out distance);
        }
        /// <summary>
        /// Pick first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing items</param>
        /// <param name="position">Hit position</param>
        /// <param name="item">Hit item</param>
        /// <param name="distance">Distance to hit</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickFirst(ref Ray ray, bool facingOnly, out Vector3 position, out T item, out float distance)
        {
            position = Vector3.Zero;
            item = default(T);
            distance = float.MaxValue;

            if (this.Children == null)
            {
                if (this.Items != null && this.Items.Length > 0)
                {
                    #region Per bound test

                    float d;
                    if (Intersection.RayIntersectsBox(ref ray, ref this.BoundingBox, out d))
                    {
                        #region Per item test

                        Vector3 pos;
                        T tri;
                        if (Intersection.IntersectFirst(ref ray, this.Items, facingOnly, out pos, out tri, out d))
                        {
                            position = pos;
                            item = tri;
                            distance = d;

                            return true;
                        }

                        #endregion
                    }

                    #endregion
                }
            }
            else
            {
                #region Find first hit

                foreach (var node in this.Children)
                {
                    float d;
                    if (Intersection.RayIntersectsBox(ref ray, ref node.BoundingBox, out d))
                    {
                        Vector3 thisHit;
                        T thisTri;
                        float thisD;
                        if (node.PickFirst(ref ray, facingOnly, out thisHit, out thisTri, out thisD))
                        {
                            position = thisHit;
                            item = thisTri;
                            distance = thisD;

                            return true;
                        }
                    }
                }

                #endregion
            }

            return false;
        }
        /// <summary>
        /// Pick all position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="positions">Hit positions</param>
        /// <param name="items">Hit items</param>
        /// <returns>Returns true if picked position found</returns>
        /// <remarks>By default, result is constrained to front faces only</remarks>
        public bool PickAll(ref Ray ray, out Vector3[] positions, out T[] items)
        {
            return this.PickAll(ref ray, true, out positions, out items);
        }
        /// <summary>
        /// Pick all position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing items</param>
        /// <param name="positions">Hit positions</param>
        /// <param name="items">Hit items</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickAll(ref Ray ray, bool facingOnly, out Vector3[] positions, out T[] items)
        {
            float[] distances;
            return this.PickAll(ref ray, facingOnly, out positions, out items, out distances);
        }
        /// <summary>
        /// Pick all position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing items</param>
        /// <param name="positions">Hit positions</param>
        /// <param name="items">Hit items</param>
        /// <param name="distances">Distances to hits</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickAll(ref Ray ray, bool facingOnly, out Vector3[] positions, out T[] items, out float[] distances)
        {
            positions = null;
            items = null;
            distances = null;

            if (this.Children == null)
            {
                if (this.Items != null && this.Items.Length > 0)
                {
                    #region Per bound test

                    float d;
                    if (Intersection.RayIntersectsBox(ref ray, ref this.BoundingBox, out d))
                    {
                        #region Per item test

                        Vector3[] pos;
                        T[] tri;
                        float[] ds;
                        if (Intersection.IntersectAll(ref ray, this.Items, facingOnly, out pos, out tri, out ds))
                        {
                            positions = pos;
                            items = tri;
                            distances = ds;

                            return true;
                        }

                        #endregion
                    }

                    #endregion
                }
            }
            else
            {
                #region Find all intersects

                bool intersect = false;

                List<Vector3> hits = new List<Vector3>();
                List<T> tris = new List<T>();
                List<float> dists = new List<float>();

                foreach (var node in this.Children)
                {
                    float d;
                    if (Intersection.RayIntersectsBox(ref ray, ref node.BoundingBox, out d))
                    {
                        Vector3[] thisHits;
                        T[] thisTris;
                        float[] thisDs;
                        if (node.PickAll(ref ray, facingOnly, out thisHits, out thisTris, out thisDs))
                        {
                            for (int i = 0; i < thisHits.Length; i++)
                            {
                                if (!hits.Contains(thisHits[i]))
                                {
                                    hits.Add(thisHits[i]);
                                    tris.Add(thisTris[i]);
                                    dists.Add(thisDs[i]);
                                }
                            }

                            intersect = true;
                        }
                    }
                }

                if (intersect)
                {
                    positions = hits.ToArray();
                    items = tris.ToArray();
                    distances = dists.ToArray();
                }

                return intersect;

                #endregion
            }

            return false;
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
        /// Gets the leaf nodes contained into the specified frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the leaf nodes contained into the frustum</returns>
        public PickingQuadTreeNode<T>[] GetNodesInVolume(ref BoundingFrustum frustum)
        {
            List<PickingQuadTreeNode<T>> nodes = new List<PickingQuadTreeNode<T>>();

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
        /// Gets the leaf nodes contained into the specified bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the leaf nodes contained into the bounding box</returns>
        public PickingQuadTreeNode<T>[] GetNodesInVolume(ref BoundingBox bbox)
        {
            List<PickingQuadTreeNode<T>> nodes = new List<PickingQuadTreeNode<T>>();

            if (this.Children == null)
            {
                if (bbox.Contains(this.BoundingBox) != ContainmentType.Disjoint)
                {
                    nodes.Add(this);
                }
            }
            else
            {
                for (int i = 0; i < this.Children.Length; i++)
                {
                    var childNodes = this.Children[i].GetNodesInVolume(ref bbox);
                    if (childNodes.Length > 0)
                    {
                        nodes.AddRange(childNodes);
                    }
                }
            }

            return nodes.ToArray();
        }
        /// <summary>
        /// Gets the leaf nodes contained into the specified bounding sphere
        /// </summary>
        /// <param name="sphere">Bounding sphere</param>
        /// <returns>Returns the leaf nodes contained into the bounding sphere</returns>
        public PickingQuadTreeNode<T>[] GetNodesInVolume(ref BoundingSphere sphere)
        {
            List<PickingQuadTreeNode<T>> nodes = new List<PickingQuadTreeNode<T>>();

            if (this.Children == null)
            {
                if (sphere.Contains(ref this.BoundingBox) != ContainmentType.Disjoint)
                {
                    nodes.Add(this);
                }
            }
            else
            {
                for (int i = 0; i < this.Children.Length; i++)
                {
                    var childNodes = this.Children[i].GetNodesInVolume(ref sphere);
                    if (childNodes.Length > 0)
                    {
                        nodes.AddRange(childNodes);
                    }
                }
            }

            return nodes.ToArray();
        }
        /// <summary>
        /// Gets all leaf nodes
        /// </summary>
        /// <returns>Returns all leaf nodes</returns>
        public PickingQuadTreeNode<T>[] GetLeafNodes()
        {
            List<PickingQuadTreeNode<T>> nodes = new List<PickingQuadTreeNode<T>>();

            if (this.Children == null)
            {
                nodes.Add(this);
            }
            else
            {
                for (int i = 0; i < this.Children.Length; i++)
                {
                    var childNodes = this.Children[i].GetLeafNodes();
                    if (childNodes.Length > 0)
                    {
                        nodes.AddRange(childNodes);
                    }
                }
            }

            return nodes.ToArray();
        }
        /// <summary>
        /// Gets node at position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns the leaf node wich contains the specified position</returns>
        public PickingQuadTreeNode<T> GetNode(Vector3 position)
        {
            if (this.Children == null)
            {
                if (this.BoundingBox.Contains(position) != ContainmentType.Disjoint)
                {
                    return this;
                }
            }
            else
            {
                for (int i = 0; i < this.Children.Length; i++)
                {
                    var childNode = this.Children[i].GetNode(position);
                    if (childNode != null)
                    {
                        return childNode;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            if (this.Children == null)
            {
                //Leaf node
                return string.Format("PickingQuadTreeNode {0}; Depth {1}; Items {2}", this.Id, this.Level, this.Items.Length);
            }
            else
            {
                //Node
                return string.Format("PickingQuadTreeNode {0}; Depth {1}; Childs {2}", this.Id, this.Level, this.Children.Length);
            }
        }
    }
}
