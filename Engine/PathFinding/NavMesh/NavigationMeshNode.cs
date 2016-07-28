using SharpDX;

namespace Engine.PathFinding.NavMesh
{
    using Engine.PathFinding;

    /// <summary>
    /// Navigation mesh node
    /// </summary>
    public class NavigationMeshNode : IGraphNode
    {
        /// <summary>
        /// Generates a navigation node array from a polygon array
        /// </summary>
        /// <param name="parent">Parent navigation mesh</param>
        /// <param name="list">Polygon array</param>
        /// <returns>Returns the generated node array</returns>
        public static NavigationMeshNode[] FromPolygonArray(NavigationMesh parent, Polygon[] list)
        {
            var nodes = new NavigationMeshNode[list.Length];

            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new NavigationMeshNode(parent, list[i]);
            }

            return nodes;
        }

        /// <summary>
        /// Parent navigation mesh
        /// </summary>
        private NavigationMesh NavigationMesh;
        /// <summary>
        /// Internal polygon
        /// </summary>
        public Polygon Poly;
        /// <summary>
        /// Node passing cost
        /// </summary>
        public float Cost { get; set; }
        /// <summary>
        /// Center position
        /// </summary>
        public Vector3 Center { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">Parent</param>
        /// <param name="poly">Polygon</param>
        public NavigationMeshNode(NavigationMesh parent, Polygon poly)
        {
            this.NavigationMesh = parent;
            this.Poly = poly;
            this.Center = poly.Center;
        }

        /// <summary>
        /// Gets whether this node contains specified point
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <returns>Returns whether this node contains specified point</returns>
        public bool Contains(Vector3 point, out float distance)
        {
            distance = 0;

            return this.Poly.Contains(point);
        }
        /// <summary>
        /// Get four node corners
        /// </summary>
        /// <returns>Returns four node corners</returns>
        public Vector3[] GetPoints()
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
