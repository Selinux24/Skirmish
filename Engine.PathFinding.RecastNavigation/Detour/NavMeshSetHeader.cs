using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Navigation mesh set header
    /// </summary>
    [Serializable]
    public class NavMeshSetHeader
    {
        /// <summary>
        /// Magic mask
        /// </summary>
        public const int NAVMESHSET_MAGIC = 'M' << 24 | 'S' << 16 | 'E' << 8 | 'T';
        /// <summary>
        /// Version mask
        /// </summary>
        public const int NAVMESHSET_VERSION = 1;

        /// <summary>
        /// Magic
        /// </summary>
        public int Magic { get; set; }
        /// <summary>
        /// Version
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// Navigation mesh parameters
        /// </summary>
        public NavMeshParams Param { get; set; }
    }
}
