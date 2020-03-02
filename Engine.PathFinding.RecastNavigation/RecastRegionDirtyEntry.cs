
namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Struct to keep track of entries in the region table that have been changed.
    /// </summary>
    public struct RecastRegionDirtyEntry
    {
        public int Index { get; set; }
        public int Region { get; set; }
        public int Distance2 { get; set; }
    }
}
