using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Represents a simple, non-overlapping contour in field space.
    /// </summary>
    public class Contour
    {
        /// <summary>
        /// Portal flag mask
        /// </summary>
        public const int RC_PORTAL_FLAG = 0x0f;
        /// <summary>
        /// Border vertex flag.
        /// If a region ID has this bit set, then the associated element lies on
        /// a tile border. If a contour vertex's region ID has this bit set, the 
        /// vertex will later be removed in order to match the segments and vertices 
        /// at tile boundaries.
        /// (Used during the build process.)
        /// </summary>
        public const int RC_BORDER_VERTEX = 0x10000;
        /// <summary>
        /// Area border flag.
        /// If a region ID has this bit set, then the associated element lies on
        /// the border of an area.
        /// (Used during the region and contour build process.)
        /// </summary>
        public const int RC_AREA_BORDER = 0x20000;
        /// <summary>
        /// Applied to the region id field of contour vertices in order to extract the region id.
        /// The region id field of a vertex may have several flags applied to it.  So the
        /// fields value can't be used directly.
        /// </summary>
        public const int RC_CONTOUR_REG_MASK = 0xffff;

        /// <summary>
        /// Gets whether the vertex has the <see cref="RC_BORDER_VERTEX"/> flag
        /// </summary>
        public static bool IsBorderVertex(int flag)
        {
            return (flag & RC_BORDER_VERTEX) != 0;
        }
        /// <summary>
        /// Gets whether the vertex has the <see cref="RC_AREA_BORDER"/> flag
        /// </summary>
        public static bool IsAreaBorder(int flag)
        {
            return (flag & RC_AREA_BORDER) != 0;
        }
        /// <summary>
        /// Gets whether the vertex has the <see cref="RC_CONTOUR_REG_MASK"/> flag
        /// </summary>
        public static bool IsRegion(int flag)
        {
            return (flag & RC_CONTOUR_REG_MASK) != 0;
        }

        /// <summary>
        /// Simplified contour vertex and connection data. [Size: 4 * #nverts]
        /// </summary>
        public ContourVertex[] Vertices { get; set; }
        /// <summary>
        /// The number of vertices in the simplified contour. 
        /// </summary>
        public int NVertices { get; set; }
        /// <summary>
        /// Raw contour vertex and connection data. [Size: 4 * #nrverts]
        /// </summary>
        public ContourVertex[] RawVertices { get; set; }
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
            ContourVertex[] verts = new ContourVertex[maxVerts];

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
        /// <returns>Returns the triangulated indices and vertices list</returns>
        public Int3[] Triangulate()
        {
            var verts = ContourVertex.ToInt3List(Vertices);

            // Triangulate contour
            var (triRes, tris) = TriangulationHelper.Triangulate(verts);
            if (!triRes || tris.Length <= 0)
            {
                // Bad triangulation, should not happen.
                Logger.WriteWarning(nameof(Contour), $"rcBuildPolyMesh: Bad triangulation Contour {RegionId}.");
            }

            return tris;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Region Id: {RegionId}; Area: {Area}; Simplified Verts: {NVertices}; Raw Verts: {NRawVertices};";
        }
    };
}
