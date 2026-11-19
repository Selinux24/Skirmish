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
                // Not intersection.
                return false;
            }

            if (coplanar)
            {
                // May be coplanar.
                var n = t1.Normal;
                return Coplanar3D(ref t1, ref t2, ref n);
            }

            var intRes = DetectIntersection3D(ref test1, ref test2, ref distances, out var inTest1, out var inTest2);
            if (!intRes)
            {
                // Not intersection or coplanar.
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
                // Not intersection.
                segment = null;
                return false;
            }

            var n1 = t1.Normal;
            var n2 = t2.Normal;

            if (coplanar)
            {
                // May be coplanar.
                segment = null;
                return Coplanar3D(ref t1, ref t2, ref n1);
            }

            var intRes = DetectIntersection3D(ref test1, ref test2, ref distances, out var inTest1, out var inTest2);
            if (!intRes)
            {
                // Not intersection or coplanar.
                segment = null;
                return Coplanar3D(ref test1, ref test2, ref n1);
            }

            return ConstructIntersection3D(ref inTest1, ref inTest2, ref n1, ref n2, out segment);
        }

        private static bool Intersection(ref Triangle t1, ref Triangle t2, out bool coplanar, out Triangle test1, out Triangle test2, out Vector3 distances)
        {
            coplanar = false;

            // Compute distance signs of p1, q1 and r1 to the plane of triangle(p2,q2,r2)
            var (dist1, dp1, dq1, dr1) = ComputeDistances(ref t1, ref t2);
            if (!dist1)
            {
                test1 = new();
                test2 = new();
                distances = new(float.MaxValue);

                return false;
            }

            // Compute distance signs of p2, q2 and r2 to the plane of triangle(p1,q1,r1)
            var (dist2, dp2, dq2, dr2) = ComputeDistances(ref t2, ref t1);
            if (!dist2)
            {
                test1 = new();
                test2 = new();
                distances = new(float.MaxValue);

                return false;
            }

            // Permutation in a canonical form of T1's vertices

            if (dp1 > 0.0f)
            {
                var resMajor = TestIntersectionDp1Major(ref t1, ref t2, dq1, dr1, dp2, dq2, dr2);

                test1 = resMajor.test1;
                test2 = resMajor.test2;
                distances = resMajor.distances;
            }
            else if (dp1 < 0.0f)
            {
                var resMinor = TestIntersectionDp1Minor(ref t1, ref t2, dq1, dr1, dp2, dq2, dr2);

                test1 = resMinor.test1;
                test2 = resMinor.test2;
                distances = resMinor.distances;
            }
            else
            {
                var resZero = TestIntersectionDp1Zero(ref t1, ref t2, dq1, dr1, dp2, dq2, dr2);

                test1 = resZero.test1;
                test2 = resZero.test2;
                distances = resZero.distances;
                coplanar = resZero.coplanar;
            }

            return true;
        }
        private static (bool, float dp, float dq, float dr) ComputeDistances(ref Triangle a, ref Triangle b)
        {
            Vector3.Subtract(ref a.Point1, ref b.Point3, out var s1);
            Vector3.Subtract(ref a.Point2, ref b.Point3, out var s2);
            Vector3.Subtract(ref a.Point3, ref b.Point3, out var s3);

            var n = b.Normal;
            Vector3.Dot(ref s1, ref n, out float dp);
            Vector3.Dot(ref s2, ref n, out float dq);
            Vector3.Dot(ref s3, ref n, out float dr);

            if (((dp * dq) > 0f) && ((dp * dr) > 0f))
            {
                return (false, 0f, 0f, 0f);
            }

            return (true, dp, dq, dr);
        }
        private static (Triangle test1, Triangle test2, Vector3 distances) TestIntersectionDp1Major(ref Triangle a, ref Triangle b, float dq1, float dr1, float dp2, float dq2, float dr2)
        {
            Triangle test1;
            Triangle test2;
            Vector3 distances;

            if (dq1 > 0.0f)
            {
                test1 = new(a.Point3, a.Point1, a.Point2);
                test2 = new(b.Point1, b.Point3, b.Point2);
                distances = new(dp2, dr2, dq2);
            }
            else if (dr1 > 0.0f)
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

            return (test1, test2, distances);
        }
        private static (Triangle test1, Triangle test2, Vector3 distances) TestIntersectionDp1Minor(ref Triangle a, ref Triangle b, float dq1, float dr1, float dp2, float dq2, float dr2)
        {
            Triangle test1;
            Triangle test2;
            Vector3 distances;

            if (dq1 < 0.0f)
            {
                test1 = new(a.Point3, a.Point1, a.Point2);
                test2 = new(b.Point1, b.Point2, b.Point3);
                distances = new(dp2, dq2, dr2);
            }
            else if (dr1 < 0.0f)
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

            return (test1, test2, distances);
        }
        private static (Triangle test1, Triangle test2, Vector3 distances, bool coplanar) TestIntersectionDp1Zero(ref Triangle a, ref Triangle b, float dq1, float dr1, float dp2, float dq2, float dr2)
        {
            Triangle test1;
            Triangle test2;
            Vector3 distances;
            bool coplanar = false;

            if (dq1 < 0.0f)
            {
                if (dr1 >= 0.0f)
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
            else if (dq1 > 0.0f)
            {
                if (dr1 > 0.0f)
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
                if (dr1 > 0.0f)
                {
                    test1 = new(a.Point3, a.Point1, a.Point2);
                    test2 = new(b.Point1, b.Point2, b.Point3);
                    distances = new(dp2, dq2, dr2);
                }
                else if (dr1 < 0.0f)
                {
                    test1 = new(a.Point3, a.Point1, a.Point2);
                    test2 = new(b.Point1, b.Point3, b.Point2);
                    distances = new(dp2, dr2, dq2);
                }
                else
                {
                    // triangles are co-planar
                    coplanar = true;
                    test1 = new();
                    test2 = new();
                    distances = new(float.MaxValue);
                }
            }

            return (test1, test2, distances, coplanar);
        }

        private static bool DetectIntersection3D(ref Triangle a, ref Triangle b, ref Vector3 distances, out Triangle test1, out Triangle test2)
        {
            test1 = new Triangle();
            test2 = new Triangle();

            float dp2 = distances.X;
            float dq2 = distances.Y;
            float dr2 = distances.Z;

            if (dp2 > 0.0f)
            {
                var resMajor = TestIntersection3DDp2Major(ref a, ref b, dq2, dr2);

                test1 = resMajor.test1;
                test2 = resMajor.test2;
            }
            else if (dp2 < 0.0f)
            {
                var resMinor = TestIntersection3DDp2Minor(ref a, ref b, dq2, dr2);

                test1 = resMinor.test1;
                test2 = resMinor.test2;
            }
            else
            {
                var resZero = TestIntersection3DDp2Zero(ref a, ref b, dq2, dr2);

                if (!resZero.res)
                {
                    return false;
                }

                test1 = resZero.test1;
                test2 = resZero.test2;
            }

            return true;
        }
        private static (Triangle test1, Triangle test2) TestIntersection3DDp2Major(ref Triangle a, ref Triangle b, float dq2, float dr2)
        {
            Triangle test1;
            Triangle test2;

            if (dq2 > 0.0f)
            {
                test1 = new(a.Point1, a.Point3, a.Point2);
                test2 = new(b.Point3, b.Point1, b.Point2);
            }
            else if (dr2 > 0.0f)
            {
                test1 = new(a.Point1, a.Point3, a.Point2);
                test2 = new(b.Point2, b.Point3, b.Point1);
            }
            else
            {
                test1 = new(a.Point1, a.Point2, a.Point3);
                test2 = new(b.Point1, b.Point2, b.Point3);
            }

            return (test1, test2);
        }
        private static (Triangle test1, Triangle test2) TestIntersection3DDp2Minor(ref Triangle a, ref Triangle b, float dq2, float dr2)
        {
            Triangle test1;
            Triangle test2;

            if (dq2 < 0.0f)
            {
                test1 = new(a.Point1, a.Point2, a.Point3);
                test2 = new(b.Point3, b.Point1, b.Point2);
            }
            else if (dr2 < 0.0f)
            {
                test1 = new(a.Point1, a.Point2, a.Point3);
                test2 = new(b.Point2, b.Point3, b.Point1);
            }
            else
            {
                test1 = new(a.Point1, a.Point3, a.Point2);
                test2 = new(b.Point1, b.Point2, b.Point3);
            }

            return (test1, test2);
        }
        private static (Triangle test1, Triangle test2, bool res) TestIntersection3DDp2Zero(ref Triangle a, ref Triangle b, float dq2, float dr2)
        {
            Triangle test1;
            Triangle test2;

            if (dq2 < 0.0f)
            {
                if (dr2 >= 0.0f)
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
            else if (dq2 > 0.0f)
            {
                if (dr2 > 0.0f)
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
                if (dr2 > 0.0f)
                {
                    test1 = new(a.Point1, a.Point2, a.Point3);
                    test2 = new(b.Point3, b.Point1, b.Point2);
                }
                else if (dr2 < 0.0f)
                {
                    test1 = new(a.Point1, a.Point3, a.Point2);
                    test2 = new(b.Point3, b.Point1, b.Point2);
                }
                else
                {
                    return (default, default, false);
                }
            }

            return (test1, test2, true);
        }

        private static bool ConstructIntersection3D(ref Triangle a, ref Triangle b, ref Vector3 n1, ref Vector3 n2, out Segment? segment)
        {
            var v1 = Vector3.Subtract(a.Point2, a.Point1);
            var v2 = Vector3.Subtract(b.Point3, a.Point1);
            var N = Vector3.Cross(v1, v2);
            var v = Vector3.Subtract(b.Point1, a.Point1);

            if (Vector3.Dot(v, N) > 0.0f)
            {
                return ConstructAxis0(ref a, ref b, ref v, ref v2, ref n1, ref n2, out segment);
            }
            else
            {
                return ConstructAxis1(ref a, ref b, ref v, ref v1, ref n1, ref n2, out segment);
            }
        }
        private static bool ConstructAxis0(ref Triangle a, ref Triangle b, ref Vector3 s1, ref Vector3 s2, ref Vector3 n1, ref Vector3 n2, out Segment? segment)
        {
            segment = null;

            Vector3.Subtract(ref a.Point3, ref a.Point1, out var v1);
            Vector3.Cross(ref v1, ref s2, out var N);
            Vector3.Dot(ref s1, ref N, out float d);
            if (d > 0.0f)
            {
                return false;
            }

            Vector3.Subtract(ref b.Point2, ref a.Point1, out var v2);
            Vector3.Cross(ref v1, ref v2, out N);
            Vector3.Dot(ref s1, ref N, out d);
            if (d > 0.0f)
            {
                Vector3.Subtract(ref a.Point1, ref b.Point1, out v1);
                Vector3.Subtract(ref a.Point1, ref a.Point3, out v2);
                float alpha = Vector3.Dot(v1, n2) / Vector3.Dot(v2, n2);
                v1 = v2 * alpha;

                Vector3.Subtract(ref a.Point1, ref v1, out Vector3 source);

                Vector3.Subtract(ref b.Point1, ref a.Point1, out v1);
                Vector3.Subtract(ref b.Point1, ref b.Point3, out v2);
                alpha = Vector3.Dot(v1, n1) / Vector3.Dot(v2, n1);
                v1 = v2 * alpha;

                Vector3.Subtract(ref b.Point1, ref v1, out Vector3 target);

                segment = new(source, target);

                return true;
            }
            else
            {
                Vector3.Subtract(ref b.Point1, ref a.Point1, out v1);
                Vector3.Subtract(ref b.Point1, ref b.Point2, out v2);
                float alpha = Vector3.Dot(v1, n1) / Vector3.Dot(v2, n1);
                v1 = v2 * alpha;
                Vector3.Subtract(ref b.Point1, ref v1, out Vector3 source);

                Vector3.Subtract(ref b.Point1, ref a.Point1, out v1);
                Vector3.Subtract(ref b.Point1, ref b.Point3, out v2);
                alpha = Vector3.Dot(v1, n1) / Vector3.Dot(v2, n1);
                v1 = v2 * alpha;
                Vector3.Subtract(ref b.Point1, ref v1, out Vector3 target);

                segment = new(source, target);

                return true;
            }
        }
        private static bool ConstructAxis1(ref Triangle a, ref Triangle b, ref Vector3 s1, ref Vector3 s2, ref Vector3 n1, ref Vector3 n2, out Segment? segment)
        {
            segment = null;

            Vector3.Subtract(ref b.Point2, ref a.Point1, out var v2);
            Vector3.Cross(ref s2, ref v2, out var N);
            Vector3.Dot(ref s1, ref N, out var d);
            if (d < 0.0f)
            {
                return false;
            }

            Vector3.Subtract(ref a.Point3, ref a.Point1, out var v1);
            Vector3.Cross(ref v1, ref v2, out N);
            Vector3.Dot(ref s1, ref N, out d);
            if (d >= 0.0f)
            {
                Vector3.Subtract(ref a.Point1, ref b.Point1, out v1);
                Vector3.Subtract(ref a.Point1, ref a.Point3, out v2);
                float alpha = Vector3.Dot(v1, n2) / Vector3.Dot(v2, n2);
                v1 = v2 * alpha;
                Vector3.Subtract(ref a.Point1, ref v1, out Vector3 source);

                Vector3.Subtract(ref a.Point1, ref b.Point1, out v1);
                Vector3.Subtract(ref a.Point1, ref a.Point2, out v2);
                alpha = Vector3.Dot(v1, n2) / Vector3.Dot(v2, n2);
                v1 = v2 * alpha;
                Vector3.Subtract(ref a.Point1, ref v1, out Vector3 target);

                segment = new(source, target);

                return true;
            }
            else
            {
                Vector3.Subtract(ref b.Point1, ref a.Point1, out v1);
                Vector3.Subtract(ref b.Point1, ref b.Point2, out v2);
                float alpha = Vector3.Dot(v1, n1) / Vector3.Dot(v2, n1);
                v1 = v2 * alpha;
                Vector3.Subtract(ref b.Point1, ref v1, out Vector3 source);

                Vector3.Subtract(ref a.Point1, ref b.Point1, out v1);
                Vector3.Subtract(ref a.Point1, ref a.Point2, out v2);
                alpha = Vector3.Dot(v1, n2) / Vector3.Dot(v2, n2);
                v1 = v2 * alpha;
                Vector3.Subtract(ref a.Point1, ref v1, out Vector3 target);

                segment = new(source, target);

                return true;
            }
        }

        private static bool Coplanar3D(ref Triangle a, ref Triangle b, ref Vector3 n1)
        {
            Vector2 p1;
            Vector2 q1;
            Vector2 r1;

            Vector2 p2;
            Vector2 q2;
            Vector2 r2;

            float nX = MathF.Abs(n1.X);
            float nY = MathF.Abs(n1.Y);
            float nZ = MathF.Abs(n1.Z);

            // Projection of the triangles in 3D onto 2D such that the area of the projection is maximized.

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

            Triangle2D t1 = new(p1, q1, r1);
            Triangle2D t2 = new(p2, q2, r2);

            return Overlap2D(ref t1, ref t2);
        }
        private static bool Overlap2D(ref Triangle2D a, ref Triangle2D b)
        {
            ref var p1 = ref a.Point1;
            ref var q1 = ref a.Point2;
            ref var r1 = ref a.Point3;

            ref var p2 = ref b.Point1;
            ref var q2 = ref b.Point2;
            ref var r2 = ref b.Point3;

            Triangle2D pq1 = new(p1, q1, r1);
            Triangle2D pq2 = new(p2, q2, r2);

            Triangle2D pr1 = new(p1, r1, q1);
            Triangle2D pr2 = new(p2, r2, q2);

            if (Orient2D(ref pq1) < 0f)
            {
                if (Orient2D(ref pq2) < 0f) return DetectIntersection2D(ref pr1, ref pr2);

                return DetectIntersection2D(ref pr1, ref pq2);
            }
            else
            {
                if (Orient2D(ref pq2) < 0f) return DetectIntersection2D(ref pq1, ref pr2);

                return DetectIntersection2D(ref pq1, ref pq2);
            }
        }
        private static float Orient2D(ref Triangle2D t)
        {
            return
                (t.Point1.X - t.Point3.X) * (t.Point2.Y - t.Point3.Y) -
                (t.Point1.Y - t.Point3.Y) * (t.Point2.X - t.Point3.X);
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

        private static bool DetectIntersection2D(ref Triangle2D a, ref Triangle2D b)
        {
            ref var p1 = ref a.Point1;
            ref var q1 = ref a.Point2;
            ref var r1 = ref a.Point3;

            ref var p2 = ref b.Point1;
            ref var q2 = ref b.Point2;
            ref var r2 = ref b.Point3;

            Triangle2D t = new(p2, q2, p1);

            if (Orient2D(ref t) >= 0f)
            {
                return TestIntersection2DMajor(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2);
            }
            else
            {
                return TestIntersection2DMinor(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2);
            }
        }
        private static bool TestIntersection2DMajor(ref Vector2 p1, ref Vector2 q1, ref Vector2 r1, ref Vector2 p2, ref Vector2 q2, ref Vector2 r2)
        {
            Triangle2D pq1 = new(p1, q1, r1);
            Triangle2D pq2 = new(p2, q2, r2);
            Triangle2D qr2 = new(q2, r2, p1);
            Triangle2D rp2 = new(r2, p2, p1);
            Triangle2D rp22 = new(r2, p2, q2);

            if (Orient2D(ref qr2) >= 0f)
            {
                if (Orient2D(ref rp2) >= 0f) return true;

                return IntersectionEdge(ref pq1, ref pq2);
            }
            else
            {
                if (Orient2D(ref rp2) >= 0f) return IntersectionEdge(ref pq1, ref rp22);

                return IntersectionVertex(ref pq1, ref pq2);
            }
        }
        private static bool TestIntersection2DMinor(ref Vector2 p1, ref Vector2 q1, ref Vector2 r1, ref Vector2 p2, ref Vector2 q2, ref Vector2 r2)
        {
            Triangle2D pq1 = new(p1, q1, r1);
            Triangle2D qr2 = new(q2, r2, p1);
            Triangle2D qr22 = new(q2, r2, p2);
            Triangle2D rp2 = new(r2, p2, p1);
            Triangle2D rp22 = new(r2, p2, q2);

            if (Orient2D(ref qr2) >= 0f)
            {
                if (Orient2D(ref rp2) >= 0f) return IntersectionEdge(ref pq1, ref qr22);

                return IntersectionVertex(ref pq1, ref qr22);
            }

            return IntersectionVertex(ref pq1, ref rp22);
        }

        private static bool IntersectionVertex(ref Triangle2D a, ref Triangle2D b)
        {
            ref var p1 = ref a.Point1;
            ref var q1 = ref a.Point2;
            ref var r1 = ref a.Point3;

            ref var p2 = ref b.Point1;
            ref var q2 = ref b.Point2;
            ref var r2 = ref b.Point3;

            Triangle2D t;
            Triangle2D t2;

            t = new(r2, p2, q1);
            if (Orient2D(ref t) >= 0.0f)
            {
                t = new(r2, q2, q1);
                if (Orient2D(ref t) <= 0.0f)
                {
                    t = new(p1, p2, q1);
                    if (Orient2D(ref t) > 0.0f)
                    {
                        t = new(p1, q2, q1);
                        return Orient2D(ref t) <= 0.0f;
                    }

                    t = new(p1, p2, r1);
                    if (Orient2D(ref t) >= 0.0f)
                    {
                        t = new(q1, r1, p2);
                        return Orient2D(ref t) >= 0.0f;
                    }
                }

                t = new(p1, q2, q1);
                t2 = new(r2, q2, r1);
                if (Orient2D(ref t) <= 0.0f && Orient2D(ref t2) <= 0.0f)
                {
                    t = new(q1, r1, q2);
                    return Orient2D(ref t) >= 0.0f;
                }
            }

            t = new(r2, p2, r1);
            if (Orient2D(ref t) >= 0.0f)
            {
                t = new(q1, r1, r2);
                if (Orient2D(ref t) >= 0.0f)
                {
                    t = new(p1, p2, r1);
                    return Orient2D(ref t) >= 0.0f;
                }

                t = new(q1, r1, q2);
                if (Orient2D(ref t) >= 0.0f)
                {
                    t = new(r2, r1, q2);
                    return Orient2D(ref t) >= 0.0f;
                }
            }

            return false;
        }

        private static bool IntersectionEdge(ref Triangle2D a, ref Triangle2D b)
        {
            ref var p1 = ref a.Point1;
            ref var q1 = ref a.Point2;
            ref var r1 = ref a.Point3;

            ref var p2 = ref b.Point1;
            ref var r2 = ref b.Point3;

            Triangle2D t;
            Triangle2D t2;

            t = new(r2, p2, q1);
            if (Orient2D(ref t) >= 0.0f)
            {
                t = new(p1, p2, q1);
                if (Orient2D(ref t) >= 0.0f)
                {
                    t = new(p1, q1, r2);
                    return Orient2D(ref t) >= 0.0f;
                }

                t = new(q1, r1, p2);
                if (Orient2D(ref t) >= 0.0f)
                {
                    t = new(r1, p1, p2);
                    return Orient2D(ref t) >= 0.0f;
                }
            }

            t = new(r2, p2, r1);
            t2 = new(p1, p2, r1);
            if (Orient2D(ref t) >= 0.0f && Orient2D(ref t2) >= 0.0f)
            {
                t = new(p1, r1, r2);
                if (Orient2D(ref t) >= 0.0f)
                {
                    return true;
                }

                t = new(q1, r1, r2);
                return Orient2D(ref t) >= 0.0f;
            }

            return false;
        }

        /// <summary>
        /// Triangle 2D
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="point1">Point 1</param>
        /// <param name="point2">Point 2</param>
        /// <param name="point3">Point 3</param>
        struct Triangle2D(Vector2 point1, Vector2 point2, Vector2 point3)
        {
            /// <summary>
            /// Point 1
            /// </summary>
            public Vector2 Point1 = point1;
            /// <summary>
            /// Point 2
            /// </summary>
            public Vector2 Point2 = point2;
            /// <summary>
            /// Point 3
            /// </summary>
            public Vector2 Point3 = point3;
        }
    }
}
