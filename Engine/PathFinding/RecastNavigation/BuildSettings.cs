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
                return new BuildSettings();
            }
        }

        /// <summary>
        /// Cell size
        /// </summary>
        public float CellSize = 0.3f;
        /// <summary>
        /// Cell height
        /// </summary>
        public float CellHeight = 0.2f;
        /// <summary>
        /// Edge maximum length
        /// </summary>
        public float EdgeMaxLength = 12.0f;
        /// <summary>
        /// Edge maximum error
        /// </summary>
        public float EdgeMaxError = 1.3f;
        /// <summary>
        /// Detail sample distance
        /// </summary>
        public float DetailSampleDist = 6.0f;
        /// <summary>
        /// Detail sample maximum error
        /// </summary>
        public float DetailSampleMaxError = 1.0f;
        /// <summary>
        /// Region minimum size
        /// </summary>
        public float RegionMinSize = 8;
        /// <summary>
        /// Region merge size
        /// </summary>
        public float RegionMergeSize = 20;
        /// <summary>
        /// Vertices per polygon
        /// </summary>
        public int VertsPerPoly = 6;
        /// <summary>
        /// Partition type
        /// </summary>
        public SamplePartitionTypeEnum PartitionType = SamplePartitionTypeEnum.Watershed;
        /// <summary>
        /// Agents list
        /// </summary>
        public Agent[] Agents = new Agent[] { new Agent() };

        /// <summary>
        /// Navigation mesh building mode
        /// </summary>
        public BuildModesEnum BuildMode = BuildModesEnum.Tiled;
        /// <summary>
        /// Tile size (if tiled mode)
        /// </summary>
        public float TileSize = 32;
        /// <summary>
        /// Maximum number of nodes
        /// </summary>
        public int MaxNodes = 2048;

        /// <summary>
        /// Filter low hanging obstacles when generation
        /// </summary>
        public bool FilterLowHangingObstacles = true;
        /// <summary>
        /// Filter ledge spans when generation
        /// </summary>
        public bool FilterLedgeSpans = true;
        /// <summary>
        /// Filter walkable low hight spans when generation
        /// </summary>
        public bool FilterWalkableLowHeightSpans = true;

        /// <summary>
        /// Navigation mesh bounds
        /// </summary>
        [NonSerialized]
        public BoundingBox? NavmeshBounds = null;
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
