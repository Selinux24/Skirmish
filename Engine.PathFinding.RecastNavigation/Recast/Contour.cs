using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Represents a simple, non-overlapping contour in field space.
    /// </summary>
    public class Contour
    {
        /// <summary>
        /// Simplified contour vertex and connection data. [Size: 4 * #nverts]
        /// </summary>
        public Int4[] Vertices { get; set; }
        /// <summary>
        /// The number of vertices in the simplified contour. 
        /// </summary>
        public int NVertices { get; set; }
        /// <summary>
        /// Raw contour vertex and connection data. [Size: 4 * #nrverts]
        /// </summary>
        public Int4[] RawVertices { get; set; }
        /// <summary>
        /// The number of vertices in the raw contour. 
        /// </summary>
        public int NRawVertices { get; set; }
        /// <summary>
        /// The region id of the contour.
        /// </summary>
        public int RegionId { get; set; }
        /// <summary>
        /// The area id of the contour.
        /// </summary>
        public AreaTypes Area { get; set; }

        /// <summary>
        /// Merges a contour into another, and clears it
        /// </summary>
        /// <param name="ca">Merged contour</param>
        /// <param name="cb">Cleared contour</param>
        /// <param name="ia">Merged merge index</param>
        /// <param name="ib">Cleared merge index</param>
        public static void Merge(Contour ca, Contour cb, int ia, int ib)
        {
            int maxVerts = ca.NVertices + cb.NVertices + 2;
            Int4[] verts = new Int4[maxVerts];

            int nv = 0;

            // Copy contour A.
            for (int i = 0; i <= ca.NVertices; ++i)
            {
                verts[nv++] = ca.Vertices[((ia + i) % ca.NVertices)];
            }

            // Copy contour B
            for (int i = 0; i <= cb.NVertices; ++i)
            {
                verts[nv++] = cb.Vertices[((ib + i) % cb.NVertices)];
            }

            ca.Vertices = verts;
            ca.NVertices = nv;

            cb.Vertices = null;
            cb.NVertices = 0;
        }

        /// <summary>
        /// Finds the left most vertex in the contour
        /// </summary>
        /// <param name="minx">Resulting minimum x value</param>
        /// <param name="minz">Resulting minimum z value</param>
        /// <param name="leftmost">Resulting left most index</param>
        public void FindLeftMostVertex(out int minx, out int minz, out int leftmost)
        {
            minx = Vertices[0].X;
            minz = Vertices[0].Z;
            leftmost = 0;
            for (int i = 1; i < NVertices; i++)
            {
                int x = Vertices[i].X;
                int z = Vertices[i].Z;
                if (x < minx || (x == minx && z < minz))
                {
                    minx = x;
                    minz = z;
                    leftmost = i;
                }
            }
        }
        /// <summary>
        /// Calculates the area of the polygon's contour in the xz plane
        /// </summary>
        public int CalcAreaOfPolygon2D()
        {
            int area = 0;

            for (int i = 0, j = NVertices - 1; i < NVertices; j = i++)
            {
                var vi = Vertices[i];
                var vj = Vertices[j];
                area += vi.X * vj.Z - vj.X * vi.Z;
            }

            return (area + 1) / 2;
        }
        /// <summary>
        /// Triangulates the polygon's contour
        /// </summary>
        /// <param name="maxVertsPerCont">Maximum vertices per contour</param>
        /// <param name="indices">Resulting polygon indices</param>
        /// <param name="tris">Resulting polygon triangles</param>
        /// <returns>Returns the number of triangles</returns>
        public int Triangulate(int maxVertsPerCont, out int[] indices, out Int3[] tris)
        {
            indices = new int[maxVertsPerCont];

            // Triangulate contour
            for (int j = 0; j < NVertices; ++j)
            {
                indices[j] = j;
            }

            int ntris = TriangulationHelper.Triangulate(Vertices, ref indices, out tris);
            if (ntris <= 0)
            {
                // Bad triangulation, should not happen.
                Logger.WriteWarning(nameof(Contour), $"rcBuildPolyMesh: Bad triangulation Contour {RegionId}.");
                ntris = -ntris;
            }

            return ntris;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Region Id: {RegionId}; Area: {Area}; Simplified Verts: {NVertices}; Raw Verts: {NRawVertices};";
        }
    };
}
