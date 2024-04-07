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
        /// Width
        /// </summary>
        public int TileWidth { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        public int TileHeight { get; set; }
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
        /// Build all tiles from the beginning
        /// </summary>
        public bool BuildAllTiles { get; set; }

        /// <summary>
        /// Tile X
        /// </summary>
        public int TX { get; set; }
        /// <summary>
        /// Tile Y
        /// </summary>
        public int TY { get; set; }
        /// <summary>
        /// Tile bounds
        /// </summary>
        public BoundingBox TileBounds { get; set; }

        /// <summary>
        /// Gets the agent configuration for "tiled" navigation mesh build
        /// </summary>
        /// <param name="settings">Build settings</param>
        /// <param name="agent">Agent</param>
        /// <param name="tileBounds">Tile bounds</param>
        /// <returns>Returns the new configuration</returns>
        public static TilesConfig GetTilesConfig(BuildSettings settings, Agent agent, BoundingBox generationBounds)
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

            CalcGridSize(generationBounds, settings.CellSize, out int gridWidth, out int gridHeight);
            int tileWidth = (gridWidth + tileSize - 1) / tileSize;
            int tileHeight = (gridHeight + tileSize - 1) / tileSize;

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
                Width = width,
                Height = height,

                FilterLedgeSpans = settings.FilterLedgeSpans,
                FilterLowHangingObstacles = settings.FilterLowHangingObstacles,
                FilterWalkableLowHeightSpans = settings.FilterWalkableLowHeightSpans,
                PartitionType = settings.PartitionType,
                
                BuildAllTiles = settings.BuildAllTiles,
                TX = -1,
                TY = -1,
                TileBounds = default,
                TileWidth = tileWidth,
                TileHeight = tileHeight,
                TileSize = tileSize,

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

            CalcGridSize(generationBounds, settings.CellSize, out int gridWidth, out int gridHeight);
            int tileWidth = (gridWidth + tileSize - 1) / tileSize;
            int tileHeight = (gridHeight + tileSize - 1) / tileSize;

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
                Width = width,
                Height = height,

                FilterLedgeSpans = settings.FilterLedgeSpans,
                FilterLowHangingObstacles = settings.FilterLowHangingObstacles,
                FilterWalkableLowHeightSpans = settings.FilterWalkableLowHeightSpans,
                PartitionType = settings.PartitionType,

                BuildAllTiles = settings.BuildAllTiles,
                TX = -1,
                TY = -1,
                TileBounds = default,
                TileWidth = tileWidth,
                TileHeight = tileHeight,
                TileSize = tileSize,

                EnableDebugInfo = settings.EnableDebugInfo,
            };
        }

        /// <summary>
        /// Updates the bounds of the tile
        /// </summary>
        /// <param name="x">X tile coordinate</param>
        /// <param name="y">Y tile coordinate</param>
        public void UpdateTileBounds(int x, int y, bool adjustBorder = false)
        {
            TX = x;
            TY = y;

            TileBounds = GetTileBounds(x, y);

            if (adjustBorder)
            {
                AdjustTileBounds();
            }
        }
        /// <summary>
        /// Gets the specified tile bounding box
        /// </summary>
        /// <param name="x">X tile coordinate</param>
        /// <param name="y">Y tile coordinate</param>
        private BoundingBox GetTileBounds(int x, int y)
        {
            var tbbox = new BoundingBox();

            float tileCellSize = TileCellSize;

            tbbox.Minimum.X = Bounds.Minimum.X + x * tileCellSize;
            tbbox.Minimum.Y = Bounds.Minimum.Y;
            tbbox.Minimum.Z = Bounds.Minimum.Z + y * tileCellSize;

            tbbox.Maximum.X = Bounds.Minimum.X + (x + 1) * tileCellSize;
            tbbox.Maximum.Y = Bounds.Maximum.Y;
            tbbox.Maximum.Z = Bounds.Minimum.Z + (y + 1) * tileCellSize;

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
        private void AdjustTileBounds()
        {
            var tbbox = TileBounds;

            tbbox.Minimum.X -= BorderSize * CellSize;
            tbbox.Minimum.Z -= BorderSize * CellSize;
            tbbox.Maximum.X += BorderSize * CellSize;
            tbbox.Maximum.Z += BorderSize * CellSize;

            TileBounds = tbbox;
        }
    }
}
