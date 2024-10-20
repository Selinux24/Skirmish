﻿using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    using Engine.PathFinding.RecastNavigation.Detour.Tiles;

    /// <summary>
    /// Navigation mesh file
    /// </summary>
    [Serializable]
    public struct NavMeshFile
    {
        /// <summary>
        /// Creates a navigation mesh file from a navigation mesh
        /// </summary>
        /// <param name="navmesh">Navigation mesh</param>
        /// <returns>Returns the navigation mesh file</returns>
        public static NavMeshFile FromNavmesh(NavMesh navmesh)
        {
            var file = new NavMeshFile
            {
                NavMeshParams = navmesh.GetParams(),
                NavMeshData = [],

                HasTileCache = navmesh.TileCache != null,
                TileCacheParams = navmesh.TileCache?.GetParams() ?? default,
                TileCacheData = []
            };

            // Store navmesh tiles.
            for (int i = 0; i < navmesh.MaxTiles; ++i)
            {
                var tile = navmesh.Tiles[i];
                if (tile != null)
                {
                    file.NavMeshData.Add(tile.Data);
                }
            }

            if (navmesh.TileCache != null)
            {
                // Store cache tiles.
                var maxTileCount = navmesh.TileCache.GetMaxTileCount();

                for (int i = 0; i < maxTileCount; ++i)
                {
                    var tile = navmesh.TileCache.GetTile(i);
                    if (tile != null)
                    {
                        file.TileCacheData.Add(new()
                        {
                            Header = tile.Header,
                            Data = tile.Data
                        });
                    }
                }
            }

            return file;
        }
        /// <summary>
        /// Creates a navigation mesh from a navigation mesh file
        /// </summary>
        /// <param name="file">Navigation mesh file</param>
        /// <returns>Returns the navigation mesh</returns>
        public static NavMesh FromNavmeshFile(NavMeshFile file)
        {
            var navmesh = new NavMesh(file.NavMeshParams);

            foreach (var tile in file.NavMeshData)
            {
                if (tile == null || !tile.Header.IsValid())
                {
                    continue;
                }

                navmesh.AddTile(tile);
            }

            if (file.HasTileCache)
            {
                navmesh.CreateTileCache(null, file.TileCacheParams);

                foreach (var tile in file.TileCacheData)
                {
                    if (!tile.Header.IsValid())
                    {
                        continue;
                    }

                    navmesh.TileCache.AddTile(tile, CompressedTileFlagTypes.Free);
                }
            }

            return navmesh;
        }

        /// <summary>
        /// Navigation mesh parameters
        /// </summary>
        public NavMeshParams NavMeshParams { get; set; }
        /// <summary>
        /// Mesh data
        /// </summary>
        public List<MeshData> NavMeshData { get; set; }

        /// <summary>
        /// Has tile cache
        /// </summary>
        public bool HasTileCache { get; set; }
        /// <summary>
        /// Tile cache parameters
        /// </summary>
        public TileCacheParams TileCacheParams { get; set; }
        /// <summary>
        /// Tile cache data
        /// </summary>
        public List<TileCacheData> TileCacheData { get; set; }
    }
}
