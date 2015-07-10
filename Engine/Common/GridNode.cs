using System;
using System.Collections.Generic;
using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Grid node
    /// </summary>
    public class GridNode
    {
        /// <summary>
        /// Connected nodes dictionary
        /// </summary>
        private Dictionary<Headings, GridNode> ConnectedNodes = new Dictionary<Headings, GridNode>();

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
                if (this.ConnectedNodes.ContainsKey(heading))
                {
                    return this.ConnectedNodes[heading];
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
        /// Node state
        /// </summary>
        public GridNodeStates State = GridNodeStates.Clear;
        /// <summary>
        /// Node passing cost
        /// </summary>
        public float Cost;
        /// <summary>
        /// Center position
        /// </summary>
        public Vector3 Center
        {
            get
            {
                return (this.NorthWest + this.NorthEast + this.SouthWest + this.SouthEast) * 0.25f;
            }
        }

        /// <summary>
        /// Gets inverse of heading
        /// </summary>
        /// <param name="heading">Heading</param>
        /// <returns>Returns inverse of heading</returns>
        public static Headings GetInverse(Headings heading)
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
            //Buscar mayor y menor X y Z
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

            if (p0.X == maxX && p0.Z == maxZ) this.NorthEast = p0;
            else if (p1.X == maxX && p1.Z == maxZ) this.NorthEast = p1;
            else if (p2.X == maxX && p2.Z == maxZ) this.NorthEast = p2;
            else if (p3.X == maxX && p3.Z == maxZ) this.NorthEast = p3;

            if (p0.X == minX && p0.Z == maxZ) this.NorthWest = p0;
            else if (p1.X == minX && p1.Z == maxZ) this.NorthWest = p1;
            else if (p2.X == minX && p2.Z == maxZ) this.NorthWest = p2;
            else if (p3.X == minX && p3.Z == maxZ) this.NorthWest = p3;

            if (p0.X == minX && p0.Z == minZ) this.SouthWest = p0;
            else if (p1.X == minX && p1.Z == minZ) this.SouthWest = p1;
            else if (p2.X == minX && p2.Z == minZ) this.SouthWest = p2;
            else if (p3.X == minX && p3.Z == minZ) this.SouthWest = p3;

            if (p0.X == maxX && p0.Z == minZ) this.SouthEast = p0;
            else if (p1.X == maxX && p1.Z == minZ) this.SouthEast = p1;
            else if (p2.X == maxX && p2.Z == minZ) this.SouthEast = p2;
            else if (p3.X == maxX && p3.Z == minZ) this.SouthEast = p3;

            this.Cost = cost;
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
            bool containsNorthWest = this.IsConnected(node.NorthWest);
            bool containsNorthEast = this.IsConnected(node.NorthEast);
            bool containsSouthWest = this.IsConnected(node.SouthWest);
            bool containsSouthEast = this.IsConnected(node.SouthEast);

            if (!containsNorthWest &&
                !containsNorthEast &&
                !containsSouthWest &&
                !containsSouthEast)
            {
                return Headings.None;
            }
            else if (containsNorthWest)
            {
                if (containsNorthEast) return Headings.North;
                else if (containsSouthWest) return Headings.West;
                else return Headings.NorthWest;
            }
            else if (containsNorthEast)
            {
                if (containsNorthWest) return Headings.North;
                else if (containsSouthEast) return Headings.East;
                else return Headings.NorthEast;
            }
            else if (containsSouthWest)
            {
                if (containsNorthWest) return Headings.West;
                else if (containsSouthEast) return Headings.South;
                else return Headings.SouthWest;
            }
            else if (containsSouthEast)
            {
                if (containsNorthEast) return Headings.East;
                else if (containsSouthWest) return Headings.South;
                else return Headings.SouthEast;
            }

            return Headings.None;
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
                Headings headingOther = GetInverse(headingThis);

                if (!this.ConnectedNodes.ContainsKey(headingThis)) this.ConnectedNodes.Add(headingThis, gridNode);
                if (!gridNode.ConnectedNodes.ContainsKey(headingOther)) gridNode.ConnectedNodes.Add(headingOther, this);
            }
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
        public Vector3[] GetCorners()
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
            return string.Format("State {0}; Cost {1:0.00}; Connections {2}; Center: {3}", this.State, this.Cost, this.ConnectedNodes.Count, this.Center);
        }
    }
}
