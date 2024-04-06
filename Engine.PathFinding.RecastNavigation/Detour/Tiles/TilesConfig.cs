using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Build tiles configuration
    /// </summary>
    public class TilesConfig : Config
    {
        /// <summary>
        /// This value specifies how many layers (or "floors") each navmesh tile is expected to have.
        /// </summary>
        const int EXPECTED_LAYERS_PER_TILE = 4;

        /// <summary>
        /// The width/height size of tile's on the xz-plane. [Limit: >= 0] [Units: vx]
        /// </summary>
        public int TileSize { get; set; }
        /// <summary>
        /// Gets the tile * cell size
        /// </summary>
        public float TileCellSize
        {
            get
            {
                return TileSize * CellSize;
            }
        }
        /// <summary>
        /// Use tile cache
        /// </summary>
        public bool UseTileCache { get; set; }
        /// <summary>
        /// Build all tiles from the beginning
        /// </summary>
        public bool BuildAllTiles { get; set; }
        /// <summary>
        /// Tile cache parameters
        /// </summary>
        public TileCacheParams TileCacheParams { get; set; }

        /// <summary>
        /// Gets the specified tile bounding box
        /// </summary>
        /// <param name="x">X tile coordinate</param>
        /// <param name="y">Y tile coordinate</param>
        /// <param name="geom">Input geometry</param>
        /// <param name="settings">Build settings</param>
        public static BoundingBox GetTileBounds(int x, int y, InputGeometry geom, BuildSettings settings)
        {
            return GetTileBounds(x, y, settings.TileCellSize, settings.Bounds ?? geom.BoundingBox);
        }
        /// <summary>
        /// Gets the specified tile bounding box
        /// </summary>
        /// <param name="x">X tile coordinate</param>
        /// <param name="y">Y tile coordinate</param>
        /// <param name="tileCellSize">Tile cell size</param>
        /// <param name="bbox">Input bounding box</param>
        public static BoundingBox GetTileBounds(int x, int y, float tileCellSize, BoundingBox bbox)
        {
            var tbbox = new BoundingBox();

            tbbox.Minimum.X = bbox.Minimum.X + x * tileCellSize;
            tbbox.Minimum.Y = bbox.Minimum.Y;
            tbbox.Minimum.Z = bbox.Minimum.Z + y * tileCellSize;

            tbbox.Maximum.X = bbox.Minimum.X + (x + 1) * tileCellSize;
            tbbox.Maximum.Y = bbox.Maximum.Y;
            tbbox.Maximum.Z = bbox.Minimum.Z + (y + 1) * tileCellSize;

            return tbbox;
        }
        /// <summary>
        /// Adjust tile bounds using border and cell size to expand
        /// </summary>
        /// <param name="tileBounds">Tile bounds</param>
        /// <param name="borderSize">Border size</param>
        /// <param name="cellsize">Cell size</param>
        /// <returns>Returns the new bounds</returns>
        /// <remarks>
        /// Expand the heighfield bounding box by border size to find the extents of geometry we need to build this tile.
        /// 
        /// This is done in order to make sure that the navmesh tiles connect correctly at the borders,
        /// and the obstacles close to the border work correctly with the dilation process.
        /// No polygons (or contours) will be created on the border area.
        /// 
        /// IMPORTANT!
        /// 
        ///   :''''''''':
        ///   : +-----+ :
        ///   : |     | :
        ///   : |     |<--- tile to build
        ///   : |     | :  
        ///   : +-----+ :<-- geometry needed
        ///   :.........:
        /// 
        /// You should use this bounding box to query your input geometry.
        /// 
        /// For example if you build a navmesh for terrain, and want the navmesh tiles to match the terrain tile size
        /// you will need to pass in data from neighbour terrain tiles too! In a simple case, just pass in all the 8 neighbours,
        /// or use the bounding box below to only pass in a sliver of each of the 8 neighbours.
        /// </remarks>
        public static BoundingBox AdjustTileBounds(BoundingBox tileBounds, int borderSize, float cellsize)
        {
            tileBounds.Minimum.X -= borderSize * cellsize;
            tileBounds.Minimum.Z -= borderSize * cellsize;
            tileBounds.Maximum.X += borderSize * cellsize;
            tileBounds.Maximum.Z += borderSize * cellsize;

            return tileBounds;
        }

        /// <summary>
        /// Gets the navigation mesh parameters for a tile
        /// </summary>
        /// <param name="settings">Build settings</param>
        /// <param name="generationBounds">Generation bounds</param>
        public static TileParams GetTileParams(BuildSettings settings, BoundingBox generationBounds)
        {
            BuildSettings.CalcGridSize(generationBounds, settings.CellSize, out int gridWidth, out int gridHeight);
            int tileSize = (int)settings.TileSize;
            int tileWidth = (gridWidth + tileSize - 1) / tileSize;
            int tileHeight = (gridHeight + tileSize - 1) / tileSize;
            float tileCellSize = settings.TileCellSize;

            return new()
            {
                Width = tileWidth,
                Height = tileHeight,
                TileCellSize = tileCellSize,
                Bounds = generationBounds,
            };
        }

        /// <summary>
        /// Gets the agent configuration for "tiled" navigation mesh build
        /// </summary>
        /// <param name="settings">Build settings</param>
        /// <param name="agent">Agent</param>
        /// <param name="tileBounds">Tile bounds</param>
        /// <returns>Returns the new configuration</returns>
        public static TilesConfig GetTilesConfig(BuildSettings settings, Agent agent, BoundingBox tileBounds)
        {
            float walkableSlopeAngle = agent.MaxSlope;
            int walkableHeight = (int)Math.Ceiling(agent.Height / settings.CellHeight);
            int walkableClimb = (int)Math.Floor(agent.MaxClimb / settings.CellHeight);
            int walkableRadius = (int)Math.Ceiling(agent.Radius / settings.CellSize);
            int maxEdgeLen = (int)(settings.EdgeMaxLength / settings.CellSize);
            int minRegionArea = (int)(settings.RegionMinSize * settings.RegionMinSize);
            int mergeRegionArea = (int)(settings.RegionMergeSize * settings.RegionMergeSize);
            float detailSampleDist = settings.DetailSampleDist < 0.9f ? 0 : settings.CellSize * settings.DetailSampleDist;
            float detailSampleMaxError = settings.CellHeight * settings.DetailSampleMaxError;

            int borderSize = walkableRadius + 3;
            int tileSize = (int)settings.TileSize;
            int width = tileSize + borderSize * 2;
            int height = tileSize + borderSize * 2;

            var generationBounds = AdjustTileBounds(tileBounds, borderSize, settings.CellSize);

            // Init build configuration from GUI
            return new()
            {
                Agent = agent,

                CellSize = settings.CellSize,
                CellHeight = settings.CellHeight,
                WalkableSlopeAngle = walkableSlopeAngle,
                WalkableHeight = walkableHeight,
                WalkableClimb = walkableClimb,
                WalkableRadius = walkableRadius,
                MaxEdgeLen = maxEdgeLen,
                MaxSimplificationError = settings.EdgeMaxError,
                MinRegionArea = minRegionArea,
                MergeRegionArea = mergeRegionArea,
                MaxVertsPerPoly = settings.VertsPerPoly,
                DetailSampleDist = detailSampleDist,
                DetailSampleMaxError = detailSampleMaxError,
                Bounds = generationBounds,
                BorderSize = borderSize,
                TileSize = tileSize,
                Width = width,
                Height = height,

                FilterLedgeSpans = settings.FilterLedgeSpans,
                FilterLowHangingObstacles = settings.FilterLowHangingObstacles,
                FilterWalkableLowHeightSpans = settings.FilterWalkableLowHeightSpans,
                PartitionType = settings.PartitionType,
                UseTileCache = settings.UseTileCache,
                BuildAllTiles = settings.BuildAllTiles,

                EnableDebugInfo = settings.EnableDebugInfo,
            };
        }
        /// <summary>
        /// Gets the agent tile cache build configuration
        /// </summary>
        /// <param name="settings">Build settings</param>
        /// <param name="agent">Agent</param>
        /// <param name="generationBounds">Generation bounds</param>
        /// <returns>Returns the new configuration</returns>
        public static TilesConfig GetTileCacheConfig(BuildSettings settings, Agent agent, BoundingBox generationBounds)
        {
            float walkableSlopeAngle = agent.MaxSlope;
            var walkableHeight = (int)Math.Ceiling(agent.Height / settings.CellHeight);
            var walkableClimb = (int)Math.Floor(agent.MaxClimb / settings.CellHeight);
            var walkableRadius = (int)Math.Ceiling(agent.Radius / settings.CellSize);
            int maxEdgeLen = (int)(settings.EdgeMaxLength / settings.CellSize);
            int minRegionArea = (int)(settings.RegionMinSize * settings.RegionMinSize);
            int mergeRegionArea = (int)(settings.RegionMergeSize * settings.RegionMergeSize);
            float detailSampleDist = settings.DetailSampleDist < 0.9f ? 0 : settings.CellSize * settings.DetailSampleDist;
            float detailSampleMaxError = settings.CellHeight * settings.DetailSampleMaxError;

            var borderSize = walkableRadius + 3;
            var tileSize = (int)settings.TileSize;
            int width = tileSize + borderSize * 2;
            int height = tileSize + borderSize * 2;

            TileCacheParams tileCacheParams = default;
            if (settings.UseTileCache)
            {
                BuildSettings.CalcGridSize(generationBounds, settings.CellSize, out int gridWidth, out int gridHeight);
                int tileWidth = (gridWidth + tileSize - 1) / tileSize;
                int tileHeight = (gridHeight + tileSize - 1) / tileSize;

                // Tile cache params.
                tileCacheParams = new()
                {
                    Origin = generationBounds.Minimum,
                    CellSize = settings.CellSize,
                    CellHeight = settings.CellHeight,
                    Width = tileSize,
                    Height = tileSize,
                    WalkableHeight = agent.Height,
                    WalkableRadius = agent.Radius,
                    WalkableClimb = agent.MaxClimb,
                    MaxSimplificationError = settings.EdgeMaxError,
                    MaxTiles = tileWidth * tileHeight * EXPECTED_LAYERS_PER_TILE,
                    TileWidth = tileWidth,
                    TileHeight = tileHeight,
                    MaxObstacles = 128,

                    EnableDebugInfo = settings.EnableDebugInfo,
                };
            }

            return new()
            {
                Agent = agent,

                CellSize = settings.CellSize,
                CellHeight = settings.CellHeight,
                WalkableSlopeAngle = walkableSlopeAngle,
                WalkableHeight = walkableHeight,
                WalkableClimb = walkableClimb,
                WalkableRadius = walkableRadius,
                MaxEdgeLen = maxEdgeLen,
                MaxSimplificationError = settings.EdgeMaxError,
                MinRegionArea = minRegionArea,
                MergeRegionArea = mergeRegionArea,
                MaxVertsPerPoly = settings.VertsPerPoly,
                DetailSampleDist = detailSampleDist,
                DetailSampleMaxError = detailSampleMaxError,
                Bounds = generationBounds,
                BorderSize = borderSize,
                TileSize = tileSize,
                Width = width,
                Height = height,

                FilterLedgeSpans = settings.FilterLedgeSpans,
                FilterLowHangingObstacles = settings.FilterLowHangingObstacles,
                FilterWalkableLowHeightSpans = settings.FilterWalkableLowHeightSpans,
                PartitionType = settings.PartitionType,
                UseTileCache = settings.UseTileCache,
                BuildAllTiles = settings.BuildAllTiles,
                TileCacheParams = tileCacheParams,

                EnableDebugInfo = settings.EnableDebugInfo,
            };
        }

        /// <summary>
        /// Gets the navigation mesh parameters for "tiled" creation
        /// </summary>
        /// <param name="settings">Build settings</param>
        /// <param name="generationBounds">Generation bounds</param>
        /// <returns>Returns the navigation mesh parameters</returns>
        public static NavMeshParams GetNavMeshParams(BuildSettings settings, BoundingBox generationBounds)
        {
            BuildSettings.CalcGridSize(generationBounds, settings.CellSize, out int gridWidth, out int gridHeight);
            int tileSize = (int)settings.TileSize;
            int tileWidth = (gridWidth + tileSize - 1) / tileSize;
            int tileHeight = (gridHeight + tileSize - 1) / tileSize;
            float tileCellSize = settings.TileCellSize;

            int tileBits = Math.Min((int)Math.Log(Helper.NextPowerOfTwo(tileWidth * tileHeight), 2), 14);
            if (tileBits > 14) tileBits = 14;
            int polyBits = 22 - tileBits;
            int maxTiles = 1 << tileBits;
            int maxPolysPerTile = 1 << polyBits;

            return new NavMeshParams
            {
                Origin = generationBounds.Minimum,
                TileWidth = tileCellSize,
                TileHeight = tileCellSize,
                MaxTiles = maxTiles,
                MaxPolys = maxPolysPerTile,
            };
        }
    }
}
