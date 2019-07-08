using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// OrientedBoundingBox (OBB) extensions
    /// </summary>
    public static class OrientedBoundingBoxExtensions
    {
        /// <summary>
        /// Creates an oriented bounding box from a transformed point list and it's transform matrix
        /// </summary>
        /// <param name="points">Point list</param>
        /// <param name="transform">Transform matrix</param>
        /// <returns>Returns the new oriented bounding box</returns>
        public static OrientedBoundingBox FromPoints(IEnumerable<Vector3> points, Matrix transform)
        {
            var inv = Matrix.Invert(transform);

            var vPoints = points.ToArray();
            var originPoints = new Vector3[vPoints.Length];
            Vector3.TransformCoordinate(vPoints, ref inv, originPoints);

            return new OrientedBoundingBox(originPoints)
            {
                Transformation = transform
            };
        }
    }
}
