using SharpDX;

namespace Engine.Common
{
    using Engine.PathFinding;

    /// <summary>
    /// Navigation mesh node
    /// </summary>
    public class NavmeshNode : GraphNode
    {
        /// <summary>
        /// Generates a navigation node array from a polygon array
        /// </summary>
        /// <param name="parent">Parent navigation mesh</param>
        /// <param name="list">Polygon array</param>
        /// <returns>Returns the generated node array</returns>
        public static NavmeshNode[] FromPolygonArray(NavMesh parent, Polygon[] list)
        {
            var nodes = new NavmeshNode[list.Length];

            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new NavmeshNode(parent, list[i]);
            }

            return nodes;
        }

        /// <summary>
        /// Parent navigation mesh
        /// </summary>
        private NavMesh NavigationMesh;
        /// <summary>
        /// Internal polygon
        /// </summary>
        public Polygon Poly;
        /// <summary>
        /// Gets node connections
        /// </summary>
        public override IGraphNode[] Connections
        {
            get
            {
                return this.NavigationMesh.GetConnections(this);
            }
        }
        /// <summary>
        /// Gets a connection by index
        /// </summary>
        /// <param name="index">Node index</param>
        /// <returns>Returns the connected node at index</returns>
        public override IGraphNode this[int index]
        {
            get
            {
                return this.NavigationMesh.GetConnections(this)[index];
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">Parent</param>
        /// <param name="poly">Polygon</param>
        public NavmeshNode(NavMesh parent, Polygon poly)
        {
            this.NavigationMesh = parent;
            this.Poly = poly;
            this.Center = poly.Center;
        }

        /// <summary>
        /// Gets whether the node contains the projected point
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="distance">Distance to point</param>
        /// <returns>Returns true if the node contains the projected point</returns>
        public override bool Contains(Vector3 point, out float distance)
        {
            distance = 0;

            if (Polygon.PointInPoly(this.Poly, point))
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Gets the point list of the navigation mesh borders
        /// </summary>
        /// <returns>Returns the point list of the navigation mesh borders</returns>
        public override Vector3[] GetPoints()
        {
            return this.Poly.Points;
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("{0}", this.Poly);
        }
    }
}
