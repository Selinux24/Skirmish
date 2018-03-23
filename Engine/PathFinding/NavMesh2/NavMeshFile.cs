using System;

namespace Engine.PathFinding.NavMesh2
{
    [Serializable]
    public class NavMeshFile
    {
        public NavMeshSetHeader header;
        public NavMeshTileHeader[] tileHeaders;
    }
}
