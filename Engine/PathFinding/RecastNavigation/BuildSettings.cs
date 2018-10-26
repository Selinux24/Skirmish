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
    }
}
