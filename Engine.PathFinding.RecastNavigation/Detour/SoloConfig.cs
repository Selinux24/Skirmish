using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Build solo configuration
    /// </summary>
    public class SoloConfig : Config
    {
        /// <summary>
        /// Gets the agent configuration for "solo" navigation mesh build
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="generationBounds">Generation bounds</param>
        /// <returns>Returns the new configuration</returns>
        public static SoloConfig GetConfig(BuildSettings settings, Agent agent, BoundingBox generationBounds)
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

            BuildSettings.CalcGridSize(generationBounds, settings.CellSize, out int width, out int height);
            int borderSize = walkableRadius + 3;
            int tileSize = 0;

            // Generation params.
            var cfg = new SoloConfig()
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
                BoundingBox = generationBounds,
                BorderSize = borderSize,
                TileSize = tileSize,
                Width = width,
                Height = height,

                FilterLedgeSpans = settings.FilterLedgeSpans,
                FilterLowHangingObstacles = settings.FilterLowHangingObstacles,
                FilterWalkableLowHeightSpans = settings.FilterWalkableLowHeightSpans,
                PartitionType = settings.PartitionType,

                EnableDebugInfo = settings.EnableDebugInfo,
            };

            return cfg;
        }

        /// <summary>
        /// Gets the navigation mesh parameters for "solo" creation
        /// </summary>
        /// <param name="generationBounds">Generation bounds</param>
        /// <param name="polyCount">Maximum polygon count</param>
        /// <returns>Returns the navigation mesh parameters</returns>
        public static NavMeshParams GetNavMeshParams(BoundingBox generationBounds, int polyCount)
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
    }
}
