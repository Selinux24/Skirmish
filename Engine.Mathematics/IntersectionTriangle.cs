using System;

namespace Engine.Mathematics
{
    /// <summary>
    /// Triangle-Triangle Overlap Test
    /// </summary>
    /// <remarks>
    /// Fast and Robust Triangle-Triangle Overlap Test
    /// Using Orientation Predicates"  P. Guigue - O. Devillers
    /// Journal of Graphics Tools, 8(1), 2003
    ///
    /// Other information are available from the Web page http://www.acm.org/jgt/papers/GuigueDevillers03/
    ///
    /// Ported code from https://github.com/benardp/contours/blob/master/freestyle/view_map/triangle_triangle_intersection.c
    /// </remarks>
    static class IntersectionTriangle
    {
        /// <summary>
        /// Gets whether the specified triangles intersects or not
        /// </summary>
        /// <param name="t1">Triangle 1</param>
        /// <param name="t2">Triangle 2</param>
        public static bool Intersection(ref Triangle t1, ref Triangle t2)
        {
            var res = Intersection(ref t1, ref t2, out var coplanar, out var test1, out var test2, out var distances);
            if (!res)
            {
                return false;
            }

            if (coplanar)
            {
                var n = t1.Normal;
                return Coplanar3D(ref t1, ref t2, ref n);
            }

            var intRes = DetectIntersection3D(ref test1, ref test2, ref distances, out var inTest1, out var inTest2);
            if (!intRes)
            {
                var n = t1.Normal;
                return Coplanar3D(ref test1, ref test2, ref n);
            }

            return CheckMinMax(ref inTest1, ref inTest2);
        }

        /// <summary>
        /// Gets whether the specified triangles intersects or not
        /// </summary>
        /// <param name="t1">Triangle 1</param>
        /// <param name="t2">Triangle 2</param>
        /// <param name="segment">If the triangles are not coplanar and there was intersection, returns de intersections segment</param>
        public static bool Intersection(ref Triangle t1, ref Triangle t2, out Segment? segment)
        {
            var res = Intersection(ref t1, ref t2, out var coplanar, out var test1, out var test2, out var distances);
            if (!res)
            {
                segment = null;
                return false;
            }

            if (coplanar)
            {
                segment = null;
                var n = t1.Normal;
                return Coplanar3D(ref t1, ref t2, ref n);
            }

            var intRes = DetectIntersection3D(ref test1, ref test2, ref distances, out var inTest1, out var inTest2);
            if (!intRes)
            {
                segment = null;
                var n = t1.Normal;
                return Coplanar3D(ref test1, ref test2, ref n);
            }

            var n1 = t1.Normal;
            var n2 = t2.Normal;
            return ConstructIntersection3D(ref inTest1, ref inTest2, ref n1, ref n2, out segment);
        }

        private static bool Intersection(ref Triangle t1, ref Triangle t2, out bool coplanar, out Triangle test1, out Triangle test2, out Vector3 distances)
        {
            coplanar = false;

            // Compute distance signs of p1, q1 and r1 to the plane of triangle(p2,q2,r2)
            ComputeDistances(ref t1, ref t2, out var dist1, out var dp1, out var dq1, out var dr1);
            if (!dist1)
            {
                test1 = default;
                test2 = default;
                distances = default;

                return false;
            }

            // Compute distance signs of p2, q2 and r2 to the plane of triangle(p1,q1,r1)
            ComputeDistances(ref t2, ref t1, out var dist2, out var dp2, out var dq2, out var dr2);
            if (!dist2)
            {
                test1 = default;
                test2 = default;
                distances = default;

                return false;
            }

            // Permutation in a canonical form of T1's vertices
            if (dp1 > 0f)
            {
                TestIntersectionDp1Major(ref t1, ref t2, dq1, dr1, dp2, dq2, dr2, out test1, out test2, out distances);
            }
            else if (dp1 < 0f)
            {
                TestIntersectionDp1Minor(ref t1, ref t2, dq1, dr1, dp2, dq2, dr2, out test1, out test2, out distances);
            }
            else
            {
                TestIntersectionDp1Zero(ref t1, ref t2, dq1, dr1, dp2, dq2, dr2, out test1, out test2, out distances, out coplanar);
            }

            return true;
        }

