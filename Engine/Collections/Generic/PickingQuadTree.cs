using SharpDX;
using System.Collections.Generic;
using System.Diagnostics;

namespace Engine.Collections.Generic
{
    using Engine.Common;

    /// <summary>
    /// Picking quad tree
    /// </summary>
    public class PickingQuadTree<T> where T : IVertexList, IRayIntersectable
    {
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

            this.BoundingBox = bbox;

            this.Root = PickingQuadTreeNode<T>.CreatePartitions(
                this, null,
                bbox, items,
                description.MaximumDepth,
                0);

            this.Root.ConnectNodes();
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

                if (this.Root.PickNearest(ray, facingOnly, out Vector3 position, out T item, out float distance))
                {
                    result.Position = position;
                    result.Item = item;
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

                if (this.Root.PickFirst(ray, facingOnly, out Vector3 position, out T item, out float distance))
                {
                    result.Position = position;
                    result.Item = item;
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
        public bool PickAll(Ray ray, bool facingOnly, out PickingResult<T>[] results)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                results = null;

                if (this.Root.PickAll(ray, facingOnly, out Vector3[] positions, out T[] items, out float[] distances))
                {
                    results = new PickingResult<T>[positions.Length];

                    for (int i = 0; i < results.Length; i++)
                    {
                        results[i] = new PickingResult<T>()
                        {
                            Position = positions[i],
                            Item = items[i],
                            Distance = distances[i],
                        };
                    }

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
        /// Gets bounding boxes of specified depth
        /// </summary>
        /// <param name="maxDepth">Maximum depth (if zero there is no limit)</param>
        /// <returns>Returns bounding boxes of specified depth</returns>
        public IEnumerable<BoundingBox> GetBoundingBoxes(int maxDepth = 0)
        {
            return this.Root.GetBoundingBoxes(maxDepth);
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
                return this.Root.GetNodesInVolume(volume);
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
            return this.Root.GetLeafNodes();
        }
        /// <summary>
        /// Gets the closest node to the specified position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns the closest node to the specified position</returns>
        public PickingQuadTreeNode<T> FindNode(Vector3 position)
        {
            var node = this.Root.GetNode(position);

            if (node == null)
            {
                //Look for the closest node
                var leafNodes = this.GetLeafNodes();

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
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            if (this.Root != null)
            {
                return string.Format("PickingQuadTree Levels {0}", this.Root.GetMaxLevel() + 1);
            }
            else
            {
                return "PickingQuadTree Empty";
            }
        }
    }
}
