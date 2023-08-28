using SharpDX;
using System.Collections.Generic;
using System.Diagnostics;

namespace Engine.Collections.Generic
{
    using Engine.Common;

    /// <summary>
    /// Quad-tree
    /// </summary>
    public class QuadTree<T>
    {
        /// <summary>
        /// Root node
        /// </summary>
        public QuadTreeNode<T> Root { get; private set; }
        /// <summary>
        /// Global bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Partitioning items</param>
        /// <param name="maxDepth">Maximum depth</param>
        public QuadTree(BoundingBox bbox, IEnumerable<(BoundingBox Box, T Item)> items, int maxDepth)
        {
            BoundingBox = bbox;

            int nodeCount = 0;
            Root = QuadTreeNode<T>.CreatePartitions(
                this, null,
                bbox, items,
                maxDepth,
                0,
                ref nodeCount);

            Root.ConnectNodes();
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
        /// Gets the nodes contained into the specified frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the nodes contained into the frustum</returns>
        public IEnumerable<QuadTreeNode<T>> GetNodesInVolume(ref BoundingFrustum frustum)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return Root.GetNodesInVolume(ref frustum);
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
        public IEnumerable<QuadTreeNode<T>> GetNodesInVolume(ref BoundingBox bbox)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return Root.GetNodesInVolume(ref bbox);
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
        public IEnumerable<QuadTreeNode<T>> GetNodesInVolume(ref BoundingSphere sphere)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return Root.GetNodesInVolume(ref sphere);
            }
            finally
            {
                w.Stop();

                FrameCounters.PickCounters.AddVolumeSphereTest((float)w.Elapsed.TotalSeconds);
            }
        }
        /// <summary>
        /// Gets all leaf nodes
        /// </summary>
        /// <returns>Returns all leaf node</returns>
        public IEnumerable<QuadTreeNode<T>> GetLeafNodes()
        {
            return Root.GetLeafNodes();
        }
        /// <summary>
        /// Gets the closest node to the specified position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns the closest node to the specified position</returns>
        public QuadTreeNode<T> FindNode(Vector3 position)
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
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            if (Root != null)
            {
                return string.Format("QuadTree Levels {0}", Root.GetMaxLevel() + 1);
            }
            else
            {
                return "QuadTree Empty";
            }
        }
    }
}
