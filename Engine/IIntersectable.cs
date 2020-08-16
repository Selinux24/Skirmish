
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
    }
}
