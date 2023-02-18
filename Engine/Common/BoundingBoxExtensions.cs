using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// BoundingBox extensions
    /// </summary>
    public static class BoundingBoxExtensions
    {
        /// <summary>
        /// Gets a box transformed by the given matrix
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="transform">Transform</param>
        public static BoundingBox SetTransform(this BoundingBox box, Matrix transform)
        {
            if (transform.IsIdentity)
            {
                return box;
            }

            // Gets the new position
            var min = Vector3.TransformCoordinate(box.Minimum, transform);
            var max = Vector3.TransformCoordinate(box.Maximum, transform);

            return new BoundingBox(min, max);
        }
    }
}