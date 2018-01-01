
namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Navigation mesh generation settings
    /// </summary>
    public class Settings : PathFinderSettings
    {
        public float CellSize;
        public float CellHeight;
        public float TileSize;
        public float EdgeMaxLength;
        public float EdgeMaxError;
        public float DetailSampleDist;
        public float DetailSampleMaxError;
        public float RegionMinSize;
        public float RegionMergeSize;
        public int VertsPerPoly;
        public int MaxNodes;
        public bool FilterLowHangingObstacles;
        public bool FilterLedgeSpans;
        public bool FilterWalkableLowHeightSpans;
        public Agent[] Agents;
    }
}
