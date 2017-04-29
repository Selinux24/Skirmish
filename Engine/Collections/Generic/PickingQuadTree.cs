using SharpDX;
using System.Diagnostics;

namespace Engine.Collections.Generic
{
    using Engine.Common;

    /// <summary>
    /// Picking quad tree
    /// </summary>
    public class PickingQuadTree<T> : IRayPickable<T> where T : IVertexList, IRayIntersectable
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
        /// Global bounding sphere
        /// </summary>
        public BoundingSphere BoundingSphere { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Partitioning items</param>
        /// <param name="maxDepth">Maximum depth</param>
        public PickingQuadTree(T[] items, int maxDepth)
        {
            var bbox = GeometryUtil.CreateBoundingBox(items);
            var bsph = GeometryUtil.CreateBoundingSphere(items);

            this.BoundingBox = bbox;
            this.BoundingSphere = bsph;

            this.Root = PickingQuadTreeNode<T>.CreatePartitions(
                this, null,
                bbox, items,
                maxDepth,
                0);

            this.Root.ConnectNodes();
        }

        /// <summary>
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="position">Hit position</param>
        /// <param name="triangle">Hit triangle</param>
        /// <param name="distance">Distance to hit</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickNearest(ref Ray ray, bool facingOnly, out Vector3 position, out T triangle, out float distance)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return this.Root.PickNearest(ref ray, facingOnly, out position, out triangle, out distance);
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
        /// <param name="position">Hit position</param>
        /// <param name="triangle">Hit triangle</param>
        /// <param name="distance">Distance to hit</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickFirst(ref Ray ray, bool facingOnly, out Vector3 position, out T triangle, out float distance)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return this.Root.PickFirst(ref ray, facingOnly, out position, out triangle, out distance);
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
        /// <param name="positions">Hit positions</param>
        /// <param name="triangles">Hit triangles</param>
        /// <param name="distances">Distances to hits</param>
        /// <returns>Returns true if picked positions found</returns>
        public bool PickAll(ref Ray ray, bool facingOnly, out Vector3[] positions, out T[] triangles, out float[] distances)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return this.Root.PickAll(ref ray, facingOnly, out positions, out triangles, out distances);
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
        public BoundingBox[] GetBoundingBoxes(int maxDepth = 0)
        {
            return this.Root.GetBoundingBoxes(maxDepth);
        }
        /// <summary>
        /// Gets the nodes contained into the specified frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the nodes contained into the frustum</returns>
        public PickingQuadTreeNode<T>[] GetNodesInVolume(ref BoundingFrustum frustum)
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
        public PickingQuadTreeNode<T>[] GetNodesInVolume(ref BoundingBox bbox)
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
        public PickingQuadTreeNode<T>[] GetNodesInVolume(ref BoundingSphere sphere)
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
        public PickingQuadTreeNode<T>[] GetLeafNodes()
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
                for (int i = 0; i < leafNodes.Length; i++)
                {
                    float d = Vector3.DistanceSquared(position, leafNodes[i].Center);
                    if (d < dist)
                    {
                        dist = d;
                        node = leafNodes[i];
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
