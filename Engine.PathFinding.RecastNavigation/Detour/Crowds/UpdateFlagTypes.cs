using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Crowd agent update flags.
    /// </summary>
    [Flags]
    public enum UpdateFlagTypes
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// DT_CROWD_ANTICIPATE_TURNS
        /// </summary>
        AnticipateTurns = 1,
        /// <summary>
        /// DT_CROWD_OBSTACLE_AVOIDANCE
        /// </summary>
        ObstacleAvoidance = 2,
        /// <summary>
        /// DT_CROWD_SEPARATION
        /// </summary>
        Separation = 4,
        /// <summary>
        /// DT_CROWD_OPTIMIZE_VIS
        /// </summary>
        OptimizeVis = 8,
        /// <summary>
        /// DT_CROWD_OPTIMIZE_TOPO
        /// </summary>
        OptimizeTopo = 16,
    }
}
