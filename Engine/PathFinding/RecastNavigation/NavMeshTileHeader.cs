using System;

namespace Engine.PathFinding.RecastNavigation
{
    [Serializable]
    public class NavMeshTileHeader
    {
        public MeshData tile;
        public int dataSize;
    }
}
