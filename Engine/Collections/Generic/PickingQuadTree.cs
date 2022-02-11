using SharpDX;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Engine.Collections.Generic
{
    using Engine.Common;

    /// <summary>
    /// Picking quad tree
    /// </summary>
    public class PickingQuadTree<T> where T : IVertexList, IRayIntersectable
    {
        /// <summary>
        /// Node id
        /// </summary>
        private int nodeId = 0;

        /// <summary>
        /// Root node
        /// </summary>
        public PickingQuadTreeNode<T> Root { get; private set; }
        /// <summary>
        /// Global bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Partitioning items</param>
        /// <param name="description">Quadtree description</param>
        public PickingQuadTree(IEnumerable<T> items, QuadtreeDescription description)
        {
            var bbox = GeometryUtil.CreateBoundingBox(items);

            BoundingBox = bbox;

            Root = PickingQuadTreeNode<T>.CreatePartitions(
                this, null,
                bbox, items,
                description.MaximumDepth,
                0);

            Root.ConnectNodes();
        }

        /// <summary>
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickNearest(Ray ray, bool facingOnly, out PickingResult<T> result)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                result = new PickingResult<T>()
                {
                    Distance = float.MaxValue,
                };

                if (Root.PickNearest(ray, facingOnly, out Vector3 position, out T item, out float distance))
                {
                    result.Position = position;
                    result.Primitive = item;
                    result.Distance = distance;

                    return true;
                }

                return false;
            }
            finally
            {
                w.Stop();

                Counters.AddPick((float)w.Elapsed.TotalSeconds);
            }
        }
        /// <summary>
        /// Pick first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickFirst(Ray ray, bool facingOnly, out PickingResult<T> result)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                result = new PickingResult<T>()
                {
                    Distance = float.MaxValue,
                };

                if (Root.PickFirst(ray, facingOnly, out Vector3 position, out T item, out float distance))
                {
                    result.Position = position;
                    result.Primitive = item;
                    result.Distance = distance;

                    return true;
                }

                return false;
            }
            finally
            {
                w.Stop();

                Counters.AddPick((float)w.Elapsed.TotalSeconds);
            }
        }
        /// <summary>
        /// Pick all positions
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if picked positions found</returns>
        public bool PickAll(Ray ray, bool facingOnly, out IEnumerable<PickingResult<T>> results)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                if (Root.PickAll(ray, facingOnly, out var positions, out var items, out var distances))
                {
                    var res = new PickingResult<T>[positions.Count()];

                    for (int i = 0; i < res.Length; i++)
                    {
                        res[i] = new PickingResult<T>()
                        {
                            Position = positions.ElementAt(i),
                            Primitive = items.ElementAt(i),
                            Distance = distances.ElementAt(i),
                        };
                    }

                    results = res;

                    return true;
                }

                results = Enumerable.Empty<PickingResult<T>>();

                return false;
            }
            finally
            {
                w.Stop();

                Counters.AddPick((float)w.Elapsed.TotalSeconds);
            }
        }
        /// <summary>
        /// Gets bounding boxes of specified depth
        /// </summary>
        /// <param name="maxDepth">Maximum depth (if zero there is no limit)</param>
        /// <returns>Returns bounding boxes of specified depth</returns>
        public IEnumerable<BoundingBox> GetBoundingBoxes(int maxDepth = 0)
        {
            return Root.GetBoundingBoxes(maxDepth);
        }
        /// <summary>
        /// Gets the nodes contained into the specified volume
        /// </summary>
        /// <param name="volume">Volume</param>
        /// <returns>Returns the nodes contained into the volume</returns>
        public IEnumerable<PickingQuadTreeNode<T>> GetNodesInVolume(IIntersectionVolume volume)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return Root.GetNodesInVolume(volume);
            }
            finally
            {
                w.Stop();

                Counters.AddVolumeFrustumTest((float)w.Elapsed.TotalSeconds);
            }
        }
        /// <summary>
        /// Gets all leaf nodes
        /// </summary>
        /// <returns>Returns all leaf nodel</returns>
        public IEnumerable<PickingQuadTreeNode<T>> GetLeafNodes()
        {
            return Root.GetLeafNodes();
        }
        /// <summary>
        /// Gets the closest node to the specified position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns the closest node to the specified position</returns>
        public PickingQuadTreeNode<T> FindNode(Vector3 position)
        {
            var node = Root.GetNode(position);

            if (node == null)
            {
                //Look for the closest node
                var leafNodes = GetLeafNodes();

                float dist = float.MaxValue;
                foreach (var leafNode in leafNodes)
                {
                    float d = Vector3.DistanceSquared(position, leafNode.Center);
                    if (d < dist)
                    {
                        dist = d;
                        node = leafNode;
                    }
                }
            }

            return node;
        }

        /// <summary>
        /// Gets the node id
        /// </summary>
        /// <returns>Returns the next node id</returns>
        public int GetNextNodeId()
        {
            return nodeId++;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Root != null)
            {
                return $"{nameof(PickingQuadTree<T>)} Levels {Root.GetMaxLevel() + 1}";
            }
            else
            {
                return $"{nameof(PickingQuadTree<T>)} Empty";
            }
        }
    }
}
