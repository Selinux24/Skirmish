using Engine.Content;
using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.Common
{
    /// <summary>
    /// Quadtree node
    /// </summary>
    public class QuadTreeNode
    {
        /// <summary>
        /// Recursive partition creation
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bbox">Parent bounding box</param>
        /// <param name="triangles">All triangles</param>
        /// <param name="treeDepth">Current depth</param>
        /// <param name="description">Description</param>
        /// <returns></returns>
        public static QuadTreeNode CreatePartitions(
            Game game,
            QuadTree quadTree,
            QuadTreeNode parent,
            BoundingBox bbox,
            Triangle[] triangles,
            int treeDepth,
            TerrainDescription description)
        {
            Triangle[] nodeTriangles = Array.FindAll(triangles, t =>
            {
                BoundingBox tbox = BoundingBox.FromPoints(t.GetCorners());

                return Collision.BoxContainsBox(ref bbox, ref tbox) != ContainmentType.Disjoint;
            });

            if (nodeTriangles.Length > 0)
            {
                QuadTreeNode node = new QuadTreeNode(quadTree, parent)
                {
                    Level = treeDepth,
                    BoundingBox = bbox,
                };

                bool haltByCount = nodeTriangles.Length < description.Quadtree.MaxTrianglesPerNode;

                if (haltByCount)
                {
                    node.Triangles = nodeTriangles;

                    //Set path finder
                    if (description.PathFinder != null)
                    {
                        node.Grid = Grid.Build(
                            bbox,
                            nodeTriangles,
                            description.PathFinder.NodeSize,
                            description.PathFinder.NodeInclination);
                    }

                    //Set vegetation
                    if (description.Vegetation != null && description.Vegetation.Length > 0)
                    {
                        for (int i = 0; i < description.Vegetation.Length; i++)
                        {
                            TerrainDescription.VegetationDescription vegetationDesc = description.Vegetation[i];

                            VertexData[] vData = null;

                            var vBillboardDesc = vegetationDesc as TerrainDescription.VegetationDescriptionBillboard;
                            if (vBillboardDesc != null)
                            {
                                Vector3[] vertices = ModelContent.GenerateRandomPositions(
                                    bbox,
                                    nodeTriangles,
                                    vBillboardDesc.Saturation,
                                    vBillboardDesc.Seed);
                                if (vertices != null && vertices.Length > 0)
                                {
                                    vData = new VertexData[vertices.Length];

                                    Random rnd = new Random();

                                    for (int v = 0; v < vertices.Length; v++)
                                    {
                                        //Set max / min sizes
                                        Vector2 bbsize = rnd.NextVector2(vBillboardDesc.MinSize, vBillboardDesc.MaxSize);

                                        Vector3 bbpos = vertices[i];
                                        bbpos.Y += bbsize.Y * 0.5f;

                                        vData[v] = VertexData.CreateVertexBillboard(bbpos, bbsize);
                                    }
                                }

                                node.vegetationBillBoardPositions.Add(i, vData);
                            }

                            var vModelDesc = vegetationDesc as TerrainDescription.VegetationDescriptionModel;
                            if (vModelDesc != null)
                            {
                                Vector3[] vertices = ModelContent.GenerateRandomPositions(
                                    bbox,
                                    nodeTriangles,
                                    vModelDesc.Saturation,
                                    vModelDesc.Seed);

                                node.vegetationModelPositions.Add(i, vertices);
                            }
                        }
                    }
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

                    QuadTreeNode topLeftChild = CreatePartitions(game, quadTree, node, topLeftBox, triangles, treeDepth + 1, description);
                    QuadTreeNode topRightChild = CreatePartitions(game, quadTree, node, topRightBox, triangles, treeDepth + 1, description);
                    QuadTreeNode bottomLeftChild = CreatePartitions(game, quadTree, node, bottomLeftBox, triangles, treeDepth + 1, description);
                    QuadTreeNode bottomRightChild = CreatePartitions(game, quadTree, node, bottomRightBox, triangles, treeDepth + 1, description);

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
                }

                return node;
            }

            return null;
        }

        public static QuadTreeNode CreatePartitions(
            Game game,
            QuadTree quadTree, QuadTreeNode parent,
            BoundingBox bbox, VertexData[] vertices,
            int treeDepth,
            TerrainDescription description)
        {
            VertexData[] nodeVertices = Array.FindAll(vertices, p =>
            {
                var containment = bbox.Contains(p.Position.Value);

                return containment != ContainmentType.Disjoint;
            });

            if (nodeVertices.Length > 0)
            {
                QuadTreeNode node = new QuadTreeNode(quadTree, parent)
                {
                    Level = treeDepth,
                    BoundingBox = bbox,
                };

                bool haltByCount = nodeVertices.Length <= description.Quadtree.MaxVerticesByNode;
                if (haltByCount)
                {
                    node.Vertices = nodeVertices;
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

                    QuadTreeNode topLeftChild = CreatePartitions(game, quadTree, node, topLeftBox, nodeVertices, treeDepth + 1, description);
                    QuadTreeNode topRightChild = CreatePartitions(game, quadTree, node, topRightBox, nodeVertices, treeDepth + 1, description);
                    QuadTreeNode bottomLeftChild = CreatePartitions(game, quadTree, node, bottomLeftBox, nodeVertices, treeDepth + 1, description);
                    QuadTreeNode bottomRightChild = CreatePartitions(game, quadTree, node, bottomRightBox, nodeVertices, treeDepth + 1, description);

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

        public QuadTreeNode TopLeftChild { get; private set; }
        public QuadTreeNode TopRightChild { get; private set; }
        public QuadTreeNode BottomLeftChild { get; private set; }
        public QuadTreeNode BottomRightChild { get; private set; }

        public QuadTreeNode TopNeighbour { get; private set; }
        public QuadTreeNode BottomNeighbour { get; private set; }
        public QuadTreeNode LeftNeighbour { get; private set; }
        public QuadTreeNode RightNeighbour { get; private set; }

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
        /// Triangle list
        /// </summary>
        public Triangle[] Triangles;
        /// <summary>
        /// Local model
        /// </summary>
        public Model Model = null;
        /// <summary>
        /// Local pathfinding grid
        /// </summary>
        public Grid Grid = null;
        /// <summary>
        /// Gets if local quad node is culled
        /// </summary>
        public bool Cull = false;

        private Dictionary<int, VertexData[]> vegetationBillBoardPositions = new Dictionary<int, VertexData[]>();

        private Dictionary<int, Vector3[]> vegetationModelPositions = new Dictionary<int, Vector3[]>();

        private VertexData[] Vertices;

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

        public void ConnectNodes()
        {
            this.TopNeighbour = this.GetNeighbourNodeAtTop();
            this.BottomNeighbour = this.GetNeighbourNodeAtBottom();
            this.LeftNeighbour = this.GetNeighbourNodeAtLeft();
            this.RightNeighbour = this.GetNeighbourNodeAtRight();

            if (this.Children != null && this.Children.Length > 0)
            {
                for (int i = 0; i < this.Children.Length; i++)
                {
                    this.Children[i].ConnectNodes();
                }
            }
        }

        private QuadTreeNode GetNeighbourNodeAtTop()
        {
            if (this.Parent != null)
            {
                if (this == this.Parent.TopLeftChild)
                {
                    var node = this.Parent.GetNeighbourNodeAtTop();
                    if (node != null)
                    {
                        return node.BottomLeftChild;
                    }
                }
                else if (this == this.Parent.TopRightChild)
                {
                    var node = this.Parent.GetNeighbourNodeAtTop();
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

        private QuadTreeNode GetNeighbourNodeAtBottom()
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
                    var node = this.Parent.GetNeighbourNodeAtBottom();
                    if (node != null)
                    {
                        return node.TopLeftChild;
                    }
                }
                else if (this == this.Parent.BottomRightChild)
                {
                    var node = this.Parent.GetNeighbourNodeAtBottom();
                    if (node != null)
                    {
                        return node.TopRightChild;
                    }
                }
            }

            return null;
        }

        private QuadTreeNode GetNeighbourNodeAtRight()
        {
            if (this.Parent != null)
            {
                if (this == this.Parent.TopLeftChild)
                {
                    return this.Parent.TopRightChild;
                }
                else if (this == this.Parent.TopRightChild)
                {
                    var node = this.Parent.GetNeighbourNodeAtRight();
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
                    var node = this.Parent.GetNeighbourNodeAtRight();
                    if (node != null)
                    {
                        return node.BottomLeftChild;
                    }
                }
            }

            return null;
        }

        private QuadTreeNode GetNeighbourNodeAtLeft()
        {
            if (this.Parent != null)
            {
                if (this == this.Parent.TopLeftChild)
                {
                    var node = this.Parent.GetNeighbourNodeAtLeft();
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
                    var node = this.Parent.GetNeighbourNodeAtLeft();
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

        public IVertexData[] GetVertexData(VertexTypes vertexType)
        {
            return VertexData.Convert(vertexType, this.Vertices, null, null, Matrix.Identity);
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
            position = Vector3.Zero;
            triangle = new Triangle();

            if (this.Children == null)
            {
                if (this.Triangles != null && this.Triangles.Length > 0)
                {
                    #region Per bound test

                    float d;
                    if (Collision.RayIntersectsBox(ref ray, ref this.BoundingBox, out d))
                    {
                        #region Per triangle test

                        Vector3 pos;
                        Triangle tri;
                        if (Triangle.IntersectNearest(ref ray, this.Triangles, facingOnly, out pos, out tri))
                        {
                            position = pos;
                            triangle = tri;

                            return true;
                        }

                        #endregion
                    }

                    #endregion
                }
            }
            else
            {
                SortedDictionary<float, QuadTreeNode> boxHitsByDistance = new SortedDictionary<float, QuadTreeNode>();

                #region Find children contacts by distance to hit in bounding box

                foreach (QuadTreeNode node in this.Children)
                {
                    float d;
                    if (Collision.RayIntersectsBox(ref ray, ref node.BoundingBox, out d))
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

                    foreach (QuadTreeNode node in boxHitsByDistance.Values)
                    {
                        Vector3 thisHit;
                        Triangle thisTri;
                        if (node.PickNearest(ref ray, out thisHit, out thisTri))
                        {
                            // check that the intersection is closer than the nearest intersection found thus far
                            float thisD = (ray.Position - thisHit).LengthSquared();
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
            position = Vector3.Zero;
            triangle = new Triangle();

            if (this.Children == null)
            {
                if (this.Triangles != null && this.Triangles.Length > 0)
                {
                    #region Per bound test

                    float d;
                    if (Collision.RayIntersectsBox(ref ray, ref this.BoundingBox, out d))
                    {
                        #region Per triangle test

                        Vector3 pos;
                        Triangle tri;
                        if (Triangle.IntersectFirst(ref ray, this.Triangles, facingOnly, out pos, out tri))
                        {
                            position = pos;
                            triangle = tri;

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

                foreach (QuadTreeNode node in this.Children)
                {
                    float d;
                    if (Collision.RayIntersectsBox(ref ray, ref node.BoundingBox, out d))
                    {
                        Vector3 thisHit;
                        Triangle thisTri;
                        if (node.PickFirst(ref ray, out thisHit, out thisTri))
                        {
                            position = thisHit;
                            triangle = thisTri;

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
            positions = null;
            triangles = null;

            if (this.Children == null)
            {
                if (this.Triangles != null && this.Triangles.Length > 0)
                {
                    #region Per bound test

                    float d;
                    if (Collision.RayIntersectsBox(ref ray, ref this.BoundingBox, out d))
                    {
                        #region Per triangle test

                        Vector3[] pos;
                        Triangle[] tri;
                        if (Triangle.IntersectAll(ref ray, this.Triangles, facingOnly, out pos, out tri))
                        {
                            positions = pos;
                            triangles = tri;

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

                foreach (QuadTreeNode node in this.Children)
                {
                    float d;
                    if (Collision.RayIntersectsBox(ref ray, ref node.BoundingBox, out d))
                    {
                        Vector3[] thisHits;
                        Triangle[] thisTris;
                        if (node.PickAll(ref ray, out thisHits, out thisTris))
                        {
                            for (int i = 0; i < thisHits.Length; i++)
                            {
                                if (!hits.Contains(thisHits[i]))
                                {
                                    hits.Add(thisHits[i]);
                                    tris.Add(thisTris[i]);
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
                }

                return intersect;

                #endregion
            }

            return false;
        }
        /// <summary>
        /// Gets the nodes contained into the specified frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the nodes contained into the frustum</returns>
        public QuadTreeNode[] Contained(ref BoundingFrustum frustum)
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
                    var childNodes = this.Children[i].Contained(ref frustum);
                    if (childNodes.Length > 0)
                    {
                        nodes.AddRange(childNodes);
                    }
                }
            }

            return nodes.ToArray();
        }

        public QuadTreeNode[] GetNodesToDraw(ref BoundingFrustum frustum)
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
                    var childNodes = this.Children[i].GetNodesToDraw(ref frustum);
                    if (childNodes.Length > 0)
                    {
                        nodes.AddRange(childNodes);
                    }
                }
            }

            return nodes.ToArray();
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
        /// Mark the node and its chils culled
        /// </summary>
        public void CullAll()
        {
            this.Cull = true;

            if (this.Children != null && this.Children.Length > 0)
            {
                for (int i = 0; i < this.Children.Length; i++)
                {
                    this.Children[i].CullAll();
                }
            }
        }
        /// <summary>
        /// Perfomrs frustum culling in the node and its childs
        /// </summary>
        /// <param name="frustum">Frustum</param>
        public void FrustumCulling(BoundingFrustum frustum)
        {
            if (frustum.Contains(this.BoundingBox) != ContainmentType.Disjoint)
            {
                this.Cull = false;

                if (this.Children != null && this.Children.Length > 0)
                {
                    for (int i = 0; i < this.Children.Length; i++)
                    {
                        this.Children[i].FrustumCulling(frustum);
                    }
                }
                else
                {
                    foreach (var index in this.vegetationBillBoardPositions.Keys)
                    {
                        var items = this.vegetationBillBoardPositions[index];
                        if (items != null && items.Length > 0)
                        {
                            //None
                        }
                    }

                    foreach (var index in this.vegetationModelPositions.Keys)
                    {
                        var items = this.vegetationModelPositions[index];
                        if (items != null && items.Length > 0)
                        {
                            //None
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Updates node and its childs
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {

        }
        /// <summary>
        /// Draws node and its childs
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Drawing context</param>
        public void Draw(GameTime gameTime, Context context)
        {
            if (!this.Cull)
            {
                if (this.Children != null && this.Children.Length > 0)
                {
                    for (int i = 0; i < this.Children.Length; i++)
                    {
                        this.Children[i].Draw(gameTime, context);
                    }
                }
                else
                {
                    foreach (var index in this.vegetationBillBoardPositions.Keys)
                    {
                        var items = this.vegetationBillBoardPositions[index];
                        if (items != null && items.Length > 0)
                        {
                            //((Billboard)this.QuadTree.Drawers[index]).WriteData(items);

                            //this.QuadTree.Drawers[index].Draw(gameTime, context);
                        }
                    }

                    foreach (var index in this.vegetationModelPositions.Keys)
                    {
                        var items = this.vegetationModelPositions[index];
                        if (items != null && items.Length > 0)
                        {
                            var model = this.QuadTree.Drawers[index] as ModelInstanced;
                            if (model != null)
                            {
                                Vector3[] f = Array.FindAll(items, it =>
                                {
                                    float dd = Vector3.DistanceSquared(it, context.EyePosition);

                                    return dd >= 0 && dd <= (100 * 100);
                                });

                                if (f.Length > 0)
                                {
                                    model.SetPositions(f);

                                    var par = context.Frustum.GetCameraParams();
                                    par.ZFar = 100;
                                    BoundingFrustum bf = BoundingFrustum.FromCamera(par);

                                    model.FrustumCulling(bf);
                                    model.Draw(gameTime, context);
                                }
                            }
                        }
                    }
                }
            }
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
                return string.Format("QuadTreeNode; Depth {0}; Triangles {1}", this.Level, this.Triangles.Length);
            }
            else
            {
                //Node
                return string.Format("QuadTreeNode; Depth {0}; Childs {1}", this.Level, this.Children.Length);
            }
        }
    }
}
