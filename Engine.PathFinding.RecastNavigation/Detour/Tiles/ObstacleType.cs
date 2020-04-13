
namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    public enum ObstacleType
    {
        /// <summary>
        /// DT_OBSTACLE_CYLINDER
        /// </summary>
        Cylinder,
        /// <summary>
        /// DT_OBSTACLE_BOX. AABB
        /// </summary>
        Box,
        /// <summary>
        /// DT_OBSTACLE_ORIENTED_BOX. OBB
        /// </summary>
        OrientedBox,
    }
}
