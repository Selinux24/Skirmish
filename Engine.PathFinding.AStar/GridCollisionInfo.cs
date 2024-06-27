using SharpDX;

namespace Engine.PathFinding.AStar
{
    /// <summary>
    /// Collision info helper
    /// </summary>
    class GridCollisionInfo
    {
        /// <summary>
        /// Collision point
        /// </summary>
        public Vector3 Point { get; set; }
        /// <summary>
        /// Collision triangle
        /// </summary>
        public Triangle Triangle { get; set; }
        /// <summary>
        /// Distance to point
        /// </summary>
        public float Distance { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Point}";
        }
    }
}
