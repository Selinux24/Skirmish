using System;
using Vortice.Mathematics;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Build tiles configuration
    /// </summary>
    public class TilesConfig : Config
    {
        /// <summary>
        /// Tile width
        /// </summary>
        public int TileWidth { get; set; }
        /// <summary>
        /// Tile height
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
        /// Gets the agent configuration for "tiled" navigation mesh build
        /// </summary>
        /// <param name="settings">Build settings</param>
        /// <param name="agent">Agent</param>
        /// <param name="generationBounds">Tile bounds</param>
        /// <returns>Returns the new configuration</returns>
        public static TilesConfig GetTilesConfig(BuildSettings settings, GraphAgentType agent, BoundingBox generationBounds)
        {
            float walkableSlopeAngle = agent.MaxSlope;
            int walkableHeight = (int)MathF.Ceiling(agent.Height / settings.CellHeight);
            int walkableClimb = (int)MathF.Floor(agent.MaxClimb / settings.CellHeight);
            int walkableRadius = (int)MathF.Ceiling(agent.Radius / settings.CellSize);
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
                Width = width,
                Height = height,

                FilterLedgeSpans = settings.FilterLedgeSpans,
                FilterLowHangingObstacles = settings.FilterLowHangingObstacles,
                FilterWalkableLowHeightSpans = settings.FilterWalkableLowHeightSpans,
                PartitionType = settings.PartitionType,

                TileWidth = tileWidth,
                TileHeight = tileHeight,
                TileSize = tileSize,

                EnableDebugInfo = settings.EnableDebugInfo,
            };
        }

        /// <summary>
        /// Calculates the tile bounds
        /// </summary>
        /// <param name="tx">Tile x</param>
        /// <param name="ty">Tile y</param>
        public BoundingBox CalculateTileBounds(int tx, int ty)
        {
            var bounds = Bounds;
            float tileCellSize = TileCellSize;
            float cellsize = CellSize;
            float borderSize = BorderSize;

            var tileBounds = new BoundingBox();

            tileBounds.Min.X = bounds.Min.X + tx * tileCellSize;
            tileBounds.Min.Y = bounds.Min.Y;
            tileBounds.Min.Z = bounds.Min.Z + ty * tileCellSize;

            tileBounds.Max.X = bounds.Min.X + (tx + 1) * tileCellSize;
            tileBounds.Max.Y = bounds.Max.Y;
            tileBounds.Max.Z = bounds.Min.Z + (ty + 1) * tileCellSize;

            tileBounds.Min.X -= borderSize * cellsize;
            tileBounds.Min.Z -= borderSize * cellsize;
            tileBounds.Max.X += borderSize * cellsize;
            tileBounds.Max.Z += borderSize * cellsize;

            return tileBounds;
        }
    }
}
