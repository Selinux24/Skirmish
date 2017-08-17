using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// Uses the PolyMesh polygon data for pathfinding
    /// </summary>
    class Poly
    {
        /// <summary>
        /// Gets or sets the indices of polygon's vertices
        /// </summary>
        public int[] Vertices { get; set; }
        /// <summary>
        /// Gets or sets packed data representing neighbor polygons references and flags for each edge
        /// </summary>
        public int[] NeighborEdges { get; set; }
        /// <summary>
        /// Gets or sets the AreaId
        /// </summary>
        public Area Area { get; set; }
        /// <summary>
        /// Gets or sets the number of vertices
        /// </summary>
        public int VertexCount { get; set; }
        /// <summary>
        /// Gets or sets the polygon type (ground or offmesh)
        /// </summary>
        public PolyType PolyType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<Link> Links { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Poly()
        {
            this.Links = new List<Link>();
        }


        public int[] GetEdgeIndices(Link link)
        {
            int v0 = this.Vertices[link.Edge];
            int v1 = this.Vertices[(link.Edge + 1) % this.VertexCount];

            return new[] { v0, v1 };
        }
    }
}
