using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    using Engine.PathFinding.RecastNavigation.Recast;

    /// <summary>
    /// Tile cache data
    /// </summary>
    [Serializable]
    public struct TileCacheData
    {
        /// <summary>
        /// Header
        /// </summary>
        public TileCacheLayerHeader Header { get; set; }
        /// <summary>
        /// Data
        /// </summary>
        public TileCacheLayerData Data { get; set; }

        /// <summary>
        /// Rasterize tile layers
        /// </summary>
        /// <param name="x">X tile coordinate</param>
        /// <param name="y">Y tile coordinate</param>
        /// <param name="geometry">Input geometry</param>
        /// <param name="cfg">Configuration</param>
        public static TileCacheData[] RasterizeTileLayers(int x, int y, InputGeometry geometry, TilesConfig cfg)
        {
            var chunkyMesh = geometry.ChunkyMesh;

            // Update tile bounds.
            var tileBounds = TilesConfig.GetTileBounds(x, y, cfg.TileCellSize, cfg.Bounds);

            // Adjust tile bounds
            tileBounds = TilesConfig.AdjustTileBounds(tileBounds, cfg.BorderSize, cfg.CellSize);

            // Create heightfield
            var solid = Heightfield.Build(cfg, tileBounds);

            var cid = chunkyMesh.GetChunksOverlappingRect(cfg.Bounds);
            if (cid.Length == 0)
            {
                return []; // empty
            }

            foreach (var id in cid)
            {
                var tris = chunkyMesh.GetTriangles(id);
                solid.Rasterize(tris, cfg.WalkableSlopeAngle, cfg.WalkableClimb);
            }

            // Once all geometry is rasterized, we do initial pass of filtering to
            // remove unwanted overhangs caused by the conservative rasterization
            // as well as filter spans where the character cannot possibly stand.
            solid.FilterHeightfield(cfg);

            // Compact the heightfield so that it is faster to handle from now on.
            // This will result more cache coherent data as well as the neighbours
            // between walkable cells will be calculated.
            var chf = CompactHeightfield.Build(solid, cfg.WalkableHeight, cfg.WalkableClimb);

            // Erode the walkable area by agent radius.
            chf.ErodeWalkableArea(cfg.WalkableRadius);

            // Mark areas.
            chf.MarkAreas(geometry);

            var lset = HeightfieldLayerSet.Build(chf, cfg.BorderSize, cfg.WalkableHeight);

            // Allocate voxel heightfield where we rasterize our input data to.
            var tiles = lset.AllocateTiles(x, y);

            return [.. tiles];
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"{Header} {Data}";
        }
    }
}
