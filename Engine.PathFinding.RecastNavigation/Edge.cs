using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Edge
    /// </summary>
    public struct Edge
    {
        /// <summary>
        /// Portal flag mask
        /// </summary>
        public const int DT_PORTAL_FLAG = 0x0f;
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
        /// A flag that indicates that an entity links to an external entity.
        /// (E.g. A polygon edge is a portal that links to another polygon.)
        /// </summary>
        public const int DT_EXT_LINK = 0x8000;

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
        /// Gets whether the flag has stored a direction or not
        /// </summary>
        public static bool HasDirection(int flag)
        {
            var dir = GetVertexDirection(flag);

            return dir != DT_PORTAL_FLAG;
        }
        /// <summary>
        /// Gets whether the vertex has the <see cref="RC_BORDER_VERTEX"/> flag
        /// </summary>
        public static bool IsBorderVertex(int flag)
        {
            return (flag & DT_BORDER_VERTEX) != 0;
        }
        /// <summary>
        /// Gets whether the flag is external link or not
        /// </summary>
        public static bool IsExternalLink(int flag)
        {
            return (flag & DT_EXT_LINK) != 0;
        }
        /// <summary>
        /// Gets the flag vertex direction
        /// </summary>
        /// <param name="flag">Vertex</param>
        public static int GetVertexDirection(int flag)
        {
            return flag & DT_PORTAL_FLAG;
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
        /// Polygon
        /// </summary>
        public int[] Poly { get; set; }
        /// <summary>
        /// Vertices
        /// </summary>
        public int[] Vert { get; set; }
        /// <summary>
        /// Polygon edges
        /// </summary>
        public int[] PolyEdge { get; set; }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Vert {Vert?.Join(",")}; PolyEdge {PolyEdge?.Join(",")}; Poly {Poly?.Join(",")}";
        }
    };
}
