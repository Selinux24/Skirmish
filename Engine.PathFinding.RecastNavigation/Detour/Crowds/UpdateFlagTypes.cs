
namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Crowd agent update flags.
    /// </summary>
    public enum UpdateFlagTypes
    {
        DT_CROWD_ANTICIPATE_TURNS = 1,
        DT_CROWD_OBSTACLE_AVOIDANCE = 2,
        DT_CROWD_SEPARATION = 4,
        DT_CROWD_OPTIMIZE_VIS = 8,
        DT_CROWD_OPTIMIZE_TOPO = 16,
    }
}
