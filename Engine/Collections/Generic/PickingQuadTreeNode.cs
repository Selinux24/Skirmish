using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Collections.Generic
{
    using Engine.Common;

    /// <summary>
    /// Picking quad tree node
    /// </summary>
    public abstract class PickingQuadTreeNode
    {
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
            BoundingBox bbox, IEnumerable<T> items,
            int maxDepth,
            int treeDepth)
        {
            if (treeDepth > maxDepth)
            {
                return null;
            }

            //Find triangles into the bounding box
            var nodeItems = items
                .Where(t =>
                {
                    var tbox = BoundingBox.FromPoints(t.GetVertices().ToArray());

                    return Intersection.BoxContainsBox(bbox, tbox) != ContainmentType.Disjoint;
                })
                .ToList(); //Break the reference

            if (!nodeItems.Any())
            {
                return null;
            }

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
                node.Id = quadTree.GetNextNodeId();
                node.Items = nodeItems;
            }
            else
            {
                // Initialize node partitions
                InitializeNode(quadTree, node, bbox, nodeItems, maxDepth, treeDepth + 1);
            }

            return node;
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
        private static void InitializeNode(
            PickingQuadTree<T> quadTree, PickingQuadTreeNode<T> node,
            BoundingBox bbox, IEnumerable<T> items,
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
                node.children.AddRange(childList);
                node.TopLeftChild = topLeftChild;
                node.TopRightChild = topRightChild;
                node.BottomLeftChild = bottomLeftChild;
                node.BottomRightChild = bottomRightChild;
            }
        }

        /// <summary>
        /// Children list
        /// </summary>
        private readonly List<PickingQuadTreeNode<T>> children = new List<PickingQuadTreeNode<T>>();

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
                return (BoundingBox.Maximum + BoundingBox.Minimum) * 0.5f;
            }
        }
        /// <summary>
        /// Children list
        /// </summary>
        public IEnumerable<PickingQuadTreeNode<T>> Children
        {
            get
            {
                //Copy collection
                return children.AsEnumerable();
            }
        }
        /// <summary>
        /// Node items
        /// </summary>
        internal IEnumerable<T> Items { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="quadTree">Quadtree</param>
        /// <param name="parent">Parent node</param>
        public PickingQuadTreeNode(PickingQuadTree<T> quadTree, PickingQuadTreeNode<T> parent) : base()
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

            if (!children.Any())
            {
                return;
            }

            foreach (var child in children)
            {
                child.ConnectNodes();
            }
        }
        /// <summary>
        /// Searchs for the neighbor node at top position (from above)
        /// </summary>
        /// <returns>Returns the neighbor node at top position if exists.</returns>
        private PickingQuadTreeNode<T> FindNeighborNodeAtTop()
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
        private PickingQuadTreeNode<T> FindNeighborNodeAtBottom()
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
        private PickingQuadTreeNode<T> FindNeighborNodeAtRight()
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
        private PickingQuadTreeNode<T> FindNeighborNodeAtLeft()
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
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="result">Pick result</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickNearest(PickingRay ray, out PickingResult<T> result)
        {
            if (!children.Any())
            {
                return PickNearestItem(ray, out result);
            }
            else
            {
                return PickNearestNode(ray, out result);
            }
        }
        /// <summary>
        /// Pick nearest position in the item collection
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="result">Pick result</param>
        /// <returns>Returns true if picked position found</returns>
        private bool PickNearestItem(PickingRay ray, out PickingResult<T> result)
        {
            result = new PickingResult<T>
            {
                Distance = float.MaxValue,
            };

            if (!Items.Any())
            {
                return false;
            }

            var inBox = Intersection.RayIntersectsBox(ray, BoundingBox, out _);
            if (!inBox)
            {
                return false;
            }

            return RayPickingHelper.PickNearest(Items, ray, out result);
        }
        /// <summary>
        /// Pick nearest position in the node collection
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="result">Pick result</param>
        /// <returns>Returns true if picked position found</returns>
        private bool PickNearestNode(PickingRay ray, out PickingResult<T> result)
        {
            var boxHitsByDistance = FindContacts(ray);
            if (!boxHitsByDistance.Any())
            {
                result = new PickingResult<T>
                {
                    Distance = float.MaxValue,
                };

                return false;
            }

            bool intersect = false;

            PickingResult<T> bestHit = new PickingResult<T>
            {
                Distance = float.MaxValue,
            };

            foreach (var node in boxHitsByDistance.Values)
            {
                // check that the intersection is closer than the nearest intersection found thus far
                var inItem = node.PickNearest(ray, out var thisHit);
                if (!inItem)
                {
                    continue;
                }

                if (thisHit.Distance < bestHit.Distance)
                {
                    // if we have found a closer intersection store the new closest intersection
                    bestHit = thisHit;

                    intersect = true;
                }
            }

            result = bestHit;

            return intersect;
        }
        /// <summary>
        /// Finds children contacts by distance to hit in bounding box
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <returns>Returns a sorted by distance node list</returns>
        private SortedDictionary<float, PickingQuadTreeNode<T>> FindContacts(PickingRay ray)
        {
            SortedDictionary<float, PickingQuadTreeNode<T>> boxHitsByDistance = new SortedDictionary<float, PickingQuadTreeNode<T>>();

            foreach (var node in children)
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

            return boxHitsByDistance;
        }

        /// <summary>
        /// Pick first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="result">Pick result</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickFirst(PickingRay ray, out PickingResult<T> result)
        {
            if (!children.Any())
            {
                return PickFirstItem(ray, out result);
            }
            else
            {
                return PickFirstNode(ray, out result);
            }
        }
        /// <summary>
        /// Pick first position in the item collection
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="result">Pick result</param>
        /// <returns>Returns true if picked position found</returns>
        private bool PickFirstItem(PickingRay ray, out PickingResult<T> result)
        {
            result = new PickingResult<T>
            {
                Distance = float.MaxValue,
            };

            if (!Items.Any())
            {
                return false;
            }

            var inBox = Intersection.RayIntersectsBox(ray, BoundingBox, out _);
            if (!inBox)
            {
                return false;
            }

            return RayPickingHelper.PickFirst(Items, ray, out result);
        }
        /// <summary>
        /// Pick first position in the node collection
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="result">Pick result</param>
        /// <returns>Returns true if picked position found</returns>
        private bool PickFirstNode(PickingRay ray, out PickingResult<T> result)
        {
            foreach (var node in children)
            {
                var inBox = Intersection.RayIntersectsBox(ray, node.BoundingBox, out _);
                if (!inBox)
                {
                    continue;
                }

                var inItem = node.PickFirst(ray, out var thisHit);
                if (!inItem)
                {
                    continue;
                }

                result = thisHit;

                return true;
            }

            result = new PickingResult<T>
            {
                Distance = float.MaxValue,
            };

            return false;
        }

        /// <summary>
        /// Pick all position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="results">Pick results</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickAll(PickingRay ray, out IEnumerable<PickingResult<T>> results)
        {
            if (!children.Any())
            {
                return PickAllItem(ray, out results);
            }
            else
            {
                return PickAllNode(ray, out results);
            }
        }
        /// <summary>
        /// Pick all position in the item collection
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="results">Pick results</param>
        /// <returns>Returns true if picked position found</returns>
        private bool PickAllItem(PickingRay ray, out IEnumerable<PickingResult<T>> results)
        {
            results = Enumerable.Empty<PickingResult<T>>();

            if (!Items.Any())
            {
                return false;
            }

            var inBox = Intersection.RayIntersectsBox(ray, BoundingBox, out _);
            if (!inBox)
            {
                return false;
            }

            return RayPickingHelper.PickAll(Items, ray, out results);
        }
        /// <summary>
        /// Pick all position in the node collection
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="results">Pick results</param>
        /// <returns>Returns true if picked position found</returns>
        private bool PickAllNode(PickingRay ray, out IEnumerable<PickingResult<T>> results)
        {
            bool intersect = false;

            List<PickingResult<T>> hits = new List<PickingResult<T>>();

            foreach (var node in children)
            {
                var inBox = Intersection.RayIntersectsBox(ray, node.BoundingBox, out float d);
                if (!inBox)
                {
                    continue;
                }

                var inItem = node.PickAll(ray, out var thisHits);
                if (!inItem)
                {
                    continue;
                }

                for (int i = 0; i < thisHits.Count(); i++)
                {
                    if (!hits.Contains(thisHits.ElementAt(i)))
                    {
                        hits.Add(thisHits.ElementAt(i));
                    }
                }

                intersect = true;
            }

            results = hits;

            return intersect;
        }

        /// <summary>
        /// Get bounding boxes of specified level
        /// </summary>
        /// <param name="maxDepth">Maximum depth (if zero there is no limit)</param>
        /// <returns>Returns bounding boxes of specified depth</returns>
        public IEnumerable<BoundingBox> GetBoundingBoxes(int maxDepth = 0)
        {
            List<BoundingBox> bboxes = new List<BoundingBox>();

            if (!children.Any())
            {
                bboxes.Add(BoundingBox);

                return bboxes;
            }

            bool haltByDepth = maxDepth > 0 && Level == maxDepth;
            if (haltByDepth)
            {
                return children.Select(c => c.BoundingBox);
            }
            else
            {
                return children.SelectMany(c => c.GetBoundingBoxes(maxDepth));
            }
        }
        /// <summary>
        /// Gets maximum level value
        /// </summary>
        /// <returns></returns>
        public int GetMaxLevel()
        {
            if (!children.Any())
            {
                return Level;
            }

            return children.Max(c => c.GetMaxLevel());
        }

        /// <summary>
        /// Gets the leaf nodes contained into the specified volume
        /// </summary>
        /// <param name="volume">Volume</param>
        /// <returns>Returns the leaf nodes contained into the volume</returns>
        public IEnumerable<PickingQuadTreeNode<T>> GetNodesInVolume(ICullingVolume volume)
        {
            List<PickingQuadTreeNode<T>> nodes = new List<PickingQuadTreeNode<T>>();

            if (!children.Any())
            {
                if (volume.Contains(BoundingBox) != ContainmentType.Disjoint)
                {
                    nodes.Add(this);
                }
            }
            else
            {
                foreach (var child in children)
                {
                    var childNodes = child.GetNodesInVolume(volume);
                    if (childNodes.Any())
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
        public IEnumerable<PickingQuadTreeNode<T>> GetLeafNodes()
        {
            List<PickingQuadTreeNode<T>> nodes = new List<PickingQuadTreeNode<T>>();

            if (!children.Any())
            {
                nodes.Add(this);
            }
            else
            {
                foreach (var child in children)
                {
                    var childNodes = child.GetLeafNodes();
                    if (childNodes.Any())
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
            if (!children.Any())
            {
                if (BoundingBox.Contains(position) != ContainmentType.Disjoint)
                {
                    return this;
                }
            }
            else
            {
                foreach (var child in children)
                {
                    var childNode = child.GetNode(position);
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
            if (!children.Any())
            {
                //Leaf node
                return $"{nameof(PickingQuadTreeNode<T>)} {Id}.Leaf; Depth {Level}; Items {Items.Count()}";
            }
            else
            {
                //Node
                return $"{nameof(PickingQuadTreeNode<T>)} {Id}.Node; Depth {Level}; Childs {children.Count}";
            }
        }
    }
}
