using SharpDX;
using System.Collections.Generic;
using System.Diagnostics;

namespace Engine.Collections
{
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
            this.BoundingBox = bbox;

            int nodeCount = 0;
            this.Root = QuadTreeNode.CreatePartitions(
                this, null,
                bbox,
                maxDepth,
                0,
                ref nodeCount);

            this.Root.ConnectNodes();
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
        /// Gets the nodes contained into the specified frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the nodes contained into the frustum</returns>
        public IEnumerable<QuadTreeNode> GetNodesInVolume(ref BoundingFrustum frustum)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return this.Root.GetNodesInVolume(ref frustum);
            }
            finally
            {
                w.Stop();

                Counters.AddVolumeFrustumTest((float)w.Elapsed.TotalSeconds);
            }
        }
        /// <summary>
        /// Gets the nodes contained into the specified bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the nodes contained into the bounding box</returns>
        public IEnumerable<QuadTreeNode> GetNodesInVolume(ref BoundingBox bbox)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return this.Root.GetNodesInVolume(ref bbox);
            }
            finally
            {
                w.Stop();

                Counters.AddVolumeBoxTest((float)w.Elapsed.TotalSeconds);
            }
        }
        /// <summary>
        /// Gets the nodes contained into the specified bounding sphere
        /// </summary>
        /// <param name="sphere">Bounding sphere</param>
        /// <returns>Returns the nodes contained into the bounding sphere</returns>
        public IEnumerable<QuadTreeNode> GetNodesInVolume(ref BoundingSphere sphere)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return this.Root.GetNodesInVolume(ref sphere);
            }
            finally
            {
                w.Stop();

                Counters.AddVolumeSphereTest((float)w.Elapsed.TotalSeconds);
            }
        }
        /// <summary>
        /// Gets all leaf nodes
        /// </summary>
        /// <returns>Returns all leaf nodel</returns>
        public IEnumerable<QuadTreeNode> GetLeafNodes()
        {
            return this.Root.GetLeafNodes();
        }
        /// <summary>
        /// Gets the closest node to the specified position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns the closest node to the specified position</returns>
        public QuadTreeNode FindNode(Vector3 position)
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
                return string.Format("QuadTree Levels {0}", this.Root.GetMaxLevel() + 1);
            }
            else
            {
                return "QuadTree Empty";
            }
        }
    }
}

