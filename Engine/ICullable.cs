using SharpDX;

namespace Engine
{
    /// <summary>
    /// Cullable interface
    /// </summary>
    public interface ICullable
    {
        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the object is outside of the frustum</returns>
        bool Cull(BoundingFrustum frustum, out float? distance);
        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the object is outside of the box</returns>
        bool Cull(BoundingBox box, out float? distance);
        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the object is outside of the sphere</returns>
        bool Cull(BoundingSphere sphere, out float? distance);
    }
}
