using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Tile flags used for various functions and fields.
    /// </summary>
    [Flags]
    public enum TileFlagTypes
    {
        /// <summary>
        /// DT_TILE_FREE_DATA. The navigation mesh owns the tile memory and is responsible for freeing it.
        /// </summary>
        FreeData = 0x01,
    }
}
