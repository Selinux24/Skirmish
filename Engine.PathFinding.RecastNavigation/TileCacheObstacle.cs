using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    public class TileCacheObstacle
    {
        public CompressedTile[] Touched { get; set; } = new CompressedTile[DetourTileCache.DT_MAX_TOUCHED_TILES];
        public CompressedTile[] Pending { get; set; } = new CompressedTile[DetourTileCache.DT_MAX_TOUCHED_TILES];
        public int Salt { get; set; }
        public ObstacleType Type { get; set; }
        public ObstacleState State { get; set; }
        public int NTouched { get; set; }
        public int NPending { get; set; }
        public int Next { get; set; }

        public ObstacleCylinder Cylinder { get; set; }
        public ObstacleBox Box { get; set; }
        public ObstacleOrientedBox OrientedBox { get; set; }
    }

    public struct ObstacleCylinder
    {
        public Vector3 Pos { get; set; }
        public float Radius { get; set; }
        public float Height { get; set; }
    }

    public struct ObstacleBox
    {
        public Vector3 BMin { get; set; }
        public Vector3 BMax { get; set; }
    }

    public struct ObstacleOrientedBox
    {
        public Vector3 Center { get; set; }
        public Vector3 HalfExtents { get; set; }
        /// <summary>
        /// { cos(0.5f*angle)*sin(-0.5f*angle); cos(0.5f*angle)*cos(0.5f*angle) - 0.5 }
        /// </summary>
        public Vector2 RotAux { get; set; }
    }
}
