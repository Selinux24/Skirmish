
namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Vertex with neigbour
    /// </summary>
    public struct VertexWithNeigbour
    {
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
    }
}
