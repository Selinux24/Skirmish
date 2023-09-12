using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.AStar
{
    /// <summary>
    /// Grid node
    /// </summary>
    public class GridNode : IGraphNode
    {
        /// <summary>
        /// Connected nodes dictionary
        /// </summary>
        private readonly Dictionary<Headings, int> nodesDictionary = new();

        /// <summary>
        /// Connections to this node list
        /// </summary>
        protected List<GridNode> ConnectedNodes = new();

        /// <summary>
        /// North West point
        /// </summary>
        public Vector3 NorthWest { get; private set; }
        /// <summary>
        /// North East point
        /// </summary>
        public Vector3 NorthEast { get; private set; }
        /// <summary>
        /// South West point
        /// </summary>
        public Vector3 SouthWest { get; private set; }
        /// <summary>
        /// South East point
        /// </summary>
        public Vector3 SouthEast { get; private set; }
        /// <inheritdoc/>
        public Vector3 Center { get; private set; }
        /// <inheritdoc/>
        public float TotalCost { get; set; }
        /// <summary>
        /// Node state
        /// </summary>
        public GridNodeStates State { get; set; }
        /// <summary>
        /// Gets whether the node is connected in all headings
        /// </summary>
        public bool FullConnected
        {
            get
            {
                return ConnectedNodes.Count == 8;
            }
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
        internal static IEnumerable<GridNode> GenerateGridNodes(int nodeCount, int xSize, int zSize, float nodeSize, GridCollisionInfo[][] collisionValues)
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

            var p0 = c0.Point;
            var p1 = c1.Point;
            var p2 = c2.Point;
            var p3 = c3.Point;

            //Look for X and Z bounds
            float maxX = float.MinValue;
            float maxZ = float.MinValue;
            float minX = float.MaxValue;
            float minZ = float.MaxValue;

            maxX = Math.Max(maxX, p0.X);
            maxX = Math.Max(maxX, p1.X);
            maxX = Math.Max(maxX, p2.X);
            maxX = Math.Max(maxX, p3.X);

            maxZ = Math.Max(maxZ, p0.Z);
            maxZ = Math.Max(maxZ, p1.Z);
            maxZ = Math.Max(maxZ, p2.Z);
            maxZ = Math.Max(maxZ, p3.Z);

            minX = Math.Min(minX, p0.X);
            minX = Math.Min(minX, p1.X);
            minX = Math.Min(minX, p2.X);
            minX = Math.Min(minX, p3.X);

            minZ = Math.Min(minZ, p0.Z);
            minZ = Math.Min(minZ, p1.Z);
            minZ = Math.Min(minZ, p2.Z);
            minZ = Math.Min(minZ, p3.Z);

            var ne = GetNorthEast(maxX, maxZ, p0, p1, p2, p3) ?? Vector3.Zero;
            var nw = GetNorthWest(minX, maxZ, p0, p1, p2, p3) ?? Vector3.Zero;
            var sw = GetSouthWest(minX, minZ, p0, p1, p2, p3) ?? Vector3.Zero;
            var se = GetSouthEast(maxX, minZ, p0, p1, p2, p3) ?? Vector3.Zero;

            var center = (p0 + p1 + p2 + p3) / 4f;

            float cost = Helper.Angle(Vector3.Up, va);

            return new GridNode(ne, nw, sw, se, center, cost);
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
                    result.Add(UniqueCollision(c0, c1, c2, c3));
                }
            }

            return result;
        }
        /// <summary>
        /// Gets the north east position in the specified points
        /// </summary>
        /// <param name="maxX">Max X</param>
        /// <param name="maxZ">Max Z</param>
        /// <param name="p0">Point 1</param>
        /// <param name="p1">Point 2</param>
        /// <param name="p2">Point 3</param>
        /// <param name="p3">Point 4</param>
        /// <returns>Returns the north east position</returns>
        private static Vector3? GetNorthEast(float maxX, float maxZ, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            if (p0.X == maxX && p0.Z == maxZ) return p0;
            else if (p1.X == maxX && p1.Z == maxZ) return p1;
            else if (p2.X == maxX && p2.Z == maxZ) return p2;
            else if (p3.X == maxX && p3.Z == maxZ) return p3;

            return null;
        }
        /// <summary>
        /// Gets the north west position in the specified points
        /// </summary>
        /// <param name="minX">Min X</param>
        /// <param name="maxZ">Max Z</param>
        /// <param name="p0">Point 1</param>
        /// <param name="p1">Point 2</param>
        /// <param name="p2">Point 3</param>
        /// <param name="p3">Point 4</param>
        /// <returns>Returns the north west position</returns>
        private static Vector3? GetNorthWest(float minX, float maxZ, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            if (p0.X == minX && p0.Z == maxZ) return p0;
            else if (p1.X == minX && p1.Z == maxZ) return p1;
            else if (p2.X == minX && p2.Z == maxZ) return p2;
            else if (p3.X == minX && p3.Z == maxZ) return p3;

            return null;
        }
        /// <summary>
        /// Gets the south west position in the specified points
        /// </summary>
        /// <param name="minX">Min X</param>
        /// <param name="minZ">Min Z</param>
        /// <param name="p0">Point 1</param>
        /// <param name="p1">Point 2</param>
        /// <param name="p2">Point 3</param>
        /// <param name="p3">Point 4</param>
        /// <returns>Returns the south west position</returns>
        private static Vector3? GetSouthWest(float minX, float minZ, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            if (p0.X == minX && p0.Z == minZ) return p0;
            else if (p1.X == minX && p1.Z == minZ) return p1;
            else if (p2.X == minX && p2.Z == minZ) return p2;
            else if (p3.X == minX && p3.Z == minZ) return p3;

            return null;
        }
        /// <summary>
        /// Gets the south east position in the specified points
        /// </summary>
        /// <param name="maxX">Max X</param>
        /// <param name="minZ">Min Z</param>
        /// <param name="p0">Point 1</param>
        /// <param name="p1">Point 2</param>
        /// <param name="p2">Point 3</param>
        /// <param name="p3">Point 4</param>
        /// <returns>Returns the south east position</returns>
        private static Vector3? GetSouthEast(float maxX, float minZ, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            if (p0.X == maxX && p0.Z == minZ) return p0;
            else if (p1.X == maxX && p1.Z == minZ) return p1;
            else if (p2.X == maxX && p2.Z == minZ) return p2;
            else if (p3.X == maxX && p3.Z == minZ) return p3;

            return null;
        }
        /// <summary>
        /// Gets opposite of heading
        /// </summary>
        /// <param name="heading">Heading</param>
        /// <returns>Returns opposite of heading</returns>
        public static Headings GetOpposite(Headings heading)
        {
            if (heading == Headings.North) return Headings.South;
            else if (heading == Headings.South) return Headings.North;
            else if (heading == Headings.East) return Headings.West;
            else if (heading == Headings.West) return Headings.East;

            else if (heading == Headings.NorthWest) return Headings.SouthEast;
            else if (heading == Headings.NorthEast) return Headings.SouthWest;
            else if (heading == Headings.SouthWest) return Headings.NorthEast;
            else if (heading == Headings.SouthEast) return Headings.NorthWest;

            return Headings.None;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ne">North east point</param>
        /// <param name="nw">North west point</param>
        /// <param name="sw">South west point</param>
        /// <param name="se">South east point</param>
        /// <param name="center">Center point</param>
        /// <param name="cost">Cost</param>
        public GridNode(Vector3 ne, Vector3 nw, Vector3 sw, Vector3 se, Vector3 center, float cost)
        {
            NorthEast = ne;
            NorthWest = nw;
            SouthEast = sw;
            SouthWest = se;
            Center = center;
            TotalCost = cost;
            State = GridNodeStates.Clear;
        }

        /// <summary>
        /// Gets if node has connection with specified position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns true if node has connection with specified position</returns>
        public bool IsConnected(Vector3 position)
        {
            return
                NorthWest == position ||
                NorthEast == position ||
                SouthWest == position ||
                SouthEast == position;
        }
        /// <summary>
        /// Gets specified node direction from current node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Returns specified node direction from current node</returns>
        public Headings GetHeadingTo(GridNode node)
        {
            bool connectedWithNorthWest = IsConnected(node.NorthWest);
            bool connectedWithNorthEast = IsConnected(node.NorthEast);
            bool connectedWithSouthWest = IsConnected(node.SouthWest);
            bool connectedWithSouthEast = IsConnected(node.SouthEast);

            if (!connectedWithNorthWest &&
                !connectedWithNorthEast &&
                !connectedWithSouthWest &&
                !connectedWithSouthEast)
            {
                return Headings.None;
            }
            else if (connectedWithNorthWest)
            {
                if (connectedWithNorthEast)
                {
                    return Headings.North;
                }
                else if (connectedWithSouthWest)
                {
                    return Headings.West;
                }

                return Headings.NorthWest;
            }
            else if (connectedWithNorthEast)
            {
                if (connectedWithSouthEast)
                {
                    return Headings.East;
                }

                return Headings.NorthEast;
            }
            else if (connectedWithSouthWest)
            {
                if (connectedWithSouthEast)
                {
                    return Headings.South;
                }

                return Headings.SouthWest;
            }
            else
            {
                return Headings.SouthEast;
            }
        }
        /// <summary>
        /// Try connect nodes
        /// </summary>
        /// <param name="gridNode">Grid node to connect</param>
        public void TryConnect(GridNode gridNode)
        {
            var headingThis = GetHeadingTo(gridNode);

            if (headingThis == Headings.None)
            {
                return;
            }

            var headingOther = GetOpposite(headingThis);

            if (!nodesDictionary.ContainsKey(headingThis))
            {
                ConnectedNodes.Add(gridNode);
                nodesDictionary.Add(headingThis, ConnectedNodes.Count - 1);
            }

            if (!gridNode.nodesDictionary.ContainsKey(headingOther))
            {
                gridNode.ConnectedNodes.Add(this);
                gridNode.nodesDictionary.Add(headingOther, gridNode.ConnectedNodes.Count - 1);
            }
        }
        /// <summary>
        /// Gets the connected node list
        /// </summary>
        public IEnumerable<GridNode> GetNodeConnections()
        {
            return ConnectedNodes.Where(n => n != null).AsEnumerable();
        }

        /// <inheritdoc/>
        public bool Contains(Vector3 point)
        {
            if (point.X >= SouthWest.X && point.Z >= SouthWest.Z &&
                point.X <= NorthEast.X && point.Z <= NorthEast.Z)
            {
                return true;
            }

            return false;
        }
        /// <inheritdoc/>
        public IEnumerable<Vector3> GetPoints()
        {
            return new[]
            {
                NorthEast,
                NorthWest,
                SouthWest,
                SouthEast,
            };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Center: {Center}; Cost: {TotalCost:0.00}; State: {State}; Connections: {ConnectedNodes.Count}";
        }
    }
}
