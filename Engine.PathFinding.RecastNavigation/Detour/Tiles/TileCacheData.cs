using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Maximum number of layers
        /// </summary>
        const int MAX_LAYERS = 32;

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
        public static TileCacheData[] RasterizeTileLayers(int x, int y, InputGeometry geometry, Config cfg)
        {
            var chunkyMesh = geometry.ChunkyMesh;

            // Update tile bounds.
            cfg.UpdateTileBounds(x, y);

            // Create heightfield
            var solid = Heightfield.Build(cfg.Width, cfg.Height, cfg.BoundingBox, cfg.CellSize, cfg.CellHeight);

            var tbmin = new Vector2(cfg.BoundingBox.Minimum.X, cfg.BoundingBox.Minimum.Z);
            var tbmax = new Vector2(cfg.BoundingBox.Maximum.X, cfg.BoundingBox.Maximum.Z);

            var cid = chunkyMesh.GetChunksOverlappingRect(tbmin, tbmax);
            if (!cid.Any())
            {
                return Array.Empty<TileCacheData>(); // empty
            }

            foreach (var id in cid)
            {
                var tris = chunkyMesh.GetTriangles(id);
                if (!solid.Rasterize(tris, cfg.WalkableSlopeAngle, cfg.WalkableClimb))
                {
                    return Array.Empty<TileCacheData>();
                }
            }

            // Once all geometry is rasterized, we do initial pass of filtering to
            // remove unwanted overhangs caused by the conservative rasterization
            // as well as filter spans where the character cannot possibly stand.
            solid.FilterHeightfield(cfg);

            // Compact the heightfield so that it is faster to handle from now on.
            // This will result more cache coherent data as well as the neighbours
            // between walkable cells will be calculated.
            var chf = solid.Build(cfg.WalkableHeight, cfg.WalkableClimb);

            // Erode the walkable area by agent radius.
            chf.ErodeWalkableArea(cfg.WalkableRadius);

            // Mark areas.
            chf.MarkAreas(geometry);

            var lset = HeightfieldLayerSet.Build(chf, cfg.BorderSize, cfg.WalkableHeight);

            // Allocate voxel heightfield where we rasterize our input data to.
            var tiles = new List<TileCacheData>();

            for (int i = 0; i < Math.Min(lset.NLayers, MAX_LAYERS); i++)
            {
                tiles.Add(lset.Layers[i].Create(x, y, i));
            }

            return tiles.ToArray();
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"{Header} {Data}";
        }
    }
}
