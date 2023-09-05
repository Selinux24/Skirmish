using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public GridInput(Func<IEnumerable<Triangle>> fnc) : base(fnc)
        {

        }

        /// <inheritdoc/>
        public override async Task<IGraph> CreateGraph(PathFinderSettings settings, Action<float> progressCallback = null)
        {
            IGraph grid = null;

            var triangles = await GetTriangles();

            await Task.Run(() =>
            {
                grid = CreateGrid(settings, triangles, progressCallback);
            });

            return grid;
        }
        /// <summary>
        /// Creates a new grid
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="triangles">Triangle list</param>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <returns>Returns the new grid</returns>
        private Grid CreateGrid(PathFinderSettings settings, IEnumerable<Triangle> triangles, Action<float> progressCallback)
        {
            var grid = new Grid(settings as GridGenerationSettings, this);

            var bbox = GeometryUtil.CreateBoundingBox(triangles);

            var dictionary = new Dictionary<Vector2, GridCollisionInfo[]>();

            float fxSize = (bbox.Maximum.X - bbox.Minimum.X) / grid.Settings.NodeSize;
            float fzSize = (bbox.Maximum.Z - bbox.Minimum.Z) / grid.Settings.NodeSize;

            int xSize = fxSize > (int)fxSize ? (int)fxSize + 1 : (int)fxSize;
            int zSize = fzSize > (int)fzSize ? (int)fzSize + 1 : (int)fzSize;

            float total = bbox.Size.X * bbox.Size.X / grid.Settings.NodeSize;
            int curr = 0;

            for (float x = bbox.Minimum.X; x < bbox.Maximum.X; x += grid.Settings.NodeSize)
            {
                for (float z = bbox.Minimum.Z; z < bbox.Maximum.Z; z += grid.Settings.NodeSize)
                {
                    GridCollisionInfo[] info;

                    var ray = new PickingRay(new Vector3(x, bbox.Maximum.Y + 0.01f, z), Vector3.Down);

                    bool intersects = RayPickingHelper.PickAllFromlist(triangles, ray, out var picks);
                    if (intersects)
                    {
                        info = new GridCollisionInfo[picks.Count()];

                        for (int i = 0; i < picks.Count(); i++)
                        {
                            var pick = picks.ElementAt(i);

                            info[i] = new GridCollisionInfo()
                            {
                                Point = pick.Position,
                                Triangle = pick.Primitive,
                                Distance = pick.Distance,
                            };
                        }
                    }
                    else
                    {
                        info = Array.Empty<GridCollisionInfo>();
                    }

                    dictionary.Add(new Vector2(x, z), info);

                    progressCallback?.Invoke(++curr / total);
                }
            }

            int gridNodeCount = (xSize - 1) * (zSize - 1);

            GridCollisionInfo[][] collisionValues = new GridCollisionInfo[dictionary.Count][];
            dictionary.Values.CopyTo(collisionValues, 0);

            //Generate grid nodes
            var result = GenerateGridNodes(gridNodeCount, xSize, zSize, grid.Settings.NodeSize, collisionValues);

            //Fill connections
            FillConnections(result);

            grid.AddRange(result.ToArray());
            grid.Initialized = true;

            return grid;
        }
        /// <summary>
        /// Generate grid nodes
        /// </summary>
        /// <param name="nodeCount">Node count</param>
        /// <param name="xSize">Total X size</param>
        /// <param name="zSize">Total Z size</param>
        /// <param name="nodeSize">Node size</param>
        /// <param name="collisionValues">Collision values</param>
        /// <returns>Generates a grid node list</returns>
        private static IEnumerable<GridNode> GenerateGridNodes(int nodeCount, int xSize, int zSize, float nodeSize, GridCollisionInfo[][] collisionValues)
        {
            var result = new List<GridNode>();

            //Generate grid nodes
            for (int n = 0; n < nodeCount; n++)
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

                var coor0 = collisionValues[i0];
                var coor1 = collisionValues[i1];
                var coor2 = collisionValues[i2];
                var coor3 = collisionValues[i3];

                int min = Helper.Min(coor0.Length, coor1.Length, coor2.Length, coor3.Length);
                int max = Helper.Max(coor0.Length, coor1.Length, coor2.Length, coor3.Length);

                if (min == 0)
                {
                    //None
                    continue;
                }

                if (max == 1 && min == 1)
                {
                    //Unique collision node
                    var resUnique = UniqueCollision(coor0[0], coor1[0], coor2[0], coor3[0]);
                    result.Add(resUnique);

                    continue;
                }

                //Process multiple point nodes
                var resMultiple = MultipleCollision(max, nodeSize, coor0, coor1, coor2, coor3);
                if (resMultiple.Any())
                {
                    result.AddRange(resMultiple);
                }
            }

            return result;
        }
        /// <summary>
        /// Generates a node list from unique collision data
        /// </summary>
        /// <param name="c0">Collision info 1</param>
        /// <param name="c1">Collision info 2</param>
        /// <param name="c2">Collision info 3</param>
        /// <param name="c3">Collision info 4</param>
        /// <returns>Returns a node list from unique collision data</returns>
        private static GridNode UniqueCollision(GridCollisionInfo c0, GridCollisionInfo c1, GridCollisionInfo c2, GridCollisionInfo c3)
        {
            Vector3 va = (
                c0.Triangle.Normal +
                c1.Triangle.Normal +
                c2.Triangle.Normal +
                c3.Triangle.Normal) * 0.25f;

            var newNode = new GridNode(
                c0.Point,
                c1.Point,
                c2.Point,
                c3.Point,
                Helper.Angle(Vector3.Up, va));

            return newNode;
        }
        /// <summary>
        /// Generates a node list from multiple collision data
        /// </summary>
        /// <param name="max">Maximum tests</param>
        /// <param name="nodeSize">Node size</param>
        /// <param name="coor0">Collision info 1</param>
        /// <param name="coor1">Collision info 2</param>
        /// <param name="coor2">Collision info 3</param>
        /// <param name="coor3">Collision info 4</param>
        /// <returns>Returns a node list from multiple collision data</returns>
        private static IEnumerable<GridNode> MultipleCollision(int max, float nodeSize, GridCollisionInfo[] coor0, GridCollisionInfo[] coor1, GridCollisionInfo[] coor2, GridCollisionInfo[] coor3)
        {
            var result = new List<GridNode>();

            for (int i = 0; i < max; i++)
            {
                var c0 = i < coor0.Length ? coor0[i] : coor0[^1];
                var c1 = i < coor1.Length ? coor1[i] : coor1[^1];
                var c2 = i < coor2.Length ? coor2[i] : coor2[^1];
                var c3 = i < coor3.Length ? coor3[i] : coor3[^1];

                float fmin = Helper.Min(c0.Point.Y, c1.Point.Y, c2.Point.Y, c3.Point.Y);
                float fmax = Helper.Max(c0.Point.Y, c1.Point.Y, c2.Point.Y, c3.Point.Y);
                float diff = Math.Abs(fmax - fmin);

                if (diff <= nodeSize)
                {
                    var resUnique = UniqueCollision(c0, c1, c2, c3);
                    result.Add(resUnique);
                }
            }

            return result;
        }
        /// <summary>
        /// Fill node connections
        /// </summary>
        /// <param name="nodes">Grid nodes</param>
        private static void FillConnections(IEnumerable<GridNode> nodes)
        {
            for (int i = 0; i < nodes.Count(); i++)
            {
                var nodeA = nodes.ElementAt(i);
                if (nodeA.FullConnected)
                {
                    continue;
                }

                for (int n = i + 1; n < nodes.Count(); n++)
                {
                    var nodeB = nodes.ElementAt(n);
                    if (nodeB.FullConnected)
                    {
                        continue;
                    }

                    nodeA.TryConnect(nodeB);
                }
            }
        }

        /// <inheritdoc/>
        public override Task Refresh()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override Task<string> GetHash(PathFinderSettings settings)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public override Task<IGraph> Load(string fileName, string hash = null)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public override Task Save(string fileName, IGraph graph)
        {
            throw new NotImplementedException();
        }
    }
}
