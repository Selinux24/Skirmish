using System.Diagnostics;
using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Quad tree
    /// </summary>
    public class QuadTree
    {
        /// <summary>
        /// Build quadtree
        /// </summary>
        /// <param name="triangles">Partitioning triangles</param>
        /// <param name="maxTrianglesPerNode">Maximum triangle count by node tail node (1024 by default)</param>
        /// <param name="maxDepth">Maximum depth for the quadtree (if zero there is no limit)</param>
        /// <returns>Returns generated quadtree</returns>
        public static QuadTree Build(Triangle[] triangles, int maxTrianglesPerNode = 1024, int maxDepth = 0)
        {
            BoundingBox bbox = Helper.CreateBoundingBox(triangles);

            QuadTree q = new QuadTree()
            {
                Root = QuadTreeNode.CreatePartitions(bbox, triangles, 0, maxTrianglesPerNode, maxDepth),
            };

            return q;
        }

        /// <summary>
        /// Root node
        /// </summary>
        public QuadTreeNode Root;

        /// <summary>
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="position">Hit position</param>
        /// <param name="triangle">Hit triangle</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickNearest(ref Ray ray, out Vector3 position, out Triangle triangle)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return this.Root.PickNearest(ref ray, out position, out triangle);
            }
            finally
            {
                w.Stop();

                float time = ((Counters.PicksPerFrame * Counters.PickingAverageTime) + (float)w.Elapsed.TotalSeconds);

                Counters.PicksPerFrame++;
                Counters.PickingAverageTime = time / Counters.PicksPerFrame;
            }
        }
        /// <summary>
        /// Pick first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="position">Hit position</param>
        /// <param name="triangle">Hit triangle</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickFirst(ref Ray ray, out Vector3 position, out Triangle triangle)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return this.Root.PickFirst(ref ray, out position, out triangle);
            }
            finally
            {
                w.Stop();

                float time = ((Counters.PicksPerFrame * Counters.PickingAverageTime) + (float)w.Elapsed.TotalSeconds);

                Counters.PicksPerFrame++;
                Counters.PickingAverageTime = time / Counters.PicksPerFrame;
            }
        }
        /// <summary>
        /// Pick all positions
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="positions">Hit positions</param>
        /// <param name="triangles">Hit triangles</param>
        /// <returns>Returns true if picked positions found</returns>
        public bool PickAll(ref Ray ray, out Vector3[] positions, out Triangle[] triangles)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                return this.Root.PickAll(ref ray, out positions, out triangles);
            }
            finally
            {
                w.Stop();

                float time = ((Counters.PicksPerFrame * Counters.PickingAverageTime) + (float)w.Elapsed.TotalSeconds);

                Counters.PicksPerFrame++;
                Counters.PickingAverageTime = time / Counters.PicksPerFrame;
            }
        }
        /// <summary>
        /// Gets the nodes contained into the specified frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the nodes contained into the frustum</returns>
        public QuadTreeNode[] Contained(ref BoundingFrustum frustum)
        {
            return this.Root.Contained(ref frustum);
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
