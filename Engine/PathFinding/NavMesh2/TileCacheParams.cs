using SharpDX;

namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Tile cache parameters
    /// </summary>
    public struct TileCacheParams
    {
        public Vector3 Origin;
        public float CellSize;
        public float CellHeight;
        public int Width;
        public int Height;
        public float WalkableHeight;
        public float WalkableRadius;
        public float WalkableClimb;
        public float MaxSimplificationError;
        public int MaxTiles;
        public int MaxObstacles;
    }
}
