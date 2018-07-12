using System;

namespace Engine.PathFinding.RecastNavigation
{
    [Serializable]
    public class NavMeshSetHeader
    {
        public const int NAVMESHSET_MAGIC = 'M' << 24 | 'S' << 16 | 'E' << 8 | 'T';
        public const int NAVMESHSET_VERSION = 1;

        public int magic;
        public int version;
        public NavMeshParams param;
    }
}
