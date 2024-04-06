﻿using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Flags for addTile
    /// </summary>
    [Flags]
    public enum CompressedTileFlagTypes
    {
        /// <summary>
        /// Empty
        /// </summary>
        DT_COMPRESSEDTILE_EMPTY_DATA = 0x00,
        /// <summary>
        /// Navmesh owns the tile memory and should free it.
        /// </summary>
        DT_COMPRESSEDTILE_FREE_DATA = 0x01,					
    }
}
