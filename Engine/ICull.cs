using SharpDX;

namespace Engine
{
    /// <summary>
    /// Culling methods
    /// </summary>
    public interface ICull
    {
        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="frustum">Frustum</param>
        bool Cull(BoundingFrustum frustum);
        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="box">Box</param>
        bool Cull(BoundingBox box);
        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="sphere">Sphere</param>
        bool Cull(BoundingSphere sphere);
    }
}
