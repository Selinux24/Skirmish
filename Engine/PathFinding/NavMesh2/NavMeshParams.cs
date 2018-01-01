using SharpDX;

namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Navigation mesh parameters
    /// </summary>
    public struct NavMeshParams
    {
        public Vector3 Origin;
        public float TileWidth;
        public float TileHeight;
        public int MaxTiles;
        public int MaxPolys;
    }
}
