﻿using System;

namespace Engine
{
    /// <summary>
    /// Scene object usajes enumeration
    /// </summary>
    [Flags]
    public enum SceneObjectUsages
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Full triangle list for path finding
        /// </summary>
        FullPathFinding = 1,
        /// <summary>
        /// Coarse list for path finding
        /// </summary>
        CoarsePathFinding = 2,
        /// <summary>
        /// Bounds path finding (OBB)
        /// </summary>
        BoundsPathFinding = 4,
        /// <summary>
        /// Scene ground
        /// </summary>
        Ground = 8,
        /// <summary>
        /// Scene agent
        /// </summary>
        Agent = 16,
        /// <summary>
        /// User interface
        /// </summary>
        UI = 32,
    }
}
