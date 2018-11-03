using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.AStar
{
    /// <summary>
    /// Grid input geometry
    /// </summary>
    public class GridInput : PathFinderInput
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fnc">Get triangles function</param>
        public GridInput(Func<Triangle[]> fnc) : base(fnc)
        {

        }

        /// <summary>
        /// Creates a new graph
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <returns>Returns the new graph</returns>
        public override IGraph CreateGraph(PathFinderSettings settings)
        {
            var grid = new Grid
            {
                Input = this,
                BuildSettings = settings as GridGenerationSettings
            };

            var triangles = this.GetTriangles();

            var bbox = GeometryUtil.CreateBoundingBox(triangles);

            List<GridNode> result = new List<GridNode>();

            Dictionary<Vector2, GridCollisionInfo[]> dictionary = new Dictionary<Vector2, GridCollisionInfo[]>();

            float fxSize = (bbox.Maximum.X - bbox.Minimum.X) / grid.BuildSettings.NodeSize;
            float fzSize = (bbox.Maximum.Z - bbox.Minimum.Z) / grid.BuildSettings.NodeSize;

            int xSize = fxSize > (int)fxSize ? (int)fxSize + 1 : (int)fxSize;
            int zSize = fzSize > (int)fzSize ? (int)fzSize + 1 : (int)fzSize;

            for (float x = bbox.Minimum.X; x < bbox.Maximum.X; x += grid.BuildSettings.NodeSize)
            {
                for (float z = bbox.Minimum.Z; z < bbox.Maximum.Z; z += grid.BuildSettings.NodeSize)
                {
                    GridCollisionInfo[] info = null;

                    Ray ray = new Ray()
                    {
                        Position = new Vector3(x, bbox.Maximum.Y + 0.01f, z),
                        Direction = Vector3.Down,
                    };

                    bool intersects = Intersection.IntersectAll(
                        ray, triangles, true,
                        out Vector3[] pickedPoints,
                        out Triangle[] pickedTriangles,
                        out float[] pickedDistances);

                    if (intersects)
                    {
                        info = new GridCollisionInfo[pickedPoints.Length];

                        for (int i = 0; i < pickedPoints.Length; i++)
                        {
                            info[i] = new GridCollisionInfo() { Point = pickedPoints[i], Triangle = pickedTriangles[i], Distance = pickedDistances[i] };
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

                        if (diff <= grid.BuildSettings.NodeSize)
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

            grid.Nodes = result.ToArray();

            return grid;
        }
        /// <summary>
        /// Refresh
        /// </summary>
        public override void Refresh()
        {

        }

        /// <summary>
        /// Loads the graph from a file
        /// </summary>
        /// <param name="fileName">File name</param>
        public override IGraph Load(string fileName)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="graph">Graph instance</param>
        public override void Save(string fileName, IGraph graph)
        {
            throw new NotImplementedException();
        }
    }
}
