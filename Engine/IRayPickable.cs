using SharpDX;

namespace Engine
{
    /// <summary>
    /// Ray-pickable object
    /// </summary>
    public interface IRayPickable<T> where T : IRayIntersectable
    {
        /// <summary>
        /// Gets nearest picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        bool PickNearest(Ray ray, bool facingOnly, out PickingResult<T> result);
        /// <summary>
        /// Gets first picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        /// <remarks>Based on geometry, not distance. For distance tests use PickNearest instead.</remarks>
        bool PickFirst(Ray ray, bool facingOnly, out PickingResult<T> result);
        /// <summary>
        /// Gets all picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if ground position found</returns>
        bool PickAll(Ray ray, bool facingOnly, out PickingResult<T>[] results);

        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        BoundingSphere GetBoundingSphere();
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        BoundingBox GetBoundingBox();

        /// <summary>
        /// Gets the volume geometry of the instance
        /// </summary>
        /// <param name="full">Gets full geometry</param>
        /// <returns>Returns the volume geometry of the instance</returns>
        Triangle[] GetVolume(bool full);
    }
}
