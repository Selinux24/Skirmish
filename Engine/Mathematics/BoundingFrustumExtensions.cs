using System;
using System.Numerics;
using Vortice.Mathematics;

namespace Engine.Mathematics
{
    public static class BoundingFrustumExtensions
    {
        public static Plane GetPlane(this BoundingFrustum frustum, int index)
        {
            return index switch
            {
                0 => new(-frustum.Left.Normal, -frustum.Left.D),
                1 => new(-frustum.Right.Normal, -frustum.Right.D),
                2 => new(-frustum.Top.Normal, -frustum.Top.D),
                3 => new(-frustum.Bottom.Normal, -frustum.Bottom.D),
                4 => new(-frustum.Near.Normal, -frustum.Near.D),
                5 => new(-frustum.Far.Normal, -frustum.Far.D),
                _ => new Plane(),
            };
        }
        public static ContainmentType Contains(this BoundingFrustum frustum, ref Vector3 point)
        {
            var result = PlaneIntersectionType.Front;

            for (int i = 0; i < 6; i++)
            {
                PlaneIntersectionType planeResult = frustum.GetPlane(i).Intersects(ref point);

                switch (planeResult)
                {
                    case PlaneIntersectionType.Back:
                        return ContainmentType.Disjoint;
                    case PlaneIntersectionType.Intersecting:
                        result = PlaneIntersectionType.Intersecting;
                        break;
                }
            }
            return result switch
            {
                PlaneIntersectionType.Intersecting => ContainmentType.Intersects,
                _ => ContainmentType.Contains,
            };
        }
        public static ContainmentType Contains(this BoundingFrustum frustum, ref BoundingBox box)
        {
            var result = ContainmentType.Contains;

            for (int i = 0; i < 6; i++)
            {
                var plane = frustum.GetPlane(i);

                GetBoxToPlanePVertexNVertex(ref box, ref plane.Normal, out Vector3 p, out Vector3 n);

                if (Collision.PlaneIntersectsPoint(ref plane, ref p) == PlaneIntersectionType.Back)
                    return ContainmentType.Disjoint;

                if (Collision.PlaneIntersectsPoint(ref plane, ref n) == PlaneIntersectionType.Back)
                    result = ContainmentType.Intersects;
            }

            return result;
        }
        public static ContainmentType Contains(this BoundingFrustum frustum, ref BoundingSphere sphere)
        {
            var result = PlaneIntersectionType.Front;

            for (int i = 0; i < 6; i++)
            {
                var planeResult = frustum.GetPlane(i).Intersects(ref sphere);
                switch (planeResult)
                {
                    case PlaneIntersectionType.Back:
                        return ContainmentType.Disjoint;
                    case PlaneIntersectionType.Intersecting:
                        result = PlaneIntersectionType.Intersecting;
                        break;
                }
            }
            return result switch
            {
                PlaneIntersectionType.Intersecting => ContainmentType.Intersects,
                _ => ContainmentType.Contains,
            };
        }

        public static bool Intersects(this BoundingFrustum frustum, ref BoundingSphere sphere)
        {
            return Contains(frustum, ref sphere) != ContainmentType.Disjoint;
        }
        public static bool Intersects(this BoundingFrustum frustum, ref BoundingBox box)
        {
            return Contains(frustum, ref box) != ContainmentType.Disjoint;
        }
        public static PlaneIntersectionType Intersects(this BoundingFrustum frustum, ref Plane plane)
        {
            return PlaneIntersectsPoints(ref plane, frustum.GetCorners());
        }
        public static bool Intersects(this BoundingFrustum frustum, ref Ray ray)
        {
            return Intersects(frustum, ref ray, out _, out _);
        }
        public static bool Intersects(this BoundingFrustum frustum, ref Ray ray, out float? inDistance, out float? outDistance)
        {
            if (Contains(frustum, ref ray.Position) != ContainmentType.Disjoint)
            {
                float nearstPlaneDistance = float.MaxValue;
                for (int i = 0; i < 6; i++)
                {
                    var plane = frustum.GetPlane(i);
                    if (Collision.RayIntersectsPlane(ref ray, ref plane, out float distance) && distance < nearstPlaneDistance)
                    {
                        nearstPlaneDistance = distance;
                    }
                }

                inDistance = nearstPlaneDistance;
                outDistance = null;
                return true;
            }
            else
            {
                //We will find the two points at which the ray enters and exists the frustum
                //These two points make a line which center inside the frustum if the ray intersects it
                //Or outside the frustum if the ray intersects frustum planes outside it.
                float minDist = float.MaxValue;
                float maxDist = float.MinValue;
                for (int i = 0; i < 6; i++)
                {
                    var plane = frustum.GetPlane(i);

                    if (Collision.RayIntersectsPlane(ref ray, ref plane, out float distance))
                    {
                        minDist = Math.Min(minDist, distance);
                        maxDist = Math.Max(maxDist, distance);
                    }
                }

                Vector3 minPoint = ray.Position + ray.Direction * minDist;
                Vector3 maxPoint = ray.Position + ray.Direction * maxDist;
                Vector3 center = (minPoint + maxPoint) / 2f;
                if (Contains(frustum, ref center) != ContainmentType.Disjoint)
                {
                    inDistance = minDist;
                    outDistance = maxDist;
                    return true;
                }
                else
                {
                    inDistance = null;
                    outDistance = null;
                    return false;
                }
            }
        }

