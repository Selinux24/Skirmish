using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    using Engine.Animation;

    /// <summary>
    /// Geometry helper
    /// </summary>
    class GeometryHelper
    {
        /// <summary>
        /// Update point cache flag
        /// </summary>
        private bool updatePoints = true;
        /// <summary>
        /// Update triangle cache flag
        /// </summary>
        private bool updateTriangles = true;
        /// <summary>
        /// Points cache
        /// </summary>
        private IEnumerable<Vector3> positionCache = Enumerable.Empty<Vector3>();
        /// <summary>
        /// Triangle list cache
        /// </summary>
        private IEnumerable<Triangle> triangleCache = Enumerable.Empty<Triangle>();

        /// <summary>
        /// Invalidates internal state
        /// </summary>
        public void Invalidate()
        {
            updatePoints = true;
            updateTriangles = true;
        }

        /// <summary>
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="drawingData">Drawing data</param>
        /// <param name="controller">Animation controller</param>
        /// <param name="manipulator">Manipulator</param>
        /// <param name="refresh">Sets if the cache must be refreshed or not</param>
        /// <returns>Returns null or position list</returns>
        public IEnumerable<Vector3> GetPoints(DrawingData drawingData, AnimationController controller, Manipulator3D manipulator, bool refresh = false)
        {
            bool update = refresh || updatePoints;

            if (!update)
            {
                //Copy collection
                return positionCache.ToArray();
            }

            if (drawingData == null)
            {
                return Enumerable.Empty<Vector3>();
            }

            if (controller != null && drawingData.SkinningData != null)
            {
                positionCache = drawingData.GetPoints(
                    manipulator.FinalTransform,
                    controller.GetCurrentPose(),
                    update);
            }
            else
            {
                positionCache = drawingData.GetPoints(
                    manipulator.FinalTransform,
                    update);
            }

            updatePoints = false;

            //Copy collection
            return positionCache.ToArray();
        }
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="drawingData">Drawing data</param>
        /// <param name="controller">Animation controller</param>
        /// <param name="manipulator">Manipulator</param>
        /// <param name="refresh">Sets if the cache must be refreshed or not</param>
        /// <returns>Returns null or triangle list</returns>
        public IEnumerable<Triangle> GetTriangles(DrawingData drawingData, AnimationController controller, Manipulator3D manipulator, bool refresh = false)
        {
            bool update = refresh || updateTriangles;

            if (!update)
            {
                //Copy collection
                return triangleCache.ToArray();
            }

            if (drawingData == null)
            {
                return Enumerable.Empty<Triangle>();
            }

            if (controller != null && drawingData.SkinningData != null)
            {
                triangleCache = drawingData.GetTriangles(
                    manipulator.LocalTransform,
                    controller.GetCurrentPose(),
                    update);
            }
            else
            {
                triangleCache = drawingData.GetTriangles(
                    manipulator.LocalTransform,
                    update);
            }

            updateTriangles = false;

            //Copy collection
            return triangleCache.ToArray();
        }
    }
}