        private static void ComputeDistances(ref Triangle a, ref Triangle b, out bool dist, out float dp, out float dq, out float dr)
        {
            Vector3.Subtract(ref a.Point1, ref b.Point3, out var s1);
            Vector3.Subtract(ref a.Point2, ref b.Point3, out var s2);
            Vector3.Subtract(ref a.Point3, ref b.Point3, out var s3);

            var n = b.Normal;
            Vector3.Dot(ref s1, ref n, out dp);
            Vector3.Dot(ref s2, ref n, out dq);
            Vector3.Dot(ref s3, ref n, out dr);

            dist = !((dp * dq) > 0f && (dp * dr) > 0f);
        }

        private static void TestIntersectionDp1Major(ref Triangle a, ref Triangle b, float dq1, float dr1, float dp2, float dq2, float dr2, out Triangle test1, out Triangle test2, out Vector3 distances)
        {
            if (dq1 > 0f)
            {
                test1 = new(a.Point3, a.Point1, a.Point2);
                test2 = new(b.Point1, b.Point3, b.Point2);
                distances = new(dp2, dr2, dq2);
            }
            else if (dr1 > 0f)
            {
                test1 = new(a.Point2, a.Point3, a.Point1);
                test2 = new(b.Point1, b.Point3, b.Point2);
                distances = new(dp2, dr2, dq2);
            }
            else
            {
                test1 = new(a.Point1, a.Point2, a.Point3);
                test2 = new(b.Point1, b.Point2, b.Point3);
                distances = new(dp2, dq2, dr2);
            }
        }

        private static void TestIntersectionDp1Minor(ref Triangle a, ref Triangle b, float dq1, float dr1, float dp2, float dq2, float dr2, out Triangle test1, out Triangle test2, out Vector3 distances)
        {
            if (dq1 < 0f)
            {
                test1 = new(a.Point3, a.Point1, a.Point2);
                test2 = new(b.Point1, b.Point2, b.Point3);
                distances = new(dp2, dq2, dr2);
            }
            else if (dr1 < 0f)
            {
                test1 = new(a.Point2, a.Point3, a.Point1);
                test2 = new(b.Point1, b.Point2, b.Point3);
                distances = new(dp2, dq2, dr2);
            }
            else
            {
                test1 = new(a.Point1, a.Point2, a.Point3);
                test2 = new(b.Point1, b.Point3, b.Point2);
                distances = new(dp2, dr2, dq2);
            }
        }

        private static void TestIntersectionDp1Zero(ref Triangle a, ref Triangle b, float dq1, float dr1, float dp2, float dq2, float dr2, out Triangle test1, out Triangle test2, out Vector3 distances, out bool coplanar)
        {
            coplanar = false;

            if (dq1 < 0f)
            {
                if (dr1 >= 0f)
                {
                    test1 = new(a.Point2, a.Point3, a.Point1);
                    test2 = new(b.Point1, b.Point3, b.Point2);
                    distances = new(dp2, dr2, dq2);
                }
                else
                {
                    test1 = new(a.Point1, a.Point2, a.Point3);
                    test2 = new(b.Point1, b.Point2, b.Point3);
                    distances = new(dp2, dq2, dr2);
                }
            }
            else if (dq1 > 0f)
            {
                if (dr1 > 0f)
                {
                    test1 = new(a.Point1, a.Point2, a.Point3);
                    test2 = new(b.Point1, b.Point3, b.Point2);
                    distances = new(dp2, dr2, dq2);
                }
                else
                {
                    test1 = new(a.Point2, a.Point3, a.Point1);
                    test2 = new(b.Point1, b.Point2, b.Point3);
                    distances = new(dp2, dq2, dr2);
                }
            }
            else
            {
                if (dr1 > 0f)
                {
                    test1 = new(a.Point3, a.Point1, a.Point2);
                    test2 = new(b.Point1, b.Point2, b.Point3);
                    distances = new(dp2, dq2, dr2);
                }
                else if (dr1 < 0f)
                {
                    test1 = new(a.Point3, a.Point1, a.Point2);
                    test2 = new(b.Point1, b.Point3, b.Point2);
                    distances = new(dp2, dr2, dq2);
                }
                else
                {
                    coplanar = true;
                    test1 = default;
                    test2 = default;
                    distances = default;
                }
            }
        }

