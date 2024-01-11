
namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Vertex flags
    /// </summary>
    public static class VertexFlags
    {
        /// <summary>
        /// Border vertex flag.
        /// If a region ID has this bit set, then the associated element lies on
        /// a tile border. If a contour vertex's region ID has this bit set, the 
        /// vertex will later be removed in order to match the segments and vertices 
        /// at tile boundaries.
        /// (Used during the build process.)
        /// </summary>
        public const int BORDER_VERTEX = 0x80;
        /// <summary>
        /// Stored direction mask
        /// </summary>
        public const int DIR_MASK = 0xf8;
        /// <summary>
        /// Portal flag mask
        /// </summary>
        public const int PORTAL_FLAG = 0x0f;
        /// <summary>
        /// A flag that indicates that an entity links to an external entity.
        /// (E.g. A polygon edge is a portal that links to another polygon.)
        /// </summary>
        public const int DT_EXT_LINK = 0x8000;

        /// <summary>
        /// Gets whether the flag has the border flag
        /// </summary>
        public static bool IsBorder(int flag)
        {
            return (flag & BORDER_VERTEX) != 0;
        }
        /// <summary>
        /// Gets the flag vertex direction
        /// </summary>
        /// <param name="flag">Vertex</param>
        public static int GetVertexDirection(int flag)
        {
            return flag & PORTAL_FLAG;
        }
        /// <summary>
        /// Gets whether the flag has stored a direction or not
        /// </summary>
        public static bool HasDirection(int flag)
        {
            var dir = GetVertexDirection(flag);

            return dir != PORTAL_FLAG;
        }
        /// <summary>
        /// Gets whether the flag is external link or not
        /// </summary>
        public static bool IsExternalLink(int flag)
        {
            return (flag & DT_EXT_LINK) != 0;
        }
    }
}
