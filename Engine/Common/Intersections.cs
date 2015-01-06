using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Intersections
    /// </summary>
    public static class Intersections
    {
        /// <summary>
        /// Intersection test between ray and plane
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="plane">Plane</param>
        /// <param name="intersectionPoint">Intersection point</param>
        /// <param name="distanceToPoint">Distance to intersection point from ray origin</param>
        /// <param name="segmentMode">Segment mode</param>
        /// <returns>Returns true if test returns intersection point</returns>
        public static bool RayAndPlane(Ray ray, Plane plane, out Vector3? intersectionPoint, out float? distanceToPoint, bool segmentMode)
        {
            intersectionPoint = null;
            distanceToPoint = null;

            Vector3 intersection;
            if (ray.Intersects(ref plane, out intersection))
            {
                float distance = Vector3.Distance(intersection, ray.Position);

                if (segmentMode)
                {
                    if (distance > ray.Direction.Length())
                    {
                        return false;
                    }
                }

                intersectionPoint = intersection;
                distanceToPoint = distance;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Intersection test between ray and triangle
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="tri">Triangle</param>
        /// <param name="intersectionPoint">Intersection point</param>
        /// <param name="distanceToPoint">Distance to intersection point from ray origin</param>
        /// <param name="segmentMode">Segment mode</param>
        /// <returns>Returns true if test returns intersection point</returns>
        public static bool RayAndTriangle(Ray ray, Triangle tri, out Vector3? intersectionPoint, out float? distanceToPoint, bool segmentMode)
        {
            intersectionPoint = null;
            distanceToPoint = null;

            Vector3 intersection;
            if (tri.Plane.Intersects(ref ray, out intersection))
            {
                float distance = Vector3.Distance(intersection, ray.Position);

                if (segmentMode)
                {
                    if (distance > ray.Direction.Length())
                    {
                        return false;
                    }
                }

                if (Triangle.PointInTriangle(tri, intersection))
                {
                    if (segmentMode)
                    {
                        if (distance <= ray.Direction.Length())
                        {
                            intersectionPoint = intersection;
                            distanceToPoint = distance;

                            return true;
                        }
                    }
                    else
                    {
                        intersectionPoint = intersection;
                        distanceToPoint = distance;

                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Intersection test between ray and triangle list
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="triangleList">Triangle soup</param>
        /// <param name="intersectionPoint">Intersection point</param>
        /// <param name="distanceToPoint">Distance to intersection point from ray origin</param>
        /// <param name="segmentMode">Segment mode</param>
        /// <returns>Returns true if test returns intersection point</returns>
        public static bool RayAndTriangleSoup(Ray ray, Triangle[] triangleList, out Vector3? intersectionPoint, out float? distanceToPoint, bool segmentMode)
        {
            intersectionPoint = null;
            distanceToPoint = null;

            Vector3? closestPoint = null;
            float? closestDistance = null;

            for (int i = 0; i < triangleList.Length; i++)
            {
                Triangle t = triangleList[i];
                Vector3? point = null;
                float? distance = null;
                if (Intersections.RayAndTriangle(ray, t, out point, out distance, segmentMode))
                {
                    if (closestDistance.HasValue)
                    {
                        if (closestDistance > distance)
                        {
                            closestDistance = distance;
                            closestPoint = point;
                        }
                    }
                    else
                    {
                        closestDistance = distance;
                        closestPoint = point;
                    }
                }
            }

            if (closestPoint.HasValue && closestDistance.HasValue)
            {
                intersectionPoint = closestPoint;
                distanceToPoint = closestDistance;

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