        private static bool DetectIntersection3D(ref Triangle a, ref Triangle b, ref Vector3 distances, out Triangle test1, out Triangle test2)
        {
            float dp2 = distances.X;
            float dq2 = distances.Y;
            float dr2 = distances.Z;

            if (dp2 > 0f)
            {
                TestIntersection3DDp2Major(ref a, ref b, dq2, dr2, out test1, out test2);
            }
            else if (dp2 < 0f)
            {
                TestIntersection3DDp2Minor(ref a, ref b, dq2, dr2, out test1, out test2);
            }
            else
            {
                if (!TestIntersection3DDp2Zero(ref a, ref b, dq2, dr2, out test1, out test2))
                {
                    return false;
                }
            }

            return true;
        }

        private static void TestIntersection3DDp2Major(ref Triangle a, ref Triangle b, float dq2, float dr2, out Triangle test1, out Triangle test2)
        {
            if (dq2 > 0f)
            {
                test1 = new(a.Point1, a.Point3, a.Point2);
                test2 = new(b.Point3, b.Point1, b.Point2);
            }
            else if (dr2 > 0f)
            {
                test1 = new(a.Point1, a.Point3, a.Point2);
                test2 = new(b.Point2, b.Point3, b.Point1);
            }
            else
            {
                test1 = new(a.Point1, a.Point2, a.Point3);
                test2 = new(b.Point1, b.Point2, b.Point3);
            }
        }

        private static void TestIntersection3DDp2Minor(ref Triangle a, ref Triangle b, float dq2, float dr2, out Triangle test1, out Triangle test2)
        {
            if (dq2 < 0f)
            {
                test1 = new(a.Point1, a.Point2, a.Point3);
                test2 = new(b.Point3, b.Point1, b.Point2);
            }
            else if (dr2 < 0f)
            {
                test1 = new(a.Point1, a.Point2, a.Point3);
                test2 = new(b.Point2, b.Point3, b.Point1);
            }
            else
            {
                test1 = new(a.Point1, a.Point3, a.Point2);
                test2 = new(b.Point1, b.Point2, b.Point3);
            }
        }

        private static bool TestIntersection3DDp2Zero(ref Triangle a, ref Triangle b, float dq2, float dr2, out Triangle test1, out Triangle test2)
        {
            if (dq2 < 0f)
            {
                if (dr2 >= 0f)
                {
                    test1 = new(a.Point1, a.Point3, a.Point2);
                    test2 = new(b.Point2, b.Point3, b.Point1);
                }
                else
                {
                    test1 = new(a.Point1, a.Point2, a.Point3);
                    test2 = new(b.Point1, b.Point2, b.Point3);
                }
            }
            else if (dq2 > 0f)
            {
                if (dr2 > 0f)
                {
                    test1 = new(a.Point1, a.Point3, a.Point2);
                    test2 = new(b.Point1, b.Point2, b.Point3);
                }
                else
                {
                    test1 = new(a.Point1, a.Point2, a.Point3);
                    test2 = new(b.Point2, b.Point3, b.Point1);
                }
            }
            else
            {
                if (dr2 > 0f)
                {
                    test1 = new(a.Point1, a.Point2, a.Point3);
                    test2 = new(b.Point3, b.Point1, b.Point2);
                }
                else if (dr2 < 0f)
                {
                    test1 = new(a.Point1, a.Point3, a.Point2);
                    test2 = new(b.Point3, b.Point1, b.Point2);
                }
                else
                {
                    test1 = default;
                    test2 = default;
                    return false;
                }
            }

            return true;
        }

        private static bool ConstructIntersection3D(ref Triangle a, ref Triangle b, ref Vector3 n1, ref Vector3 n2, out Segment? segment)
        {
            Vector3.Subtract(ref a.Point2, ref a.Point1, out var v1);
            Vector3.Subtract(ref b.Point3, ref a.Point1, out var v2);
            Vector3.Cross(ref v1, ref v2, out var N);
            Vector3.Subtract(ref b.Point1, ref a.Point1, out var v);

            if (Vector3.Dot(v, N) > 0f)
            {
                return ConstructAxis0(ref a, ref b, v, ref v2, n1, n2, out segment);
            }
            else
            {
                return ConstructAxis1(ref a, ref b, v, ref v1, n1, n2, out segment);
            }
        }

