using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile cache contour
    /// </summary>
    public struct TileCacheContour
    {
        /// <summary>
        /// Border vertex flag.
        /// If a region ID has this bit set, then the associated element lies on
        /// a tile border. If a contour vertex's region ID has this bit set, the 
        /// vertex will later be removed in order to match the segments and vertices 
        /// at tile boundaries.
        /// (Used during the build process.)
        /// </summary>
        public const int DT_BORDER_VERTEX = 0x80;
        /// <summary>
        /// Stored direction mask
        /// </summary>
        public const int DT_DIR_MASK = 0xf8;
        /// <summary>
        /// Portal flag mask
        /// </summary>
        public const int DT_PORTAL_FLAG = 0x0f;
        /// <summary>
        /// A flag that indicates that an entity links to an external entity.
        /// (E.g. A polygon edge is a portal that links to another polygon.)
        /// </summary>
        public const int DT_EXT_LINK = 0x8000;

        /// <summary>
        /// Gets the flag vertex direction
        /// </summary>
        /// <param name="flag">Vertex</param>
        public static int GetVertexDirection(int flag)
        {
            return flag & DT_PORTAL_FLAG;
        }
        /// <summary>
        /// Gets whether the flag has stored a direction or not
        /// </summary>
        public static bool HasDirection(int flag)
        {
            var dir = GetVertexDirection(flag);

            return dir != DT_PORTAL_FLAG;
        }
        /// <summary>
        /// Gets whether the flag is external link or not
        /// </summary>
        public static bool IsExternalLink(int flag)
        {
            return (flag & DT_EXT_LINK) != 0;
        }

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
        /// Returns the portal value, if any
        /// </summary>
        /// <param name="va">First vertex</param>
        /// <param name="vb">Second vertex</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="portalValue">Returns the portal value</param>
        /// <returns>Returns true if found</returns>
        public static bool IsPortal(Int3 va, Int3 vb, int w, int h, out int portalValue)
        {
            if (va.X == 0 && vb.X == 0)
            {
                portalValue = DT_EXT_LINK;

                return true;
            }
            else if (va.Z == h && vb.Z == h)
            {
                portalValue = DT_EXT_LINK | 1;

                return true;
            }
            else if (va.X == w && vb.X == w)
            {
                portalValue = DT_EXT_LINK | 2;

                return true;
            }
            else if (va.Z == 0 && vb.Z == 0)
            {
                portalValue = DT_EXT_LINK | 3;

                return true;
            }

            portalValue = -1;

            return false;
        }
        /// <summary>
        /// Calculates the vertex portal flag direction value
        /// </summary>
        /// <param name="v">Vertex</param>
        /// <returns>Returns the vertex portal flag direction value</returns>
        public static int CalculateVertexPortalFlag(int v)
        {
            var dir = v & DT_PORTAL_FLAG;

            if (dir == DT_PORTAL_FLAG) // Border
            {
                return 0;
            }
            else if (dir == 0) // Portal x-
            {
                return DT_EXT_LINK | 4;
            }
            else if (dir == 1) // Portal z+
            {
                return DT_EXT_LINK | 2;
            }
            else if (dir == 2) // Portal x+
            {
                return DT_EXT_LINK;
            }
            else if (dir == 3) // Portal z-
            {
                return DT_EXT_LINK | 6;
            }

            return v;
        }
        /// <summary>
        /// Gets the point to side index
        /// </summary>
        /// <param name="side">Side</param>
        public static int PointToSide(int side)
        {
            return DT_EXT_LINK | side;
        }

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
                int flag = DT_PORTAL_FLAG;
                if (nei != DT_PORTAL_FLAG && nei >= DT_DIR_MASK)
                {
                    flag = nei - DT_DIR_MASK;
                }
                if (shouldRemove)
                {
                    flag |= DT_BORDER_VERTEX;
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
