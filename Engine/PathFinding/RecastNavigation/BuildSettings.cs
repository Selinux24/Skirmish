
using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Navigation mesh generation settings
    /// </summary>
    public class BuildSettings : PathFinderSettings
    {
        /// <summary>
        /// Default settings
        /// </summary>
        public static BuildSettings Default
        {
            get
            {
                return new BuildSettings()
                {
                    CellSize = 0.3f,
                    CellHeight = 0.2f,
                    EdgeMaxLength = 12.0f,
                    EdgeMaxError = 1.3f,
                    DetailSampleDist = 6.0f,
                    DetailSampleMaxError = 1.0f,
                    RegionMinSize = 8,
                    RegionMergeSize = 20,
                    VertsPerPoly = 6,
                    PartitionType = SamplePartitionTypeEnum.Watershed,

                    Agents = new Agent[]
                    {
                        Agent.Default,
                    },

                    BuildMode = BuildModesEnum.Solo,
                    TileSize = 32,
                    MaxNodes = 2048,

                    FilterLowHangingObstacles = true,
                    FilterLedgeSpans = true,
                    FilterWalkableLowHeightSpans = true,

                    NavmeshBounds = null,
                };
            }
        }

        public float CellSize;
        public float CellHeight;
        public float EdgeMaxLength;
        public float EdgeMaxError;
        public float DetailSampleDist;
        public float DetailSampleMaxError;
        public float RegionMinSize;
        public float RegionMergeSize;
        public int VertsPerPoly;
        public SamplePartitionTypeEnum PartitionType;
        public Agent[] Agents;

        public BuildModesEnum BuildMode = BuildModesEnum.Solo;
        public float TileSize;
        public int MaxNodes;

        public bool FilterLowHangingObstacles;
        public bool FilterLedgeSpans;
        public bool FilterWalkableLowHeightSpans;

        public BoundingBox? NavmeshBounds;
    }

    public enum BuildModesEnum
    {
        Solo,
        Tiled,
        TempObstacles,
    }
}