        private static bool ConstructAxis0(ref Triangle a, ref Triangle b, Vector3 s1, ref Vector3 s2, Vector3 n1, Vector3 n2, out Segment? segment)
        {
            segment = null;

            Vector3.Subtract(ref a.Point3, ref a.Point1, out var v1);
            Vector3.Cross(ref v1, ref s2, out var N);
            Vector3.Dot(ref s1, ref N, out float d);
            if (d > 0f) return false;

            Vector3.Subtract(ref b.Point2, ref a.Point1, out var v2);
            Vector3.Cross(ref v1, ref v2, out N);
            Vector3.Dot(ref s1, ref N, out d);
            if (d > 0.0f)
            {
                Vector3.Subtract(ref a.Point1, ref b.Point1, out v1);
                Vector3.Subtract(ref a.Point1, ref a.Point3, out v2);
                float alpha = Vector3.Dot(v1, n2) / Vector3.Dot(v2, n2);
                Vector3.Multiply(ref v2, alpha, out v1);
                Vector3.Subtract(ref a.Point1, ref v1, out var source);

                Vector3.Subtract(ref b.Point1, ref a.Point1, out v1);
                Vector3.Subtract(ref b.Point1, ref b.Point3, out v2);
                alpha = Vector3.Dot(v1, n1) / Vector3.Dot(v2, n1);
                Vector3.Multiply(ref v2, alpha, out v1);
                Vector3.Subtract(ref b.Point1, ref v1, out var target);

                segment = new(source, target);
                return true;
            }
            else
            {
                Vector3.Subtract(ref b.Point1, ref a.Point1, out v1);
                Vector3.Subtract(ref b.Point1, ref b.Point2, out v2);
                float alpha = Vector3.Dot(v1, n1) / Vector3.Dot(v2, n1);
                Vector3.Multiply(ref v2, alpha, out v1);
                Vector3.Subtract(ref b.Point1, ref v1, out var source);

                Vector3.Subtract(ref b.Point1, ref a.Point1, out v1);
                Vector3.Subtract(ref b.Point1, ref b.Point3, out v2);
                alpha = Vector3.Dot(v1, n1) / Vector3.Dot(v2, n1);
                Vector3.Multiply(ref v2, alpha, out v1);
                Vector3.Subtract(ref b.Point1, ref v1, out var target);

                segment = new(source, target);
                return true;
            }
        }

        private static bool ConstructAxis1(ref Triangle a, ref Triangle b, Vector3 s1, ref Vector3 s2, Vector3 n1, Vector3 n2, out Segment? segment)
        {
            segment = null;

            Vector3.Subtract(ref b.Point2, ref a.Point1, out var v2);
            Vector3.Cross(ref s2, ref v2, out var N);
            Vector3.Dot(ref s1, ref N, out var d);
            if (d < 0f) return false;

            Vector3.Subtract(ref a.Point3, ref a.Point1, out var v1);
            Vector3.Cross(ref v1, ref v2, out N);
            Vector3.Dot(ref s1, ref N, out d);
            if (d >= 0f)
            {
                Vector3.Subtract(ref a.Point1, ref b.Point1, out v1);
                Vector3.Subtract(ref a.Point1, ref a.Point3, out v2);
                float alpha = Vector3.Dot(v1, n2) / Vector3.Dot(v2, n2);
                Vector3.Multiply(ref v2, alpha, out v1);
                Vector3.Subtract(ref a.Point1, ref v1, out var source);

                Vector3.Subtract(ref a.Point1, ref b.Point1, out v1);
                Vector3.Subtract(ref a.Point1, ref a.Point2, out v2);
                alpha = Vector3.Dot(v1, n2) / Vector3.Dot(v2, n2);
                Vector3.Multiply(ref v2, alpha, out v1);
                Vector3.Subtract(ref a.Point1, ref v1, out var target);

                segment = new(source, target);
                return true;
            }
            else
            {
                Vector3.Subtract(ref b.Point1, ref a.Point1, out v1);
                Vector3.Subtract(ref b.Point1, ref b.Point2, out v2);
                float alpha = Vector3.Dot(v1, n1) / Vector3.Dot(v2, n1);
                Vector3.Multiply(ref v2, alpha, out v1);
                Vector3.Subtract(ref b.Point1, ref v1, out var source);

                Vector3.Subtract(ref a.Point1, ref b.Point1, out v1);
                Vector3.Subtract(ref a.Point1, ref a.Point2, out v2);
                alpha = Vector3.Dot(v1, n2) / Vector3.Dot(v2, n2);
                Vector3.Multiply(ref v2, alpha, out v1);
                Vector3.Subtract(ref a.Point1, ref v1, out var target);

                segment = new(source, target);
                return true;
            }
        }

