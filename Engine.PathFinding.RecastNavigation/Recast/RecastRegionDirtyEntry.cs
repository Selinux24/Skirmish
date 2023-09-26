
namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Struct to keep track of entries in the region table that have been changed.
    /// </summary>
    struct RecastRegionDirtyEntry
    {
        /// <summary>
        /// Index
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Region
        /// </summary>
        public int Region { get; set; }
        /// <summary>
        /// Distance
        /// </summary>
        public int Distance2 { get; set; }
    }
}
