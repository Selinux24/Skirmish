
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
        public const int DT_NEI_DIR_MASK = 0xf8;
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

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"X: {X}; Y: {Y}; Z: {Z}; Nei: {Nei}";
        }
    }
}
