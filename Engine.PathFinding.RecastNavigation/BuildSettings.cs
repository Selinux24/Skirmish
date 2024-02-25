using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;
    using Engine.PathFinding.RecastNavigation.Detour.Tiles;

    /// <summary>
    /// Navigation mesh generation settings
    /// </summary>
    [Serializable]
    public class BuildSettings : PathFinderSettings
    {
        /// <summary>
        /// This value specifies how many layers (or "floors") each navmesh tile is expected to have.
        /// </summary>
        const int EXPECTED_LAYERS_PER_TILE = 4;

        /// <summary>
        /// Calculates the grid size
        /// </summary>
        /// <param name="bounds">Bounds</param>
        /// <param name="cellSize">Cell size</param>
        /// <param name="width">Resulting width</param>
        /// <param name="height">Resulting height</param>
        private static void CalcGridSize(BoundingBox bounds, float cellSize, out int width, out int height)
        {
            width = (int)((bounds.Maximum.X - bounds.Minimum.X) / cellSize + 0.5f);
            height = (int)((bounds.Maximum.Z - bounds.Minimum.Z) / cellSize + 0.5f);
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
        private static BoundingBox AdjustTileBBox(BoundingBox tileBounds, int borderSize, float cellsize)
        {
            tileBounds.Minimum.X -= borderSize * cellsize;
            tileBounds.Minimum.Z -= borderSize * cellsize;
            tileBounds.Maximum.X += borderSize * cellsize;
            tileBounds.Maximum.Z += borderSize * cellsize;

            return tileBounds;
        }

        /// <summary>
        /// Default settings
        /// </summary>
        public static BuildSettings Default
        {
            get
            {
                return new();
            }
        }

        /// <summary>
        /// Cell size
        /// </summary>
        public float CellSize { get; set; } = 0.3f;
        /// <summary>
        /// Cell height
        /// </summary>
        public float CellHeight { get; set; } = 0.2f;
        /// <summary>
        /// Edge maximum length
        /// </summary>
        public float EdgeMaxLength { get; set; } = 12.0f;
        /// <summary>
        /// Edge maximum error
        /// </summary>
        public float EdgeMaxError { get; set; } = 1.3f;
        /// <summary>
        /// Detail sample distance
        /// </summary>
        public float DetailSampleDist { get; set; } = 6.0f;
        /// <summary>
        /// Detail sample maximum error
        /// </summary>
        public float DetailSampleMaxError { get; set; } = 1.0f;
        /// <summary>
        /// Region minimum size
        /// </summary>
        public float RegionMinSize { get; set; } = 8;
        /// <summary>
        /// Region merge size
        /// </summary>
        public float RegionMergeSize { get; set; } = 20;
        /// <summary>
        /// Vertices per polygon
        /// </summary>
        public int VertsPerPoly { get; set; } = 6;
        /// <summary>
        /// Partition type
        /// </summary>
        public SamplePartitionTypes PartitionType { get; set; } = SamplePartitionTypes.Watershed;
        /// <summary>
        /// Agents list
        /// </summary>
        public Agent[] Agents { get; set; } = new Agent[] { new() };

        /// <summary>
        /// Navigation mesh building mode
        /// </summary>
        public BuildModes BuildMode { get; set; } = BuildModes.Tiled;
        /// <summary>
        /// Tile size (if tiled mode)
        /// </summary>
        public float TileSize { get; set; } = 32;
        /// <summary>
        /// Maximum number of nodes
        /// </summary>
        public int MaxNodes { get; set; } = 2048;
        /// <summary>
        /// Use tile cache
        /// </summary>
        public bool UseTileCache { get; set; } = false;
        /// <summary>
        /// Build all tiles from the beginning
        /// </summary>
        public bool BuildAllTiles { get; set; } = true;
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
        /// Filter low hanging obstacles when generation
        /// </summary>
        public bool FilterLowHangingObstacles { get; set; } = true;
        /// <summary>
        /// Filter ledge spans when generation
        /// </summary>
        public bool FilterLedgeSpans { get; set; } = true;
        /// <summary>
        /// Filter walkable low hight spans when generation
        /// </summary>
        public bool FilterWalkableLowHeightSpans { get; set; } = true;

        /// <summary>
        /// Gets the agent configuration for "solo" navigation mesh build
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="generationBounds">Generation bounds</param>
        /// <returns>Returns the new configuration</returns>
        internal Config GetSoloConfig(Agent agent, BoundingBox generationBounds)
        {
            float walkableSlopeAngle = agent.MaxSlope;
            int walkableHeight = (int)Math.Ceiling(agent.Height / CellHeight);
            int walkableClimb = (int)Math.Floor(agent.MaxClimb / CellHeight);
            int walkableRadius = (int)Math.Ceiling(agent.Radius / CellSize);
            int maxEdgeLen = (int)(EdgeMaxLength / CellSize);
            int minRegionArea = (int)(RegionMinSize * RegionMinSize);
            int mergeRegionArea = (int)(RegionMergeSize * RegionMergeSize);
            float detailSampleDist = DetailSampleDist < 0.9f ? 0 : CellSize * DetailSampleDist;
            float detailSampleMaxError = CellHeight * DetailSampleMaxError;

            CalcGridSize(generationBounds, CellSize, out int width, out int height);
            int borderSize = walkableRadius + 3;
            int tileSize = 0;

            // Generation params.
            var cfg = new Config()
            {
                Agent = agent,

                CellSize = CellSize,
                CellHeight = CellHeight,
                WalkableSlopeAngle = walkableSlopeAngle,
                WalkableHeight = walkableHeight,
                WalkableClimb = walkableClimb,
                WalkableRadius = walkableRadius,
                MaxEdgeLen = maxEdgeLen,
                MaxSimplificationError = EdgeMaxError,
                MinRegionArea = minRegionArea,
                MergeRegionArea = mergeRegionArea,
                MaxVertsPerPoly = VertsPerPoly,
                DetailSampleDist = detailSampleDist,
                DetailSampleMaxError = detailSampleMaxError,
                BoundingBox = generationBounds,
                BorderSize = borderSize,
                TileSize = tileSize,
                Width = width,
                Height = height,

                FilterLedgeSpans = FilterLedgeSpans,
                FilterLowHangingObstacles = FilterLowHangingObstacles,
                FilterWalkableLowHeightSpans = FilterWalkableLowHeightSpans,
                PartitionType = PartitionType,
                UseTileCache = UseTileCache,
                BuildAllTiles = BuildAllTiles,
            };

            return cfg;
        }
        /// <summary>
        /// Gets the agent configuration for "tiled" navigation mesh build
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="tileBounds">Tile bounds</param>
        /// <returns>Returns the new configuration</returns>
        internal Config GetTiledConfig(Agent agent, BoundingBox tileBounds)
        {
            float walkableSlopeAngle = agent.MaxSlope;
            int walkableHeight = (int)Math.Ceiling(agent.Height / CellHeight);
            int walkableClimb = (int)Math.Floor(agent.MaxClimb / CellHeight);
            int walkableRadius = (int)Math.Ceiling(agent.Radius / CellSize);
            int maxEdgeLen = (int)(EdgeMaxLength / CellSize);
            int minRegionArea = (int)(RegionMinSize * RegionMinSize);
            int mergeRegionArea = (int)(RegionMergeSize * RegionMergeSize);
            float detailSampleDist = DetailSampleDist < 0.9f ? 0 : CellSize * DetailSampleDist;
            float detailSampleMaxError = CellHeight * DetailSampleMaxError;

            int borderSize = walkableRadius + 3;
            int tileSize = (int)TileSize;
            int width = tileSize + borderSize * 2;
            int height = tileSize + borderSize * 2;

            var generationBounds = AdjustTileBBox(tileBounds, borderSize, CellSize);

            // Init build configuration from GUI
            var cfg = new Config
            {
                Agent = agent,

                CellSize = CellSize,
                CellHeight = CellHeight,
                WalkableSlopeAngle = walkableSlopeAngle,
                WalkableHeight = walkableHeight,
                WalkableClimb = walkableClimb,
                WalkableRadius = walkableRadius,
                MaxEdgeLen = maxEdgeLen,
                MaxSimplificationError = EdgeMaxError,
                MinRegionArea = minRegionArea,
                MergeRegionArea = mergeRegionArea,
                MaxVertsPerPoly = VertsPerPoly,
                DetailSampleDist = detailSampleDist,
                DetailSampleMaxError = detailSampleMaxError,
                BoundingBox = generationBounds,
                BorderSize = borderSize,
                TileSize = tileSize,
                Width = width,
                Height = height,

                FilterLedgeSpans = FilterLedgeSpans,
                FilterLowHangingObstacles = FilterLowHangingObstacles,
                FilterWalkableLowHeightSpans = FilterWalkableLowHeightSpans,
                PartitionType = PartitionType,
                UseTileCache = UseTileCache,
                BuildAllTiles = BuildAllTiles,
            };

            return cfg;
        }
        /// <summary>
        /// Gets the agent tile cache build configuration
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="generationBounds">Generation bounds</param>
        /// <returns>Returns the new configuration</returns>
        internal Config GetTileCacheConfig(Agent agent, BoundingBox generationBounds)
        {
            float walkableSlopeAngle = agent.MaxSlope;
            var walkableHeight = (int)Math.Ceiling(agent.Height / CellHeight);
            var walkableClimb = (int)Math.Floor(agent.MaxClimb / CellHeight);
            var walkableRadius = (int)Math.Ceiling(agent.Radius / CellSize);
            int maxEdgeLen = (int)(EdgeMaxLength / CellSize);
            int minRegionArea = (int)(RegionMinSize * RegionMinSize);
            int mergeRegionArea = (int)(RegionMergeSize * RegionMergeSize);
            float detailSampleDist = DetailSampleDist < 0.9f ? 0 : CellSize * DetailSampleDist;
            float detailSampleMaxError = CellHeight * DetailSampleMaxError;

            var borderSize = walkableRadius + 3;
            var tileSize = (int)TileSize;
            int width = tileSize + borderSize * 2;
            int height = tileSize + borderSize * 2;

            var cfg = new Config
            {
                Agent = agent,

                CellSize = CellSize,
                CellHeight = CellHeight,
                WalkableSlopeAngle = walkableSlopeAngle,
                WalkableHeight = walkableHeight,
                WalkableClimb = walkableClimb,
                WalkableRadius = walkableRadius,
                MaxEdgeLen = maxEdgeLen,
                MaxSimplificationError = EdgeMaxError,
                MinRegionArea = minRegionArea,
                MergeRegionArea = mergeRegionArea,
                MaxVertsPerPoly = VertsPerPoly,
                DetailSampleDist = detailSampleDist,
                DetailSampleMaxError = detailSampleMaxError,
                BoundingBox = generationBounds,
                BorderSize = borderSize,
                TileSize = tileSize,
                Width = width,
                Height = height,

                FilterLedgeSpans = FilterLedgeSpans,
                FilterLowHangingObstacles = FilterLowHangingObstacles,
                FilterWalkableLowHeightSpans = FilterWalkableLowHeightSpans,
                PartitionType = PartitionType,
                UseTileCache = UseTileCache,
                BuildAllTiles = BuildAllTiles,
            };

            if (UseTileCache)
            {
                CalcGridSize(generationBounds, CellSize, out int gridWidth, out int gridHeight);
                int tileWidth = (gridWidth + tileSize - 1) / tileSize;
                int tileHeight = (gridHeight + tileSize - 1) / tileSize;

                // Tile cache params.
                cfg.TileCacheParams = new TileCacheParams()
                {
                    Origin = generationBounds.Minimum,
                    CellSize = cfg.CellSize,
                    CellHeight = cfg.CellHeight,
                    Width = cfg.TileSize,
                    Height = cfg.TileSize,
                    WalkableHeight = agent.Height,
                    WalkableRadius = agent.Radius,
                    WalkableClimb = agent.MaxClimb,
                    MaxSimplificationError = cfg.MaxSimplificationError,
                    MaxTiles = tileWidth * tileHeight * EXPECTED_LAYERS_PER_TILE,
                    TileWidth = tileWidth,
                    TileHeight = tileHeight,
                    MaxObstacles = 128,
                };
            }

            return cfg;
        }

        /// <summary>
        /// Gets the navigation mesh parameters for "solo" creation
        /// </summary>
        /// <param name="generationBounds">Generation bounds</param>
        /// <param name="polyCount">Maximum polygon count</param>
        /// <returns>Returns the navigation mesh parameters</returns>
        internal static NavMeshParams GetSoloNavMeshParams(BoundingBox generationBounds, int polyCount)
        {
            return new NavMeshParams
            {
                Origin = generationBounds.Minimum,
                TileWidth = generationBounds.Maximum.X - generationBounds.Minimum.X,
                TileHeight = generationBounds.Maximum.Z - generationBounds.Minimum.Z,
                MaxTiles = 1,
                MaxPolys = polyCount,
            };
        }
        /// <summary>
        /// Gets the navigation mesh parameters for "tiled" creation
        /// </summary>
        /// <param name="generationBounds">Generation bounds</param>
        /// <returns>Returns the navigation mesh parameters</returns>
        internal NavMeshParams GetTiledNavMeshParams(BoundingBox generationBounds)
        {
            CalcGridSize(generationBounds, CellSize, out int gridWidth, out int gridHeight);
            int tileSize = (int)TileSize;
            int tileWidth = (gridWidth + tileSize - 1) / tileSize;
            int tileHeight = (gridHeight + tileSize - 1) / tileSize;
            float tileCellSize = TileCellSize;

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

        /// <summary>
        /// Gets the navigation mesh parameters for a tile
        /// </summary>
        /// <param name="generationBounds">Generation bounds</param>
        internal TileParams GetTileParams(BoundingBox generationBounds)
        {
            CalcGridSize(generationBounds, CellSize, out int gridWidth, out int gridHeight);
            int tileSize = (int)TileSize;
            int tileWidth = (gridWidth + tileSize - 1) / tileSize;
            int tileHeight = (gridHeight + tileSize - 1) / tileSize;
            float tileCellSize = TileCellSize;

            return new TileParams
            {
                Width = tileWidth,
                Height = tileHeight,
                CellSize = tileCellSize,
                Bounds = generationBounds,
            };
        }
    }
}
