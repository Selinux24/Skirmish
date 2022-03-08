using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Intersections
    /// </summary>
    public static class Intersection
    {
        /// <summary>
        /// Determines whether a sphere intersects with a sphere
        /// </summary>
        /// <param name="sphere1">Sphere one</param>
        /// <param name="sphere2">Sphere two</param>
        /// <returns>Returns true if the sphere one intersects the sphere two</returns>
        public static bool SphereIntersectsSphere(BoundingSphere sphere1, BoundingSphere sphere2)
        {
            return sphere1.Intersects(sphere2);
        }
        /// <summary>
        /// Determines whether a sphere intersects with a box
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="box">Axis aligned box</param>
        /// <returns>Returns true if the sphere intersects the box</returns>
        public static bool SphereIntersectsBox(BoundingSphere sphere, BoundingBox box)
        {
            return sphere.Intersects(box);
        }
        /// <summary>
        /// Determines whether a sphere intersects with a frustum
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns true if the sphere intersects the frustum</returns>
        public static bool SphereIntersectsFrustum(BoundingSphere sphere, BoundingFrustum frustum)
        {
            return SphereContainsFrustum(sphere, frustum) != ContainmentType.Disjoint;
        }
        /// <summary>
        /// Determines whether a sphere intersects with a triangle
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns true if the sphere intersects the triangle</returns>
        public static bool SphereIntersectsTriangle(BoundingSphere sphere, Triangle triangle)
        {
            var p1 = triangle.Point1;
            var p2 = triangle.Point2;
            var p3 = triangle.Point3;

            return sphere.Intersects(ref p1, ref p2, ref p3);
        }
        /// <summary>
        /// Determines whether a sphere intersects with a triangle
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="triangle">Triangle</param>
        /// <param name="result">Returns the piking result</param>
        /// <returns>Returns true if the sphere intersects the triangle</returns>
        public static bool SphereIntersectsTriangle(BoundingSphere sphere, Triangle triangle, out PickingResult<Triangle> result)
        {
            var point = ClosestPointInTriangle(sphere.Center, triangle);

            float distance = Vector3.Distance(sphere.Center, point);

            if (distance <= sphere.Radius)
            {
                result = new PickingResult<Triangle>
                {
                    Primitive = triangle,
                    Distance = distance,
                    Position = point,
                };

                return true;
            }

            result = new PickingResult<Triangle>
            {
                Distance = float.MaxValue,
            };

            return false;
        }
        /// <summary>
        /// Determines whether a sphere intersects with a triangle mesh
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="result">Returns the piking result</param>
        /// <returns>Returns true if the sphere intersects the triangle mesh</returns>
        public static bool SphereIntersectsMesh(BoundingSphere sphere, IEnumerable<Triangle> mesh, out PickingResult<Triangle> result)
        {
            ClosestPointInMesh(sphere.Center, mesh, out var triangle, out var point);

            float distance = Vector3.Distance(sphere.Center, point);

            if (distance <= sphere.Radius)
            {
                result = new PickingResult<Triangle>
                {
                    Primitive = triangle,
                    Distance = distance,
                    Position = point,
                };

                return true;
            }

            result = new PickingResult<Triangle>
            {
                Distance = float.MaxValue,
            };

            return false;
        }
        /// <summary>
        /// Determines whether a sphere intersects with a triangle mesh
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="triangles">Returns the intersected triangle list</param>
        /// <param name="points">Returns the intersection point list</param>
        /// <param name="distances">Returns the distance list from the sphere center to the intersection points</param>
        /// <returns>Returns true if the sphere intersects the triangle mesh</returns>
        public static bool SphereIntersectsMeshAll(BoundingSphere sphere, IEnumerable<Triangle> mesh, out IEnumerable<PickingResult<Triangle>> results)
        {
            List<PickingResult<Triangle>> picks = new List<PickingResult<Triangle>>();

            foreach (var t in mesh)
            {
                var closestToTri = ClosestPointInTriangle(sphere.Center, t);

                float dist = Vector3.DistanceSquared(sphere.Center, closestToTri);
                if (dist <= sphere.Radius)
                {
                    // Store intersection data
                    picks.Add(new PickingResult<Triangle> { Primitive = t, Position = closestToTri, Distance = dist });
                }
            }

            results = picks;

            return picks.Any();
        }

        /// <summary>
        /// Determines whether a box intersects with a sphere
        /// </summary>
        /// <param name="box">Axis aligned box</param>
        /// <param name="sphere">Sphere</param>
        /// <returns>Returns true if the box intersects the sphere</returns>
        public static bool BoxIntersectsSphere(BoundingBox box, BoundingSphere sphere)
        {
            return box.Intersects(sphere);
        }
        /// <summary>
        /// Determines whether a box intersects with a box
        /// </summary>
        /// <param name="box1">Axis aligned box one</param>
        /// <param name="box2">Axis aligned box two</param>
        /// <returns>Returns true if the box one intersects the box two</returns>
        public static bool BoxIntersectsBox(BoundingBox box1, BoundingBox box2)
        {
            return box1.Intersects(box2);
        }
        /// <summary>
        /// Determines whether a box intersects with a frustum
        /// </summary>
        /// <param name="box">Axis aligned box</param>
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns true if the box intersects the frustum</returns>
        public static bool BoxIntersectsFrustum(BoundingBox box, BoundingFrustum frustum)
        {
            return BoxContainsFrustum(box, frustum) != ContainmentType.Disjoint;
        }
        /// <summary>
        /// Determines whether a box intersects with a triangle
        /// </summary>
        /// <param name="box">Bounding box</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns true if the box intersects the triangle</returns>
        /// <remarks>Separating axis theorem implementation adapted from "https://github.com/typicalMoves/dava.engine/blob/main/Sources/Internal/Math/AABBox3.cpp"</remarks>
        public static bool BoxIntersectsTriangle(BoundingBox box, Triangle triangle)
        {
            // Translate the triangle to origin, and use only the box extents to refer to the box
            var boxCenter = box.GetCenter();
            var boxExtents = box.GetExtents();
            Triangle origTri = new Triangle(triangle.Point1 - boxCenter, triangle.Point2 - boxCenter, triangle.Point3 - boxCenter);

            // Test first 3 edges
            var edge1 = triangle.GetEdge1();
            float fex = Math.Abs(edge1.X);
            float fey = Math.Abs(edge1.Y);
            float fez = Math.Abs(edge1.Z);
            if (!AxisTestX01(boxExtents, origTri, edge1.Z, edge1.Y, fez, fey)) return false;
            if (!AxisTestY02(boxExtents, origTri, edge1.Z, edge1.X, fez, fex)) return false;
            if (!AxisTestZ12(boxExtents, origTri, edge1.Y, edge1.X, fey, fex)) return false;

            // Test second 3 edges
            var edge2 = triangle.GetEdge2();
            fex = Math.Abs(edge2.X);
            fey = Math.Abs(edge2.Y);
            fez = Math.Abs(edge2.Z);
            if (!AxisTestX01(boxExtents, origTri, edge2.Z, edge2.Y, fez, fey)) return false;
            if (!AxisTestY02(boxExtents, origTri, edge2.Z, edge2.X, fez, fex)) return false;
            if (!AxisTestZ0(boxExtents, origTri, edge2.Y, edge2.X, fey, fex)) return false;

            // Test third 3 edges
            var edge3 = triangle.GetEdge3();
            fex = Math.Abs(edge3.X);
            fey = Math.Abs(edge3.Y);
            fez = Math.Abs(edge3.Z);
            if (!AxisTestX2(boxExtents, origTri, edge3.Z, edge3.Y, fez, fey)) return false;
            if (!AxisTestY1(boxExtents, origTri, edge3.Z, edge3.X, fez, fex)) return false;
            if (!AxisTestZ12(boxExtents, origTri, edge3.Y, edge3.X, fey, fex)) return false;

            // Test X direction
            Helper.MinMax(origTri.Point1.X, origTri.Point2.X, origTri.Point3.X, out float min, out float max);
            if (min > boxExtents.X || max < -boxExtents.X)
            {
                return false;
            }

            // Test Y direction
            Helper.MinMax(origTri.Point1.Y, origTri.Point2.Y, origTri.Point3.Y, out min, out max);
            if (min > boxExtents.Y || max < -boxExtents.Y)
            {
                return false;
            }

            // Test Z direction
            Helper.MinMax(origTri.Point1.Z, origTri.Point2.Z, origTri.Point3.Z, out min, out max);
            if (min > boxExtents.Z || max < -boxExtents.Z)
            {
                return false;
            }

            // Test the box extents vs the triangle plane
            BoundingBox aabb = new BoundingBox(-boxExtents, boxExtents);
            return triangle.Plane.Intersects(ref aabb) == PlaneIntersectionType.Intersecting;
        }
        /// <summary>
        /// Determines whether a box intersects with a mesh
        /// </summary>
        /// <param name="box">Bounding box</param>
        /// <param name="mesh">Mesh</param>
        /// <returns>Returns true if the box intersects the mesh</returns>
        public static bool BoxIntersectsMesh(BoundingBox box, IEnumerable<Triangle> mesh)
        {
            //Test containment 1 to 2
            foreach (var tri in mesh)
            {
                if (BoxContainsTriangle(box, tri) != ContainmentType.Disjoint)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AxisTestX01(Vector3 extents, Triangle tri, float a, float b, float fa, float fb)
        {
            float p0 = a * tri.Point1.Y - b * tri.Point1.Z;
            float p2 = a * tri.Point3.Y - b * tri.Point3.Z;
            Helper.MinMax(p0, p2, out float min, out float max);

            float rad = fa * extents.Y + fb * extents.Z;
            if (min > rad || max < -rad)
            {
                return false;
            }

            return true;
        }
        private static bool AxisTestX2(Vector3 extents, Triangle tri, float a, float b, float fa, float fb)
        {
            float p0 = a * tri.Point1.Y - b * tri.Point1.Z;
            float p1 = a * tri.Point2.Y - b * tri.Point2.Z;
            Helper.MinMax(p0, p1, out float min, out float max);

            float rad = fa * extents.Y + fb * extents.Z;
            if (min > rad || max < -rad)
            {
                return false;
            }

            return true;
        }
        private static bool AxisTestY02(Vector3 extents, Triangle tri, float a, float b, float fa, float fb)
        {
            float p0 = -a * tri.Point1.X + b * tri.Point1.Z;
            float p2 = -a * tri.Point3.X + b * tri.Point3.Z;
            Helper.MinMax(p0, p2, out float min, out float max);

            float rad = fa * extents.X + fb * extents.Z;
            if (min > rad || max < -rad)
            {
                return false;
            }

            return true;
        }
        private static bool AxisTestY1(Vector3 extents, Triangle tri, float a, float b, float fa, float fb)
        {
            float p0 = -a * tri.Point1.X + b * tri.Point1.Z;
            float p1 = -a * tri.Point2.X + b * tri.Point2.Z;
            Helper.MinMax(p0, p1, out float min, out float max);

            float rad = fa * extents.X + fb * extents.Z;
            if (min > rad || max < -rad)
            {
                return false;
            }

            return true;
        }
        private static bool AxisTestZ12(Vector3 extents, Triangle tri, float a, float b, float fa, float fb)
        {
            float p1 = a * tri.Point2.X - b * tri.Point2.Y;
            float p2 = a * tri.Point3.X - b * tri.Point3.Y;
            Helper.MinMax(p1, p2, out float min, out float max);

            float rad = fa * extents.X + fb * extents.Y;
            if (min > rad || max < -rad)
            {
                return false;
            }

            return true;
        }
        private static bool AxisTestZ0(Vector3 extents, Triangle tri, float a, float b, float fa, float fb)
        {
            float p0 = a * tri.Point1.X - b * tri.Point1.Y;
            float p1 = a * tri.Point2.X - b * tri.Point2.Y;
            Helper.MinMax(p0, p1, out float min, out float max);

            float rad = fa * extents.X + fb * extents.Y;
            if (min > rad || max < -rad)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether a frustum intersects with a frustum
        /// </summary>
        /// <param name="frustum1">Frustum one</param>
        /// <param name="frustum2">Frustum two</param>
        /// <returns>Returns true if the frustum one intersects the frustum two</returns>
        public static bool FrustumIntersectsFrustum(BoundingFrustum frustum1, BoundingFrustum frustum2)
        {
            var res1to2 = FrustumContainsFrustum(frustum1, frustum2);
            if (res1to2 != ContainmentType.Disjoint)
            {
                return true;
            }

            return FrustumContainsFrustum(frustum2, frustum1) != ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines whether a mesh intersects with a mesh
        /// </summary>
        /// <param name="mesh1">Mesh one</param>
        /// <param name="mesh2">Mesh two</param>
        /// <param name="triangles">Returns the intersected triangles of the mesh two</param>
        /// <param name="segments">Returns the intersection segment list</param>
        /// <returns>Returns true if the mesh one intersects the mesh two</returns>
        public static bool MeshIntersectsMesh(IEnumerable<Triangle> mesh1, IEnumerable<Triangle> mesh2, out IEnumerable<Triangle> triangles, out IEnumerable<Line3D> segments)
        {
            List<Triangle> tris = new List<Triangle>();
            List<Line3D> segs = new List<Line3D>();

            foreach (var t in mesh1)
            {
                if (TriangleIntersectsMesh(t, mesh2, out var mTris, out var mSegs))
                {
                    tris.AddRange(mTris);
                    segs.AddRange(mSegs);
                }
            }

            triangles = tris.Distinct().ToArray();
            segments = segs.Distinct().ToArray();

            return triangles.Any();
        }

        /// <summary>
        /// Determines whether there is an intersection between a Ray and a BoundingBox
        /// </summary>
        /// <param name="ray">The ray to test</param>
        /// <param name="box">The box to test</param>
        /// <returns>Whether the two objects intersected</returns>
        public static bool RayIntersectsBox(PickingRay ray, BoundingBox box)
        {
            return RayIntersectsBox(ray, box, out _);
        }
        /// <summary>
        /// Determines whether there is an intersection between a Ray and a BoundingBox
        /// </summary>
        /// <param name="ray">The ray to test</param>
        /// <param name="box">The box to test</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection, or 0 if there was no intersection</param>
        /// <returns>Whether the two objects intersected</returns>
        public static bool RayIntersectsBox(PickingRay ray, BoundingBox box, out float distance)
        {
            Ray rRay = ray;
            if (Collision.RayIntersectsBox(ref rRay, ref box, out distance) && distance <= ray.MaxDistance)
            {
                return true;
            }

            distance = float.MaxValue;

            return false;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a triangle.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="tri">Triangle</param>
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <param name="distance">Distance to point</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsTriangle(PickingRay ray, Triangle tri, out Vector3 point, out float distance)
        {
            Ray rRay = ray;
            Vector3 vertex1 = tri.Point1;
            Vector3 vertex2 = tri.Point2;
            Vector3 vertex3 = tri.Point3;
            if (Collision.RayIntersectsTriangle(ref rRay, ref vertex1, ref vertex2, ref vertex3, out Vector3 collisionPoint))
            {
                point = collisionPoint;
                distance = Vector3.Distance(collisionPoint, ray.Position);

                return true;
            }

            point = Vector3.Zero;
            distance = float.MaxValue;

            return false;
        }

        /// <summary>
        /// Determines whether the triangles intersects or not
        /// </summary>
        /// <param name="triangle1">Triangle one</param>
        /// <param name="triangle2">Triangle two</param>
        /// <returns>Returns true if the triangles intersects</returns>
        public static bool TriangleIntersectsTriangle(Triangle triangle1, Triangle triangle2)
        {
            return IntersectionTriangle.Intersection(triangle1, triangle2);
        }
        /// <summary>
        /// Determines whether the triangles intersects or not
        /// </summary>
        /// <param name="triangle1">Triangle one</param>
        /// <param name="triangle2">Triangle two</param>
        /// <param name="coplanar">Returns true if the triangles are coplanar</param>
        /// <param name="segment">Returns the intersection segment (if the triangles are not coplanar)</param>
        /// <returns>Returns true if the triangles intersects</returns>
        public static bool TriangleIntersectsTriangle(Triangle triangle1, Triangle triangle2, out bool coplanar, out Line3D segment)
        {
            return IntersectionTriangle.Intersection(triangle1, triangle2, out coplanar, out segment);
        }
        /// <summary>
        /// Determines whether the triangle and the mesh intersects or not
        /// </summary>
        /// <param name="triangle">Triangle</param>
        /// <param name="mesh">Mesh</param>
        /// <param name="triangles">Returns the intersected triangle list</param>
        /// <returns>Returns true if the triangle intersects the mesh</returns>
        public static bool TriangleIntersectsMesh(Triangle triangle, IEnumerable<Triangle> mesh, out IEnumerable<Triangle> triangles)
        {
            triangles = mesh
                .Where(t => TriangleIntersectsTriangle(triangle, t))
                .Distinct()
                .ToArray();

            return triangles.Any();
        }
        /// <summary>
        /// Determines whether the triangle and the mesh intersects or not
        /// </summary>
        /// <param name="triangle">Triangle</param>
        /// <param name="mesh">Mesh</param>
        /// <param name="triangles">Returns the intersected triangle list</param>
        /// <param name="segments">Returns the intersection segment list</param>
        /// <returns>Returns true if the triangle intersects the mesh</returns>
        public static bool TriangleIntersectsMesh(Triangle triangle, IEnumerable<Triangle> mesh, out IEnumerable<Triangle> triangles, out IEnumerable<Line3D> segments)
        {
            List<Triangle> tris = new List<Triangle>();
            List<Line3D> segs = new List<Line3D>();

            bool intersected = false;
            foreach (var t in mesh)
            {
                if (TriangleIntersectsTriangle(triangle, t, out _, out Line3D segment))
                {
                    tris.Add(t);
                    segs.Add(segment);

                    intersected = true;
                }
            }

            triangles = tris.Distinct().ToArray();
            segments = segs.Where(seg => seg.Point1 != seg.Point2).Distinct().ToArray();

            return intersected;
        }

        /// <summary>
        /// Finds whether the point is contantained within the triangle, and the distance between the point and the triangle.
        /// </summary>
        /// <param name="point">A point.</param>
        /// <param name="triangle">A triangle</param>
        /// <returns>A value indicating whether the point is contained within the triangle.</returns>
        public static bool PointInTriangle(Vector3 point, Triangle triangle)
        {
            // Assume the point is in the center of coordinates

            // Substract the triangle points to the test point
            Vector3 p1 = triangle.Point1 - point;
            Vector3 p2 = triangle.Point2 - point;
            Vector3 p3 = triangle.Point3 - point;

            // As the point is in the center, all the triangle normals must be facing to the same direction
            Vector3 u = Vector3.Cross(p2, p3);
            Vector3 v = Vector3.Cross(p3, p1);
            if (Vector3.Dot(u, v) < 0f)
            {
                return false;
            }

            Vector3 w = Vector3.Cross(p1, p2);
            if (Vector3.Dot(u, w) < 0f)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Finds whether the point is contantained within the triangle mesh, and the distance between the point and the closest triangle.
        /// </summary>
        /// <param name="p">A point.</param>
        /// <param name="mesh">A mesh.</param>
        /// <returns>A value indicating whether the point is contained within the triangle mesh.</returns>
        public static bool PointInMesh(Vector3 p, IEnumerable<Triangle> mesh)
        {
            return mesh.Any(t => PointInTriangle(p, t));
        }
        /// <summary>
        /// Gets the closest point in a ray from a specified point
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="point">Point</param>
        /// <returns>Returns the resulting point along the ray</returns>
        public static Vector3 ClosestPointInRay(Ray ray, Vector3 point)
        {
            var to = point - ray.Position;
            float rayLength = ray.Direction.Length();
            var dir = Vector3.Normalize(ray.Direction);

            var dist = Vector3.Dot(to, dir);
            if (dist < 0f)
            {
                return ray.Position;
            }
            if (dist > rayLength)
            {
                return ray.Position + ray.Direction;
            }

            return ray.Position + (dir * dist);
        }
        /// <summary>
        /// Gets the closest point in a ray from a specified point
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="point">Point</param>
        /// <param name="distance">Distance</param>
        /// <returns>Returns the resulting minimum distance from point to ray</returns>
        public static Vector3 ClosestPointInRay(Ray ray, Vector3 point, out float distance)
        {
            var closestPoint = ClosestPointInRay(ray, point);

            distance = Vector3.Distance(closestPoint, point);

            return closestPoint;
        }

        /// <summary>
        /// Gets the closest triangle point from the specified point
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns the closest point in the triangle from point</returns>
        public static Vector3 ClosestPointInTriangle(Vector3 point, Triangle triangle)
        {
            Vector3 a = triangle.Point1;
            Vector3 b = triangle.Point2;
            Vector3 c = triangle.Point3;

            // Check if P in vertex region outside A
            Vector3 ab = Vector3.Subtract(b, a);
            Vector3 ac = Vector3.Subtract(c, a);
            Vector3 ap = Vector3.Subtract(point, a);
            float d1 = Vector3.Dot(ab, ap);
            float d2 = Vector3.Dot(ac, ap);
            if (d1 <= 0.0f && d2 <= 0.0f)
            {
                // barycentric coordinates (1,0,0)
                return a;
            }

            // Check if P in vertex region outside B
            Vector3 bp = Vector3.Subtract(point, b);
            float d3 = Vector3.Dot(ab, bp);
            float d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0.0f && d4 <= d3)
            {
                // barycentric coordinates (0,1,0)
                return b;
            }

            // Check if P in edge region of AB, if so return projection of P onto AB
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
            {
                // barycentric coordinates (1-v,v,0)
                float v = d1 / (d1 - d3);
                return new Vector3
                {
                    X = a.X + v * ab.X,
                    Y = a.Y + v * ab.Y,
                    Z = a.Z + v * ab.Z,
                };
            }

            // Check if P in vertex region outside C
            Vector3 cp = Vector3.Subtract(point, c);
            float d5 = Vector3.Dot(ab, cp);
            float d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0.0f && d5 <= d6)
            {
                // barycentric coordinates (0,0,1)
                return c;
            }

            // Check if P in edge region of AC, if so return projection of P onto AC
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
            {
                // barycentric coordinates (1-w,0,w)
                float w = d2 / (d2 - d6);
                return new Vector3
                {
                    X = a.X + w * ac.X,
                    Y = a.Y + w * ac.Y,
                    Z = a.Z + w * ac.Z,
                };
            }

            // Check if P in edge region of BC, if so return projection of P onto BC
            float va = d3 * d6 - d5 * d4;
            if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
            {
                // barycentric coordinates (0,1-w,w)
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                return new Vector3
                {
                    X = b.X + w * (c.X - b.X),
                    Y = b.Y + w * (c.Y - b.Y),
                    Z = b.Z + w * (c.Z - b.Z),
                };
            }

            // P inside face region. Compute Q through its barycentric coordinates (u,v,w)
            float denom = 1.0f / (va + vb + vc);
            float closestV = vb * denom;
            float closestW = vc * denom;
            return new Vector3
            {
                X = a.X + ab.X * closestV + ac.X * closestW,
                Y = a.Y + ab.Y * closestV + ac.Y * closestW,
                Z = a.Z + ab.Z * closestV + ac.Z * closestW,
            };
        }
        /// <summary>
        /// Gets the closest point in a triangle mesh from the specified point p
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="closest">Returns the closest triangle from point p</param>
        /// <param name="contactPoint">Returns the closest point in the triangle from point p</param>
        public static void ClosestPointInMesh(Vector3 point, IEnumerable<Triangle> mesh, out Triangle closest, out Vector3 contactPoint)
        {
            closest = new Triangle();
            contactPoint = point;

            float distance = float.MaxValue;
            foreach (var t in mesh)
            {
                var closestToTri = ClosestPointInTriangle(point, t);

                float sqrDist = Vector3.DistanceSquared(point, closestToTri);

                if (sqrDist < distance)
                {
                    distance = sqrDist;

                    closest = t;
                    contactPoint = closestToTri;
                }
            }
        }

        /// <summary>
        /// Determines whether a BoundingSphere contains a BoundingSphere.
        /// </summary>
        /// <param name="sphere1">The first sphere to test</param>
        /// <param name="sphere2">The second sphere to test</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType SphereContainsSphere(BoundingSphere sphere1, BoundingSphere sphere2)
        {
            return Collision.SphereContainsSphere(ref sphere1, ref sphere2);
        }
        /// <summary>
        /// Determines whether a BoundingSphere contains a BoundingBox.
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="box">Axis aligned box</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType SphereContainsBox(BoundingSphere sphere, BoundingBox box)
        {
            return Collision.SphereContainsBox(ref sphere, ref box);
        }
        /// <summary>
        /// Determines whether a BoundingSphere contains a BoundingFrustum.
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType SphereContainsFrustum(BoundingSphere sphere, BoundingFrustum frustum)
        {
            if (!frustum.Intersects(ref sphere))
            {
                // Not intersection at all
                return ContainmentType.Disjoint;
            }

            // Test if all corners are into the sphere
            var corners = frustum.GetCorners();
            for (int i = 0; i < corners.Length; i++)
            {
                var v = corners[i];
                if (sphere.Contains(ref v) == ContainmentType.Disjoint)
                {
                    return ContainmentType.Intersects;
                }
            }

            return ContainmentType.Contains;
        }
        /// <summary>
        /// Determines whether a BoundingSphere contains a Triangle.
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType SphereContainsTriangle(BoundingSphere sphere, Triangle triangle)
        {
            var p1 = triangle.Point1;
            var p2 = triangle.Point2;
            var p3 = triangle.Point3;

            return Collision.SphereContainsTriangle(ref sphere, ref p1, ref p2, ref p3);
        }
        /// <summary>
        /// Determines whether a BoundingSphere contains a Triangle mesh.
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType SphereContainsMesh(BoundingSphere sphere, IEnumerable<Triangle> mesh)
        {
            ContainmentType res = ContainmentType.Disjoint;

            foreach (var t in mesh)
            {
                ContainmentType c = SphereContainsTriangle(sphere, t);

                if (c == res)
                {
                    continue;
                }

                if (res == ContainmentType.Disjoint)
                {
                    // From Disjoint to something
                    res = c;
                }
                else if (res == ContainmentType.Contains && c == ContainmentType.Intersects)
                {
                    // From contains to intersects
                    res = c;

                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// Determines whether a BoundingBox contains a BoundingSphere.
        /// </summary>
        /// <param name="box">Axis aligned box</param>
        /// <param name="sphere">Sphere</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType BoxContainsSphere(BoundingBox box, BoundingSphere sphere)
        {
            return Collision.BoxContainsSphere(ref box, ref sphere);
        }
        /// <summary>
        /// Determines whether a BoundingBox contains a BoundingBox.
        /// </summary>
        /// <param name="box1">The first box to test</param>
        /// <param name="box2">The second box to test</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType BoxContainsBox(BoundingBox box1, BoundingBox box2)
        {
            return Collision.BoxContainsBox(ref box1, ref box2);
        }
        /// <summary>
        /// Determines whether a BoundingBox contains a BoundingFrustum.
        /// </summary>
        /// <param name="box">Axis aligned box</param>
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType BoxContainsFrustum(BoundingBox box, BoundingFrustum frustum)
        {
            if (!frustum.Intersects(ref box))
            {
                // Not intersection at all
                return ContainmentType.Disjoint;
            }

            // Test if all corners are into the box
            var corners = frustum.GetCorners();
            for (int i = 0; i < corners.Length; i++)
            {
                var v = corners[i];
                if (box.Contains(ref v) == ContainmentType.Disjoint)
                {
                    return ContainmentType.Intersects;
                }
            }

            return ContainmentType.Contains;
        }
        /// <summary>
        /// Determines whether a BoundingBox contains a Triangle.
        /// </summary>
        /// <param name="box">Axis aligned box</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType BoxContainsTriangle(BoundingBox box, Triangle triangle)
        {
            // Test containment
            var test1 = box.Contains(triangle.Point1);
            var test2 = box.Contains(triangle.Point2);
            var test3 = box.Contains(triangle.Point3);

            if (test1 == ContainmentType.Contains && test2 == ContainmentType.Contains && test3 == ContainmentType.Contains)
            {
                // All three points into the box
                return ContainmentType.Contains;
            }

            if (test1 == ContainmentType.Contains || test2 == ContainmentType.Contains || test3 == ContainmentType.Contains)
            {
                // One point at least, into the box
                return ContainmentType.Intersects;
            }

            // Test intersection
            if (BoxIntersectsTriangle(box, triangle))
            {
                return ContainmentType.Intersects;
            }

            return ContainmentType.Disjoint;
        }
        /// <summary>
        /// Determines whether a BoundingBox contains a Triangle mesh.
        /// </summary>
        /// <param name="box">Axis aligned box</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType BoxContainsMesh(BoundingBox box, IEnumerable<Triangle> mesh)
        {
            ContainmentType res = ContainmentType.Disjoint;

            foreach (var t in mesh)
            {
                var c = BoxContainsTriangle(box, t);
                if (c == res)
                {
                    continue;
                }

                if (res == ContainmentType.Disjoint)
                {
                    // From Disjoint to something
                    res = c;
                }
                else if (res == ContainmentType.Contains && c == ContainmentType.Intersects)
                {
                    // From contains to intersects
                    res = c;

                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// Determines whether a BoundingFrustum contains a BoundingSphere.
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="sphere">Sphere</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType FrustumContainsSphere(BoundingFrustum frustum, BoundingSphere sphere)
        {
            return frustum.Contains(ref sphere);
        }
        /// <summary>
        /// Determines whether a BoundingFrustum contains a BoundingBox.
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="box">Axis aligned box</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType FrustumContainsBox(BoundingFrustum frustum, BoundingBox box)
        {
            return frustum.Contains(ref box);
        }
        /// <summary>
        /// Determines whether a BoundingFrustum contains a BoundingFrustum.
        /// </summary>
        /// <param name="frustum1">Frustum one</param>
        /// <param name="frustum2">Frustum trwo</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType FrustumContainsFrustum(BoundingFrustum frustum1, BoundingFrustum frustum2)
        {
            if (frustum1 == frustum2)
            {
                return ContainmentType.Contains;
            }

            //Test points
            var corners = frustum2.GetCorners();
            var f1Contains2 = FrustumContainsPoints(frustum1, corners);
            if (f1Contains2 != ContainmentType.Disjoint)
            {
                return f1Contains2;
            }

            var f2Contains1 = FrustumContainsPoints(frustum2, frustum1.GetCorners());
            if (f2Contains1 != ContainmentType.Disjoint)
            {
                return ContainmentType.Intersects;
            }

            //Test near to far segments
            for (int i = 0; i < 4; i++)
            {
                var p1 = corners[i + 0];
                var p2 = corners[i + 4];

                if (FrustumIntersectsSegment(frustum1, p1, p2))
                {
                    return ContainmentType.Intersects;
                }
            }

            //Test near plane segments
            for (int i = 0; i < 4; i++)
            {
                var p1 = corners[i + 0];
                var p2 = corners[(i + 1) % 4];

                if (FrustumIntersectsSegment(frustum1, p1, p2))
                {
                    return ContainmentType.Intersects;
                }
            }

            //Test far plane segments
            for (int i = 0; i < 4; i++)
            {
                var p1 = corners[i + 4];
                var p2 = corners[((i + 1) % 4) + 4];

                if (FrustumIntersectsSegment(frustum1, p1, p2))
                {
                    return ContainmentType.Intersects;
                }
            }

            return ContainmentType.Disjoint;
        }
        /// <summary>
        /// Determines whether a BoundingFrustum contains a Triangle.
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType FrustumContainsTriangle(BoundingFrustum frustum, Triangle triangle)
        {
            bool allVerticesIn = true;
            bool verticesIn = false;
            var vertices = triangle.GetVertices();
            for (int i = 0; i < vertices.Count(); i++)
            {
                var v = vertices.ElementAt(i);
                if (frustum.Contains(ref v) == ContainmentType.Disjoint)
                {
                    allVerticesIn = false;
                }
                else
                {
                    verticesIn = true;
                }
            }

            if (allVerticesIn)
            {
                return ContainmentType.Contains;
            }
            else if (verticesIn)
            {
                return ContainmentType.Intersects;
            }

            // Test triangle segments
            var edges = triangle.GetEdges();
            foreach (var (point1, point2) in edges)
            {
                if (FrustumIntersectsSegment(frustum, point1, point2))
                {
                    return ContainmentType.Intersects;
                }
            }

            return ContainmentType.Disjoint;
        }
        /// <summary>
        /// Determines whether a BoundingFrustum contains a Triangle mesh.
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType FrustumContainsMesh(BoundingFrustum frustum, IEnumerable<Triangle> mesh)
        {
            ContainmentType res = ContainmentType.Disjoint;

            foreach (var t in mesh)
            {
                ContainmentType c = FrustumContainsTriangle(frustum, t);

                if (c == res)
                {
                    continue;
                }

                if (res == ContainmentType.Disjoint)
                {
                    // From Disjoint to something
                    res = c;
                }
                else if (res == ContainmentType.Contains && c == ContainmentType.Intersects)
                {
                    // From contains to intersects
                    res = c;

                    break;
                }
            }

            return res;
        }
        /// <summary>
        /// Gets whether the frustum contains the point list
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="points">Point collection</param>
        private static ContainmentType FrustumContainsPoints(BoundingFrustum frustum, IEnumerable<Vector3> points)
        {
            bool allCornersContained = true;
            bool intersects = false;
            foreach (var vertex in points)
            {
                var res = frustum.Contains(vertex);
                if (res != ContainmentType.Disjoint)
                {
                    intersects = true;
                }
                else
                {
                    allCornersContained = false;
                }
            }

            if (intersects)
            {
                return allCornersContained ? ContainmentType.Contains : ContainmentType.Intersects;
            }

            return ContainmentType.Disjoint;
        }
        
        /// <summary>
        /// Gets whether a frustum is intersected with the specified segment or not
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="p1">First segment point</param>
        /// <param name="p2">Second segment point</param>
        /// <returns>Returs true if the frustum is intersected with the specified segment</returns>
        public static bool FrustumIntersectsSegment(BoundingFrustum frustum, Vector3 p1, Vector3 p2)
        {
            Ray ray = new Ray(p1, Vector3.Normalize(p2 - p1));

            if (!frustum.Intersects(ref ray, out var inDistance, out _))
            {
                return false;
            }

            if (!inDistance.HasValue || Math.Abs(inDistance.Value) == float.MaxValue)
            {
                return false;
            }

            float rayLength = Vector3.Distance(p1, p2);
            if (inDistance >= rayLength)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Gets whether a frustum is intersected with the specified segment or not
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="p1">First segment point</param>
        /// <param name="p2">Second segment point</param>
        /// <param name="distance">Returns the intersection distance</param>
        /// <param name="point">Returns the intersection point</param>
        /// <returns>Returs true if the frustum is intersected with the specified segment</returns>
        public static bool FrustumIntersectsSegment(BoundingFrustum frustum, Vector3 p1, Vector3 p2, out float distance, out Vector3 point)
        {
            distance = float.MaxValue;
            point = Vector3.Zero;

            Ray ray = new Ray(p1, Vector3.Normalize(p2 - p1));

            if (!frustum.Intersects(ref ray, out var inDistance, out _))
            {
                return false;
            }

            if (!inDistance.HasValue || Math.Abs(inDistance.Value) == float.MaxValue)
            {
                return false;
            }

            float rayLength = Vector3.Distance(p1, p2);
            if (inDistance >= rayLength)
            {
                return false;
            }

            distance = inDistance.Value;
            point = ray.Position + (Vector3.Normalize(ray.Direction) * inDistance.Value);

            return true;
        }
        /// <summary>
        /// Gets whether a frustum is intersected with the specified segment or not
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="p1">First segment point</param>
        /// <param name="p2">Second segment point</param>
        /// <param name="enteringPoint">Entering segment point</param>
        /// <param name="exitingPoint">Exiting segment point</param>
        /// <returns>Returs true if the frustum is intersected with the specified segment</returns>
        public static bool FrustumIntersectsSegment(BoundingFrustum frustum, Vector3 p1, Vector3 p2, out Vector3? enteringPoint, out Vector3? exitingPoint)
        {
            enteringPoint = null;
            exitingPoint = null;

            Ray ray = new Ray(p1, Vector3.Normalize(p2 - p1));

            if (!frustum.Intersects(ref ray, out var inDistance, out var outDistance))
            {
                return false;
            }

            float rayLength = Vector3.Distance(p1, p2);

            bool intoSegmentBounds = false;

            if (inDistance.HasValue && Math.Abs(inDistance.Value) != float.MaxValue && inDistance <= rayLength)
            {
                enteringPoint = ray.Position + (Vector3.Normalize(ray.Direction) * inDistance.Value);
                intoSegmentBounds = true;
            }

            if (outDistance.HasValue && Math.Abs(outDistance.Value) != float.MaxValue && outDistance <= rayLength)
            {
                exitingPoint = ray.Position + (Vector3.Normalize(ray.Direction) * outDistance.Value);
                intoSegmentBounds = true;
            }

            return intoSegmentBounds;
        }

        /// <summary>
        /// Determines whether a Triangle mesh contains a BoundingSphere.
        /// </summary>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="sphere">Sphere</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType MeshContainsSphere(IEnumerable<Triangle> mesh, BoundingSphere sphere)
        {
            BoundingBox mbox = BoundingBox.FromPoints(mesh.SelectMany(t => t.GetVertices()).ToArray());

            return mbox.Contains(ref sphere);
        }
        /// <summary>
        /// Determines whether a Triangle mesh contains a BoundingBox.
        /// </summary>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="box">Axis aligned box</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType MeshContainsBox(IEnumerable<Triangle> mesh, BoundingBox box)
        {
            BoundingBox mbox = BoundingBox.FromPoints(mesh.SelectMany(t => t.GetVertices()).ToArray());

            return mbox.Contains(ref box);
        }
        /// <summary>
        /// Determines whether a Triangle mesh contains a BoundingFrustum.
        /// </summary>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType MeshContainsFrustum(IEnumerable<Triangle> mesh, BoundingFrustum frustum)
        {
            BoundingBox mbox = BoundingBox.FromPoints(mesh.SelectMany(t => t.GetVertices()).ToArray());

            return BoxContainsFrustum(mbox, frustum);
        }
        /// <summary>
        /// Determines whether a Triangle mesh contains a Triangle mesh.
        /// </summary>
        /// <param name="mesh1">Triangle mesh one</param>
        /// <param name="mesh2">Triangle mesh two</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType MeshContainsMesh(IEnumerable<Triangle> mesh1, IEnumerable<Triangle> mesh2)
        {
            // A mesh is not a volume. Calculate the bounding volumes of the tow meshes
            var bbox1 = BoundingBox.FromPoints(mesh1.SelectMany(t => t.GetVertices()).ToArray());
            var bbox2 = BoundingBox.FromPoints(mesh2.SelectMany(t => t.GetVertices()).ToArray());

            if (bbox1.Contains(ref bbox2) == ContainmentType.Disjoint)
            {
                // If volumes disjoint, no intersection is possible
                return ContainmentType.Disjoint;
            }

            if (bbox2.Contains(ref bbox1) == ContainmentType.Contains)
            {
                // Box2 contains box1. Then box1 intersects box2
                return ContainmentType.Intersects;
            }

            // Test each triangle of the mesh
            foreach (var t in mesh1)
            {
                if (TriangleIntersectsMesh(t, mesh2, out _))
                {
                    return ContainmentType.Intersects;
                }
            }

            // If no intersection found, must be a containment result
            return ContainmentType.Contains;
        }
    }
}