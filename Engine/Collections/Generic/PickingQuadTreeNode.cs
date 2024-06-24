using Engine.Common;
using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Collections.Generic
{
    using Engine.Collections.Helpers;

    /// <summary>
    /// Picking quad tree node
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="quadTree">Quadtree</param>
    /// <param name="parent">Parent node</param>
    public class PickingQuadTreeNode<T>(PickingQuadTreeNode<T> parent) : IQuadTreeNode<PickingQuadTreeNode<T>> where T : IVertexList, IRayIntersectable
    {
        const string cName = nameof(PickingQuadTreeNode<T>);

        /// <summary>
        /// Recursive partition creation
        /// </summary>
        /// <param name="parent">Parent node</param>
        /// <param name="bbox">Parent bounding box</param>
        /// <param name="items">All items</param>
        /// <param name="maxDepth">Maximum depth</param>
        /// <param name="treeDepth">Current depth</param>
        /// <param name="nodeCount">Node count</param>
        /// <returns>Returns new node</returns>
        public static PickingQuadTreeNode<T> CreatePartitions(
            PickingQuadTreeNode<T> parent,
            BoundingBox bbox, IEnumerable<T> items,
            int maxDepth,
            int treeDepth,
            ref int nodeCount)
        {
            if (treeDepth > maxDepth)
            {
                return null;
            }

            // Creates a new node
            PickingQuadTreeNode<T> node = new(parent)
            {
                Id = -1,
                Level = treeDepth,
                BoundingBox = bbox,
            };

            //Find triangles into the bounding box
            var nodeItems = items
                .Where(t =>
                {
                    var tbox = SharpDXExtensions.BoundingBoxFromPoints(t.GetVertices());

                    return Intersection.BoxContainsBox(bbox, tbox) != ContainmentType.Disjoint;
                })
                .ToList(); //Break the reference

            bool haltByDepth = treeDepth == maxDepth;
            if (haltByDepth)
            {
                // Maximum tree depth reached. Stop the process
                node.Id = nodeCount++;
                node.items.AddRange(nodeItems);
            }
            else
            {
                // Initialize node partitions
                InitializeNode(node, bbox, nodeItems, maxDepth, treeDepth + 1, ref nodeCount);
            }

            if (parent == null)
            {
                ConnectNodes(node);
            }

            return node;
        }
        /// <summary>
        /// Initializes node partitinos
        /// </summary>
        /// <param name="node">Current node</param>
        /// <param name="bbox">Bounding box</param>
        /// <param name="items">Items into the node</param>
        /// <param name="maxDepth">Maximum depth</param>
        /// <param name="nextTreeDepth">Next depth</param>
        /// <param name="nodeCount">Node count</param>
        private static void InitializeNode(
            PickingQuadTreeNode<T> node,
            BoundingBox bbox, IEnumerable<T> items,
            int maxDepth,
            int nextTreeDepth,
            ref int nodeCount)
        {
            BoundingBox[] boxes = [.. bbox.SubdivideQuadtree()];

            var topLeftChild = CreatePartitions(node, boxes[0], items, maxDepth, nextTreeDepth, ref nodeCount);
            var topRightChild = CreatePartitions(node, boxes[1], items, maxDepth, nextTreeDepth, ref nodeCount);
            var bottomLeftChild = CreatePartitions(node, boxes[2], items, maxDepth, nextTreeDepth, ref nodeCount);
            var bottomRightChild = CreatePartitions(node, boxes[3], items, maxDepth, nextTreeDepth, ref nodeCount);

            List<PickingQuadTreeNode<T>> childList = [];

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
        /// Connect nodes in the grid
        /// </summary>
        /// <param name="node">Parent node</param>
        private static void ConnectNodes(PickingQuadTreeNode<T> node)
        {
            if (node == null)
            {
                return;
            }

            node.TopNeighbor = QuadTreeNodeHelper<PickingQuadTreeNode<T>>.FindNeighborNodeAtTop(node);
            node.BottomNeighbor = QuadTreeNodeHelper<PickingQuadTreeNode<T>>.FindNeighborNodeAtBottom(node);
            node.LeftNeighbor = QuadTreeNodeHelper<PickingQuadTreeNode<T>>.FindNeighborNodeAtLeft(node);
            node.RightNeighbor = QuadTreeNodeHelper<PickingQuadTreeNode<T>>.FindNeighborNodeAtRight(node);

            node.TopLeftNeighbor = QuadTreeNodeHelper<PickingQuadTreeNode<T>>.FindNeighborNodeAtLeft(node.TopNeighbor);
            node.TopRightNeighbor = QuadTreeNodeHelper<PickingQuadTreeNode<T>>.FindNeighborNodeAtRight(node.TopNeighbor);
            node.BottomLeftNeighbor = QuadTreeNodeHelper<PickingQuadTreeNode<T>>.FindNeighborNodeAtLeft(node.BottomNeighbor);
            node.BottomRightNeighbor = QuadTreeNodeHelper<PickingQuadTreeNode<T>>.FindNeighborNodeAtRight(node.BottomNeighbor);

            if (node.IsLeaf)
            {
                return;
            }

            for (int i = 0; i < node.children.Count; i++)
            {
                ConnectNodes(node.children[i]);
            }
        }

        /// <summary>
        /// Children list
        /// </summary>
        private readonly List<PickingQuadTreeNode<T>> children = [];
        /// <summary>
        /// Items list
        /// </summary>
        private readonly List<T> items = [];

        /// <inheritdoc/>
        public PickingQuadTreeNode<T> Parent { get; private set; } = parent;
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> TopLeftChild { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> TopRightChild { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> BottomLeftChild { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> BottomRightChild { get; private set; }

        /// <inheritdoc/>
        public PickingQuadTreeNode<T> TopNeighbor { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> BottomNeighbor { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> LeftNeighbor { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> RightNeighbor { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> TopLeftNeighbor { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> TopRightNeighbor { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> BottomLeftNeighbor { get; private set; }
        /// <inheritdoc/>
        public PickingQuadTreeNode<T> BottomRightNeighbor { get; private set; }

        /// <inheritdoc/>
        public int Id { get; private set; }
        /// <inheritdoc/>
        public int Level { get; private set; }
        /// <inheritdoc/>
        public BoundingBox BoundingBox { get; private set; }
        /// <inheritdoc/>
        public Vector3 Center { get => BoundingBox.Center; }
        /// <inheritdoc/>
        public IEnumerable<PickingQuadTreeNode<T>> Children { get => children.AsReadOnly(); }
        /// <inheritdoc/>
        public bool IsLeaf { get => children.Count == 0; }
        /// <summary>
        /// Node item collection
        /// </summary>
        public IEnumerable<T> Items { get => items.AsReadOnly(); }

        /// <summary>
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="result">Pick result</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickNearest(PickingRay ray, out PickingResult<T> result)
        {
            if (children.Count == 0)
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

            if (items.Count == 0)
            {
                return false;
            }

            var inBox = Intersection.RayIntersectsBox(ray, BoundingBox, out _);
            if (!inBox)
            {
                return false;
            }

            return RayPickingHelper.PickNearestFromList(items, ray, out result);
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
            if (boxHitsByDistance.Count == 0)
            {
                result = new PickingResult<T>
                {
                    Distance = float.MaxValue,
                };

                return false;
            }

            bool intersect = false;

            PickingResult<T> bestHit = new()
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
            SortedDictionary<float, PickingQuadTreeNode<T>> boxHitsByDistance = [];

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
            if (children.Count == 0)
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

            if (items.Count == 0)
            {
                return false;
            }

            var inBox = Intersection.RayIntersectsBox(ray, BoundingBox, out _);
            if (!inBox)
            {
                return false;
            }

            return RayPickingHelper.PickFirstFromList(items, ray, out result);
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
            if (children.Count == 0)
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
            results = [];

            if (items.Count == 0)
            {
                return false;
            }

            var inBox = Intersection.RayIntersectsBox(ray, BoundingBox, out _);
            if (!inBox)
            {
                return false;
            }

            return RayPickingHelper.PickAllFromlist(items, ray, out results);
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

            List<PickingResult<T>> hits = [];

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

        /// <inheritdoc/>
        public override string ToString()
        {
            if (IsLeaf)
            {
                //Leaf node
                return $"{cName} {Id}; Depth {Level}; Items {items.Count}";
            }
            else
            {
                //Node
                return $"{cName} {Id}; Depth {Level}; Children {children.Count}";
            }
        }
    }
}
