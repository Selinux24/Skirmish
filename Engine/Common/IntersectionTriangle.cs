using SharpDX;
using System;

namespace Engine.Common
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
        public static bool Intersection(Triangle t1, Triangle t2)
        {
            var res = Intersection(t1, t2, out var coplanar, out var test1, out var test2, out var distances);
            if (!res)
            {
                // Not intersection.
                return false;
            }

            if (coplanar)
            {
                // May be coplanar.
                return Coplanar3D(t1, t2, t1.Normal);
            }

            var intRes = DetectIntersection3D(test1, test2, distances, out var inTest1, out var inTest2);
            if (!intRes)
            {
                // Not intersection or coplanar.
                return Coplanar3D(test1, test2, t1.Normal);
            }

            return CheckMinMax(inTest1, inTest2);
        }
        /// <summary>
        /// Gets whether the specified triangles intersects or not
        /// </summary>
        /// <param name="t1">Triangle 1</param>
        /// <param name="t2">Triangle 2</param>
        /// <param name="segment">If the triangles are not coplanar and there was intersection, returns de intersections segment</param>
        public static bool Intersection(Triangle t1, Triangle t2, out Line3D? segment)
        {
            var res = Intersection(t1, t2, out var coplanar, out var test1, out var test2, out var distances);
            if (!res)
            {
                // Not intersection.
                segment = null;
                return false;
            }

            if (coplanar)
            {
                // May be coplanar.
                segment = null;
                return Coplanar3D(t1, t2, t1.Normal);
            }

            var intRes = DetectIntersection3D(test1, test2, distances, out var inTest1, out var inTest2);
            if (!intRes)
            {
                // Not intersection or coplanar.
                segment = null;
                return Coplanar3D(test1, test2, t1.Normal);
            }

            return ConstructIntersection3D(inTest1, inTest2, t1.Normal, t2.Normal, out segment);
        }

        private static bool Intersection(Triangle t1, Triangle t2, out bool coplanar, out Triangle test1, out Triangle test2, out Vector3 distances)
        {
            coplanar = false;

            // Compute distance signs of p1, q1 and r1 to the plane of triangle(p2,q2,r2)
            var (dist1, dp1, dq1, dr1) = ComputeDistances(t1, t2);
            if (!dist1)
            {
                test1 = new();
                test2 = new();
                distances = new(float.MaxValue);

                return false;
            }

            // Compute distance signs of p2, q2 and r2 to the plane of triangle(p1,q1,r1)
            var (dist2, dp2, dq2, dr2) = ComputeDistances(t2, t1);
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
                var resMajor = TestIntersectionDp1Major(t1, t2, dq1, dr1, dp2, dq2, dr2);

                test1 = resMajor.test1;
                test2 = resMajor.test2;
                distances = resMajor.distances;
            }
            else if (dp1 < 0.0f)
            {
                var resMinor = TestIntersectionDp1Minor(t1, t2, dq1, dr1, dp2, dq2, dr2);

                test1 = resMinor.test1;
                test2 = resMinor.test2;
                distances = resMinor.distances;
            }
            else
            {
                var resZero = TestIntersectionDp1Zero(t1, t2, dq1, dr1, dp2, dq2, dr2);

                test1 = resZero.test1;
                test2 = resZero.test2;
                distances = resZero.distances;
                coplanar = resZero.coplanar;
            }

            return true;
        }
        private static (bool, float dp, float dq, float dr) ComputeDistances(Triangle a, Triangle b)
        {
            float dp = Vector3.Dot(Vector3.Subtract(a.Point1, b.Point3), b.Normal);
            float dq = Vector3.Dot(Vector3.Subtract(a.Point2, b.Point3), b.Normal);
            float dr = Vector3.Dot(Vector3.Subtract(a.Point3, b.Point3), b.Normal);

            if (((dp * dq) > 0.0f) && ((dp * dr) > 0.0f))
            {
                return (false, 0f, 0f, 0f);
            }

            return (true, dp, dq, dr);
        }
        private static (Triangle test1, Triangle test2, Vector3 distances) TestIntersectionDp1Major(Triangle a, Triangle b, float dq1, float dr1, float dp2, float dq2, float dr2)
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
        private static (Triangle test1, Triangle test2, Vector3 distances) TestIntersectionDp1Minor(Triangle a, Triangle b, float dq1, float dr1, float dp2, float dq2, float dr2)
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
        private static (Triangle test1, Triangle test2, Vector3 distances, bool coplanar) TestIntersectionDp1Zero(Triangle a, Triangle b, float dq1, float dr1, float dp2, float dq2, float dr2)
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

        private static bool DetectIntersection3D(Triangle a, Triangle b, Vector3 distances, out Triangle test1, out Triangle test2)
        {
            test1 = new Triangle();
            test2 = new Triangle();

            float dp2 = distances.X;
            float dq2 = distances.Y;
            float dr2 = distances.Z;

            if (dp2 > 0.0f)
            {
                var resMajor = TestIntersection3DDp2Major(a, b, dq2, dr2);

                test1 = resMajor.test1;
                test2 = resMajor.test2;
            }
            else if (dp2 < 0.0f)
            {
                var resMinor = TestIntersection3DDp2Minor(a, b, dq2, dr2);

                test1 = resMinor.test1;
                test2 = resMinor.test2;
            }
            else
            {
                var resZero = TestIntersection3DDp2Zero(a, b, dq2, dr2);

                if (!resZero.res)
                {
                    return false;
                }

                test1 = resZero.test1;
                test2 = resZero.test2;
            }

            return true;
        }
        private static (Triangle test1, Triangle test2) TestIntersection3DDp2Major(Triangle a, Triangle b, float dq2, float dr2)
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
        private static (Triangle test1, Triangle test2) TestIntersection3DDp2Minor(Triangle a, Triangle b, float dq2, float dr2)
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
        private static (Triangle test1, Triangle test2, bool res) TestIntersection3DDp2Zero(Triangle a, Triangle b, float dq2, float dr2)
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

        private static bool ConstructIntersection3D(Triangle a, Triangle b, Vector3 n1, Vector3 n2, out Line3D? segment)
        {
            var v1 = Vector3.Subtract(a.Point2, a.Point1);
            var v2 = Vector3.Subtract(b.Point3, a.Point1);
            var N = Vector3.Cross(v1, v2);
            var v = Vector3.Subtract(b.Point1, a.Point1);

            if (Vector3.Dot(v, N) > 0.0f)
            {
                return ConstructAxis0(a, b, v, v2, n1, n2, out segment);
            }
            else
            {
                return ConstructAxis1(a, b, v, v1, n1, n2, out segment);
            }
        }
        private static bool ConstructAxis0(Triangle a, Triangle b, Vector3 s1, Vector3 s2, Vector3 n1, Vector3 n2, out Line3D? segment)
        {
            segment = null;

            var v1 = Vector3.Subtract(a.Point3, a.Point1);
            var N = Vector3.Cross(v1, s2);
            if (Vector3.Dot(s1, N) > 0.0f)
            {
                return false;
            }

            var v2 = Vector3.Subtract(b.Point2, a.Point1);
            N = Vector3.Cross(v1, v2);
            if (Vector3.Dot(s1, N) > 0.0f)
            {
                v1 = Vector3.Subtract(a.Point1, b.Point1);
                v2 = Vector3.Subtract(a.Point1, a.Point3);
                float alpha = Vector3.Dot(v1, n2) / Vector3.Dot(v2, n2);
                v1 = v2 * alpha;

                Vector3 source = Vector3.Subtract(a.Point1, v1);

                v1 = Vector3.Subtract(b.Point1, a.Point1);
                v2 = Vector3.Subtract(b.Point1, b.Point3);
                alpha = Vector3.Dot(v1, n1) / Vector3.Dot(v2, n1);
                v1 = v2 * alpha;

                Vector3 target = Vector3.Subtract(b.Point1, v1);

                segment = new Line3D(source, target);

                return true;
            }
            else
            {
                v1 = Vector3.Subtract(b.Point1, a.Point1);
                v2 = Vector3.Subtract(b.Point1, b.Point2);
                float alpha = Vector3.Dot(v1, n1) / Vector3.Dot(v2, n1);
                v1 = v2 * alpha;
                Vector3 source = Vector3.Subtract(b.Point1, v1);

                v1 = Vector3.Subtract(b.Point1, a.Point1);
                v2 = Vector3.Subtract(b.Point1, b.Point3);
                alpha = Vector3.Dot(v1, n1) / Vector3.Dot(v2, n1);
                v1 = v2 * alpha;
                Vector3 target = Vector3.Subtract(b.Point1, v1);

                segment = new Line3D(source, target);

                return true;
            }
        }
        private static bool ConstructAxis1(Triangle a, Triangle b, Vector3 s1, Vector3 s2, Vector3 n1, Vector3 n2, out Line3D? segment)
        {
            segment = null;

            var v2 = Vector3.Subtract(b.Point2, a.Point1);
            var N = Vector3.Cross(s2, v2);
            if (Vector3.Dot(s1, N) < 0.0f)
            {
                return false;
            }

            var v1 = Vector3.Subtract(a.Point3, a.Point1);
            N = Vector3.Cross(v1, v2);
            if (Vector3.Dot(s1, N) >= 0.0f)
            {
                v1 = Vector3.Subtract(a.Point1, b.Point1);
                v2 = Vector3.Subtract(a.Point1, a.Point3);
                float alpha = Vector3.Dot(v1, n2) / Vector3.Dot(v2, n2);
                v1 = v2 * alpha;
                Vector3 source = Vector3.Subtract(a.Point1, v1);

                v1 = Vector3.Subtract(a.Point1, b.Point1);
                v2 = Vector3.Subtract(a.Point1, a.Point2);
                alpha = Vector3.Dot(v1, n2) / Vector3.Dot(v2, n2);
                v1 = v2 * alpha;
                Vector3 target = Vector3.Subtract(a.Point1, v1);

                segment = new Line3D(source, target);

                return true;
            }
            else
            {
                v1 = Vector3.Subtract(b.Point1, a.Point1);
                v2 = Vector3.Subtract(b.Point1, b.Point2);
                float alpha = Vector3.Dot(v1, n1) / Vector3.Dot(v2, n1);
                v1 = v2 * alpha;
                Vector3 source = Vector3.Subtract(b.Point1, v1);

                v1 = Vector3.Subtract(a.Point1, b.Point1);
                v2 = Vector3.Subtract(a.Point1, a.Point2);
                alpha = Vector3.Dot(v1, n2) / Vector3.Dot(v2, n2);
                v1 = v2 * alpha;
                Vector3 target = Vector3.Subtract(a.Point1, v1);

                segment = new Line3D(source, target);

                return true;
            }
        }

        private static bool Coplanar3D(Triangle a, Triangle b, Vector3 n1)
        {
            Vector2 p1;
            Vector2 q1;
            Vector2 r1;

            Vector2 p2;
            Vector2 q2;
            Vector2 r2;

            float nX = Math.Abs(n1.X);
            float nY = Math.Abs(n1.Y);
            float nZ = Math.Abs(n1.Z);

            // Projection of the triangles in 3D onto 2D such that the area of the projection is maximized.

            if ((nX > nZ) && (nX >= nY))
            {
                // Project onto plane YZ
                p1 = a.Point2.ZY();
                q1 = a.Point1.ZY();
                r1 = a.Point3.ZY();

                p2 = b.Point2.ZY();
                q2 = b.Point1.ZY();
                r2 = b.Point3.ZY();
            }
            else if ((nY > nZ) && (nY >= nX))
            {
                // Project onto plane XZ
                p1 = a.Point2.XZ();
                q1 = a.Point1.XZ();
                r1 = a.Point3.XZ();

                p2 = b.Point2.XZ();
                q2 = b.Point1.XZ();
                r2 = b.Point3.XZ();
            }
            else
            {
                // Project onto plane XY
                p1 = a.Point1.XY();
                q1 = a.Point2.XY();
                r1 = a.Point3.XY();

                p2 = b.Point1.XY();
                q2 = b.Point2.XY();
                r2 = b.Point3.XY();
            }

            return Overlap2D(new(p1, q1, r1), new(p2, q2, r2));
        }
        private static bool Overlap2D(Triangle2D a, Triangle2D b)
        {
            var p1 = a.Point1;
            var q1 = a.Point2;
            var r1 = a.Point3;

            var p2 = b.Point1;
            var q2 = b.Point2;
            var r2 = b.Point3;

            if (Orient2D(new(p1, q1, r1)) < 0.0f)
            {
                if (Orient2D(new(p2, q2, r2)) < 0.0f)
                {
                    return DetectIntersection2D(new(p1, r1, q1), new(p2, r2, q2));
                }
                else
                {
                    return DetectIntersection2D(new(p1, r1, q1), new(p2, q2, r2));
                }
            }
            else
            {
                if (Orient2D(new(p2, q2, r2)) < 0.0f)
                {
                    return DetectIntersection2D(new(p1, q1, r1), new(p2, r2, q2));
                }
                else
                {
                    return DetectIntersection2D(new(p1, q1, r1), new(p2, q2, r2));
                }
            }
        }
        private static float Orient2D(Triangle2D t)
        {
            var a = t.Point1;
            var b = t.Point2;
            var c = t.Point3;

            return (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X);
        }

        private static bool CheckMinMax(Triangle a, Triangle b)
        {
            var n1 = Vector3.Cross(Vector3.Subtract(b.Point1, a.Point2), Vector3.Subtract(a.Point1, a.Point2));

            if (Vector3.Dot(Vector3.Subtract(b.Point2, a.Point2), n1) > 0.0f)
            {
                return false;
            }

            n1 = Vector3.Cross(Vector3.Subtract(b.Point1, a.Point1), Vector3.Subtract(a.Point3, a.Point1));

            if (Vector3.Dot(Vector3.Subtract(b.Point3, a.Point1), n1) > 0.0f)
            {
                return false;
            }

            return true;
        }

        private static bool DetectIntersection2D(Triangle2D a, Triangle2D b)
        {
            var p1 = a.Point1;
            var q1 = a.Point2;
            var r1 = a.Point3;

            var p2 = b.Point1;
            var q2 = b.Point2;
            var r2 = b.Point3;

            if (Orient2D(new Triangle2D(p2, q2, p1)) >= 0.0f)
            {
                return TestIntersection2DMajor(p1, q1, r1, p2, q2, r2);
            }
            else
            {
                return TestIntersection2DMinor(p1, q1, r1, p2, q2, r2);
            }
        }
        private static bool TestIntersection2DMajor(Vector2 p1, Vector2 q1, Vector2 r1, Vector2 p2, Vector2 q2, Vector2 r2)
        {
            if (Orient2D(new(q2, r2, p1)) >= 0.0f)
            {
                if (Orient2D(new(r2, p2, p1)) >= 0.0f)
                {
                    return true;
                }
                else
                {
                    return IntersectionEdge(new(p1, q1, r1), new(p2, q2, r2));
                }
            }
            else
            {
                if (Orient2D(new(r2, p2, p1)) >= 0.0f)
                {
                    return IntersectionEdge(new(p1, q1, r1), new(r2, p2, q2));
                }
                else
                {
                    return IntersectionVertex(new(p1, q1, r1), new(p2, q2, r2));
                }
            }
        }
        private static bool TestIntersection2DMinor(Vector2 p1, Vector2 q1, Vector2 r1, Vector2 p2, Vector2 q2, Vector2 r2)
        {
            if (Orient2D(new(q2, r2, p1)) >= 0.0f)
            {
                if (Orient2D(new(r2, p2, p1)) >= 0.0f)
                {
                    return IntersectionEdge(new(p1, q1, r1), new(q2, r2, p2));
                }
                else
                {
                    return IntersectionVertex(new(p1, q1, r1), new(q2, r2, p2));
                }
            }
            else
            {
                return IntersectionVertex(new(p1, q1, r1), new(r2, p2, q2));
            }
        }

        private static bool IntersectionVertex(Triangle2D a, Triangle2D b)
        {
            var p1 = a.Point1;
            var q1 = a.Point2;
            var r1 = a.Point3;

            var p2 = b.Point1;
            var q2 = b.Point2;
            var r2 = b.Point3;

            if (Orient2D(new(r2, p2, q1)) >= 0.0f)
            {
                if (Orient2D(new(r2, q2, q1)) <= 0.0f)
                {
                    if (Orient2D(new(p1, p2, q1)) > 0.0f)
                    {
                        return Orient2D(new(p1, q2, q1)) <= 0.0f;
                    }
                    else if (Orient2D(new(p1, p2, r1)) >= 0.0f)
                    {
                        return Orient2D(new(q1, r1, p2)) >= 0.0f;
                    }
                }
                else if (Orient2D(new(p1, q2, q1)) <= 0.0f && Orient2D(new(r2, q2, r1)) <= 0.0f)
                {
                    return Orient2D(new(q1, r1, q2)) >= 0.0f;
                }
            }
            else if (Orient2D(new(r2, p2, r1)) >= 0.0f)
            {
                if (Orient2D(new(q1, r1, r2)) >= 0.0f)
                {
                    return Orient2D(new(p1, p2, r1)) >= 0.0f;
                }
                else if (Orient2D(new(q1, r1, q2)) >= 0.0f)
                {
                    return Orient2D(new(r2, r1, q2)) >= 0.0f;
                }
            }

            return false;
        }

        private static bool IntersectionEdge(Triangle2D a, Triangle2D b)
        {
            var p1 = a.Point1;
            var q1 = a.Point2;
            var r1 = a.Point3;

            var p2 = b.Point1;
            var r2 = b.Point3;

            if (Orient2D(new(r2, p2, q1)) >= 0.0f)
            {
                if (Orient2D(new(p1, p2, q1)) >= 0.0f)
                {
                    return Orient2D(new(p1, q1, r2)) >= 0.0f;
                }
                else if (Orient2D(new(q1, r1, p2)) >= 0.0f)
                {
                    return Orient2D(new(r1, p1, p2)) >= 0.0f;
                }
            }
            else if (Orient2D(new(r2, p2, r1)) >= 0.0f && Orient2D(new(p1, p2, r1)) >= 0.0f)
            {
                if (Orient2D(new(p1, r1, r2)) >= 0.0f)
                {
                    return true;
                }

                return Orient2D(new(q1, r1, r2)) >= 0.0f;
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
            public Vector2 Point1 { get; set; } = point1;
            /// <summary>
            /// Point 2
            /// </summary>
            public Vector2 Point2 { get; set; } = point2;
            /// <summary>
            /// Point 3
            /// </summary>
            public Vector2 Point3 { get; set; } = point3;
        }
    }
}
