using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// Uses the PolyMesh polygon data for pathfinding
    /// </summary>
    public class Poly
    {
        /// <summary>
        /// 
        /// </summary>
        public List<Link> Links { get; private set; }
        /// <summary>
        /// Gets or sets the indices of polygon's vertices
        /// </summary>
        public int[] Verts { get; set; }
        /// <summary>
        /// Gets or sets packed data representing neighbor polygons references and flags for each edge
        /// </summary>
        public int[] Neis { get; set; }
        /// <summary>
        /// Gets or sets the number of vertices
        /// </summary>
        public int VertCount { get; set; }
        /// <summary>
        /// Gets or sets the AreaId
        /// </summary>
        public Area Area { get; set; }
        /// <summary>
        /// Gets or sets the polygon type (ground or offmesh)
        /// </summary>
        public PolygonType PolyType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Poly()
        {
            Links = new List<Link>();
        }
    }
}
