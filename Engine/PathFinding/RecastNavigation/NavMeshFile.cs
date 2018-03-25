using System;

namespace Engine.PathFinding.RecastNavigation
{
    [Serializable]
    public class NavMeshFile
    {
        public NavMeshSetHeader header;
        public NavMeshTileHeader[] tileHeaders;
    }
}