        private static bool CheckMinMax(ref Triangle a, ref Triangle b)
        {
            Vector3.Subtract(ref b.Point1, ref a.Point2, out var b1a2);
            Vector3.Subtract(ref a.Point1, ref a.Point2, out var a1a2);
            Vector3.Subtract(ref b.Point2, ref a.Point2, out var b2a2);

            Vector3.Cross(ref b1a2, ref a1a2, out var n1);
            Vector3.Dot(ref b2a2, ref n1, out float d);
            if (d > 0f) return false;

            Vector3.Subtract(ref b.Point1, ref a.Point1, out var b1a3);
            Vector3.Subtract(ref a.Point3, ref a.Point1, out var a3a1);
            Vector3.Subtract(ref b.Point3, ref a.Point1, out var b3a1);

            Vector3.Cross(ref b1a3, ref a3a1, out n1);
            Vector3.Dot(ref b3a1, ref n1, out d);
            if (d > 0f) return false;

            return true;
        }

        private static bool Coplanar3D(ref Triangle a, ref Triangle b, ref Vector3 n1)
        {
            // Projection of the triangles in 3D onto 2D such that the area of the projection is maximized.
            float nX = MathF.Abs(n1.X);
            float nY = MathF.Abs(n1.Y);
            float nZ = MathF.Abs(n1.Z);

            Vector2 p1, q1, r1, p2, q2, r2;

            if ((nX > nZ) && (nX >= nY))
            {
                // Project onto plane YZ
                p1 = a.Point2.ZY;
                q1 = a.Point1.ZY;
                r1 = a.Point3.ZY;

                p2 = b.Point2.ZY;
                q2 = b.Point1.ZY;
                r2 = b.Point3.ZY;
            }
            else if ((nY > nZ) && (nY >= nX))
            {
                // Project onto plane XZ
                p1 = a.Point2.XZ;
                q1 = a.Point1.XZ;
                r1 = a.Point3.XZ;

                p2 = b.Point2.XZ;
                q2 = b.Point1.XZ;
                r2 = b.Point3.XZ;
            }
            else
            {
                // Project onto plane XY
                p1 = a.Point1.XY;
                q1 = a.Point2.XY;
                r1 = a.Point3.XY;

                p2 = b.Point1.XY;
                q2 = b.Point2.XY;
                r2 = b.Point3.XY;
            }

            return Overlap2D(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2);
        }

        private static bool Overlap2D(
            ref Vector2 p1, ref Vector2 q1, ref Vector2 r1,
            ref Vector2 p2, ref Vector2 q2, ref Vector2 r2)
        {
            if (Orient2D(ref p1, ref q1, ref r1) < 0f)
            {
                if (Orient2D(ref p2, ref q2, ref r2) < 0f)
                {
                    return DetectIntersection2D(ref p1, ref r1, ref q1, ref p2, ref r2, ref q2);
                }
                else
                {
                    return DetectIntersection2D(ref p1, ref r1, ref q1, ref p2, ref q2, ref r2);
                }
            }
            else
            {
                if (Orient2D(ref p2, ref q2, ref r2) < 0f)
                {
                    return DetectIntersection2D(ref p1, ref q1, ref r1, ref p2, ref r2, ref q2);
                }
                else
                {
                    return DetectIntersection2D(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2);
                }
            }
        }

        private static float Orient2D(
            ref Vector2 p1, ref Vector2 p2, ref Vector2 p3)
        {
            return
                (p1.X - p3.X) * (p2.Y - p3.Y) -
                (p1.Y - p3.Y) * (p2.X - p3.X);
        }

