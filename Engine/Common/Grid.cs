using System.Collections.Generic;
using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Grid
    /// </summary>
    public class Grid
    {
        /// <summary>
        /// Node side
        /// </summary>
        private float NodeSide = 0;
        /// <summary>
        /// Grid nodes
        /// </summary>
        public GridNode[] Nodes;

        /// <summary>
        /// Build node list from triangles
        /// </summary>
        /// <param name="triangles">Triangles</param>
        /// <param name="size">Node size</param>
        /// <returns>Returns generated grid node list</returns>
        public static Grid Build(ref BoundingBox bbox, Triangle[] triangles, float size)
        {
            List<GridNode> result = new List<GridNode>();

            float half = size * 0.5f;

            Vector3 dP0 = new Vector3(-half, bbox.Maximum.Y, -half);
            Vector3 dP1 = new Vector3(+half, bbox.Maximum.Y, -half);
            Vector3 dP2 = new Vector3(-half, bbox.Maximum.Y, +half);
            Vector3 dP3 = new Vector3(+half, bbox.Maximum.Y, +half);

            for (float x = bbox.Minimum.X + 1; x < bbox.Maximum.X - 1; x += size)
            {
                for (float z = bbox.Minimum.Z + 1; z < bbox.Maximum.Z - 1; z += size)
                {
                    Ray ray = new Ray(new Vector3(x, bbox.Maximum.Y + 1f, z), Vector3.Down);
                    Vector3[] points;
                    if (TestPoint(ref ray, triangles, out points))
                    {
                        for (int i = 0; i < points.Length; i++)
                        {
                            Vector3 point = points[i];

                            //Each point is a node
                            Ray ray0 = new Ray(point + dP0, Vector3.Down);
                            Vector3[] p0;
                            if (!TestPoint(ref ray0, triangles, out p0)) continue;

                            Ray ray1 = new Ray(point + dP1, Vector3.Down);
                            Vector3[] p1;
                            if (!TestPoint(ref ray1, triangles, out p1)) continue;

                            Ray ray2 = new Ray(point + dP2, Vector3.Down);
                            Vector3[] p2;
                            if (!TestPoint(ref ray2, triangles, out p2)) continue;

                            Ray ray3 = new Ray(point + dP3, Vector3.Down);
                            Vector3[] p3;
                            if (!TestPoint(ref ray3, triangles, out p3)) continue;

                            if (p0.Length == p1.Length &&
                                p0.Length == p2.Length &&
                                p0.Length == p3.Length)
                            {
                                for (int n = 0; n < p0.Length; n++)
                                {
                                    GridNode newNode = new GridNode(p0[n], p1[n], p2[n], p3[n]);

                                    result.Add(newNode);
                                }
                            }
                            else
                            {
                                //TODO: Process shared nodes. May exists?
                            }
                        }
                    }
                }
            }

            //Fill connections
            for (int i = 0; i < result.Count; i++)
            {
                for (int n = i + 1; n < result.Count; n++)
                {
                    result[i].TryConnect(result[n]);
                }
            }

            return new Grid()
            {
                NodeSide = size,
                Nodes = result.ToArray(),
            };
        }
        /// <summary>
        /// Performs validation test for specified point
        /// </summary>
        /// <param name="triangles">Triangle list</param>
        /// <param name="ray">Ray</param>
        /// <param name="p">Result point list</param>
        /// <returns>Returns true if point is valid.</returns>
        private static bool TestPoint(ref Ray ray, Triangle[] triangles, out Vector3[] p)
        {
            p = null;

            List<Vector3> points = new List<Vector3>();

            Vector3[] pickedPositions;
            Triangle[] pickedTriangles;
            if (Triangle.IntersectAll(ref ray, triangles, out pickedPositions, out pickedTriangles))
            {
                for (int i = 0; i < pickedPositions.Length; i++)
                {
                    float a = Helper.Angle(Vector3.Up, pickedTriangles[i].Normal);

                    if (a <= MathUtil.PiOverFour)
                    {
                        points.Add(pickedPositions[i]);
                    }
                }
            }

            p = points.ToArray();

            return points.Count > 0;
        }

        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation</returns>
        public override string ToString()
        {
            return string.Format("Nodes {0}; Side {1:0.00};", this.Nodes.Length, this.NodeSide);
        }
        /// <summary>
        /// Gets node wich contains specified point
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Returns the node wich contains the specified point if exists</returns>
        public GridNode FindNode(Vector3 point)
        {
            for (int i = 0; i < this.Nodes.Length; i++)
            {
                if (this.Nodes[i].Contains(point))
                {
                    return this.Nodes[i];
                }
            }

            return null;
        }
    }
}
