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
        /// <param name="position">Ground position if exists</param>
        /// <param name="item">Ray intersectable object found</param>
        /// <param name="distance">Distance to position</param>
        /// <returns>Returns true if ground position found</returns>
        bool PickNearest(ref Ray ray, bool facingOnly, out Vector3 position, out T item, out float distance);
        /// <summary>
        /// Gets first picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="item">Ray intersectable object found</param>
        /// <param name="distance">Distance to position</param>
        /// <returns>Returns true if ground position found</returns>
        /// <remarks>Based on geometry, not distance. For distance tests use PickNearest instead.</remarks>
        bool PickFirst(ref Ray ray, bool facingOnly, out Vector3 position, out T item, out float distance);
        /// <summary>
        /// Gets all picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="positions">Ground positions if exists</param>
        /// <param name="item">Ray intersectable objects found</param>
        /// <param name="distances">Distances to positions</param>
        /// <returns>Returns true if ground position found</returns>
        bool PickAll(ref Ray ray, bool facingOnly, out Vector3[] positions, out T[] item, out float[] distances);

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
