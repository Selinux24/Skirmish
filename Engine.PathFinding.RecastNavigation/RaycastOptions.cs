using System;

namespace Engine.PathFinding.RecastNavigation
{
    [Flags]
    public enum RaycastOptions
    {
        /// <summary>
        /// Raycast should calculate movement cost along the ray and fill RaycastHit::cost
        /// </summary>
        DT_RAYCAST_USE_COSTS = 0x01,		
    }
}
