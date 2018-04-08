using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    public class TileCacheObstacle
    {
        public CompressedTile[] touched = new CompressedTile[DetourTileCache.DT_MAX_TOUCHED_TILES];
        public CompressedTile[] pending = new CompressedTile[DetourTileCache.DT_MAX_TOUCHED_TILES];
        public int Salt;
        public ObstacleType type;
        public ObstacleState state;
        public int ntouched;
        public int npending;
        public TileCacheObstacle Next;

        public ObstacleCylinder cylinder;
        public ObstacleBox box;
        public ObstacleOrientedBox orientedBox;
    }

    public struct ObstacleCylinder
    {
        public Vector3 pos;
        public float radius;
        public float height;
    }

    public struct ObstacleBox
    {
        public Vector3 bmin;
        public Vector3 bmax;
    }

    public struct ObstacleOrientedBox
    {
        public Vector3 center;
        public Vector3 halfExtents;
        /// <summary>
        /// { cos(0.5f*angle)*sin(-0.5f*angle); cos(0.5f*angle)*cos(0.5f*angle) - 0.5 }
        /// </summary>
        public Vector2 rotAux;
    }
}