        public static FrustumCameraParams GetCameraParams(this BoundingFrustum frustum)
        {
            var pLeft = frustum.GetPlane(0);
            var pRight = frustum.GetPlane(1);
            var pTop = frustum.GetPlane(2);
            var pNear = frustum.GetPlane(4);
            var pFar = frustum.GetPlane(5);
            var corners = frustum.GetCorners();

            var position = Get3PlanesInterPoint(ref pRight, ref pTop, ref pLeft);

            return new FrustumCameraParams
            {
                Position = position,
                LookAtDir = pNear.Normal,
                UpDir = Vector3.Normalize(Vector3.Cross(pRight.Normal, pNear.Normal)),
                FOV = (float)((Math.PI / 2.0 - Math.Acos(Vector3.Dot(pNear.Normal, pTop.Normal))) * 2),
                AspectRatio = (corners[6] - corners[5]).Length() / (corners[4] - corners[5]).Length(),
                ZNear = (position + (pNear.Normal * pNear.D)).Length(),
                ZFar = (position + (pFar.Normal * pFar.D)).Length()
            };
        }
        private static Vector3 Get3PlanesInterPoint(ref Plane p1, ref Plane p2, ref Plane p3)
        {
            //P = -d1 * N2xN3 / N1.N2xN3 - d2 * N3xN1 / N2.N3xN1 - d3 * N1xN2 / N3.N1xN2 
            Vector3 v =
                -p1.D * Vector3.Cross(p2.Normal, p3.Normal) / Vector3.Dot(p1.Normal, Vector3.Cross(p2.Normal, p3.Normal))
                - p2.D * Vector3.Cross(p3.Normal, p1.Normal) / Vector3.Dot(p2.Normal, Vector3.Cross(p3.Normal, p1.Normal))
                - p3.D * Vector3.Cross(p1.Normal, p2.Normal) / Vector3.Dot(p3.Normal, Vector3.Cross(p1.Normal, p2.Normal));

            return v;
        }
        private static void GetBoxToPlanePVertexNVertex(ref BoundingBox box, ref Vector3 planeNormal, out Vector3 p, out Vector3 n)
        {
            p = box.Min;
            if (planeNormal.X >= 0)
                p.X = box.Max.X;
            if (planeNormal.Y >= 0)
                p.Y = box.Max.Y;
            if (planeNormal.Z >= 0)
                p.Z = box.Max.Z;

            n = box.Max;
            if (planeNormal.X >= 0)
                n.X = box.Min.X;
            if (planeNormal.Y >= 0)
                n.Y = box.Min.Y;
            if (planeNormal.Z >= 0)
                n.Z = box.Min.Z;
        }
        private static PlaneIntersectionType PlaneIntersectsPoints(ref Plane plane, Vector3[] points)
        {
            var result = Collision.PlaneIntersectsPoint(ref plane, ref points[0]);
            for (int i = 1; i < points.Length; i++)
                if (Collision.PlaneIntersectsPoint(ref plane, ref points[i]) != result)
                    return PlaneIntersectionType.Intersecting;
            return result;
        }
    }

    public struct FrustumCameraParams
    {
        /// <summary>
        /// Position of the camera.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Looking at direction of the camera.
        /// </summary>
        public Vector3 LookAtDir;

        /// <summary>
        /// Up direction.
        /// </summary>
        public Vector3 UpDir;

        /// <summary>
        /// Field of view.
        /// </summary>
        public float FOV;

        /// <summary>
        /// Z near distance.
        /// </summary>
        public float ZNear;

        /// <summary>
        /// Z far distance.
        /// </summary>
        public float ZFar;

        /// <summary>
        /// Aspect ratio.
        /// </summary>
        public float AspectRatio;
    }
}
