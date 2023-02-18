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
            if (transform.IsIdentity)
            {
                return new OrientedBoundingBox(points.ToArray());
            }

            //First, get item points
            Vector3[] ptArray = points.ToArray();

            //Next, remove any point transform and set points to origin
            Matrix inv = Matrix.Invert(transform);
            Vector3.TransformCoordinate(ptArray, ref inv, ptArray);

            //Create the obb from origin points
            var obb = new OrientedBoundingBox(ptArray);

            //Apply the original transform to obb
            obb.Transformation *= transform;

            return obb;
        }

        /// <summary>
        /// Gets a oriented bounding box transformed by the given matrix
        /// </summary>
        /// <param name="obb">Oriented bounding box</param>
        /// <param name="transform">Transform</param>
        public static OrientedBoundingBox SetTransform(this OrientedBoundingBox obb, Matrix transform)
        {
            if (transform.IsIdentity)
            {
                return obb;
            }

            var trnObb = obb;
            trnObb.Transformation = transform;
            return trnObb;
        }
    }
}
