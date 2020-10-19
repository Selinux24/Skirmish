using SharpDX;
using System;

namespace Engine.PathFinding
{
    [Serializable]
    public abstract class PathFinderSettings
    {
        /// <summary>
        /// Path Finder bounds
        /// </summary>
        public BoundingBox? Bounds { get; set; } = null;
    }
}
