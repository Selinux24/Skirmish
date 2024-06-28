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
        public static SoloConfig GetConfig(BuildSettings settings, GraphAgentType agent, BoundingBox generationBounds)
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

            CalcGridSize(generationBounds, settings.CellSize, out int width, out int height);
            int borderSize = walkableRadius + 3;

            // Generation params.
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

                EnableDebugInfo = settings.EnableDebugInfo,
            };
        }
    }
}
