
namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Level stack entry
    /// </summary>
    struct LevelStackEntry
    {
        /// <summary>
        /// X coordinate
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// Y coordinate
        /// </summary>
        public int Y { get; set; }
        /// <summary>
        /// Index
        /// </summary>
        public int Index { get; set; }
    }
}
