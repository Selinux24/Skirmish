
namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// AddTile types
    /// </summary>
    public enum CompressedTileFlagTypes
    {
        /// <summary>
        /// DT_COMPRESSEDTILE_EMPTY_DATA
        /// </summary>
        None = 0x00,
        /// <summary>
        /// DT_COMPRESSEDTILE_FREE_DATA. Navmesh owns the tile memory and should free it.
        /// </summary>
        Free = 0x01,					
    }
}
