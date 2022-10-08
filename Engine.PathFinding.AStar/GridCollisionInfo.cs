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
        public Vector3 Point;
        /// <summary>
        /// Collision triangle
        /// </summary>
        public Triangle Triangle;
        /// <summary>
        /// Distance to point
        /// </summary>
        public float Distance;

        /// <summary>
        /// Gets text representarion of collision
        /// </summary>
        /// <returns>Returns text representarion of collision</returns>
        public override string ToString()
        {
            return string.Format("{0}", this.Point);
        }
    }
}
