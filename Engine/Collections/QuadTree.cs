using SharpDX;
using System.Collections.Generic;
using System.Diagnostics;

namespace Engine.Collections
{
    using Engine.Common;

    /// <summary>
    /// Quad tree
    /// </summary>
    public class QuadTree
    {
        /// <summary>
        /// Root node
        /// </summary>
        public QuadTreeNode Root { get; private set; }
        /// <summary>
        /// Global bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Partitioning items</param>
        /// <param name="maxDepth">Maximum depth</param>
        public QuadTree(BoundingBox bbox, int maxDepth)
        {
            BoundingBox = bbox;

            int nodeCount = 0;
            Root = QuadTreeNode.CreatePartitions(
                null,
                bbox,
                maxDepth,
                0,
                ref nodeCount);
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
        /// Gets all leaf nodes
        /// </summary>
        /// <returns>Returns all leaf nodel</returns>
        public IEnumerable<QuadTreeNode> GetLeafNodes()
        {
            return Root.GetLeafNodes();
        }

        /// <summary>
        /// Gets the closest node to the specified position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns the closest node to the specified position</returns>
        public QuadTreeNode FindClosestNode(Vector3 position)
        {
            var node = QuadTreeNode.GetNodeAtPosition(Root, position);
            if (node != null)
            {
                // Position is into a node
                return node;
            }

            //Look for the closest node
            return QuadTreeNode.GetClosestNodeAtPosition(Root, position);
        }
        /// <summary>
        /// Gets the nodes contained into the specified frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the nodes contained into the frustum</returns>
        public IEnumerable<QuadTreeNode> FindNodesInVolume(BoundingFrustum frustum)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return QuadTreeNode.GetNodesInVolume(Root, frustum);
            }
            finally
            {
                w.Stop();

                FrameCounters.PickCounters.AddVolumeFrustumTest((float)w.Elapsed.TotalSeconds);
            }
        }
        /// <summary>
        /// Gets the nodes contained into the specified bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the nodes contained into the bounding box</returns>
        public IEnumerable<QuadTreeNode> FindNodesInVolume(BoundingBox bbox)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return QuadTreeNode.GetNodesInVolume(Root, bbox);
            }
            finally
            {
                w.Stop();

                FrameCounters.PickCounters.AddVolumeBoxTest((float)w.Elapsed.TotalSeconds);
            }
        }
        /// <summary>
        /// Gets the nodes contained into the specified bounding sphere
        /// </summary>
        /// <param name="sphere">Bounding sphere</param>
        /// <returns>Returns the nodes contained into the bounding sphere</returns>
        public IEnumerable<QuadTreeNode> FindNodesInVolume(BoundingSphere sphere)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return QuadTreeNode.GetNodesInVolume(Root, sphere);
            }
            finally
            {
                w.Stop();

                FrameCounters.PickCounters.AddVolumeSphereTest((float)w.Elapsed.TotalSeconds);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Root != null)
            {
                return $"QuadTree Levels {Root.GetMaxLevel() + 1}";
            }
            else
            {
                return "QuadTree Empty";
            }
        }
    }
}

