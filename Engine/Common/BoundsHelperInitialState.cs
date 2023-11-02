using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Initial helper state
    /// </summary>
    public sealed class BoundsHelperInitialState
    {
        /// <summary>
        /// Initial bounding sphere
        /// </summary>
        public BoundingSphere BoundingSphere { get; private set; }
        /// <summary>
        /// Initial bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; private set; }

        /// <summary>
        /// Sets the point list
        /// </summary>
        /// <param name="points">Point list</param>
        public void SetPoints(IEnumerable<Vector3> points)
        {
            if (points?.Any() != true)
            {
                BoundingSphere = new();

                BoundingBox = new();
            }
            else
            {
                var distinctPoints = points.Distinct().ToArray();

                //Initialize the identity sphere
                BoundingSphere = SharpDXExtensions.BoundingSphereFromPoints(distinctPoints);

                //Initialize the identity box
                BoundingBox = SharpDXExtensions.BoundingBoxFromPoints(distinctPoints);
            }
        }
    }
}
