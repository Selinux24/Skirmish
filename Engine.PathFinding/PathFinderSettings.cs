using SharpDX;
using System;

namespace Engine.PathFinding
{
    /// <summary>
    /// Path finder settings
    /// </summary>
    [Serializable]
    public abstract class PathFinderSettings
    {
        /// <summary>
        /// Path Finder bounds
        /// </summary>
        public BoundingBox? Bounds { get; set; } = null;
        /// <summary>
        /// Enables debug information
        /// </summary>
        public bool EnableDebugInfo { get; set; } = false;
    }
}
