using System;
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
            NavMeshFile file = new NavMeshFile
            {
                NavMeshParams = navmesh.GetParams(),
                NavMeshData = new List<MeshData>(),

                HasTileCache = navmesh.TileCache != null,
                TileCacheParams = navmesh.TileCache != null ? navmesh.TileCache.GetParams() : new TileCacheParams(),
                TileCacheData = new List<TileCacheData>()
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
                var tileCount = navmesh.TileCache.GetTileCount();

                for (int i = 0; i < tileCount; ++i)
                {
                    var tile = navmesh.TileCache.GetTile(i);
                    if (tile != null)
                    {
                        file.TileCacheData.Add(new TileCacheData
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
            NavMesh navmesh = new NavMesh(file.NavMeshParams);

            foreach (var tile in file.NavMeshData)
            {
                if (tile == null || tile.Header.Magic != DetourUtils.DT_NAVMESH_MAGIC)
                {
                    continue;
                }

                navmesh.AddTile(tile, TileFlagTypes.DT_TILE_FREE_DATA, 0);
            }

            if (file.HasTileCache)
            {
                var tmproc = new TileCacheMeshProcess(null);

                navmesh.TileCache = new TileCache(navmesh, tmproc, file.TileCacheParams);

                foreach (var tile in file.TileCacheData)
                {
                    if (tile.Header.Magic != DetourTileCache.DT_TILECACHE_MAGIC)
                    {
                        continue;
                    }

                    navmesh.TileCache.AddTile(tile, CompressedTileFlagTypes.DT_COMPRESSEDTILE_FREE_DATA);
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
