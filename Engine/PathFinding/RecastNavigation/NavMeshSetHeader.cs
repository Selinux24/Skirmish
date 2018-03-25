using System;

namespace Engine.PathFinding.RecastNavigation
{
    [Serializable]
    public class NavMeshSetHeader
    {
        public int magic;
        public int version;
        public int numTiles;
        public NavMeshParams param;
    }
}
