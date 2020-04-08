using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour.Tiles;
    using Engine.PathFinding.RecastNavigation.Recast;

    /// <summary>
    /// Navigation mesh generation settings
    /// </summary>
    [Serializable]
    public class BuildSettings : PathFinderSettings
    {
        /// <summary>
        /// This value specifies how many layers (or "floors") each navmesh tile is expected to have.
        /// </summary>
        public const int EXPECTED_LAYERS_PER_TILE = 4;

        /// <summary>
        /// Default settings
        /// </summary>
        public static BuildSettings Default
        {
            get
            {
                return new BuildSettings();
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
        public Agent[] Agents { get; set; } = new Agent[] { new Agent() };

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


        internal Config GetSoloConfig(Agent agent, BoundingBox generationBounds)
        {
            float walkableSlopeAngle = agent.MaxSlope;
            int walkableHeight = (int)Math.Ceiling(agent.Height / this.CellHeight);
            int walkableClimb = (int)Math.Floor(agent.MaxClimb / this.CellHeight);
            int walkableRadius = (int)Math.Ceiling(agent.Radius / this.CellSize);
            int maxEdgeLen = (int)(this.EdgeMaxLength / this.CellSize);
            int minRegionArea = (int)(this.RegionMinSize * this.RegionMinSize);
            int mergeRegionArea = (int)(this.RegionMergeSize * this.RegionMergeSize);
            float detailSampleDist = this.DetailSampleDist < 0.9f ? 0 : this.CellSize * this.DetailSampleDist;
            float detailSampleMaxError = this.CellHeight * this.DetailSampleMaxError;

            RecastUtils.CalcGridSize(generationBounds, this.CellSize, out int width, out int height);
            int borderSize = walkableRadius + 3;
            int tileSize = 0;

            // Generation params.
            var cfg = new Config()
            {
                Agent = agent,

                CellSize = this.CellSize,
                CellHeight = this.CellHeight,
                WalkableSlopeAngle = walkableSlopeAngle,
                WalkableHeight = walkableHeight,
                WalkableClimb = walkableClimb,
                WalkableRadius = walkableRadius,
                MaxEdgeLen = maxEdgeLen,
                MaxSimplificationError = this.EdgeMaxError,
                MinRegionArea = minRegionArea,
                MergeRegionArea = mergeRegionArea,
                MaxVertsPerPoly = this.VertsPerPoly,
                DetailSampleDist = detailSampleDist,
                DetailSampleMaxError = detailSampleMaxError,
                BoundingBox = generationBounds,
                BorderSize = borderSize,
                TileSize = tileSize,
                Width = width,
                Height = height,

                FilterLedgeSpans = this.FilterLedgeSpans,
                FilterLowHangingObstacles = this.FilterLowHangingObstacles,
                FilterWalkableLowHeightSpans = this.FilterWalkableLowHeightSpans,
                PartitionType = this.PartitionType,
                UseTileCache = this.UseTileCache,
                BuildAllTiles = this.BuildAllTiles,
            };

            return cfg;
        }
        internal Config GetTiledConfig(Agent agent, BoundingBox tileBounds)
        {
            float walkableSlopeAngle = agent.MaxSlope;
            int walkableHeight = (int)Math.Ceiling(agent.Height / this.CellHeight);
            int walkableClimb = (int)Math.Floor(agent.MaxClimb / this.CellHeight);
            int walkableRadius = (int)Math.Ceiling(agent.Radius / this.CellSize);
            int maxEdgeLen = (int)(this.EdgeMaxLength / this.CellSize);
            int minRegionArea = (int)(this.RegionMinSize * this.RegionMinSize);
            int mergeRegionArea = (int)(this.RegionMergeSize * this.RegionMergeSize);
            float detailSampleDist = this.DetailSampleDist < 0.9f ? 0 : this.CellSize * this.DetailSampleDist;
            float detailSampleMaxError = this.CellHeight * this.DetailSampleMaxError;

            int borderSize = walkableRadius + 3;
            int tileSize = (int)this.TileSize;
            int width = tileSize + borderSize * 2;
            int height = tileSize + borderSize * 2;

            var generationBounds = AdjustTileBBox(tileBounds, borderSize, this.CellSize);

            // Init build configuration from GUI
            Config cfg = new Config
            {
                Agent = agent,

                CellSize = this.CellSize,
                CellHeight = this.CellHeight,
                WalkableSlopeAngle = walkableSlopeAngle,
                WalkableHeight = walkableHeight,
                WalkableClimb = walkableClimb,
                WalkableRadius = walkableRadius,
                MaxEdgeLen = maxEdgeLen,
                MaxSimplificationError = this.EdgeMaxError,
                MinRegionArea = minRegionArea,
                MergeRegionArea = mergeRegionArea,
                MaxVertsPerPoly = this.VertsPerPoly,
                DetailSampleDist = detailSampleDist,
                DetailSampleMaxError = detailSampleMaxError,
                BoundingBox = generationBounds,
                BorderSize = borderSize,
                TileSize = tileSize,
                Width = width,
                Height = height,

                FilterLedgeSpans = this.FilterLedgeSpans,
                FilterLowHangingObstacles = this.FilterLowHangingObstacles,
                FilterWalkableLowHeightSpans = this.FilterWalkableLowHeightSpans,
                PartitionType = this.PartitionType,
                UseTileCache = this.UseTileCache,
                BuildAllTiles = this.BuildAllTiles,
            };

            return cfg;
        }
        private static BoundingBox AdjustTileBBox(BoundingBox tileBounds, int borderSize, float cellsize)
        {
            // Expand the heighfield bounding box by border size to find the extents of geometry we need to build this tile.
            //
            // This is done in order to make sure that the navmesh tiles connect correctly at the borders,
            // and the obstacles close to the border work correctly with the dilation process.
            // No polygons (or contours) will be created on the border area.
            //
            // IMPORTANT!
            //
            //   :''''''''':
            //   : +-----+ :
            //   : |     | :
            //   : |     |<--- tile to build
            //   : |     | :  
            //   : +-----+ :<-- geometry needed
            //   :.........:
            //
            // You should use this bounding box to query your input geometry.
            //
            // For example if you build a navmesh for terrain, and want the navmesh tiles to match the terrain tile size
            // you will need to pass in data from neighbour terrain tiles too! In a simple case, just pass in all the 8 neighbours,
            // or use the bounding box below to only pass in a sliver of each of the 8 neighbours.

            tileBounds.Minimum.X -= borderSize * cellsize;
            tileBounds.Minimum.Z -= borderSize * cellsize;
            tileBounds.Maximum.X += borderSize * cellsize;
            tileBounds.Maximum.Z += borderSize * cellsize;

            return tileBounds;
        }
        internal Config GetTileCacheConfig(Agent agent, BoundingBox generationBounds)
        {
            float walkableSlopeAngle = agent.MaxSlope;
            var walkableHeight = (int)Math.Ceiling(agent.Height / this.CellHeight);
            var walkableClimb = (int)Math.Floor(agent.MaxClimb / this.CellHeight);
            var walkableRadius = (int)Math.Ceiling(agent.Radius / this.CellSize);
            int maxEdgeLen = (int)(this.EdgeMaxLength / this.CellSize);
            int minRegionArea = (int)(this.RegionMinSize * this.RegionMinSize);
            int mergeRegionArea = (int)(this.RegionMergeSize * this.RegionMergeSize);
            float detailSampleDist = this.DetailSampleDist < 0.9f ? 0 : this.CellSize * this.DetailSampleDist;
            float detailSampleMaxError = this.CellHeight * this.DetailSampleMaxError;

            var borderSize = walkableRadius + 3;
            var tileSize = (int)this.TileSize;
            int width = tileSize + borderSize * 2;
            int height = tileSize + borderSize * 2;

            Config cfg = new Config
            {
                Agent = agent,

                CellSize = this.CellSize,
                CellHeight = this.CellHeight,
                WalkableSlopeAngle = walkableSlopeAngle,
                WalkableHeight = walkableHeight,
                WalkableClimb = walkableClimb,
                WalkableRadius = walkableRadius,
                MaxEdgeLen = maxEdgeLen,
                MaxSimplificationError = this.EdgeMaxError,
                MinRegionArea = minRegionArea,
                MergeRegionArea = mergeRegionArea,
                MaxVertsPerPoly = this.VertsPerPoly,
                DetailSampleDist = detailSampleDist,
                DetailSampleMaxError = detailSampleMaxError,
                BoundingBox = generationBounds,
                BorderSize = borderSize,
                TileSize = tileSize,
                Width = width,
                Height = height,

                FilterLedgeSpans = this.FilterLedgeSpans,
                FilterLowHangingObstacles = this.FilterLowHangingObstacles,
                FilterWalkableLowHeightSpans = this.FilterWalkableLowHeightSpans,
                PartitionType = this.PartitionType,
                UseTileCache = this.UseTileCache,
                BuildAllTiles = this.BuildAllTiles,
            };

            if (this.UseTileCache)
            {
                RecastUtils.CalcGridSize(generationBounds, CellSize, out int gridWidth, out int gridHeight);
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
                    WalkableHeight = cfg.Height,
                    WalkableRadius = cfg.WalkableRadius,
                    WalkableClimb = cfg.WalkableClimb,
                    MaxSimplificationError = cfg.MaxSimplificationError,
                    MaxTiles = tileWidth * tileHeight * EXPECTED_LAYERS_PER_TILE,
                    MaxObstacles = 128,
                };
            }

            return cfg;
        }
    }
}
