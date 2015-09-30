using System;
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
        /// Collision info helper
        /// </summary>
        class GridCollisionInfo
        {
            /// <summary>
            /// Collision point
            /// </summary>
            public Vector3 Point;
            /// <summary>
            /// Collision triangle
            /// </summary>
            public Triangle Triangle;

            /// <summary>
            /// Gets text representarion of collision
            /// </summary>
            /// <returns>Returns text representarion of collision</returns>
            public override string ToString()
            {
                return string.Format("{0}", this.Point);
            }
        }

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
        /// <param name="bbox">Bounding box</param>
        /// <param name="triangles">Triangles</param>
        /// <param name="size">Node size</param>
        /// <param name="angle">Maximum angle of node</param>
        /// <returns>Returns generated grid node list</returns>
        public static Grid Build(BoundingBox bbox, Triangle[] triangles, float size, float angle = MathUtil.PiOverFour)
        {
            List<GridNode> result = new List<GridNode>();

            Dictionary<Vector2, GridCollisionInfo[]> dictionary = new Dictionary<Vector2, GridCollisionInfo[]>();

            float fxSize = (bbox.Maximum.X - bbox.Minimum.X) / size;
            float fzSize = (bbox.Maximum.Z - bbox.Minimum.Z) / size;

            int xSize = fxSize > (int)fxSize ? (int)fxSize + 1 : (int)fxSize;
            int zSize = fzSize > (int)fzSize ? (int)fzSize + 1 : (int)fzSize;

            for (float x = bbox.Minimum.X; x < bbox.Maximum.X; x += size)
            {
                for (float z = bbox.Minimum.Z; z < bbox.Maximum.Z; z += size)
                {
                    GridCollisionInfo[] info = null;

                    Ray ray = new Ray()
                    {
                        Position = new Vector3(x, bbox.Maximum.Y + 0.01f, z),
                        Direction = Vector3.Down,
                    };

                    Vector3[] pickedPoints;
                    Triangle[] pickedTriangles;
                    if (Triangle.IntersectAll(ref ray, triangles, true, out pickedPoints, out pickedTriangles))
                    {
                        info = new GridCollisionInfo[pickedPoints.Length];

                        for (int i = 0; i < pickedPoints.Length; i++)
                        {
                            info[i] = new GridCollisionInfo() { Point = pickedPoints[i], Triangle = pickedTriangles[i], };
                        }
                    }
                    else
                    {
                        info = new GridCollisionInfo[] { };
                    }

                    dictionary.Add(new Vector2(x, z), info);
                }
            }

            int gridNodeCount = (xSize - 1) * (zSize - 1);

            GridCollisionInfo[][] collisionValues = new GridCollisionInfo[dictionary.Count][];
            dictionary.Values.CopyTo(collisionValues, 0);

            //Generate grid nodes
            for (int n = 0; n < gridNodeCount; n++)
            {
                int x = n / xSize;
                int z = n - (x * xSize);

                if (x == zSize - 1) continue;
                if (z == xSize - 1) continue;

                //Find node corners
                int i0 = ((x + 0) * zSize) + (z + 0);
                int i1 = ((x + 0) * zSize) + (z + 1);
                int i2 = ((x + 1) * zSize) + (z + 0);
                int i3 = ((x + 1) * zSize) + (z + 1);

                GridCollisionInfo[] coor0 = collisionValues[i0];
                GridCollisionInfo[] coor1 = collisionValues[i1];
                GridCollisionInfo[] coor2 = collisionValues[i2];
                GridCollisionInfo[] coor3 = collisionValues[i3];

                int min = Helper.Min(coor0.Length, coor1.Length, coor2.Length, coor3.Length);
                int max = Helper.Max(coor0.Length, coor1.Length, coor2.Length, coor3.Length);

                if (min == 0)
                {
                    //None
                }
                else if (max == 1 && min == 1)
                {
                    //Unique collision node
                    for (int i = 0; i < max; i++)
                    {
                        Vector3 va = (
                            coor0[i].Triangle.Normal +
                            coor1[i].Triangle.Normal +
                            coor2[i].Triangle.Normal +
                            coor3[i].Triangle.Normal) * 0.25f;

                        GridNode newNode = new GridNode(
                            coor0[i].Point,
                            coor1[i].Point,
                            coor2[i].Point,
                            coor3[i].Point,
                            Helper.Angle(Vector3.Up, va));

                        result.Add(newNode);
                    }
                }
                else
                {
                    //Process multiple point nodes
                    for (int i = 0; i < max; i++)
                    {
                        GridCollisionInfo c0 = i < coor0.Length ? coor0[i] : coor0[coor0.Length - 1];
                        GridCollisionInfo c1 = i < coor1.Length ? coor1[i] : coor1[coor1.Length - 1];
                        GridCollisionInfo c2 = i < coor2.Length ? coor2[i] : coor2[coor2.Length - 1];
                        GridCollisionInfo c3 = i < coor3.Length ? coor3[i] : coor3[coor3.Length - 1];

                        float fmin = Helper.Min(c0.Point.Y, c1.Point.Y, c2.Point.Y, c3.Point.Y);
                        float fmax = Helper.Max(c0.Point.Y, c1.Point.Y, c2.Point.Y, c3.Point.Y);
                        float diff = Math.Abs(fmax - fmin);

                        if (diff <= size)
                        {
                            Vector3 va = (
                                c0.Triangle.Normal +
                                c1.Triangle.Normal +
                                c2.Triangle.Normal +
                                c3.Triangle.Normal) * 0.25f;

                            GridNode newNode = new GridNode(
                                c0.Point,
                                c1.Point,
                                c2.Point,
                                c3.Point,
                                Helper.Angle(Vector3.Up, va));

                            result.Add(newNode);
                        }
                    }
                }
            }

            //Fill connections
            for (int i = 0; i < result.Count; i++)
            {
                if (!result[i].FullConnected)
                {
                    for (int n = i + 1; n < result.Count; n++)
                    {
                        if (!result[n].FullConnected)
                        {
                            result[i].TryConnect(result[n]);
                        }
                    }
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
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="terrain">Terrain class</param>
        /// <param name="angle">Maximum angle of node</param>
        /// <param name="points">Result point list</param>
        /// <param name="tris">Result triangle list</param>
        /// <returns>Returns true if point is valid.</returns>
        private static bool TestPoint(float x, float z, Terrain terrain, float angle, out Vector3[] points, out Triangle[] tris)
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
                    if (a <= angle)
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
        /// Gets node wich contains specified point
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Returns the node wich contains the specified point if exists</returns>
        public GridNode FindNode(Vector3 point)
        {
            float minDistance = float.MaxValue;
            GridNode bestNode = null;

            for (int i = 0; i < this.Nodes.Length; i++)
            {
                float distance;
                if (this.Nodes[i].Contains(point, out distance))
                {
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestNode = this.Nodes[i];
                    }
                }
            }

            return bestNode;
        }

        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation</returns>
        public override string ToString()
        {
            return string.Format("Nodes {0}; Side {1:0.00};", this.Nodes.Length, this.NodeSide);
        }
    }
}
