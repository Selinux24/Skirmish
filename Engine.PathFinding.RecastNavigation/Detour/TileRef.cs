
namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Tile reference
    /// </summary>
    public struct TileRef
    {
        /// <summary>
        /// Returns the null tile
        /// </summary>
        public static TileRef Null
        {
            get
            {
                return new TileRef();
            }
        }

        /// <summary>
        /// Tile reference
        /// </summary>
        public int Ref { get; set; }
        /// <summary>
        /// Tile data
        /// </summary>
        public MeshTile Tile { get; set; }
        /// <summary>
        /// Polygon data
        /// </summary>
        public Poly Poly { get; set; }
        /// <summary>
        /// Node data
        /// </summary>
        public Node Node { get; set; }
    }
}
