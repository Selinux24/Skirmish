using SharpDX;
using System;

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
        /// <param name="list">Polygon array</param>
        /// <returns>Returns the generated node array</returns>
        public static NavmeshNode[] FromPolygonArray(Polygon[] list)
        {
            var nodes = new NavmeshNode[list.Length];

            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new NavmeshNode(list[i]);
            }

            return nodes;
        }

        /// <summary>
        /// Internal polygon
        /// </summary>
        public Polygon Poly;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="poly">Polygon</param>
        public NavmeshNode(Polygon poly)
        {
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
