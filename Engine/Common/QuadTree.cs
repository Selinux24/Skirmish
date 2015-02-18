using System;
using System.Collections.Generic;
using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Quad tree
    /// </summary>
    public class QuadTree
    {
        /// <summary>
        /// Maximum number of triangles per node (aprox)
        /// </summary>
        public const int TrianglesPerNode = 64;

        /// <summary>
        /// Build quadtree
        /// </summary>
        /// <param name="triangles">Partitioning triangles</param>
        /// <returns>Returns generated quadtree</returns>
        /// <remarks>Used partitioning depth was caculated from number of total triangles and TrianglesPerNode constant</remarks>
        public static QuadTree Build(Triangle[] triangles)
        {
            int depth = 0;
            int triCount = triangles.Length;
            while (triCount > 256)
            {
                depth++;
                triCount = (int)Math.Sqrt(triCount);
            }

            return Build(triangles, depth);
        }
        /// <summary>
        /// Build quadtree
        /// </summary>
        /// <param name="triangles">Partitioning triangles</param>
        /// <param name="depth">Speciefied partition depth</param>
        /// <returns>Returns generated quadtree</returns>
        public static QuadTree Build(Triangle[] triangles, int depth)
        {
            BoundingBox bbox = new BoundingBox();

            for (int i = 0; i < triangles.Length; i++)
            {
                BoundingBox tbox = BoundingBox.FromPoints(triangles[i].GetCorners());

                if (i == 0)
                {
                    bbox = tbox;
                }
                else
                {
                    bbox = BoundingBox.Merge(bbox, tbox);
                }
            }

            QuadTree q = new QuadTree()
            {
                Depth = depth,
                Root = CreatePartitions(bbox, 0, depth, triangles),
            };

            return q;
        }
        /// <summary>
        /// Recursive partition creation
        /// </summary>
        /// <param name="bbox">Parent bounding box</param>
        /// <param name="depth">Current depth</param>
        /// <param name="maxDepth">Maximum quadtree depth</param>
        /// <param name="triangles">All triangles</param>
        /// <returns></returns>
        private static QuadTreeNode CreatePartitions(BoundingBox bbox, int depth, int maxDepth, Triangle[] triangles)
        {
            QuadTreeNode node = new QuadTreeNode()
            {
                BoundingBox = bbox,
            };

            if (depth < maxDepth)
            {
                Vector3 M = bbox.Maximum;
                Vector3 c = (bbox.Maximum + bbox.Minimum) * 0.5f;
                Vector3 m = bbox.Minimum;

                //-1-1-1   +0+1+0   -->   mmm    cMc
                BoundingBox half0 = new BoundingBox(new Vector3(m.X, m.Y, m.Z), new Vector3(c.X, M.Y, c.Z));
                //+0-1+0   +1+1+1   -->   cmc    MMM
                BoundingBox half1 = new BoundingBox(new Vector3(c.X, m.Y, c.Z), new Vector3(M.X, M.Y, M.Z));
                //-1-1+0   +0+1+1   -->   mmc    cMM
                BoundingBox half2 = new BoundingBox(new Vector3(m.X, m.Y, c.Z), new Vector3(c.X, M.Y, M.Z));
                //+0-1-1   +1+1+0   -->   cmm    MMc
                BoundingBox half3 = new BoundingBox(new Vector3(c.X, m.Y, m.Z), new Vector3(M.X, M.Y, c.Z));

                node.Children = new QuadTreeNode[4];

                node.Children[0] = CreatePartitions(half0, depth + 1, maxDepth, triangles);
                node.Children[1] = CreatePartitions(half1, depth + 1, maxDepth, triangles);
                node.Children[2] = CreatePartitions(half2, depth + 1, maxDepth, triangles);
                node.Children[3] = CreatePartitions(half3, depth + 1, maxDepth, triangles);
            }
            else if (depth == maxDepth)
            {
                node.Triangles = Array.FindAll(triangles, t =>
                {
                    return
                        Collision.BoxContainsPoint(ref bbox, ref t.Point1) != ContainmentType.Disjoint ||
                        Collision.BoxContainsPoint(ref bbox, ref t.Point2) != ContainmentType.Disjoint ||
                        Collision.BoxContainsPoint(ref bbox, ref t.Point3) != ContainmentType.Disjoint;
                });
            }

            return node;
        }
        /// <summary>
        /// Get bounding boxes of specified level
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="depth">Current depth</param>
        /// <param name="maxDepth">Specified depth</param>
        /// <returns>Returns bounding boxes of specified depth</returns>
        private static BoundingBox[] GetBoundingBoxes(QuadTreeNode node, int depth, int maxDepth)
        {
            List<BoundingBox> bboxes = new List<BoundingBox>();

            if (node.Children != null)
            {
                if (depth == maxDepth)
                {
                    Array.ForEach(node.Children, (c) =>
                    {
                        bboxes.Add(c.BoundingBox);
                    });
                }
                else
                {
                    Array.ForEach(node.Children, (c) =>
                    {
                        bboxes.AddRange(GetBoundingBoxes(c, depth + 1, maxDepth));
                    });
                }
            }
            else
            {
                bboxes.Add(node.BoundingBox);
            }

            return bboxes.ToArray();
        }

        /// <summary>
        /// Partition depth
        /// </summary>
        public int Depth;
        /// <summary>
        /// Root node
        /// </summary>
        public QuadTreeNode Root;

        /// <summary>
        /// Pick position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="position">Hit position</param>
        /// <param name="triangle">Hit triangle</param>
        /// <returns>Returns true if picked position found</returns>
        public bool Pick(ref Ray ray, out Vector3 position, out Triangle triangle)
        {
            return this.Root.Pick(ref ray, out position, out triangle);
        }
        /// <summary>
        /// Gets bounding boxes of specified depth
        /// </summary>
        /// <param name="depth">Depth</param>
        /// <returns>Returns bounding boxes of specified depth</returns>
        public BoundingBox[] GetBoundingBoxes(int depth)
        {
            return GetBoundingBoxes(this.Root, 0, depth > this.Depth ? this.Depth : depth);
        }
    }
}
