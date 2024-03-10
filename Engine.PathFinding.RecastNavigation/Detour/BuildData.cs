using Engine.PathFinding.RecastNavigation.Recast;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Navigation mesh build data
    /// </summary>
    struct BuildData
    {
        /// <summary>
        /// Height field
        /// </summary>
        public Heightfield Heightfield { get; set; }
    }
}
