﻿using System;

namespace Engine.PathFinding.RecastNavigation
{
    [Flags]
    public enum FindPathOptions
    {
        /// <summary>
        /// Use raycasts during pathfind to "shortcut" (raycast still consider costs)
        /// </summary>
        DT_FINDPATH_ANY_ANGLE = 0x02,
    }
}
