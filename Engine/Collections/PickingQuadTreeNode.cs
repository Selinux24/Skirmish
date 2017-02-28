using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.Collections
{
    using Engine.Common;

    /// <summary>
    /// Quadtree node
    /// </summary>
    public class PickingQuadTreeNode : IPickable
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
        /// <param name="triangles">All triangles</param>
        /// <param name="maxDepth">Maximum depth</param>
        /// <param name="treeDepth">Current depth</param>
        /// <returns>Returns new node</returns>
        public static PickingQuadTreeNode CreatePartitions(
            PickingQuadTree quadTree, PickingQuadTreeNode parent,
            BoundingBox bbox, Triangle[] triangles,
            int maxDepth,
            int treeDepth)
        {
            if (parent == null) NodeCount = 0;

            if (treeDepth <= maxDepth)
            {
                Triangle[] nodeTriangles = Array.FindAll(triangles, t =>
                {
                    BoundingBox tbox = BoundingBox.FromPoints(t.GetCorners());

                    return Intersection.BoxContainsBox(ref bbox, ref tbox) != ContainmentType.Disjoint;
                });

                if (nodeTriangles.Length > 0)
                {
                    PickingQuadTreeNode node = new PickingQuadTreeNode(quadTree, parent)
                    {
                        Id = -1,
                        Level = treeDepth,
                        BoundingBox = bbox,
                    };

                    bool haltByDepth = treeDepth == maxDepth;
                    if (haltByDepth)
                    {
                        node.Id = NodeCount++;
                        node.Triangles = nodeTriangles;
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

                        PickingQuadTreeNode topLeftChild = CreatePartitions(quadTree, node, topLeftBox, triangles, maxDepth, treeDepth + 1);
                        PickingQuadTreeNode topRightChild = CreatePartitions(quadTree, node, topRightBox, triangles, maxDepth, treeDepth + 1);
                        PickingQuadTreeNode bottomLeftChild = CreatePartitions(quadTree, node, bottomLeftBox, triangles, maxDepth, treeDepth + 1);
                        PickingQuadTreeNode bottomRightChild = CreatePartitions(quadTree, node, bottomRightBox, triangles, maxDepth, treeDepth + 1);

                        List<PickingQuadTreeNode> childList = new List<PickingQuadTreeNode>();

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
        /// Recursive partition creation
        /// </summary>
        /// <param name="quadTree">Quadtree</param>
        /// <param name="parent">Parent node</param>
        /// <param name="bbox">Parent bounding box</param>
        /// <param name="vertices">Vertices contained into parent bounding box</param>
        /// <param name="maxDepth">Maximum depth</param>
        /// <param name="treeDepth">Current depth</param>
        /// <returns>Returns new node</returns>
        public static PickingQuadTreeNode CreatePartitions(
            PickingQuadTree quadTree, PickingQuadTreeNode parent,
            BoundingBox bbox, VertexData[] vertices,
            int maxDepth,
            int treeDepth)
        {
            if (parent == null) NodeCount = 0;

            if (treeDepth <= maxDepth)
            {
                var nodeVertices = Array.FindAll(vertices, p =>
                {
                    var containment = bbox.Contains(p.Position.Value);

                    return containment != ContainmentType.Disjoint;
                });

                if (nodeVertices.Length > 0)
                {
                    PickingQuadTreeNode node = new PickingQuadTreeNode(quadTree, parent)
                    {
                        Id = -1,
                        Level = treeDepth,
                        BoundingBox = bbox,
                    };

                    bool haltByDepth = treeDepth == maxDepth;
                    if (haltByDepth)
                    {
                        node.Id = NodeCount++;
                        node.Vertices = nodeVertices;

                        //Get positions
                        List<Vector3> positions = new List<Vector3>();
                        Array.ForEach(nodeVertices, v => positions.Add(v.Position.Value));

                        //Triangles per node
                        int nodeSide = (int)Math.Sqrt(positions.Count) - 1;

                        //Get indices
                        uint[] indices = GeometryUtil.GenerateIndices(IndexBufferShapeEnum.Full, nodeSide * nodeSide * 2);

                        node.Triangles = Triangle.ComputeTriangleList(
                            SharpDX.Direct3D.PrimitiveTopology.TriangleList,
                            positions.ToArray(),
                            indices);
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

                        PickingQuadTreeNode topLeftChild = CreatePartitions(quadTree, node, topLeftBox, nodeVertices, maxDepth, treeDepth + 1);
                        PickingQuadTreeNode topRightChild = CreatePartitions(quadTree, node, topRightBox, nodeVertices, maxDepth, treeDepth + 1);
                        PickingQuadTreeNode bottomLeftChild = CreatePartitions(quadTree, node, bottomLeftBox, nodeVertices, maxDepth, treeDepth + 1);
                        PickingQuadTreeNode bottomRightChild = CreatePartitions(quadTree, node, bottomRightBox, nodeVertices, maxDepth, treeDepth + 1);

                        List<PickingQuadTreeNode> childList = new List<PickingQuadTreeNode>();

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
        public PickingQuadTree QuadTree { get; private set; }
        /// <summary>
        /// Parent node
        /// </summary>
        public PickingQuadTreeNode Parent { get; private set; }
        /// <summary>
        /// Gets the child node al top lef position (from above)
        /// </summary>
        public PickingQuadTreeNode TopLeftChild { get; private set; }
        /// <summary>
        /// Gets the child node al top right position (from above)
        /// </summary>
        public PickingQuadTreeNode TopRightChild { get; private set; }
        /// <summary>
        /// Gets the child node al bottom lef position (from above)
        /// </summary>
        public PickingQuadTreeNode BottomLeftChild { get; private set; }
        /// <summary>
        /// Gets the child node al bottom right position (from above)
        /// </summary>
        public PickingQuadTreeNode BottomRightChild { get; private set; }

        /// <summary>
        /// Gets the neighbor at top position (from above)
        /// </summary>
        public PickingQuadTreeNode TopNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at bottom position (from above)
        /// </summary>
        public PickingQuadTreeNode BottomNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at left position (from above)
        /// </summary>
        public PickingQuadTreeNode LeftNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at right position (from above)
        /// </summary>
        public PickingQuadTreeNode RightNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at top left position (from above)
        /// </summary>
        public PickingQuadTreeNode TopLeftNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at top right position (from above)
        /// </summary>
        public PickingQuadTreeNode TopRightNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at bottom left position (from above)
        /// </summary>
        public PickingQuadTreeNode BottomLeftNeighbor { get; private set; }
        /// <summary>
        /// Gets the neighbor at bottom right position (from above)
        /// </summary>
        public PickingQuadTreeNode BottomRightNeighbor { get; private set; }

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
        public PickingQuadTreeNode[] Children;
        /// <summary>
        /// Node vertices
        /// </summary>
        internal VertexData[] Vertices;
        /// <summary>
        /// Node triangles
        /// </summary>
        internal Triangle[] Triangles;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="quadTree">Quadtree</param>
        /// <param name="parent">Parent node</param>
        public PickingQuadTreeNode(PickingQuadTree quadTree, PickingQuadTreeNode parent)
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
        private PickingQuadTreeNode FindNeighborNodeAtTop()
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
        private PickingQuadTreeNode FindNeighborNodeAtBottom()
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
        private PickingQuadTreeNode FindNeighborNodeAtRight()
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
        private PickingQuadTreeNode FindNeighborNodeAtLeft()
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
        /// <param name="triangle">Hit triangle</param>
        /// <returns>Returns true if picked position found</returns>
        /// <remarks>By default, result is constrained to front faces only</remarks>
        public bool PickNearest(ref Ray ray, out Vector3 position, out Triangle triangle)
        {
            return this.PickNearest(ref ray, true, out position, out triangle);
        }
        /// <summary>
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="position">Hit position</param>
        /// <param name="triangle">Hit triangle</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickNearest(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle)
        {
            float distance;
            return this.PickNearest(ref ray, facingOnly, out position, out triangle, out distance);
        }
        /// <summary>
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="position">Hit position</param>
        /// <param name="triangle">Hit triangle</param>
        /// <param name="distance">Distance to hit</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickNearest(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            position = Vector3.Zero;
            triangle = new Triangle();
            distance = float.MaxValue;

            if (this.Children == null)
            {
                if (this.Triangles != null && this.Triangles.Length > 0)
                {
                    #region Per bound test

                    float d;
                    if (Intersection.RayIntersectsBox(ref ray, ref this.BoundingBox, out d))
                    {
                        #region Per triangle test

                        Vector3 pos;
                        Triangle tri;
                        if (Triangle.IntersectNearest(ref ray, this.Triangles, facingOnly, out pos, out tri, out d))
                        {
                            position = pos;
                            triangle = tri;
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
                SortedDictionary<float, PickingQuadTreeNode> boxHitsByDistance = new SortedDictionary<float, PickingQuadTreeNode>();

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

                    #region Find closest triangle node by node, from closest to farthest

                    Vector3 bestHit = Vector3.Zero;
                    Triangle bestTri = new Triangle();
                    float bestD = float.MaxValue;

                    foreach (var node in boxHitsByDistance.Values)
                    {
                        Vector3 thisHit;
                        Triangle thisTri;
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
                        triangle = bestTri;
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
        /// <param name="triangle">Hit triangle</param>
        /// <returns>Returns true if picked position found</returns>
        /// <remarks>By default, result is constrained to front faces only</remarks>
        public bool PickFirst(ref Ray ray, out Vector3 position, out Triangle triangle)
        {
            return this.PickFirst(ref ray, true, out position, out triangle);
        }
        /// <summary>
        /// Pick first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="position">Hit position</param>
        /// <param name="triangle">Hit triangle</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickFirst(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle)
        {
            float distance;
            return this.PickFirst(ref ray, facingOnly, out position, out triangle, out distance);
        }
        /// <summary>
        /// Pick first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="position">Hit position</param>
        /// <param name="triangle">Hit triangle</param>
        /// <param name="distance">Distance to hit</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickFirst(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            position = Vector3.Zero;
            triangle = new Triangle();
            distance = float.MaxValue;

            if (this.Children == null)
            {
                if (this.Triangles != null && this.Triangles.Length > 0)
                {
                    #region Per bound test

                    float d;
                    if (Intersection.RayIntersectsBox(ref ray, ref this.BoundingBox, out d))
                    {
                        #region Per triangle test

                        Vector3 pos;
                        Triangle tri;
                        if (Triangle.IntersectFirst(ref ray, this.Triangles, facingOnly, out pos, out tri, out d))
                        {
                            position = pos;
                            triangle = tri;
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
                        Triangle thisTri;
                        float thisD;
                        if (node.PickFirst(ref ray, facingOnly, out thisHit, out thisTri, out thisD))
                        {
                            position = thisHit;
                            triangle = thisTri;
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
        /// <param name="triangles">Hit triangles</param>
        /// <returns>Returns true if picked position found</returns>
        /// <remarks>By default, result is constrained to front faces only</remarks>
        public bool PickAll(ref Ray ray, out Vector3[] positions, out Triangle[] triangles)
        {
            return this.PickAll(ref ray, true, out positions, out triangles);
        }
        /// <summary>
        /// Pick all position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="positions">Hit positions</param>
        /// <param name="triangles">Hit triangles</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickAll(ref Ray ray, bool facingOnly, out Vector3[] positions, out Triangle[] triangles)
        {
            float[] distances;
            return this.PickAll(ref ray, facingOnly, out positions, out triangles, out distances);
        }
        /// <summary>
        /// Pick all position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="positions">Hit positions</param>
        /// <param name="triangles">Hit triangles</param>
        /// <param name="distances">Distances to hits</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickAll(ref Ray ray, bool facingOnly, out Vector3[] positions, out Triangle[] triangles, out float[] distances)
        {
            positions = null;
            triangles = null;
            distances = null;

            if (this.Children == null)
            {
                if (this.Triangles != null && this.Triangles.Length > 0)
                {
                    #region Per bound test

                    float d;
                    if (Intersection.RayIntersectsBox(ref ray, ref this.BoundingBox, out d))
                    {
                        #region Per triangle test

                        Vector3[] pos;
                        Triangle[] tri;
                        float[] ds;
                        if (Triangle.IntersectAll(ref ray, this.Triangles, facingOnly, out pos, out tri, out ds))
                        {
                            positions = pos;
                            triangles = tri;
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
                List<Triangle> tris = new List<Triangle>();
                List<float> dists = new List<float>();

                foreach (var node in this.Children)
                {
                    float d;
                    if (Intersection.RayIntersectsBox(ref ray, ref node.BoundingBox, out d))
                    {
                        Vector3[] thisHits;
                        Triangle[] thisTris;
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
                    triangles = tris.ToArray();
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
        /// Gets the tail nodes contained into the specified frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the tail nodes contained into the frustum</returns>
        public PickingQuadTreeNode[] GetNodesInVolume(ref BoundingFrustum frustum)
        {
            List<PickingQuadTreeNode> nodes = new List<PickingQuadTreeNode>();

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
        public PickingQuadTreeNode[] GetTailNodes()
        {
            List<PickingQuadTreeNode> nodes = new List<PickingQuadTreeNode>();

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
        /// Gets node at position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns the tail node wich contains the specified position</returns>
        public PickingQuadTreeNode GetNode(Vector3 position)
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
                //Tail node
                return string.Format("QuadTreeNode {0}; Depth {1}; Triangles {2}", this.Id, this.Level, this.Triangles.Length);
            }
            else
            {
                //Node
                return string.Format("QuadTreeNode {0}; Depth {1}; Childs {2}", this.Id, this.Level, this.Children.Length);
            }
        }
    }
}
