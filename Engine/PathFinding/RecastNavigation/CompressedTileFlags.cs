
namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Flags for addTile
    /// </summary>
    public enum CompressedTileFlags
    {
        /// <summary>
        /// Navmesh owns the tile memory and should free it.
        /// </summary>
        DT_COMPRESSEDTILE_FREE_DATA = 0x01,					
    }
}