        private static bool DetectIntersection2D(
            ref Vector2 p1, ref Vector2 q1, ref Vector2 r1,
            ref Vector2 p2, ref Vector2 q2, ref Vector2 r2)
        {
            if (Orient2D(ref p2, ref q2, ref p1) >= 0f)
            {
                return TestIntersection2DMajor(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2);
            }
            else
            {
                return TestIntersection2DMinor(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2);
            }
        }

        private static bool TestIntersection2DMajor(
            ref Vector2 p1, ref Vector2 q1, ref Vector2 r1,
            ref Vector2 p2, ref Vector2 q2, ref Vector2 r2)
        {
            if (Orient2D(ref q2, ref r2, ref p1) >= 0f)
            {
                if (Orient2D(ref r2, ref p2, ref p1) >= 0f)
                {
                    return true;
                }
                else
                {
                    return IntersectionEdge(ref p1, ref q1, ref r1, ref p2, ref r2);
                }
            }
            else
            {
                if (Orient2D(ref r2, ref p2, ref p1) >= 0f)
                {
                    return IntersectionEdge(ref p1, ref q1, ref r1, ref r2, ref q2);
                }
                else
                {
                    return IntersectionVertex(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2);
                }
            }
        }

        private static bool TestIntersection2DMinor(
            ref Vector2 p1, ref Vector2 q1, ref Vector2 r1,
            ref Vector2 p2, ref Vector2 q2, ref Vector2 r2)
        {
            if (Orient2D(ref q2, ref r2, ref p1) >= 0f)
            {
                if (Orient2D(ref r2, ref p2, ref p1) >= 0f)
                {
                    return IntersectionEdge(ref p1, ref q1, ref r1, ref q2, ref p2);
                }
                else
                {
                    return IntersectionVertex(ref p1, ref q1, ref r1, ref q2, ref r2, ref p2);
                }
            }
            else
            {
                return IntersectionVertex(ref p1, ref q1, ref r1, ref r2, ref p2, ref q2);
            }
        }

        private static bool IntersectionVertex(
            ref Vector2 p1, ref Vector2 q1, ref Vector2 r1,
            ref Vector2 p2, ref Vector2 q2, ref Vector2 r2)
        {
            if (Orient2D(ref r2, ref p2, ref q1) >= 0f)
            {
                if (Orient2D(ref r2, ref q2, ref q1) <= 0f)
                {
                    if (Orient2D(ref p1, ref p2, ref q1) > 0f)
                    {
                        return Orient2D(ref p1, ref q2, ref q1) <= 0f;
                    }
                    else if (Orient2D(ref p1, ref p2, ref r1) >= 0f)
                    {
                        return Orient2D(ref q1, ref r1, ref p2) >= 0f;
                    }
                }
                else if (Orient2D(ref p1, ref q2, ref q1) <= 0f && Orient2D(ref r2, ref q2, ref r1) <= 0f)
                {
                    return Orient2D(ref q1, ref r1, ref q2) >= 0f;
                }
            }
            else if (Orient2D(ref r2, ref p2, ref r1) >= 0f)
            {
                if (Orient2D(ref q1, ref r1, ref r2) >= 0f)
                {
                    return Orient2D(ref p1, ref p2, ref r1) >= 0f;
                }
                else if (Orient2D(ref q1, ref r1, ref q2) >= 0f)
                {
                    return Orient2D(ref r2, ref r1, ref q2) >= 0f;
                }
            }

            return false;
        }

        private static bool IntersectionEdge(
            ref Vector2 p1, ref Vector2 q1, ref Vector2 r1,
            ref Vector2 p2, ref Vector2 r2)
        {
            if (Orient2D(ref r2, ref p2, ref q1) >= 0f)
            {
                if (Orient2D(ref p1, ref p2, ref q1) >= 0f)
                {
                    return Orient2D(ref p1, ref q1, ref r2) >= 0f;
                }
                else if (Orient2D(ref q1, ref r1, ref p2) >= 0f)
                {
                    return Orient2D(ref r1, ref p1, ref p2) >= 0f;
                }
            }
            else if (Orient2D(ref r2, ref p2, ref r1) >= 0f && Orient2D(ref p1, ref p2, ref r1) >= 0f)
            {
                if (Orient2D(ref p1, ref r1, ref r2) >= 0f)
                {
                    return true;
                }

                return Orient2D(ref q1, ref r1, ref r2) >= 0f;
            }

            return false;
        }
    }
}
