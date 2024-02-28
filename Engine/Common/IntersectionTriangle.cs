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

            float dp1 = Vector3.Dot(Vector3.Subtract(t1.Point1, t2.Point3), t2.Normal);
            float dq1 = Vector3.Dot(Vector3.Subtract(t1.Point2, t2.Point3), t2.Normal);
            float dr1 = Vector3.Dot(Vector3.Subtract(t1.Point3, t2.Point3), t2.Normal);

            if (((dp1 * dq1) > 0.0f) && ((dp1 * dr1) > 0.0f))
            {
                test1 = new Triangle();
                test2 = new Triangle();
                distances = new Vector3(float.MaxValue);

                return false;
            }

            // Compute distance signs of p2, q2 and r2 to the plane of triangle(p1,q1,r1)

            float dp2 = Vector3.Dot(Vector3.Subtract(t2.Point1, t1.Point3), t1.Normal);
            float dq2 = Vector3.Dot(Vector3.Subtract(t2.Point2, t1.Point3), t1.Normal);
            float dr2 = Vector3.Dot(Vector3.Subtract(t2.Point3, t1.Point3), t1.Normal);

            if (((dp2 * dq2) > 0.0f) && ((dp2 * dr2) > 0.0f))
            {
                test1 = new Triangle();
                test2 = new Triangle();
                distances = new Vector3(float.MaxValue);

                return false;
            }

            // Permutation in a canonical form of T1's vertices

            if (dp1 > 0.0f)
            {
                if (dq1 > 0.0f)
                {
                    test1 = new Triangle(t1.Point3, t1.Point1, t1.Point2);
                    test2 = new Triangle(t2.Point1, t2.Point3, t2.Point2);
                    distances = new Vector3(dp2, dr2, dq2);
                }
                else if (dr1 > 0.0f)
                {
                    test1 = new Triangle(t1.Point2, t1.Point3, t1.Point1);
                    test2 = new Triangle(t2.Point1, t2.Point3, t2.Point2);
                    distances = new Vector3(dp2, dr2, dq2);
                }
                else
                {
                    test1 = new Triangle(t1.Point1, t1.Point2, t1.Point3);
                    test2 = new Triangle(t2.Point1, t2.Point2, t2.Point3);
                    distances = new Vector3(dp2, dq2, dr2);
                }
            }
            else if (dp1 < 0.0f)
            {
                if (dq1 < 0.0f)
                {
                    test1 = new Triangle(t1.Point3, t1.Point1, t1.Point2);
                    test2 = new Triangle(t2.Point1, t2.Point2, t2.Point3);
                    distances = new Vector3(dp2, dq2, dr2);
                }
                else if (dr1 < 0.0f)
                {
                    test1 = new Triangle(t1.Point2, t1.Point3, t1.Point1);
                    test2 = new Triangle(t2.Point1, t2.Point2, t2.Point3);
                    distances = new Vector3(dp2, dq2, dr2);
                }
                else
                {
                    test1 = new Triangle(t1.Point1, t1.Point2, t1.Point3);
                    test2 = new Triangle(t2.Point1, t2.Point3, t2.Point2);
                    distances = new Vector3(dp2, dr2, dq2);
                }
            }
            else
            {
                if (dq1 < 0.0f)
                {
                    if (dr1 >= 0.0f)
                    {
                        test1 = new Triangle(t1.Point2, t1.Point3, t1.Point1);
                        test2 = new Triangle(t2.Point1, t2.Point3, t2.Point2);
                        distances = new Vector3(dp2, dr2, dq2);
                    }
                    else
                    {
                        test1 = new Triangle(t1.Point1, t1.Point2, t1.Point3);
                        test2 = new Triangle(t2.Point1, t2.Point2, t2.Point3);
                        distances = new Vector3(dp2, dq2, dr2);
                    }
                }
                else if (dq1 > 0.0f)
                {
                    if (dr1 > 0.0f)
                    {
                        test1 = new Triangle(t1.Point1, t1.Point2, t1.Point3);
                        test2 = new Triangle(t2.Point1, t2.Point3, t2.Point2);
                        distances = new Vector3(dp2, dr2, dq2);
                    }
                    else
                    {
                        test1 = new Triangle(t1.Point2, t1.Point3, t1.Point1);
                        test2 = new Triangle(t2.Point1, t2.Point2, t2.Point3);
                        distances = new Vector3(dp2, dq2, dr2);
                    }
                }
                else
                {
                    if (dr1 > 0.0f)
                    {
                        test1 = new Triangle(t1.Point3, t1.Point1, t1.Point2);
                        test2 = new Triangle(t2.Point1, t2.Point2, t2.Point3);
                        distances = new Vector3(dp2, dq2, dr2);
                    }
                    else if (dr1 < 0.0f)
                    {
                        test1 = new Triangle(t1.Point3, t1.Point1, t1.Point2);
                        test2 = new Triangle(t2.Point1, t2.Point3, t2.Point2);
                        distances = new Vector3(dp2, dr2, dq2);
                    }
                    else
                    {
                        // triangles are co-planar
                        coplanar = true;
                        test1 = new Triangle();
                        test2 = new Triangle();
                        distances = new Vector3(float.MaxValue);

                        return true;
                    }
                }
            }

            return true;
        }

        private static bool DetectIntersection3D(Triangle t1, Triangle t2, Vector3 distances, out Triangle test1, out Triangle test2)
        {
            test1 = new Triangle();
            test2 = new Triangle();

            float dp2 = distances.X;
            float dq2 = distances.Y;
            float dr2 = distances.Z;

            if (dp2 > 0.0f)
            {
                if (dq2 > 0.0f)
                {
                    test1 = new Triangle(t1.Point1, t1.Point3, t1.Point2);
                    test2 = new Triangle(t2.Point3, t2.Point1, t2.Point2);
                }
                else if (dr2 > 0.0f)
                {
                    test1 = new Triangle(t1.Point1, t1.Point3, t1.Point2);
                    test2 = new Triangle(t2.Point2, t2.Point3, t2.Point1);
                }
                else
                {
                    test1 = new Triangle(t1.Point1, t1.Point2, t1.Point3);
                    test2 = new Triangle(t2.Point1, t2.Point2, t2.Point3);
                }
            }
            else if (dp2 < 0.0f)
            {
                if (dq2 < 0.0f)
                {
                    test1 = new Triangle(t1.Point1, t1.Point2, t1.Point3);
                    test2 = new Triangle(t2.Point3, t2.Point1, t2.Point2);
                }
                else if (dr2 < 0.0f)
                {
                    test1 = new Triangle(t1.Point1, t1.Point2, t1.Point3);
                    test2 = new Triangle(t2.Point2, t2.Point3, t2.Point1);
                }
                else
                {
                    test1 = new Triangle(t1.Point1, t1.Point3, t1.Point2);
                    test2 = new Triangle(t2.Point1, t2.Point2, t2.Point3);
                }
            }
            else
            {
                if (dq2 < 0.0f)
                {
                    if (dr2 >= 0.0f)
                    {
                        test1 = new Triangle(t1.Point1, t1.Point3, t1.Point2);
                        test2 = new Triangle(t2.Point2, t2.Point3, t2.Point1);
                    }
                    else
                    {
                        test1 = new Triangle(t1.Point1, t1.Point2, t1.Point3);
                        test2 = new Triangle(t2.Point1, t2.Point2, t2.Point3);
                    }
                }
                else if (dq2 > 0.0f)
                {
                    if (dr2 > 0.0f)
                    {
                        test1 = new Triangle(t1.Point1, t1.Point3, t1.Point2);
                        test2 = new Triangle(t2.Point1, t2.Point2, t2.Point3);
                    }
                    else
                    {
                        test1 = new Triangle(t1.Point1, t1.Point2, t1.Point3);
                        test2 = new Triangle(t2.Point2, t2.Point3, t2.Point1);
                    }
                }
                else
                {
                    if (dr2 > 0.0f)
                    {
                        test1 = new Triangle(t1.Point1, t1.Point2, t1.Point3);
                        test2 = new Triangle(t2.Point3, t2.Point1, t2.Point2);
                    }
                    else if (dr2 < 0.0f)
                    {
                        test1 = new Triangle(t1.Point1, t1.Point3, t1.Point2);
                        test2 = new Triangle(t2.Point3, t2.Point1, t2.Point2);
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool ConstructIntersection3D(Triangle t1, Triangle t2, Vector3 n1, Vector3 n2, out Line3D? segment)
        {
            var v1 = Vector3.Subtract(t1.Point2, t1.Point1);
            var v2 = Vector3.Subtract(t2.Point3, t1.Point1);
            var N = Vector3.Cross(v1, v2);
            var v = Vector3.Subtract(t2.Point1, t1.Point1);

            if (Vector3.Dot(v, N) > 0.0f)
            {
                return ConstructAxis0(t1, t2, v, v2, n1, n2, out segment);
            }
            else
            {
                return ConstructAxis1(t1, t2, v, v1, n1, n2, out segment);
            }
        }

        private static bool ConstructAxis0(Triangle t1, Triangle t2, Vector3 s1, Vector3 s2, Vector3 n1, Vector3 n2, out Line3D? segment)
        {
            segment = null;

            var v1 = Vector3.Subtract(t1.Point3, t1.Point1);
            var N = Vector3.Cross(v1, s2);
            if (Vector3.Dot(s1, N) > 0.0f)
            {
                return false;
            }

            var v2 = Vector3.Subtract(t2.Point2, t1.Point1);
            N = Vector3.Cross(v1, v2);
            if (Vector3.Dot(s1, N) > 0.0f)
            {
                v1 = Vector3.Subtract(t1.Point1, t2.Point1);
                v2 = Vector3.Subtract(t1.Point1, t1.Point3);
                float alpha = Vector3.Dot(v1, n2) / Vector3.Dot(v2, n2);
                v1 = v2 * alpha;

                Vector3 source = Vector3.Subtract(t1.Point1, v1);

                v1 = Vector3.Subtract(t2.Point1, t1.Point1);
                v2 = Vector3.Subtract(t2.Point1, t2.Point3);
                alpha = Vector3.Dot(v1, n1) / Vector3.Dot(v2, n1);
                v1 = v2 * alpha;

                Vector3 target = Vector3.Subtract(t2.Point1, v1);

                segment = new Line3D(source, target);

                return true;
            }
            else
            {
                v1 = Vector3.Subtract(t2.Point1, t1.Point1);
                v2 = Vector3.Subtract(t2.Point1, t2.Point2);
                float alpha = Vector3.Dot(v1, n1) / Vector3.Dot(v2, n1);
                v1 = v2 * alpha;
                Vector3 source = Vector3.Subtract(t2.Point1, v1);

                v1 = Vector3.Subtract(t2.Point1, t1.Point1);
                v2 = Vector3.Subtract(t2.Point1, t2.Point3);
                alpha = Vector3.Dot(v1, n1) / Vector3.Dot(v2, n1);
                v1 = v2 * alpha;
                Vector3 target = Vector3.Subtract(t2.Point1, v1);

                segment = new Line3D(source, target);

                return true;
            }
        }

        private static bool ConstructAxis1(Triangle t1, Triangle t2, Vector3 s1, Vector3 s2, Vector3 n1, Vector3 n2, out Line3D? segment)
        {
            segment = null;

            var v2 = Vector3.Subtract(t2.Point2, t1.Point1);
            var N = Vector3.Cross(s2, v2);
            if (Vector3.Dot(s1, N) < 0.0f)
            {
                return false;
            }

            var v1 = Vector3.Subtract(t1.Point3, t1.Point1);
            N = Vector3.Cross(v1, v2);
            if (Vector3.Dot(s1, N) >= 0.0f)
            {
                v1 = Vector3.Subtract(t1.Point1, t2.Point1);
                v2 = Vector3.Subtract(t1.Point1, t1.Point3);
                float alpha = Vector3.Dot(v1, n2) / Vector3.Dot(v2, n2);
                v1 = v2 * alpha;
                Vector3 source = Vector3.Subtract(t1.Point1, v1);

                v1 = Vector3.Subtract(t1.Point1, t2.Point1);
                v2 = Vector3.Subtract(t1.Point1, t1.Point2);
                alpha = Vector3.Dot(v1, n2) / Vector3.Dot(v2, n2);
                v1 = v2 * alpha;
                Vector3 target = Vector3.Subtract(t1.Point1, v1);

                segment = new Line3D(source, target);

                return true;
            }
            else
            {
                v1 = Vector3.Subtract(t2.Point1, t1.Point1);
                v2 = Vector3.Subtract(t2.Point1, t2.Point2);
                float alpha = Vector3.Dot(v1, n1) / Vector3.Dot(v2, n1);
                v1 = v2 * alpha;
                Vector3 source = Vector3.Subtract(t2.Point1, v1);

                v1 = Vector3.Subtract(t1.Point1, t2.Point1);
                v2 = Vector3.Subtract(t1.Point1, t1.Point2);
                alpha = Vector3.Dot(v1, n2) / Vector3.Dot(v2, n2);
                v1 = v2 * alpha;
                Vector3 target = Vector3.Subtract(t1.Point1, v1);

                segment = new Line3D(source, target);

                return true;
            }
        }

        private static bool Coplanar3D(Triangle t1, Triangle t2, Vector3 n1)
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
                p1 = t1.Point2.ZY();
                q1 = t1.Point1.ZY();
                r1 = t1.Point3.ZY();

                p2 = t2.Point2.ZY();
                q2 = t2.Point1.ZY();
                r2 = t2.Point3.ZY();
            }
            else if ((nY > nZ) && (nY >= nX))
            {
                // Project onto plane XZ
                p1 = t1.Point2.XZ();
                q1 = t1.Point1.XZ();
                r1 = t1.Point3.XZ();

                p2 = t2.Point2.XZ();
                q2 = t2.Point1.XZ();
                r2 = t2.Point3.XZ();
            }
            else
            {
                // Project onto plane XY
                p1 = t1.Point1.XY();
                q1 = t1.Point2.XY();
                r1 = t1.Point3.XY();

                p2 = t2.Point1.XY();
                q2 = t2.Point2.XY();
                r2 = t2.Point3.XY();
            }

            return Overlap2D(new Triangle2D(p1, q1, r1), new Triangle2D(p2, q2, r2));
        }

        private static bool CheckMinMax(Triangle t1, Triangle t2)
        {
            var n1 = Vector3.Cross(Vector3.Subtract(t2.Point1, t1.Point2), Vector3.Subtract(t1.Point1, t1.Point2));

            if (Vector3.Dot(Vector3.Subtract(t2.Point2, t1.Point2), n1) > 0.0f)
            {
                return false;
            }

            n1 = Vector3.Cross(Vector3.Subtract(t2.Point1, t1.Point1), Vector3.Subtract(t1.Point3, t1.Point1));

            if (Vector3.Dot(Vector3.Subtract(t2.Point3, t1.Point1), n1) > 0.0f)
            {
                return false;
            }

            return true;
        }

        private static bool Overlap2D(Triangle2D t1, Triangle2D t2)
        {
            var p1 = t1.Point1;
            var q1 = t1.Point2;
            var r1 = t1.Point3;

            var p2 = t2.Point1;
            var q2 = t2.Point2;
            var r2 = t2.Point3;

            if (Orient2D(new Triangle2D(p1, q1, r1)) < 0.0f)
            {
                if (Orient2D(new Triangle2D(p2, q2, r2)) < 0.0f)
                {
                    return DetectIntersection2D(new Triangle2D(p1, r1, q1), new Triangle2D(p2, r2, q2));
                }
                else
                {
                    return DetectIntersection2D(new Triangle2D(p1, r1, q1), new Triangle2D(p2, q2, r2));
                }
            }
            else
            {
                if (Orient2D(new Triangle2D(p2, q2, r2)) < 0.0f)
                {
                    return DetectIntersection2D(new Triangle2D(p1, q1, r1), new Triangle2D(p2, r2, q2));
                }
                else
                {
                    return DetectIntersection2D(new Triangle2D(p1, q1, r1), new Triangle2D(p2, q2, r2));
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

        private static bool DetectIntersection2D(Triangle2D t1, Triangle2D t2)
        {
            var p1 = t1.Point1;
            var q1 = t1.Point2;
            var r1 = t1.Point3;

            var p2 = t2.Point1;
            var q2 = t2.Point2;
            var r2 = t2.Point3;

            if (Orient2D(new Triangle2D(p2, q2, p1)) >= 0.0f)
            {
                if (Orient2D(new Triangle2D(q2, r2, p1)) >= 0.0f)
                {
                    if (Orient2D(new Triangle2D(r2, p2, p1)) >= 0.0f)
                    {
                        return true;
                    }
                    else
                    {
                        return IntersectionEdge(new Triangle2D(p1, q1, r1), new Triangle2D(p2, q2, r2));
                    }
                }
                else
                {
                    if (Orient2D(new Triangle2D(r2, p2, p1)) >= 0.0f)
                    {
                        return IntersectionEdge(new Triangle2D(p1, q1, r1), new Triangle2D(r2, p2, q2));
                    }
                    else
                    {
                        return IntersectionVertex(new Triangle2D(p1, q1, r1), new Triangle2D(p2, q2, r2));
                    }
                }
            }
            else
            {
                if (Orient2D(new Triangle2D(q2, r2, p1)) >= 0.0f)
                {
                    if (Orient2D(new Triangle2D(r2, p2, p1)) >= 0.0f)
                    {
                        return IntersectionEdge(new Triangle2D(p1, q1, r1), new Triangle2D(q2, r2, p2));
                    }
                    else
                    {
                        return IntersectionVertex(new Triangle2D(p1, q1, r1), new Triangle2D(q2, r2, p2));
                    }
                }
                else
                {
                    return IntersectionVertex(new Triangle2D(p1, q1, r1), new Triangle2D(r2, p2, q2));
                }
            }
        }

        private static bool IntersectionVertex(Triangle2D t1, Triangle2D t2)
        {
            var p1 = t1.Point1;
            var q1 = t1.Point2;
            var r1 = t1.Point3;

            var p2 = t2.Point1;
            var q2 = t2.Point2;
            var r2 = t2.Point3;

            if (Orient2D(new Triangle2D(r2, p2, q1)) >= 0.0f)
            {
                if (Orient2D(new Triangle2D(r2, q2, q1)) <= 0.0f)
                {
                    if (Orient2D(new Triangle2D(p1, p2, q1)) > 0.0f)
                    {
                        return Orient2D(new Triangle2D(p1, q2, q1)) <= 0.0f;
                    }
                    else if (Orient2D(new Triangle2D(p1, p2, r1)) >= 0.0f)
                    {
                        return Orient2D(new Triangle2D(q1, r1, p2)) >= 0.0f;
                    }
                }
                else if (Orient2D(new Triangle2D(p1, q2, q1)) <= 0.0f && Orient2D(new Triangle2D(r2, q2, r1)) <= 0.0f)
                {
                    return Orient2D(new Triangle2D(q1, r1, q2)) >= 0.0f;
                }
            }
            else if (Orient2D(new Triangle2D(r2, p2, r1)) >= 0.0f)
            {
                if (Orient2D(new Triangle2D(q1, r1, r2)) >= 0.0f)
                {
                    return Orient2D(new Triangle2D(p1, p2, r1)) >= 0.0f;
                }
                else if (Orient2D(new Triangle2D(q1, r1, q2)) >= 0.0f)
                {
                    return Orient2D(new Triangle2D(r2, r1, q2)) >= 0.0f;
                }
            }

            return false;
        }

        private static bool IntersectionEdge(Triangle2D t1, Triangle2D t2)
        {
            var p1 = t1.Point1;
            var q1 = t1.Point2;
            var r1 = t1.Point3;

            var p2 = t2.Point1;
            var r2 = t2.Point3;

            if (Orient2D(new Triangle2D(r2, p2, q1)) >= 0.0f)
            {
                if (Orient2D(new Triangle2D(p1, p2, q1)) >= 0.0f)
                {
                    return Orient2D(new Triangle2D(p1, q1, r2)) >= 0.0f;
                }
                else if (Orient2D(new Triangle2D(q1, r1, p2)) >= 0.0f)
                {
                    return Orient2D(new Triangle2D(r1, p1, p2)) >= 0.0f;
                }
            }
            else if (Orient2D(new Triangle2D(r2, p2, r1)) >= 0.0f && Orient2D(new Triangle2D(p1, p2, r1)) >= 0.0f)
            {
                if (Orient2D(new Triangle2D(p1, r1, r2)) >= 0.0f)
                {
                    return true;
                }

                return Orient2D(new Triangle2D(q1, r1, r2)) >= 0.0f;
            }

            return false;
        }

        /// <summary>
        /// Triangle 2D
        /// </summary>
        struct Triangle2D
        {
            /// <summary>
            /// Point 1
            /// </summary>
            public Vector2 Point1 { get; set; }
            /// <summary>
            /// Point 2
            /// </summary>
            public Vector2 Point2 { get; set; }
            /// <summary>
            /// Point 3
            /// </summary>
            public Vector2 Point3 { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="point1">Point 1</param>
            /// <param name="point2">Point 2</param>
            /// <param name="point3">Point 3</param>
            public Triangle2D(Vector2 point1, Vector2 point2, Vector2 point3)
            {
                Point1 = point1;
                Point2 = point2;
                Point3 = point3;
            }
        }
    }
}
