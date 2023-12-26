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
        public int NVerts { get; set; }
        /// <summary>
        /// Vertex list
        /// </summary>
        public ContourVertex[] Verts { get; set; }
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
            for (int j = 0; j < NVerts; ++j)
            {
                indices[j] = j;
            }

            int ntris = TriangulationHelper.Triangulate(ContourVertex.ToInt3List(Verts), ref indices, out tris);
            if (ntris <= 0)
            {
                Logger.WriteWarning(nameof(TileCacheContourSet), $"Polygon contour triangulation error: Reg {Reg}");
                ntris = -ntris;
            }

            return ntris;
        }
    }
}
