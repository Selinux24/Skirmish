using SharpDX;
using System;

namespace Engine.PathFinding.AStar
{
    [Serializable]
    public class GridGenerationSettings : PathFinderSettings
    {
        /// <summary>
        /// Path node side size
        /// </summary>
        public float NodeSize { get; set; } = 10f;
        /// <summary>
        /// Path node maximum inclination
        /// </summary>
        public float NodeInclination { get; set; } = MathUtil.PiOverFour;
    }
}
