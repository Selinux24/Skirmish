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
        public static Grid Build(Terrain terrain, float size)
        {
            List<GridNode> result = new List<GridNode>();

            float half = size * 0.5f;

            BoundingBox bbox = terrain.GetBoundingBox();

            for (float x = bbox.Minimum.X + 1; x < bbox.Maximum.X - 1; x += size)
            {
                for (float z = bbox.Minimum.Z + 1; z < bbox.Maximum.Z - 1; z += size)
                {
                    Vector3[] points;
                    Triangle[] triangles;
                    if (TestPoint(x, z, terrain, out points, out triangles))
                    {
                        //Each point is a node
                        for (int i = 0; i < points.Length; i++)
                        {
                            Vector3 point = points[i];

                            Vector3[] p0;
                            Triangle[] t0;
                            if (!TestPoint(point.X + -half, point.Z + -half, terrain, out p0, out t0)) continue;

                            Vector3[] p1;
                            Triangle[] t1;
                            if (!TestPoint(point.X + +half, point.Z + -half, terrain, out p1, out t1)) continue;

                            Vector3[] p2;
                            Triangle[] t2;
                            if (!TestPoint(point.X + -half, point.Z + +half, terrain, out p2, out t2)) continue;

                            Vector3[] p3;
                            Triangle[] t3;
                            if (!TestPoint(point.X + +half, point.Z + +half, terrain, out p3, out t3)) continue;

                            if (p0.Length == p1.Length &&
                                p0.Length == p2.Length &&
                                p0.Length == p3.Length)
                            {
                                for (int n = 0; n < p0.Length; n++)
                                {
                                    Vector3 va = (t0[n].Normal + t1[n].Normal + t2[n].Normal + t3[n].Normal) * 0.25f;

                                    GridNode newNode = new GridNode(p0[n], p1[n], p2[n], p3[n], Helper.Angle(Vector3.Up, va));

                                    result.Add(newNode);
                                }
                            }
                            else
                            {
                                //TODO: Process shared nodes. May exists?
                                throw new System.NotImplementedException();
                            }
                        }
                    }
                    else
                    {
                        //TODO: May be?
                        throw new System.NotImplementedException();
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
        /// <param name="points">Result point list</param>
        /// <returns>Returns true if point is valid.</returns>
        private static bool TestPoint(float x, float z, Terrain terrain, out Vector3[] points, out Triangle[] tris)
        {
            points = null;
            tris = null;

            List<Vector3> pointList = new List<Vector3>();
            List<Triangle> triangleList = new List<Triangle>();

            Vector3[] pickedPositions;
            Triangle[] pickedTriangles;
            if (terrain.FindAllGroundPosition(x, z, out pickedPositions, out pickedTriangles))
            {
                for (int i = 0; i < pickedPositions.Length; i++)
                {
                    float a = Helper.Angle(Vector3.Up, pickedTriangles[i].Normal);

                    if (a <= MathUtil.PiOverFour)
                    {
                        pointList.Add(pickedPositions[i]);
                        triangleList.Add(pickedTriangles[i]);
                    }
                }
            }

            if (pointList.Count > 0)
            {
                points = pointList.ToArray();
                tris = triangleList.ToArray();

                return true;
            }
            else
            {
                return false;
            }
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
