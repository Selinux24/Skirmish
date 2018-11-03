using SharpDX;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Engine.Collections.Generic
{
    using Engine.Common;

    /// <summary>
    /// Picking quad tree node
    /// </summary>
    public abstract class PickingQuadTreeNode
    {
        /// <summary>
        /// Static node count
        /// </summary>
        protected static int NodeCount { get; set; } = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        protected PickingQuadTreeNode()
        {

        }
    }

    /// <summary>
    /// Picking quad tree node
    /// </summary>
    public class PickingQuadTreeNode<T> : PickingQuadTreeNode where T : IVertexList, IRayIntersectable
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
                //Find triangles into the bounding box
                var nodeItems = Array.FindAll(items, t =>
                {
                    var tbox = BoundingBox.FromPoints(t.GetVertices());

                    return Intersection.BoxContainsBox(ref bbox, ref tbox) != ContainmentType.Disjoint;
                });

                if (nodeItems.Length > 0)
                {
                    // Creates a new node
                    var node = new PickingQuadTreeNode<T>(quadTree, parent)
                    {
                        Id = -1,
                        Level = treeDepth,
                        BoundingBox = bbox,
                    };

                    bool haltByDepth = treeDepth == maxDepth;
                    if (haltByDepth)
                    {
                        // Maximum tree depth reached. Stop the process
                        node.Id = NodeCount++;
                        node.Items = nodeItems;
                    }
                    else
                    {
                        // Initialize node partitions
                        IntializeNode(quadTree, node, bbox, nodeItems, maxDepth, treeDepth + 1);
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
        private static void IntializeNode(
            PickingQuadTree<T> quadTree, PickingQuadTreeNode<T> node,
            BoundingBox bbox, T[] items,
            int maxDepth,
            int nextTreeDepth)
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

            var topLeftChild = CreatePartitions(quadTree, node, topLeftBox, items, maxDepth, nextTreeDepth);
            var topRightChild = CreatePartitions(quadTree, node, topRightBox, items, maxDepth, nextTreeDepth);
            var bottomLeftChild = CreatePartitions(quadTree, node, bottomLeftBox, items, maxDepth, nextTreeDepth);
            var bottomRightChild = CreatePartitions(quadTree, node, bottomRightBox, items, maxDepth, nextTreeDepth);

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

        /// <summary>
        /// Bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; set; }

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
        public int Id { get; set; }
        /// <summary>
        /// Depth level
        /// </summary>
        public int Level { get; set; }
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
        public PickingQuadTreeNode<T>[] Children { get; set; }
        /// <summary>
        /// Node items
        /// </summary>
        internal T[] Items { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="quadTree">Quadtree</param>
        /// <param name="parent">Parent node</param>
        public PickingQuadTreeNode(PickingQuadTree<T> quadTree, PickingQuadTreeNode<T> parent) : base()
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

            this.TopLeftNeighbor = this.TopNeighbor?.FindNeighborNodeAtLeft();
            this.TopRightNeighbor = this.TopNeighbor?.FindNeighborNodeAtRight();
            this.BottomLeftNeighbor = this.BottomNeighbor?.FindNeighborNodeAtLeft();
            this.BottomRightNeighbor = this.BottomNeighbor?.FindNeighborNodeAtRight();

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
        public bool PickNearest(Ray ray, out Vector3 position, out T item)
        {
            return this.PickNearest(ray, true, out position, out item);
        }
        /// <summary>
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing items</param>
        /// <param name="position">Hit position</param>
        /// <param name="item">Hit item</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickNearest(Ray ray, bool facingOnly, out Vector3 position, out T item)
        {
            return this.PickNearest(ray, facingOnly, out position, out item, out float distance);
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
        public bool PickNearest(Ray ray, bool facingOnly, out Vector3 position, out T item, out float distance)
        {
            position = Vector3.Zero;
            item = default(T);
            distance = float.MaxValue;

            if (this.Children == null)
            {
                if (this.PickNearestItem(ray, facingOnly, out var iPosition, out var iItem, out var iDistance))
                {
                    position = iPosition;
                    item = iItem;
                    distance = iDistance;

                    return true;
                }
            }
            else
            {
                if (this.PickNearestNode(ray, facingOnly, out var nPosition, out var nItem, out var nDistance))
                {
                    position = nPosition;
                    item = nItem;
                    distance = nDistance;

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Pick nearest position in the item collection
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing items</param>
        /// <param name="position">Hit position</param>
        /// <param name="item">Hit item</param>
        /// <param name="distance">Distance to hit</param>
        /// <returns>Returns true if picked position found</returns>
        private bool PickNearestItem(Ray ray, bool facingOnly, out Vector3 position, out T item, out float distance)
        {
            position = Vector3.Zero;
            item = default(T);
            distance = float.MaxValue;

            if (this.Items?.Length > 0)
            {
                var inBox = Intersection.RayIntersectsBox(ray, this.BoundingBox, out float d);
                if (inBox)
                {
                    var inItem = Intersection.IntersectNearest(ray, this.Items, facingOnly, out Vector3 pos, out T tri, out d);
                    if (inItem)
                    {
                        position = pos;
                        item = tri;
                        distance = d;

                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Pick nearest position in the node collection
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing items</param>
        /// <param name="position">Hit position</param>
        /// <param name="item">Hit item</param>
        /// <param name="distance">Distance to hit</param>
        /// <returns>Returns true if picked position found</returns>
        private bool PickNearestNode(Ray ray, bool facingOnly, out Vector3 position, out T item, out float distance)
        {
            position = Vector3.Zero;
            item = default(T);
            distance = float.MaxValue;

            SortedDictionary<float, PickingQuadTreeNode<T>> boxHitsByDistance = new SortedDictionary<float, PickingQuadTreeNode<T>>();

            #region Find children contacts by distance to hit in bounding box

            foreach (var node in this.Children)
            {
                if (Intersection.RayIntersectsBox(ray, node.BoundingBox, out float d))
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
                    // check that the intersection is closer than the nearest intersection found thus far
                    var inNode = node.PickNearest(ray, facingOnly, out Vector3 thisHit, out T thisTri, out float thisD);
                    if (inNode && thisD < bestD)
                    {
                        // if we have found a closer intersection store the new closest intersection
                        bestHit = thisHit;
                        bestTri = thisTri;
                        bestD = thisD;
                        intersect = true;
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
        public bool PickFirst(Ray ray, out Vector3 position, out T item)
        {
            return this.PickFirst(ray, true, out position, out item);
        }
        /// <summary>
        /// Pick first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing items</param>
        /// <param name="position">Hit position</param>
        /// <param name="item">Hit item</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickFirst(Ray ray, bool facingOnly, out Vector3 position, out T item)
        {
            return this.PickFirst(ray, facingOnly, out position, out item, out float distance);
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
        public bool PickFirst(Ray ray, bool facingOnly, out Vector3 position, out T item, out float distance)
        {
            position = Vector3.Zero;
            item = default(T);
            distance = float.MaxValue;

            if (this.Children == null)
            {
                if (this.PickFirstItem(ray, facingOnly, out var iPosition, out var iItem, out var iDistance))
                {
                    position = iPosition;
                    item = iItem;
                    distance = iDistance;

                    return true;
                }
            }
            else
            {
                if (this.PickFirstNode(ray, facingOnly, out var nPosition, out var nItem, out var nDistance))
                {
                    position = nPosition;
                    item = nItem;
                    distance = nDistance;

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Pick first position in the item collection
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing items</param>
        /// <param name="position">Hit position</param>
        /// <param name="item">Hit item</param>
        /// <param name="distance">Distance to hit</param>
        /// <returns>Returns true if picked position found</returns>
        private bool PickFirstItem(Ray ray, bool facingOnly, out Vector3 position, out T item, out float distance)
        {
            position = Vector3.Zero;
            item = default(T);
            distance = float.MaxValue;

            if (this.Items?.Length > 0)
            {
                var inBox = Intersection.RayIntersectsBox(ray, this.BoundingBox, out float d);
                if (inBox)
                {
                    var inItem = Intersection.IntersectFirst(ray, this.Items, facingOnly, out Vector3 pos, out T tri, out d);
                    if (inItem)
                    {
                        position = pos;
                        item = tri;
                        distance = d;

                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Pick first position in the node collection
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing items</param>
        /// <param name="position">Hit position</param>
        /// <param name="item">Hit item</param>
        /// <param name="distance">Distance to hit</param>
        /// <returns>Returns true if picked position found</returns>
        private bool PickFirstNode(Ray ray, bool facingOnly, out Vector3 position, out T item, out float distance)
        {
            position = Vector3.Zero;
            item = default(T);
            distance = float.MaxValue;

            foreach (var node in this.Children)
            {
                var inBox = Intersection.RayIntersectsBox(ray, node.BoundingBox, out float d);
                if (inBox)
                {
                    var inItem = node.PickFirst(ray, facingOnly, out Vector3 thisHit, out T thisTri, out float thisD);
                    if (inItem)
                    {
                        position = thisHit;
                        item = thisTri;
                        distance = thisD;

                        return true;
                    }
                }
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
        public bool PickAll(Ray ray, out Vector3[] positions, out T[] items)
        {
            return this.PickAll(ray, true, out positions, out items);
        }
        /// <summary>
        /// Pick all position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing items</param>
        /// <param name="positions">Hit positions</param>
        /// <param name="items">Hit items</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickAll(Ray ray, bool facingOnly, out Vector3[] positions, out T[] items)
        {
            return this.PickAll(ray, facingOnly, out positions, out items, out float[] distances);
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
        public bool PickAll(Ray ray, bool facingOnly, out Vector3[] positions, out T[] items, out float[] distances)
        {
            positions = null;
            items = null;
            distances = null;

            if (this.Children == null)
            {
                if (this.PickAllItem(ray, facingOnly, out var iPositions, out var iItems, out var iDistances))
                {
                    positions = iPositions;
                    items = iItems;
                    distances = iDistances;

                    return true;
                }
            }
            else
            {
                if (this.PickAllNode(ray, facingOnly, out var nPositions, out var nItems, out var nDistances))
                {
                    positions = nPositions;
                    items = nItems;
                    distances = nDistances;

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Pick all position in the item collection
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing items</param>
        /// <param name="positions">Hit positions</param>
        /// <param name="items">Hit items</param>
        /// <param name="distances">Distances to hits</param>
        /// <returns>Returns true if picked position found</returns>
        private bool PickAllItem(Ray ray, bool facingOnly, out Vector3[] positions, out T[] items, out float[] distances)
        {
            positions = null;
            items = null;
            distances = null;

            if (this.Items?.Length > 0)
            {
                var inBox = Intersection.RayIntersectsBox(ray, this.BoundingBox, out float d);
                if (inBox)
                {
                    var inItem = Intersection.IntersectAll(ray, this.Items, facingOnly, out Vector3[] pos, out T[] tri, out float[] ds);
                    if (inItem)
                    {
                        positions = pos;
                        items = tri;
                        distances = ds;

                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Pick all position in the node collection
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing items</param>
        /// <param name="positions">Hit positions</param>
        /// <param name="items">Hit items</param>
        /// <param name="distances">Distances to hits</param>
        /// <returns>Returns true if picked position found</returns>
        private bool PickAllNode(Ray ray, bool facingOnly, out Vector3[] positions, out T[] items, out float[] distances)
        {
            positions = null;
            items = null;
            distances = null;

            bool intersect = false;

            List<Vector3> hits = new List<Vector3>();
            List<T> tris = new List<T>();
            List<float> dists = new List<float>();

            foreach (var node in this.Children)
            {
                var inBox = Intersection.RayIntersectsBox(ray, node.BoundingBox, out float d);
                if (inBox)
                {
                    var inItem = node.PickAll(ray, facingOnly, out Vector3[] thisHits, out T[] thisTris, out float[] thisDs);
                    if (inItem)
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

            positions = hits.ToArray();
            items = tris.ToArray();
            distances = dists.ToArray();

            return intersect;
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
                bool haltByDepth = maxDepth > 0 && this.Level == maxDepth;
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
        /// Gets the leaf nodes contained into the specified volume
        /// </summary>
        /// <param name="volume">Volume</param>
        /// <returns>Returns the leaf nodes contained into the volume</returns>
        public PickingQuadTreeNode<T>[] GetNodesInVolume(ICullingVolume volume)
        {
            List<PickingQuadTreeNode<T>> nodes = new List<PickingQuadTreeNode<T>>();

            if (this.Children == null)
            {
                if (volume.Contains(this.BoundingBox) != ContainmentType.Disjoint)
                {
                    nodes.Add(this);
                }
            }
            else
            {
                for (int i = 0; i < this.Children.Length; i++)
                {
                    var childNodes = this.Children[i].GetNodesInVolume(volume);
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
