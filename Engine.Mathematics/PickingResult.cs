
namespace Engine.Mathematics
{
    /// <summary>
    /// Picking result
    /// </summary>
    /// <typeparam name="T"><see cref="IRayIntersectable"/> primitive type</typeparam>
    public readonly struct PickingResult<T>(Vector3 position, float distance, T primitive) where T : IRayIntersectable
    {
        /// <summary>
        /// Empty result
        /// </summary>
        public readonly static PickingResult<T> Empty = new(Vector3.Zero, float.MaxValue, default);

        /// <summary>
        /// Intersection position
        /// </summary>
        public readonly Vector3 Position = position;
        /// <summary>
        /// Distance from ray origin to the intersecion position
        /// </summary>
        public readonly float Distance = distance;
        /// <summary>
        /// Intersection Primitive
        /// </summary>
        public readonly T Primitive = primitive;
    }
}
