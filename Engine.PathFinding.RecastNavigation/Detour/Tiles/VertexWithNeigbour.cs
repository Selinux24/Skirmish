
namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Vertex with neigbour
    /// </summary>
    public struct VertexWithNeigbour
    {
        /// <summary>
        /// Stored direction mask
        /// </summary>
        const int DT_NEI_DIR_MASK = 0xf8;
        /// <summary>
        /// Stored portal mask
        /// </summary>
        public const int DT_NEI_PORTAL_MASK = 0xff;

        /// <summary>
        /// X
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// Y
        /// </summary>
        public int Y { get; set; }
        /// <summary>
        /// Z
        /// </summary>
        public int Z { get; set; }
        /// <summary>
        /// Neighbour
        /// </summary>
        public int Nei { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public VertexWithNeigbour(VertexWithNeigbour v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            Nei = v.Nei;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public VertexWithNeigbour(int x, int y, int z, int nei)
        {
            X = x;
            Y = y;
            Z = z;
            Nei = nei;
        }

        /// <summary>
        /// Calculates the portal direction removing the status to the fourth component.
        /// </summary>
        public readonly int CalculatePortalFlag()
        {
            int flag = Edge.DT_PORTAL_FLAG;
            if (Nei != DT_NEI_PORTAL_MASK && Nei >= DT_NEI_DIR_MASK)
            {
                flag = Nei - DT_NEI_DIR_MASK;
            }

            return flag;
        }
        /// <summary>
        /// Reads a portal region value from the specified connection value, if any
        /// </summary>
        /// <param name="con">Connection value</param>
        /// <param name="dir">Direction</param>
        /// <param name="regionId">Resulting region Id</param>
        /// <returns>Returns true if the connection value has no connections</returns>
        public static bool ReadPortalRegion(int con, int dir, out int regionId)
        {
            regionId = LayerMonotoneRegion.NULL_ID;

            int conDir = Edge.GetVertexDirection(con);
            int portal = con >> 4;

            if (IsPortalAtDirection(conDir, dir))
            {
                return false;
            }

            // No connection, return portal or hard edge.
            if (IsPortalAtDirection(portal, dir))
            {
                regionId = DT_NEI_DIR_MASK + dir;
            }

            return true;
        }
        /// <summary>
        /// Gets the portal count from the specified connection
        /// </summary>
        /// <param name="con">Connection</param>
        public static int GetPortalCount(int con)
        {
            int portalCount = 0;
            for (int dir = 0; dir < 4; ++dir)
            {
                if (IsPortalAtDirection(con, dir))
                {
                    portalCount++;
                }
            }

            return portalCount;
        }
        /// <summary>
        /// Gets whether the especified connection has a portal at the speficied direction
        /// </summary>
        /// <param name="con">Connection</param>
        /// <param name="dir">Direction</param>
        private static bool IsPortalAtDirection(int con, int dir)
        {
            int mask = 1 << dir;

            return (con & mask) != 0;
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"X: {X}; Y: {Y}; Z: {Z}; Nei: {Nei}";
        }
    }
}
