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
            List<Triangle> triangleList = new List<Triangle>();

            if (topology == Topology.TriangleList)
            {
                var v = new Vector3[24];

                float xm = bbox.Minimum.X;
                float ym = bbox.Minimum.Y;
                float zm = bbox.Minimum.Z;

                float xM = bbox.Maximum.X;
                float yM = bbox.Maximum.Y;
                float zM = bbox.Maximum.Z;

                // Fill in the front face vertex data.
                v[0] = new Vector3(xm, ym, zm);
                v[1] = new Vector3(xm, yM, zm);
                v[2] = new Vector3(xM, yM, zm);
                v[3] = new Vector3(xM, ym, zm);

                // Fill in the back face vertex data.
                v[4] = new Vector3(xm, ym, zM);
                v[5] = new Vector3(xM, ym, zM);
                v[6] = new Vector3(xM, yM, zM);
                v[7] = new Vector3(xm, yM, zM);

                // Fill in the top face vertex data.
                v[8] = new Vector3(xm, yM, zm);
                v[9] = new Vector3(xm, yM, zM);
                v[10] = new Vector3(xM, yM, zM);
                v[11] = new Vector3(xM, yM, zm);

                // Fill in the bottom face vertex data.
                v[12] = new Vector3(xm, ym, zm);
                v[13] = new Vector3(xM, ym, zm);
                v[14] = new Vector3(xM, ym, zM);
                v[15] = new Vector3(xm, ym, zM);

                // Fill in the left face vertex data.
                v[16] = new Vector3(xm, ym, zM);
                v[17] = new Vector3(xm, yM, zM);
                v[18] = new Vector3(xm, yM, zm);
                v[19] = new Vector3(xm, ym, zm);

                // Fill in the right face vertex data.
                v[20] = new Vector3(xM, ym, zm);
                v[21] = new Vector3(xM, yM, zm);
                v[22] = new Vector3(xM, yM, zM);
                v[23] = new Vector3(xM, ym, zM);

                // Fill in the front face index data
                triangleList.Add(new Triangle(v[0], v[1], v[2]));
                triangleList.Add(new Triangle(v[0], v[2], v[3]));

                // Fill in the back face index data
                triangleList.Add(new Triangle(v[4], v[5], v[6]));
                triangleList.Add(new Triangle(v[4], v[6], v[7]));

                // Fill in the top face index data
                triangleList.Add(new Triangle(v[8], v[9], v[10]));
                triangleList.Add(new Triangle(v[8], v[10], v[11]));

                // Fill in the bottom face index data
                triangleList.Add(new Triangle(v[12], v[13], v[14]));
                triangleList.Add(new Triangle(v[12], v[14], v[15]));

                // Fill in the left face index data
                triangleList.Add(new Triangle(v[16], v[17], v[18]));
                triangleList.Add(new Triangle(v[16], v[18], v[19]));

                // Fill in the right face index data
                triangleList.Add(new Triangle(v[20], v[21], v[22]));
                triangleList.Add(new Triangle(v[20], v[22], v[23]));
            }
            else
            {
                throw new NotImplementedException();
            }

            return triangleList.ToArray();
        }
        /// <summary>
        /// Generate a triangle list from OBB
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="obb">OBB</param>
        /// <returns>Returns the triangle list</returns>
        public static IEnumerable<Triangle> ComputeTriangleList(Topology topology, OrientedBoundingBox obb)
        {
            List<Triangle> triangleList = new List<Triangle>();

            if (topology == Topology.TriangleList)
            {
                Vector3[] v = obb.GetCorners();

                // Fill in the front face index data
                triangleList.Add(new Triangle(v[0], v[1], v[2]));
                triangleList.Add(new Triangle(v[0], v[2], v[3]));

                // Fill in the back face index data
                triangleList.Add(new Triangle(v[4], v[6], v[5]));
                triangleList.Add(new Triangle(v[4], v[7], v[6]));

                // Fill in the top face index data
                triangleList.Add(new Triangle(v[0], v[3], v[7]));
                triangleList.Add(new Triangle(v[0], v[7], v[4]));

                // Fill in the bottom face index data
                triangleList.Add(new Triangle(v[1], v[6], v[2]));
                triangleList.Add(new Triangle(v[1], v[5], v[6]));

                // Fill in the left face index data
                triangleList.Add(new Triangle(v[3], v[2], v[6]));
                triangleList.Add(new Triangle(v[3], v[6], v[7]));

                // Fill in the right face index data
                triangleList.Add(new Triangle(v[0], v[5], v[1]));
                triangleList.Add(new Triangle(v[0], v[4], v[5]));
            }
            else
            {
                throw new NotImplementedException();
            }

            return triangleList.ToArray();
        }
        /// <summary>
        /// Generate a triangle list from sphere
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="sph">Sphere</param>
        /// <param name="sliceCount">Slices</param>
        /// <param name="stackCount">Stacks</param>
        /// <returns>Returns the triangle list</returns>
        public static IEnumerable<Triangle> ComputeTriangleList(Topology topology, BoundingSphere sph, uint sliceCount, uint stackCount)
        {
            List<Vector3> vertList = new List<Vector3>();

            sliceCount--;
            stackCount++;

            #region Positions

            //North pole
            vertList.Add(new Vector3(0.0f, sph.Radius, 0.0f) + sph.Center);

            float phiStep = MathUtil.Pi / stackCount;
            float thetaStep = 2.0f * MathUtil.Pi / sliceCount;

            for (int st = 1; st <= stackCount - 1; ++st)
            {
                float phi = st * phiStep;

                for (int sl = 0; sl <= sliceCount; ++sl)
                {
                    float theta = sl * thetaStep;

                    float x = (float)Math.Sin(phi) * (float)Math.Cos(theta);
                    float y = (float)Math.Cos(phi);
                    float z = (float)Math.Sin(phi) * (float)Math.Sin(theta);

                    Vector3 position = sph.Radius * new Vector3(x, y, z);

                    vertList.Add(position + sph.Center);
                }
            }

            //South pole
            vertList.Add(new Vector3(0.0f, -sph.Radius, 0.0f) + sph.Center);

            #endregion

            List<uint> indexList = new List<uint>();

            #region Indexes

            for (uint index = 1; index <= sliceCount; ++index)
            {
                indexList.Add(0);
                indexList.Add(index + 1);
                indexList.Add(index);
            }

            uint baseIndex = 1;
            uint ringVertexCount = sliceCount + 1;
            for (uint st = 0; st < stackCount - 2; ++st)
            {
                for (uint sl = 0; sl < sliceCount; ++sl)
                {
                    indexList.Add(baseIndex + st * ringVertexCount + sl);
                    indexList.Add(baseIndex + st * ringVertexCount + sl + 1);
                    indexList.Add(baseIndex + (st + 1) * ringVertexCount + sl);

                    indexList.Add(baseIndex + (st + 1) * ringVertexCount + sl);
                    indexList.Add(baseIndex + st * ringVertexCount + sl + 1);
                    indexList.Add(baseIndex + (st + 1) * ringVertexCount + sl + 1);
                }
            }

            uint southPoleIndex = (uint)vertList.Count - 1;

            baseIndex = southPoleIndex - ringVertexCount;

            for (uint index = 0; index < sliceCount; ++index)
            {
                indexList.Add(southPoleIndex);
                indexList.Add(baseIndex + index);
                indexList.Add(baseIndex + index + 1);
            }

            #endregion

            return ComputeTriangleList(topology, vertList, indexList);
        }
        /// <summary>
        /// Generate a triangle list from cylinder
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="cylinder">Cylinder</param>
        /// <param name="segments">Number of segments</param>
        /// <returns>Returns the triangle list</returns>
        public static IEnumerable<Triangle> ComputeTriangleList(Topology topology, BoundingCylinder cylinder, int segments)
        {
            List<Triangle> triangleList = new List<Triangle>();

            if (topology == Topology.TriangleList)
            {
                var verts = cylinder.GetVertices(segments).ToArray();

                for (int i = 0; i < segments - 2; i++)
                {
                    triangleList.Add(new Triangle(verts[0], verts[i + 2], verts[i + 1]));
                    triangleList.Add(new Triangle(verts[0 + segments], verts[i + 1 + segments], verts[i + 2 + segments]));
                }

                for (int i = 0; i < segments; i++)
                {
                    if (i == segments - 1)
                    {
                        triangleList.Add(new Triangle(verts[i], verts[0], verts[i + segments]));
                        triangleList.Add(new Triangle(verts[0], verts[0 + segments], verts[i + segments]));
                    }
                    else
                    {
                        triangleList.Add(new Triangle(verts[i], verts[i + 1], verts[i + segments]));
                        triangleList.Add(new Triangle(verts[i + 1], verts[i + 1 + segments], verts[i + segments]));
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            return triangleList.ToArray();
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
        public IEnumerable<(Vector3 P1, Vector3 P2)> GetEdges()
        {
            return new[]
            {
                (Point2, Point1),
                (Point3, Point2),
                (Point1, Point2),
            };
        }
        /// <summary>
        /// Gets the edge vector between points 2 and 1
        /// </summary>
        public Vector3 GetEdge1()
        {
            return Vector3.Subtract(Point2, Point1);
        }
        /// <summary>
        /// Gets the edge vector between points 3 and 2
        /// </summary>
        public Vector3 GetEdge2()
        {
            return Vector3.Subtract(Point3, Point2);
        }
        /// <summary>
        /// Gets the edge vector between points 1 and 3
        /// </summary>
        public Vector3 GetEdge3()
        {
            return Vector3.Subtract(Point1, Point3);
        }

        /// <summary>
        /// Gets a new triangle with reversed normal vector
        /// </summary>
        /// <returns>Returns a new revered normal triangle</returns>
        public Triangle ReverseNormal()
        {
            return new Triangle(Point1, Point3, Point2);
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
            if (!(obj is Triangle))
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
            unchecked
            {
                var hashCode = Point1.GetHashCode();
                hashCode = (hashCode * 397) ^ Point2.GetHashCode();
                hashCode = (hashCode * 397) ^ Point3.GetHashCode();
                return hashCode;
            }
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Vertex 1 {Point1}; Vertex 2 {Point2}; Vertex 3 {Point3};";
        }
    }
}
