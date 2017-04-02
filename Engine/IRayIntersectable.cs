using SharpDX;

namespace Engine
{
    /// <summary>
    /// Ray intersectable interface
    /// </summary>
    public interface IRayIntersectable
    {
        /// <summary>
        /// Intersection test between ray and ray intersectable object
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <returns>Returns true if ray intersects with this ray intersectable object</returns>
        bool Intersects(ref Ray ray);
        /// <summary>
        /// Intersection test between ray and ray intersectable object
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="distance">Distance from ray origin and intersection point, if any</param>
        /// <returns>Returns true if ray intersects with this ray intersectable object</returns>
        bool Intersects(ref Ray ray, out float distance);
        /// <summary>
        /// Intersection test between ray and ray intersectable object
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="point">Intersection point, if any</param>
        /// <param name="distance">Distance from ray origin and intersection point, if any</param>
        /// <returns>Returns true if ray intersects with this ray intersectable object</returns>
        bool Intersects(ref Ray ray, out Vector3 point, out float distance);
        /// <summary>
        /// Intersection test between ray and ray intersectable object
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Test facing only triangles</param>
        /// <returns>Returns true if ray intersects with this ray intersectable object</returns>
        bool Intersects(ref Ray ray, bool facingOnly);
        /// <summary>
        /// Intersection test between ray and ray intersectable object
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Test facing only triangles</param>
        /// <param name="distance">Distance from ray origin and intersection point, if any</param>
        /// <returns>Returns true if ray intersects with this ray intersectable object</returns>
        bool Intersects(ref Ray ray, bool facingOnly, out float distance);
        /// <summary>
        /// Intersection test between ray and ray intersectable object
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Test facing only triangles</param>
        /// <param name="point">Intersection point, if any</param>
        /// <param name="distance">Distance from ray origin and intersection point, if any</param>
        /// <returns>Returns true if ray intersects with this ray intersectable object</returns>
        bool Intersects(ref Ray ray, bool facingOnly, out Vector3 point, out float distance);
    }
}
