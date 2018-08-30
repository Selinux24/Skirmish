using SharpDX;

namespace Engine
{
    /// <summary>
    /// Picking result
    /// </summary>
    /// <typeparam name="T">IRayIntersectable item type</typeparam>
    public struct PickingResult<T> where T : IRayIntersectable
    {
        /// <summary>
        /// Piked position
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Picked item
        /// </summary>
        public T Item { get; set; }
        /// <summary>
        /// Distance from ray origin
        /// </summary>
        public float Distance { get; set; }
    }
}
