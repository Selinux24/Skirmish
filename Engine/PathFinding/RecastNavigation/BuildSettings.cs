using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Navigation mesh generation settings
    /// </summary>
    [Serializable]
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

        /// <summary>
        /// Cell size
        /// </summary>
        public float CellSize;
        /// <summary>
        /// Cell height
        /// </summary>
        public float CellHeight;
        /// <summary>
        /// Edge maximum length
        /// </summary>
        public float EdgeMaxLength;
        /// <summary>
        /// Edge maximum error
        /// </summary>
        public float EdgeMaxError;
        /// <summary>
        /// Detail sample distance
        /// </summary>
        public float DetailSampleDist;
        /// <summary>
        /// Detail sample maximum error
        /// </summary>
        public float DetailSampleMaxError;
        /// <summary>
        /// Region minimum size
        /// </summary>
        public float RegionMinSize;
        /// <summary>
        /// Region merge size
        /// </summary>
        public float RegionMergeSize;
        /// <summary>
        /// Vertices per polygon
        /// </summary>
        public int VertsPerPoly;
        /// <summary>
        /// Partition type
        /// </summary>
        public SamplePartitionTypeEnum PartitionType;
        /// <summary>
        /// Agents list
        /// </summary>
        public Agent[] Agents;

        /// <summary>
        /// Navigation mesh building mode
        /// </summary>
        public BuildModesEnum BuildMode = BuildModesEnum.Solo;
        /// <summary>
        /// Tile size (if tiled mode)
        /// </summary>
        public float TileSize;
        /// <summary>
        /// Maximum number of nodes
        /// </summary>
        public int MaxNodes;

        /// <summary>
        /// Filter low hanging obstacles when generation
        /// </summary>
        public bool FilterLowHangingObstacles;
        /// <summary>
        /// Filter ledge spans when generation
        /// </summary>
        public bool FilterLedgeSpans;
        /// <summary>
        /// Filter walkable low hight spans when generation
        /// </summary>
        public bool FilterWalkableLowHeightSpans;

        /// <summary>
        /// Navigation mesh bounds
        /// </summary>
        [NonSerialized]
        public BoundingBox? NavmeshBounds;
        /// <summary>
        /// Serialization property
        /// </summary>
        internal float[] InternalNavmeshBounds
        {
            get
            {
                if (NavmeshBounds.HasValue)
                {
                    return new float[]
                    {
                        NavmeshBounds.Value.Minimum.X,
                        NavmeshBounds.Value.Minimum.Y,
                        NavmeshBounds.Value.Minimum.Z,

                        NavmeshBounds.Value.Maximum.X,
                        NavmeshBounds.Value.Maximum.Y,
                        NavmeshBounds.Value.Maximum.Z,
                    };
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null && value.Length == 6)
                {
                    NavmeshBounds = new BoundingBox(
                        new Vector3(value[0], value[1], value[2]),
                        new Vector3(value[3], value[4], value[5]));
                }
                else
                {
                    NavmeshBounds = null;
                }
            }
        }
    }
}
