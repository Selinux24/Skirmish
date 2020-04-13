using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Flags for addTile
    /// </summary>
    [Flags]
    public enum CompressedTileFlagTypes
    {
        /// <summary>
        /// DT_COMPRESSEDTILE_FREE_DATA. Navmesh owns the tile memory and should free it.
        /// </summary>
        FreeData = 0x01,
    }
}
