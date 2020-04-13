using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    [Flags]
    public enum RaycastOptions
    {
        /// <summary>
        /// DT_RAYCAST_USE_COSTS. Raycast should calculate movement cost along the ray and fill RaycastHit::cost
        /// </summary>
        UseCosts = 0x01,
    }
}
