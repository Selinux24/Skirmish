using System;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// Each polygon is a collection of vertices. It is the basic unit of the PolyMesh
    /// </summary>
    class PolyMeshPolygon
    {
        /// <summary>
        /// Gets the indices for the vertices.
        /// </summary>
        /// <value>The vertices.</value>
        public int[] Vertices { get; private set; }
        /// <summary>
        /// Gets the neighbor edges.
        /// </summary>
        /// <value>The neighbor edges.</value>
        public int[] NeighborEdges { get; private set; }
        /// <summary>
        /// Gets or sets the area id
        /// </summary>
        public Area Area { get; set; }
        /// <summary>
        /// Gets the the number of vertex.
        /// </summary>
        /// <value>The vertex count.</value>
        public int VertexCount
        {
            get
            {
                for (int i = 0; i < Vertices.Length; i++)
                {
                    if (Vertices[i] == PolyMesh.NullId)
                    {
                        return i;
                    }
                }

                return Vertices.Length;
            }
        }
        /// <summary>
        /// Gets or sets the region identifier.
        /// </summary>
        /// <value>The region identifier.</value>
        public RegionId RegionId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon" /> class.
        /// </summary>
        /// <param name="numVertsPerPoly">The number of vertices per polygon.</param>
        /// <param name="area">The AreaId</param>
        /// <param name="regionId">The RegionId</param>
        public PolyMeshPolygon(int numVertsPerPoly, Area area, RegionId regionId)
        {
            this.Vertices = new int[numVertsPerPoly];
            this.NeighborEdges = new int[numVertsPerPoly];
            this.Area = area;
            this.RegionId = regionId;

            for (int i = 0; i < numVertsPerPoly; i++)
            {
                this.Vertices[i] = PolyMesh.NullId;
                this.NeighborEdges[i] = PolyMesh.NullId;
            }
        }

        /// <summary>
        /// Determine if the vertex is in polygon.
        /// </summary>
        /// <returns><c>true</c>, if vertex was containsed, <c>false</c> otherwise.</returns>
        /// <param name="vertex">The Vertex.</param>
        public bool ContainsVertex(int vertex)
        {
            //iterate through all the vertices
            for (int i = 0; i < this.Vertices.Length; i++)
            {
                //find the vertex, return false if at end of defined polygon.
                int v = this.Vertices[i];
                if (v == vertex)
                {
                    return true;
                }
                else if (v == PolyMesh.NullId)
                {
                    return false;
                }
            }

            return false;
        }
        /// <summary>
        /// Merges another polygon with this one.
        /// </summary>
        /// <param name="other">The other polygon to merge into this one.</param>
        /// <param name="startEdge">This starting edge for this polygon.</param>
        /// <param name="otherStartEdge">The starting edge for the other polygon.</param>
        /// <param name="temp">A temporary vertex buffer. Must be at least <c>numVertsPerPoly</c> long.</param>
        public void MergeWith(PolyMeshPolygon other, int startEdge, int otherStartEdge, int[] temp)
        {
            if (temp.Length < this.Vertices.Length)
            {
                throw new ArgumentException(string.Format("Buffer not large enough. Must be at least numVertsPerPoly ({0})", this.Vertices.Length), "temp");
            }

            int thisCount = this.VertexCount;
            int otherCount = other.VertexCount;

            int n = 0;

            for (int t = 0; t < temp.Length; t++)
            {
                temp[t] = PolyMesh.NullId;
            }

            //add self, starting at best edge
            for (int i = 0; i < thisCount - 1; i++)
            {
                temp[n++] = this.Vertices[(startEdge + 1 + i) % thisCount];
            }

            //add other polygon
            for (int i = 0; i < otherCount - 1; i++)
            {
                temp[n++] = other.Vertices[(otherStartEdge + 1 + i) % otherCount];
            }

            //save merged data back
            Array.Copy(temp, this.Vertices, this.Vertices.Length);
        }
    }
}
