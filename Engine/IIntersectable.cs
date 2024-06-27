
namespace Engine
{
    /// <summary>
    /// IIntersectable interface
    /// </summary>
    public interface IIntersectable
    {
        /// <summary>
        /// Gets whether the sphere intersects with the current object
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="result">Picking results</param>
        /// <returns>Returns true if intersects</returns>
        bool Intersects(IntersectionVolumeSphere sphere, out PickingResult<Triangle> result);

        /// <summary>
        /// Gets whether the actual object have intersection with the intersectable or not
        /// </summary>
        /// <param name="detectionModeThis">Detection mode for this object</param>
        /// <param name="other">Other intersectable</param>
        /// <param name="detectionModeOther">Detection mode for the other object</param>
        /// <returns>Returns true if have intersection</returns>
        bool Intersects(IntersectDetectionMode detectionModeThis, IIntersectable other, IntersectDetectionMode detectionModeOther);
        /// <summary>
        /// Gets whether the actual object have intersection with the volume or not
        /// </summary>
        /// <param name="detectionModeThis">Detection mode for this object</param>
        /// <param name="volume">Volume</param>
        /// <returns>Returns true if have intersection</returns>
        bool Intersects(IntersectDetectionMode detectionModeThis, ICullingVolume volume);

        /// <summary>
        /// Gets the intersection volume based on the specified detection mode
        /// </summary>
        /// <param name="detectionMode">Detection mode</param>
        /// <returns>Returns an intersection volume</returns>
        ICullingVolume GetIntersectionVolume(IntersectDetectionMode detectionMode);
    }
}
