using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile cache contour
    /// </summary>
    public struct TileCacheContour
    {
        /// <summary>
        /// Vertex list
        /// </summary>
        public ContourVertex[] Vertices { get; set; }
        /// <summary>
        /// Number of vertices
        /// </summary>
        public int NVertices { get; set; }
        /// <summary>
        /// Region id
        /// </summary>
        public int Reg { get; set; }
        /// <summary>
        /// Area type
        /// </summary>
        public AreaTypes Area { get; set; }

        /// <summary>
        /// Triangulates the polygon's contour
        /// </summary>
        /// <param name="maxVertsPerCont">Maximum vertices per contour</param>
        /// <returns>Returns the polygon indices, polygon triangles and number of triangles</returns>
        public readonly (int[] Indices, Int3[] Tris, int NTris) Triangulate(int maxVertsPerCont)
        {
            int[] indices = new int[maxVertsPerCont];

            // Triangulate contour
            for (int j = 0; j < NVertices; ++j)
            {
                indices[j] = j;
            }

            int ntris = TriangulationHelper.Triangulate(ContourVertex.ToInt3List(Vertices), ref indices, out var tris);
            if (ntris <= 0)
            {
                Logger.WriteWarning(nameof(TileCacheContourSet), $"Polygon contour triangulation error: Reg {Reg}");
                ntris = -ntris;
            }

            return (indices, tris, ntris);
        }
        /// <summary>
        /// Stores the specified vertices in the contour
        /// </summary>
        /// <param name="verts">Vertex list</param>
        /// <param name="nverts">Number of vertices in the list</param>
        /// <param name="tcl">Tile cache layer</param>
        /// <param name="walkableClimb">Walkable climb value</param>
        public void StoreVerts(VertexWithNeigbour[] verts, int nverts, TileCacheLayer tcl, int walkableClimb)
        {
            NVertices = nverts;
            if (NVertices <= 0)
            {
                return;
            }

            Vertices = new ContourVertex[nverts];

            for (int i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                var v = verts[j];
                var vn = verts[i];
                int nei = vn.Nei; // The neighbour reg is stored at segment vertex of a segment. 
                int lh = tcl.GetCornerHeight(v, walkableClimb, out bool shouldRemove);

                // Store portal direction and remove status to the fourth component.
                int flag = VertexFlags.PORTAL_FLAG;
                if (nei != VertexFlags.PORTAL_FLAG && nei >= VertexFlags.DIR_MASK)
                {
                    flag = nei - VertexFlags.DIR_MASK;
                }
                if (shouldRemove)
                {
                    flag |= VertexFlags.BORDER_VERTEX;
                }

                Vertices[j] = new()
                {
                    X = v.X,
                    Y = lh,
                    Z = v.Z,
                    Flag = flag,
                };
            }
        }
    }
}
