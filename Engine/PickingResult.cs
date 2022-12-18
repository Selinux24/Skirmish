using SharpDX;

namespace Engine
{
    /// <summary>
    /// Picking result
    /// </summary>
    /// <typeparam name="T"><see cref="IRayIntersectable"/> primitive type</typeparam>
    public struct PickingResult<T> where T : IRayIntersectable
    {
        /// <summary>
        /// Intersection position
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Distance from ray origin to the intersecion position
        /// </summary>
        public float Distance { get; set; }
        /// <summary>
        /// Intersection Primitive
        /// </summary>
        public T Primitive { get; set; }
    }
}
