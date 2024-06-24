using Engine.Common;
using SharpDX;
using System.Collections.Generic;
using System.Diagnostics;

namespace Engine.Collections.Generic
{
    using Engine.Collections.Helpers;

    /// <summary>
    /// Picking quad tree
    /// </summary>
    public class PickingQuadTree<T> : IQuadTree<PickingQuadTreeNode<T>> where T : IVertexList, IRayIntersectable
    {
        const string cName = nameof(PickingQuadTree<T>);

        /// <inheritdoc/>
        public PickingQuadTreeNode<T> Root { get; private set; }
        /// <inheritdoc/>
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

            int nodeCount = 0;
            Root = PickingQuadTreeNode<T>.CreatePartitions(
                null,
                bbox, items,
                description.MaximumDepth,
                0,
                ref nodeCount);
        }

        /// <inheritdoc/>
        public IEnumerable<BoundingBox> GetBoundingBoxes(int maxDepth = 0)
        {
            return QuadTreeNodeHelper<PickingQuadTreeNode<T>>.GetBoundingBoxes(Root, maxDepth);
        }
        /// <inheritdoc/>
        public IEnumerable<PickingQuadTreeNode<T>> GetLeafNodes()
        {
            return QuadTreeNodeHelper<PickingQuadTreeNode<T>>.GetLeafNodes(Root);
        }

        /// <inheritdoc/>
        public PickingQuadTreeNode<T> FindClosestNode(Vector3 position)
        {
            var node = QuadTreeNodeHelper<PickingQuadTreeNode<T>>.GetNodeAtPosition(Root, position);
            if (node != null)
            {
                // Position is into a node
                return node;
            }

            //Look for the closest node
            return QuadTreeNodeHelper<PickingQuadTreeNode<T>>.GetClosestNodeAtPosition(Root, position);
        }
        /// <inheritdoc/>
        public IEnumerable<PickingQuadTreeNode<T>> FindNodesInVolume(ICullingVolume volume)
        {
            return QuadTreeNodeHelper<PickingQuadTreeNode<T>>.GetNodesInVolume(Root, volume);
        }

        /// <summary>
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickNearest(PickingRay ray, out PickingResult<T> result)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return Root.PickNearest(ray, out result);
            }
            finally
            {
                w.Stop();

                FrameCounters.PickCounters.AddPick((float)w.Elapsed.TotalSeconds);
            }
        }
        /// <summary>
        /// Pick first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickFirst(PickingRay ray, out PickingResult<T> result)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return Root.PickFirst(ray, out result);
            }
            finally
            {
                w.Stop();

                FrameCounters.PickCounters.AddPick((float)w.Elapsed.TotalSeconds);
            }
        }
        /// <summary>
        /// Pick all positions
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if picked positions found</returns>
        public bool PickAll(PickingRay ray, out IEnumerable<PickingResult<T>> results)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return Root.PickAll(ray, out results);
            }
            finally
            {
                w.Stop();

                FrameCounters.PickCounters.AddPick((float)w.Elapsed.TotalSeconds);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Root != null)
            {
                return $"{cName} Levels {QuadTreeNodeHelper<PickingQuadTreeNode<T>>.GetMaxLevel(Root) + 1}";
            }
            else
            {
                return $"{cName} Empty";
            }
        }
    }
}
