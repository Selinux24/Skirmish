using SharpDX;
using System;
using System.Collections.Generic;

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
        private readonly Dictionary<Headings, int> nodesDictionary = new Dictionary<Headings, int>();

        /// <summary>
        /// Connections to this node list
        /// </summary>
        protected List<GridNode> ConnectedNodes = new List<GridNode>();

        /// <summary>
        /// Gets the connected node list
        /// </summary>
        public GridNode[] Connections
        {
            get
            {
                return this.ConnectedNodes.ToArray();
            }
        }
        /// <summary>
        /// Gets a connected node by index
        /// </summary>
        /// <param name="index">Node index</param>
        /// <returns>Returns the connected node by index</returns>
        public GridNode this[int index]
        {
            get
            {
                return this.ConnectedNodes[index];
            }
        }
        /// <summary>
        /// Node state
        /// </summary>
        public GridNodeStates State { get; set; }
        /// <summary>
        /// Node passing cost
        /// </summary>
        public float TotalCost { get; set; }
        /// <summary>
        /// Center position
        /// </summary>
        public Vector3 Center { get; protected set; }
        /// <summary>
        /// North West point
        /// </summary>
        public readonly Vector3 NorthWest;
        /// <summary>
        /// North East point
        /// </summary>
        public readonly Vector3 NorthEast;
        /// <summary>
        /// South West point
        /// </summary>
        public readonly Vector3 SouthWest;
        /// <summary>
        /// South East point
        /// </summary>
        public readonly Vector3 SouthEast;
        /// <summary>
        /// Gets connected node of specified heading
        /// </summary>
        /// <param name="heading">Heading</param>
        /// <returns>Returns connected node of specified heading if exists</returns>
        public GridNode this[Headings heading]
        {
            get
            {
                if (this.nodesDictionary.ContainsKey(heading))
                {
                    int index = this.nodesDictionary[heading];

                    return this.ConnectedNodes[index];
                }

                return null;
            }
        }
        /// <summary>
        /// Gets whether the node is connected in all headings
        /// </summary>
        public bool FullConnected
        {
            get
            {
                return this.ConnectedNodes.Count == 8;
            }
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
        /// <param name="p0">Vertex 0</param>
        /// <param name="p1">Vertex 1</param>
        /// <param name="p2">Vertex 2</param>
        /// <param name="p3">Vertex 3</param>
        /// <param name="cost">Cost</param>
        public GridNode(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float cost)
        {
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

            var ne = GetNorthEast(maxX, maxZ, p0, p1, p2, p3);
            if (ne.HasValue) this.NorthEast = ne.Value;

            var nw = GetNorthWest(minX, maxZ, p0, p1, p2, p3);
            if (nw.HasValue) this.NorthWest = nw.Value;

            var sw = GetSouthWest(minX, minZ, p0, p1, p2, p3);
            if (sw.HasValue) this.SouthWest = sw.Value;

            var se = GetSouthEast(maxX, minZ, p0, p1, p2, p3);
            if (se.HasValue) this.SouthEast = se.Value;

            this.TotalCost = cost;
            this.State = GridNodeStates.Clear;
            this.Center = (p0 + p1 + p2 + p3) / 4f;
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
        private Vector3? GetNorthEast(float maxX, float maxZ, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
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
        private Vector3? GetNorthWest(float minX, float maxZ, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
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
        private Vector3? GetSouthWest(float minX, float minZ, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
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
        private Vector3? GetSouthEast(float maxX, float minZ, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            if (p0.X == maxX && p0.Z == minZ) return p0;
            else if (p1.X == maxX && p1.Z == minZ) return p1;
            else if (p2.X == maxX && p2.Z == minZ) return p2;
            else if (p3.X == maxX && p3.Z == minZ) return p3;

            return null;
        }

        /// <summary>
        /// Gets if node has connection with specified position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns true if node has connection with specified position</returns>
        public bool IsConnected(Vector3 position)
        {
            return (
                this.NorthWest == position ||
                this.NorthEast == position ||
                this.SouthWest == position ||
                this.SouthEast == position);
        }
        /// <summary>
        /// Gets specified node direction from current node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Returns specified node direction from current node</returns>
        public Headings GetHeadingTo(GridNode node)
        {
            bool connectedWithNorthWest = this.IsConnected(node.NorthWest);
            bool connectedWithNorthEast = this.IsConnected(node.NorthEast);
            bool connectedWithSouthWest = this.IsConnected(node.SouthWest);
            bool connectedWithSouthEast = this.IsConnected(node.SouthEast);

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
            Headings headingThis = this.GetHeadingTo(gridNode);

            if (headingThis != Headings.None)
            {
                Headings headingOther = GetOpposite(headingThis);

                if (!this.nodesDictionary.ContainsKey(headingThis))
                {
                    this.ConnectedNodes.Add(gridNode);
                    this.nodesDictionary.Add(headingThis, this.ConnectedNodes.Count - 1);
                }

                if (!gridNode.nodesDictionary.ContainsKey(headingOther))
                {
                    gridNode.ConnectedNodes.Add(this);
                    gridNode.nodesDictionary.Add(headingOther, gridNode.ConnectedNodes.Count - 1);
                }
            }
        }
        /// <summary>
        /// Gets whether this node contains specified point
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <returns>Returns whether this node contains specified point</returns>
        public bool Contains(Vector3 point)
        {
            if (point.X >= this.SouthWest.X && point.Z >= this.SouthWest.Z &&
                point.X <= this.NorthEast.X && point.Z <= this.NorthEast.Z)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Gets whether this node contains specified point
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <returns>Returns whether this node contains specified point</returns>
        public bool Contains(Vector3 point, out float distance)
        {
            distance = float.MaxValue;

            if (point.X >= this.SouthWest.X && point.Z >= this.SouthWest.Z &&
                point.X <= this.NorthEast.X && point.Z <= this.NorthEast.Z)
            {
                distance = Vector3.DistanceSquared(point, this.Center);

                return true;
            }

            return false;
        }
        /// <summary>
        /// Get four node corners
        /// </summary>
        /// <returns>Returns four node corners</returns>
        public Vector3[] GetPoints()
        {
            return new[]
            {
                this.NorthEast,
                this.NorthWest,
                this.SouthWest,
                this.SouthEast,
            };
        }
        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation</returns>
        public override string ToString()
        {
            return string.Format("State {0}; Cost {1:0.00}; Connections {2}; Center: {3}", this.State, this.TotalCost, this.ConnectedNodes.Count, this.Center);
        }
    }
}
