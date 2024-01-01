using Engine.PathFinding.RecastNavigation.Recast;
using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile cache contour
    /// </summary>
    public struct TileCacheContour
    {
        /// <summary>
        /// Number of vertices
        /// </summary>
        public int NVertices { get; set; }
        /// <summary>
        /// Vertex list
        /// </summary>
        public ContourVertex[] Vertices { get; set; }
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
        /// <param name="indices">Resulting polygon indices</param>
        /// <param name="tris">Resulting polygon triangles</param>
        /// <returns>Returns the number of triangles</returns>
        public readonly int Triangulate(int maxVertsPerCont, out int[] indices, out Int3[] tris)
        {
            indices = new int[maxVertsPerCont];

            // Triangulate contour
            for (int j = 0; j < NVertices; ++j)
            {
                indices[j] = j;
            }

            int ntris = TriangulationHelper.Triangulate(ContourVertex.ToInt3List(Vertices), ref indices, out tris);
            if (ntris <= 0)
            {
                Logger.WriteWarning(nameof(TileCacheContourSet), $"Polygon contour triangulation error: Reg {Reg}");
                ntris = -ntris;
            }

            return ntris;
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
            if (NVertices > 0)
            {
                Vertices = new ContourVertex[nverts];

                for (int i = 0, j = nverts - 1; i < nverts; j = i++)
                {
                    var v = verts[j];
                    var vn = verts[i];
                    int nei = vn.Nei; // The neighbour reg is stored at segment vertex of a segment. 
                    bool shouldRemove = false;
                    int lh = tcl.GetCornerHeight(v.X, v.Y, v.Z, walkableClimb, ref shouldRemove);

                    var dst = new ContourVertex()
                    {
                        X = v.X,
                        Y = lh,
                        Z = v.Z,
                        Flag = 0x0f,
                    };

                    // Store portal direction and remove status to the fourth component.
                    if (nei != 0xff && nei >= 0xf8)
                    {
                        dst.Flag = nei - 0xf8;
                    }
                    if (shouldRemove)
                    {
                        dst.Flag |= TileCacheContourSet.BORDER_VERTEX;
                    }

                    Vertices[j] = dst;
                }
            }
        }
    }
}
