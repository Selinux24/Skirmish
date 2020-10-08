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
        private Vector3[] positionCache = null;
        /// <summary>
        /// Triangle list cache
        /// </summary>
        private Triangle[] triangleCache = null;

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
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or position list</returns>
        public IEnumerable<Vector3> GetPoints(DrawingData drawingData, AnimationController controller, Manipulator3D manipulator, bool refresh = false)
        {
            bool update = refresh || updatePoints;

            if (update)
            {
                Logger.WriteTrace(this, "GeometryHelper GetPoints Forced");

                if (drawingData == null)
                {
                    return new Vector3[] { };
                }

                IEnumerable<Vector3> cache;

                if (controller != null && drawingData.SkinningData != null)
                {
                    cache = drawingData.GetPoints(
                        manipulator.FinalTransform,
                        controller.GetCurrentPose(drawingData.SkinningData),
                        update);
                }
                else
                {
                    cache = drawingData.GetPoints(
                        manipulator.FinalTransform,
                        update);
                }

                positionCache = cache.ToArray();

                updatePoints = false;
            }
            else
            {
                Logger.WriteTrace(this, "GeometryHelper GetPoints Cached");
            }

            return positionCache?.ToArray() ?? new Vector3[] { };
        }
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="drawingData">Drawing data</param>
        /// <param name="controller">Animation controller</param>
        /// <param name="manipulator">Manipulator</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or triangle list</returns>
        public IEnumerable<Triangle> GetTriangles(DrawingData drawingData, AnimationController controller, Manipulator3D manipulator, bool refresh = false)
        {
            bool update = refresh || updateTriangles;

            if (update)
            {
                Logger.WriteTrace(this, "GeometryHelper GetTriangles Forced");

                if (drawingData == null)
                {
                    return new Triangle[] { };
                }

                IEnumerable<Triangle> cache;

                if (controller != null && drawingData.SkinningData != null)
                {
                    cache = drawingData.GetTriangles(
                        manipulator.LocalTransform,
                        controller.GetCurrentPose(drawingData.SkinningData),
                        update);
                }
                else
                {
                    cache = drawingData.GetTriangles(
                        manipulator.LocalTransform,
                        update);
                }

                triangleCache = cache.ToArray();

                updateTriangles = false;
            }
            else
            {
                Logger.WriteTrace(this, $"GeometryHelper GetTriangles Cached");
            }

            return triangleCache?.ToArray() ?? new Triangle[] { };
        }
    }
}
