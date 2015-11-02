using Engine.Content;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        /// <param name="game">Game</param>
        /// <param name="triangles">Partitioning triangles</param>
        /// <param name="description">Description</param>
        /// <returns>Returns generated quadtree</returns>
        public static QuadTree Build(
            Game game,
            Triangle[] triangles,
            TerrainDescription description)
        {
            BoundingBox bbox = Helper.CreateBoundingBox(triangles);
            BoundingSphere bsph = Helper.CreateBoundingSphere(triangles);

            QuadTree quadTree = new QuadTree()
            {
                BoundingBox = bbox,
                BoundingSphere = bsph,
            };

            quadTree.Root = QuadTreeNode.CreatePartitions(
                game,
                quadTree,
                null,
                bbox,
                triangles,
                0,
                description);

            quadTree.Root.ConnectNodes();

            return quadTree;
        }
        /// <summary>
        /// Build quadtree
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="description">Description</param>
        /// <returns>Returns generated quadtree</returns>
        public static QuadTree Build(
            Game game,
            VertexData[] vertices,
            TerrainDescription description)
        {
            //Process Quadtree. Each tail quadnode must contains the same number of vertices

            //Compute bounding volumes
            long index = 0;
            Vector3[] positions = new Vector3[vertices.Length];
            Array.ForEach(vertices, v => positions[index++] = v.Position.Value);

            BoundingBox bbox = BoundingBox.FromPoints(positions);
            BoundingSphere bsph = BoundingSphere.FromPoints(positions);

            QuadTree quadTree = new QuadTree()
            {
                BoundingBox = bbox,
                BoundingSphere = bsph,
            };

            quadTree.Root = QuadTreeNode.CreatePartitions(
                game,
                quadTree, null,
                bbox, vertices,
                0,
                description);

            quadTree.Root.ConnectNodes();

            return quadTree;
        }

        /// <summary>
        /// Root node
        /// </summary>
        public QuadTreeNode Root { get; private set; }
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
        public QuadTree()
        {
            
        }

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
        public QuadTreeNode[] GetNodesInVolume(ref BoundingFrustum frustum)
        {
            return this.Root.GetNodesInVolume(ref frustum);
        }
        /// <summary>
        /// Gets all tail nodes
        /// </summary>
        /// <returns>Returns all tais nodel</returns>
        public QuadTreeNode[] GetTailNodes()
        {
            return this.Root.GetTailNodes();
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
