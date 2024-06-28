using Engine.PathFinding.RecastNavigation.Recast;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
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
        /// <param name="tiledCfg">Configuration</param>
        public static TileCacheData[] RasterizeTileLayers(int x, int y, InputGeometry geometry, TilesConfig tiledCfg)
        {
            var chunkyMesh = geometry.ChunkyMesh;

            // Update tile bounds.
            var tileBounds = tiledCfg.CalculateTileBounds(x, y);

            // Create heightfield
            var solid = Heightfield.Build(tiledCfg, tileBounds);

            var cid = chunkyMesh.GetChunksOverlappingRect(tileBounds);
            if (cid.Length == 0)
            {
                return []; // empty
            }

            foreach (var id in cid)
            {
                var tris = chunkyMesh.GetTriangles(id);
                solid.Rasterize(tris, tiledCfg.WalkableSlopeAngle, tiledCfg.WalkableClimb);
            }

            // Once all geometry is rasterized, we do initial pass of filtering to
            // remove unwanted overhangs caused by the conservative rasterization
            // as well as filter spans where the character cannot possibly stand.
            solid.FilterHeightfield(tiledCfg);

            // Compact the heightfield so that it is faster to handle from now on.
            // This will result more cache coherent data as well as the neighbours
            // between walkable cells will be calculated.
            var chf = CompactHeightfield.Build(solid, tiledCfg.WalkableHeight, tiledCfg.WalkableClimb);

            // Erode the walkable area by agent radius.
            chf.ErodeWalkableArea(tiledCfg.WalkableRadius);

            // Mark areas.
            chf.MarkAreas(geometry);

            var lset = HeightfieldLayerSet.Build(chf, tiledCfg.BorderSize, tiledCfg.WalkableHeight);

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
