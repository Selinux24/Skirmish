using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Triangle
    /// </summary>
    public struct Triangle : IVertexList, IRayIntersectable, IEquatable<Triangle>
    {
        /// <summary>
        /// First point
        /// </summary>
        public Vector3 Point1 { get; set; }
        /// <summary>
        /// Second point
        /// </summary>
        public Vector3 Point2 { get; set; }
        /// <summary>
        /// Third point
        /// </summary>
        public Vector3 Point3 { get; set; }
        /// <summary>
        /// Center
        /// </summary>
        public Vector3 Center { get; set; }
        /// <summary>
        /// First index
        /// </summary>
        public int I1 { get; set; }
        /// <summary>
        /// Second index
        /// </summary>
        public int I2 { get; set; }
        /// <summary>
        /// Plane
        /// </summary>
        public Plane Plane { get; set; }
        /// <summary>
        /// Normal
        /// </summary>
        public Vector3 Normal
        {
            get
            {
                return Plane.Normal;
            }
        }
        /// <summary>
        /// Min
        /// </summary>
        public Vector3 Min
        {
            get
            {
                return Vector3.Min(Point1, Vector3.Min(Point2, Point3));
            }
        }
        /// <summary>
        /// Max
        /// </summary>
        public Vector3 Max
        {
            get
            {
                return Vector3.Max(Point1, Vector3.Max(Point2, Point3));
            }
        }
        /// <summary>
        /// Triangle area
        /// </summary>
        /// <remarks>Heron</remarks>
        public float Area
        {
            get
            {
                float a = (Point1 - Point2).Length();
                float b = (Point1 - Point3).Length();
                float c = (Point2 - Point3).Length();

                float p = (a + b + c) * 0.5f;
                float z = p * (p - a) * (p - b) * (p - c);

                return (float)Math.Sqrt(z);
            }
        }
        /// <summary>
        /// Inclination angle
        /// </summary>
        public float Inclination
        {
            get
            {
                return Helper.Angle(Normal, Vector3.Down);
            }
        }
        /// <summary>
        /// Returns the triangle vertex by index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns a triangle vertex</returns>
        public Vector3 this[int index]
        {
            get
            {
                if (index == 0) return Point1;
                if (index == 1) return Point2;
                if (index == 2) return Point3;

                return Vector3.Zero;
            }
        }

        /// <summary>
        /// Generate a triangle list from vertices
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="vertices">Vertices</param>
        /// <returns>Returns the triangle list</returns>
        public static IEnumerable<Triangle> ComputeTriangleList(Topology topology, IEnumerable<Vector3> vertices)
        {
            List<Triangle> triangleList = new List<Triangle>();

            if (topology == Topology.TriangleList)
            {
                var tmpVerts = vertices.ToArray();

                for (int i = 0; i < tmpVerts.Length; i += 3)
                {
                    Triangle tri = new Triangle(
                        tmpVerts[i + 0],
                        tmpVerts[i + 1],
                        tmpVerts[i + 2]);

                    triangleList.Add(tri);
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            return triangleList.ToArray();
        }
        /// <summary>
        /// Generate a triangle list from vertices and indices
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        /// <returns>Returns the triangle list</returns>
        public static IEnumerable<Triangle> ComputeTriangleList(Topology topology, IEnumerable<Vector3> vertices, IEnumerable<uint> indices)
        {
            List<Triangle> triangleList = new List<Triangle>();

            if (topology == Topology.TriangleList)
            {
                var tmpVerts = vertices.ToArray();
                var tmpIndxs = indices.ToArray();

                for (int i = 0; i < tmpIndxs.Length; i += 3)
                {
                    Triangle tri = new Triangle(
                        tmpVerts[tmpIndxs[i + 0]],
                        tmpVerts[tmpIndxs[i + 1]],
                        tmpVerts[tmpIndxs[i + 2]]);

                    triangleList.Add(tri);
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            return triangleList.ToArray();
        }
        /// <summary>
        /// Generate a triangle list from AABB
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="bbox">AABB</param>
        /// <returns>Returns the triangle list</returns>
        public static IEnumerable<Triangle> ComputeTriangleList(Topology topology, BoundingBox bbox)
        {
            var geom = GeometryUtil.CreateBox(topology, bbox);

            return ComputeTriangleList(topology, geom.Vertices, geom.Indices);
        }
        /// <summary>
        /// Generate a triangle list from OBB
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="obb">OBB</param>
        /// <returns>Returns the triangle list</returns>
        public static IEnumerable<Triangle> ComputeTriangleList(Topology topology, OrientedBoundingBox obb)
        {
            var geom = GeometryUtil.CreateBox(topology, obb);

            return ComputeTriangleList(topology, geom.Vertices, geom.Indices);
        }
        /// <summary>
        /// Generate a triangle list from sphere
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="sph">Sphere</param>
        /// <param name="sliceCount">Slices</param>
        /// <param name="stackCount">Stacks</param>
        /// <returns>Returns the triangle list</returns>
        public static IEnumerable<Triangle> ComputeTriangleList(Topology topology, BoundingSphere sph, int sliceCount, int stackCount)
        {
            var geom = GeometryUtil.CreateSphere(topology, sph, sliceCount, stackCount);

            return ComputeTriangleList(topology, geom.Vertices, geom.Indices);
        }
        /// <summary>
        /// Generate a triangle list from cylinder
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="cylinder">Cylinder</param>
        /// <param name="stackCount">Stack count</param>
        /// <returns>Returns the triangle list</returns>
        public static IEnumerable<Triangle> ComputeTriangleList(Topology topology, BoundingCylinder cylinder, int stackCount)
        {
            var geom = GeometryUtil.CreateCylinder(topology, cylinder, stackCount);

            return ComputeTriangleList(topology, geom.Vertices, geom.Indices);
        }
        /// <summary>
        /// Generate a triangle list from capsule
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="capsule">Capsule</param>
        /// <param name="sliceCount">Slices</param>
        /// <param name="stackCount">Stacks</param>
        /// <returns>Returns the triangle list</returns>
        public static IEnumerable<Triangle> ComputeTriangleList(Topology topology, BoundingCapsule capsule, int sliceCount, int stackCount)
        {
            var geom = GeometryUtil.CreateCapsule(topology, capsule, sliceCount, stackCount);

            return ComputeTriangleList(topology, geom.Vertices, geom.Indices);
        }
        /// <summary>
        /// Generate a triangle list from polygon
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="poly">Polygon</param>
        /// <returns>Returns the triangle list</returns>
        public static IEnumerable<Triangle> ComputeTriangleList(Topology topology, Polygon poly)
        {
            if (topology == Topology.TriangleList)
            {
                return poly.Triangulate();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Transform triangle coordinates
        /// </summary>
        /// <param name="triangle">Triangle</param>
        /// <param name="transform">Transformation</param>
        /// <returns>Returns new triangle</returns>
        public static Triangle Transform(Triangle triangle, Matrix transform)
        {
            return new Triangle(
                Vector3.TransformCoordinate(triangle.Point1, transform),
                Vector3.TransformCoordinate(triangle.Point2, transform),
                Vector3.TransformCoordinate(triangle.Point3, transform));
        }
        /// <summary>
        /// Transform triangle list coordinates
        /// </summary>
        /// <param name="triangles">Triangle list</param>
        /// <param name="transform">Transformation</param>
        /// <returns>Returns new triangle list</returns>
        public static IEnumerable<Triangle> Transform(IEnumerable<Triangle> triangles, Matrix transform)
        {
            if (triangles?.Any() != true)
            {
                return triangles;
            }

            List<Triangle> res = new List<Triangle>(triangles.Count());

            foreach (var tri in triangles)
            {
                res.Add(Transform(tri, transform));
            }

            return res;
        }
        /// <summary>
        /// Reverses the normal of all the triangles of the list
        /// </summary>
        /// <param name="triangles">Triangle list</param>
        /// <returns>Returns a new triangle list</returns>
        public static IEnumerable<Triangle> Reverse(IEnumerable<Triangle> triangles)
        {
            if (triangles?.Any() != true)
            {
                return Enumerable.Empty<Triangle>();
            }

            List<Triangle> res = new List<Triangle>(triangles.Count());

            foreach (var tri in triangles)
            {
                res.Add(tri.ReverseNormal());
            }

            return res.ToArray();
        }
        /// <summary>
        /// Reverses the normal of all the triangles of the list
        /// </summary>
        /// <param name="vertices">Point list</param>
        /// <returns>Returns a new point list</returns>
        public static IEnumerable<Vector3> Reverse(IEnumerable<Vector3> vertices)
        {
            if (vertices.Count() % 3 != 0)
            {
                throw new ArgumentException("The point list must be divisible by three.", nameof(vertices));
            }

            List<Vector3> result = new List<Vector3>();

            for (int i = 0; i < vertices.Count(); i += 3)
            {
                result.Add(vertices.ElementAt(i + 0));
                result.Add(vertices.ElementAt(i + 2));
                result.Add(vertices.ElementAt(i + 1));
            }

            return result;
        }
        /// <summary>
        /// Reverses the normal of all the triangles of the list
        /// </summary>
        /// <param name="indices">Index list</param>
        /// <returns>Returns a new index list</returns>
        public static IEnumerable<uint> Reverse(IEnumerable<uint> indices)
        {
            if (indices.Count() % 3 != 0)
            {
                throw new ArgumentException("The index list must be divisible by three.", nameof(indices));
            }

            List<uint> result = new List<uint>();

            for (int i = 0; i < indices.Count(); i += 3)
            {
                result.Add(indices.ElementAt(i + 0));
                result.Add(indices.ElementAt(i + 2));
                result.Add(indices.ElementAt(i + 1));
            }

            return result;
        }
        /// <summary>
        /// Gets the barycentric coordinates of a triangle, given a reference point
        /// </summary>
        /// <param name="a">First triangle point</param>
        /// <param name="b">Second triangle point</param>
        /// <param name="c">Third triangle point</param>
        /// <param name="p">Point</param>
        /// <remarks>
        /// Point must be into the triangle
        /// Code from Christen Erickson's Real-Time Collision Detection
        /// </remarks>
        public static Vector3 CalculateBarycenter(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
        {
            Vector3 v0 = b - a;
            Vector3 v1 = c - a;
            Vector3 v2 = p - a;

            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);
            float denom = d00 * d11 - d01 * d01;

            float v = (d11 * d20 - d01 * d21) / denom;
            float w = (d00 * d21 - d01 * d20) / denom;
            float u = 1.0f - v - w;

            return new Vector3(u, v, w);
        }
        /// <summary>
        /// Gets the barycentric coordinates of a triangle, given a reference point
        /// </summary>
        /// <param name="t">Triangle</param>
        /// <param name="p">Point</param>
        public static Vector3 CalculateBarycenter(Triangle t, Vector3 p)
        {
            return CalculateBarycenter(t.Point1, t.Point2, t.Point3, p);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="point1">Point 1</param>
        /// <param name="point2">Point 2</param>
        /// <param name="point3">Point 3</param>
        public Triangle(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;
            Center = Vector3.Multiply(point1 + point2 + point3, 1.0f / 3.0f);
            Plane = new Plane(Point1, Point2, Point3);

            Vector3 n = Plane.Normal;
            float absX = Math.Abs(n.X);
            float absY = Math.Abs(n.Y);
            float absZ = Math.Abs(n.Z);

            Vector3 a = new Vector3(absX, absY, absZ);
            if (a.X > a.Y)
            {
                if (a.X > a.Z)
                {
                    I1 = 1;
                    I2 = 2;
                }
                else
                {
                    I1 = 0;
                    I2 = 1;
                }
            }
            else
            {
                if (a.Y > a.Z)
                {
                    I1 = 0;
                    I2 = 2;
                }
                else
                {
                    I1 = 0;
                    I2 = 1;
                }
            }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="v1X">Point 1 X</param>
        /// <param name="v1Y">Point 1 Y</param>
        /// <param name="v1Z">Point 1 Z</param>
        /// <param name="v2X">Point 2 X</param>
        /// <param name="v2Y">Point 2 Y</param>
        /// <param name="v2Z">Point 2 Z</param>
        /// <param name="v3X">Point 3 X</param>
        /// <param name="v3Y">Point 3 Y</param>
        /// <param name="v3Z">Point 3 Z</param>
        public Triangle(float v1X, float v1Y, float v1Z, float v2X, float v2Y, float v2Z, float v3X, float v3Y, float v3Z)
            : this(new Vector3(v1X, v1Y, v1Z), new Vector3(v2X, v2Y, v2Z), new Vector3(v3X, v3Y, v3Z))
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="points">Point array</param>
        public Triangle(Vector3[] points) : this(points[0], points[1], points[2])
        {

        }

        /// <summary>
        /// Intersection test between ray and triangle
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <returns>Returns true if ray intersects with this triangle</returns>
        public bool Intersects(PickingRay ray)
        {
            return Intersects(ray, out _, out _);
        }
        /// <summary>
        /// Intersection test between ray and triangle
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="distance">Distance from ray origin and intersection point, if any</param>
        /// <returns>Returns true if ray intersects with this triangle</returns>
        public bool Intersects(PickingRay ray, out float distance)
        {
            return Intersects(ray, out _, out distance);
        }
        /// <summary>
        /// Intersection test between ray and triangle
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="point">Intersection point, if any</param>
        /// <param name="distance">Distance from ray origin and intersection point, if any</param>
        /// <returns>Returns true if ray intersects with this triangle</returns>
        public bool Intersects(PickingRay ray, out Vector3 point, out float distance)
        {
            if (ray.FacingOnly)
            {
                bool cull = Vector3.Dot(ray.Direction, Normal) >= 0f;
                if (cull)
                {
                    point = Vector3.Zero;
                    distance = float.MaxValue;

                    return false;
                }
            }

            return Intersection.RayIntersectsTriangle(ray, this, out point, out distance);
        }

        /// <summary>
        /// Projects the triangle into the given vector
        /// </summary>
        /// <param name="vector">Direction vector</param>
        public float ProjectToVector(Vector3 vector)
        {
            var p1 = Point1;
            var p2 = Point2;
            var p3 = Point3;
            var d = Vector3.Normalize(vector);

            var p1_proj = p1 - Vector3.Dot(p1 - Vector3.Zero, d) * d;
            var p2_proj = p2 - Vector3.Dot(p2 - Vector3.Zero, d) * d;
            var p3_proj = p3 - Vector3.Dot(p3 - Vector3.Zero, d) * d;

            var v1 = p2_proj - p1_proj;
            var v2 = p3_proj - p1_proj;
            var area = Vector3.Cross(v1, v2).Length() * 0.5f;

            return area * Math.Abs(Vector3.Dot(d, Vector3.Normalize(v1 + v2)));
        }

        /// <summary>
        /// Retrieves the three vertices of the triangle.
        /// </summary>
        /// <returns>An array of points representing the three vertices of the triangle.</returns>
        public IEnumerable<Vector3> GetVertices()
        {
            return new[]
            {
                Point1,
                Point2,
                Point3,
            };
        }
        /// <summary>
        /// Gets the vertex list stride
        /// </summary>
        /// <returns>Returns the list stride</returns>
        public int GetStride()
        {
            return 3;
        }
        /// <summary>
        /// Gets the vertex list topology
        /// </summary>
        /// <returns>Returns the list topology</returns>
        public Topology GetTopology()
        {
            return Topology.TriangleList;
        }

        /// <summary>
        /// Retrieves the three edges of the triangle.
        /// </summary>
        /// <returns>An array of vectors representing the three edges of the triangle.</returns>
        public IEnumerable<Segment> GetEdges()
        {
            return new[]
            {
                new Segment(Point2, Point1),
                new Segment(Point3, Point2),
                new Segment(Point1, Point2),
            };
        }
        /// <summary>
        /// Gets the edge direction vector between points 2 and 1
        /// </summary>
        public Vector3 GetEdge1()
        {
            return Vector3.Subtract(Point2, Point1);
        }
        /// <summary>
        /// Gets the edge direction vector between points 3 and 2
        /// </summary>
        public Vector3 GetEdge2()
        {
            return Vector3.Subtract(Point3, Point2);
        }
        /// <summary>
        /// Gets the edge direction vector between points 1 and 3
        /// </summary>
        public Vector3 GetEdge3()
        {
            return Vector3.Subtract(Point1, Point3);
        }
        /// <summary>
        /// Gets the triangle radius
        /// </summary>
        public float GetRadius()
        {
            Vector3 center = Center;

            return Math.Max(Vector3.Distance(center, Point1), Math.Max(Vector3.Distance(center, Point2), Vector3.Distance(center, Point3)));
        }

        /// <summary>
        /// Gets a new triangle with reversed normal vector
        /// </summary>
        /// <returns>Returns a new revered normal triangle</returns>
        public Triangle ReverseNormal()
        {
            return new Triangle(Point1, Point3, Point2);
        }

        /// <summary>
        /// Gets the barycenter
        /// </summary>
        /// <param name="p">Reference point</param>
        public Vector3 GetBarycenter(Vector3 p)
        {
            return CalculateBarycenter(this, p);
        }

        /// <inheritdoc/>
        public static bool operator ==(Triangle left, Triangle right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        public static bool operator !=(Triangle left, Triangle right)
        {
            return !left.Equals(ref right);
        }
        /// <inheritdoc/>
        public bool Equals(Triangle other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is not Triangle)
            {
                return false;
            }

            var strongValue = (Triangle)obj;
            return Equals(ref strongValue);
        }

        /// <inheritdoc/>
        public bool Equals(ref Triangle other)
        {
            return
                other.Point1.Equals(Point1) &&
                other.Point2.Equals(Point2) &&
                other.Point3.Equals(Point3);
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Point1, Point2, Point3);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Vertex 1 {Point1}; Vertex 2 {Point2}; Vertex 3 {Point3};";
        }
    }
}
