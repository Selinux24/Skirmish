using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Grid pool item
    /// </summary>
    public class ProximityGridItem<T> where T : class
    {
        /// <summary>
        /// Item
        /// </summary>
        public T Item { get; set; }
        /// <summary>
        /// X position
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// Y position
        /// </summary>
        public int Y { get; set; }
        /// <summary>
        /// Next item in the pool
        /// </summary>
        public int Next { get; set; }
        /// <summary>
        /// Real item position
        /// </summary>
        public Vector3 RealPosition { get; set; }
        /// <summary>
        /// Item radius
        /// </summary>
        public float Radius { get; set; }
    };
}
