using System;

namespace Engine.PathFinding.NavMesh2
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
