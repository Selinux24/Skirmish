using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    [Flags]
    public enum FindPathOptions
    {
        /// <summary>
        /// DT_FINDPATH_ANY_ANGLE. Use raycasts during pathfind to "shortcut" (raycast still consider costs)
        /// </summary>
        AnyAngle = 0x02,
    }
}
