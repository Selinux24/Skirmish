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
        public float NodeSize = 10f;
        /// <summary>
        /// Path node maximum inclination
        /// </summary>
        public float NodeInclination = MathUtil.PiOverFour;
    }
}
