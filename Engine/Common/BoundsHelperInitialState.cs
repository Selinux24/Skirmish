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
        /// Creates an initial state from a point list
        /// </summary>
        /// <param name="points">Point list</param>
        public static BoundsHelperInitialState FromPoints(IEnumerable<Vector3> points)
        {
            BoundsHelperInitialState initialState = new();
            initialState.SetPoints(points);
            return initialState;
        }

        /// <summary>
        /// Initial bounding sphere
        /// </summary>
        public BoundingSphere BoundingSphere { get; private set; }
        /// <summary>
        /// Initial bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; private set; }
        /// <summary>
        /// Salt value
        /// </summary>
        /// <remarks>This value increments each time the initial points changes</remarks>
        public int Salt { get; private set; } = 0;

        /// <summary>
        /// Sets the point list
        /// </summary>
        /// <param name="points">Point list</param>
        public void SetPoints(IEnumerable<Vector3> points)
        {
            BoundingSphere sphere;
            BoundingBox box;

            if (points?.Any() != true)
            {
                sphere = new();

                box = new();
            }
            else
            {
                var distinctPoints = points.Distinct().ToArray();

                //Initialize the identity sphere
                sphere = SharpDXExtensions.BoundingSphereFromPoints(distinctPoints);

                //Initialize the identity box
                box = SharpDXExtensions.BoundingBoxFromPoints(distinctPoints);
            }

            if (sphere != BoundingSphere || box != BoundingBox)
            {
                //Update salt value only if state changes
                Salt++;

                BoundingSphere = sphere;
                BoundingBox = box;
            }
        }
    }
}
