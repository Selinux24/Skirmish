using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D;

namespace Engine
{
    /// <summary>
    /// Triangle
    /// </summary>
    public struct Triangle
    {
        /// <summary>
        /// First point
        /// </summary>
        public Vector3 Point1;
        /// <summary>
        /// Second point
        /// </summary>
        public Vector3 Point2;
        /// <summary>
        /// Third point
        /// </summary>
        public Vector3 Point3;
        /// <summary>
        /// Center
        /// </summary>
        public Vector3 Center;
        /// <summary>
        /// First index
        /// </summary>
        public int I1;
        /// <summary>
        /// Second index
        /// </summary>
        public int I2;
        /// <summary>
        /// Plane
        /// </summary>
        public Plane Plane;
        /// <summary>
        /// Normal
        /// </summary>
        public Vector3 Normal
        {
            get
            {
                return this.Plane.Normal;
            }
        }
        /// <summary>
        /// Min
        /// </summary>
        public Vector3 Min
        {
            get
            {
                return Vector3.Min(this.Point1, Vector3.Min(this.Point2, this.Point3));
            }
        }
        /// <summary>
        /// Max
        /// </summary>
        public Vector3 Max
        {
            get
            {
                return Vector3.Max(this.Point1, Vector3.Max(this.Point2, this.Point3));
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
                float a = (this.Point1 - this.Point2).Length();
                float b = (this.Point1 - this.Point3).Length();
                float c = (this.Point2 - this.Point3).Length();

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
                return Helper.Angle(this.Normal, Vector3.Down);
            }
        }

        /// <summary>
        /// Generate a triangle list from vertices
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="vertices">Vertices</param>
        /// <returns>Returns the triangle list</returns>
        public static Triangle[] ComputeTriangleList(PrimitiveTopology topology, Vector3[] vertices)
        {
            List<Triangle> triangleList = new List<Triangle>();

            if (topology == PrimitiveTopology.TriangleList || topology == PrimitiveTopology.TriangleListWithAdjacency)
            {
                for (int i = 0; i < vertices.Length; i += 3)
                {
                    Triangle tri = new Triangle(
                        vertices[i + 0],
                        vertices[i + 1],
                        vertices[i + 2]);

                    triangleList.Add(tri);
                }
            }
            else if (topology == PrimitiveTopology.TriangleStrip || topology == PrimitiveTopology.TriangleStripWithAdjacency)
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
        public static Triangle[] ComputeTriangleList(PrimitiveTopology topology, Vector3[] vertices, uint[] indices)
        {
            List<Triangle> triangleList = new List<Triangle>();

            if (topology == PrimitiveTopology.TriangleList || topology == PrimitiveTopology.TriangleListWithAdjacency)
            {
                for (int i = 0; i < indices.Length; i += 3)
                {
                    Triangle tri = new Triangle(
                        vertices[indices[i + 0]],
                        vertices[indices[i + 1]],
                        vertices[indices[i + 2]]);

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
        /// Generate a triangle list from polygon
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <returns>Returns the triangle list</returns>
        public static Triangle[] ComputeTriangleList(PrimitiveTopology topology, Polygon poly)
        {
            if (topology == PrimitiveTopology.TriangleList || topology == PrimitiveTopology.TriangleListWithAdjacency)
            {
                Triangle[] triList = new Triangle[poly.Count - 2];

                for (int i = 0; i < triList.Length; i++)
                {
                    triList[i] = new Triangle(poly[0], poly[i + 1], poly[i + 2]);
                }

                return triList;
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
        public static Triangle[] Transform(Triangle[] triangles, Matrix transform)
        {
            Triangle[] trnTriangles = new Triangle[triangles.Length];

            for (int i = 0; i < triangles.Length; i++)
            {
                trnTriangles[i] = Transform(triangles[i], transform);
            }

            return trnTriangles;
        }
        /// <summary>
        /// Performs intersection test with ray and triangle list
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="triangles">Triangle list</param>
        /// <param name="facingOnly">Select only triangles facing to ray origin</param>
        /// <param name="position">Result picked position</param>
        /// <param name="triangle">Result picked triangle</param>
        /// <returns>Returns first intersection if exists</returns>
        public static bool IntersectFirst(ref Ray ray, Triangle[] triangles, bool facingOnly, out Vector3 position, out Triangle triangle)
        {
            position = Vector3.Zero;
            triangle = new Triangle();

            for (int i = 0; i < triangles.Length; i++)
            {
                Triangle tri = triangles[i];

                bool cull = false;
                if (facingOnly == true)
                {
                    cull = Vector3.Dot(ray.Direction, tri.Normal) >= 0f;
                }

                if (!cull)
                {
                    Vector3 pos;
                    if (tri.Intersects(ref ray, out pos))
                    {
                        position = pos;
                        triangle = tri;

                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Performs intersection test with ray and triangle list
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="triangles">Triangle list</param>
        /// <param name="facingOnly">Select only triangles facing to ray origin</param>
        /// <param name="position">Result picked position</param>
        /// <param name="triangle">Result picked triangle</param>
        /// <returns>Returns nearest intersection if exists</returns>
        public static bool IntersectNearest(ref Ray ray, Triangle[] triangles, bool facingOnly, out Vector3 position, out Triangle triangle)
        {
            position = Vector3.Zero;
            triangle = new Triangle();

            Vector3[] pickedPositions;
            Triangle[] pickedTriangles;
            if (IntersectAll(ref ray, triangles, facingOnly, out pickedPositions, out pickedTriangles))
            {
                float distanceMin = float.MaxValue;

                for (int i = 0; i < pickedPositions.Length; i++)
                {
                    float dist = Vector3.DistanceSquared(pickedPositions[i], ray.Position);
                    if (dist < distanceMin)
                    {
                        distanceMin = dist;
                        position = pickedPositions[i];
                        triangle = pickedTriangles[i];
                    }
                }

                return true;
            }

            return false;
        }
        /// <summary>
        /// Performs intersection test with ray and triangle list
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="triangles">Triangle list</param>
        /// <param name="facingOnly">Select only triangles facing to ray origin</param>
        /// <param name="pickedPositions">Picked position list</param>
        /// <param name="pickedTriangles">Picked triangle list</param>
        /// <returns>Returns all intersections if exists</returns>
        public static bool IntersectAll(ref Ray ray, Triangle[] triangles, bool facingOnly, out Vector3[] pickedPositions, out Triangle[] pickedTriangles)
        {
            SortedDictionary<float, Vector3> pickedPositionList = new SortedDictionary<float, Vector3>();
            SortedDictionary<float, Triangle> pickedTriangleList = new SortedDictionary<float, Triangle>();

            foreach (Triangle t in triangles)
            {
                bool cull = false;
                if (facingOnly == true)
                {
                    cull = Vector3.Dot(ray.Direction, t.Normal) >= 0f;
                }

                if (!cull)
                {
                    Vector3 pos;
                    if (t.Intersects(ref ray, out pos))
                    {
                        //Avoid duplicate picked positions
                        if (!pickedPositionList.ContainsValue(pos))
                        {
                            float d = Vector3.DistanceSquared(ray.Position, pos);
                            while (pickedPositionList.ContainsKey(d))
                            {
                                //Avoid duplicate distance keys
                                d += 0.001f;
                            }

                            pickedPositionList.Add(d, pos);
                            pickedTriangleList.Add(d, t);
                        }
                    }
                }
            }

            if (pickedPositionList.Values.Count > 0)
            {
                pickedPositions = new Vector3[pickedPositionList.Values.Count];
                pickedTriangles = new Triangle[pickedTriangleList.Values.Count];

                pickedPositionList.Values.CopyTo(pickedPositions, 0);
                pickedTriangleList.Values.CopyTo(pickedTriangles, 0);

                return true;
            }
            else
            {
                pickedPositions = null;
                pickedTriangles = null;

                return false;
            }
        }
        /// <summary>
        /// Gets the area of the triangle projected onto the XZ-plane.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <param name="c">The third point.</param>
        /// <param name="area">The calculated area.</param>
        public static void Area2D(ref Vector3 a, ref Vector3 b, ref Vector3 c, out float area)
        {
            float abx = b.X - a.X;
            float abz = b.Z - a.Z;
            float acx = c.X - a.X;
            float acz = c.Z - a.Z;
            area = acx * abz - abx * acz;
        }
        /// <summary>
        /// Gets the area of the triangle projected onto the XZ-plane.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <param name="c">The third point.</param>
        /// <returns>The calculated area.</returns>
        public static float Area2D(Vector3 a, Vector3 b, Vector3 c)
        {
            float result;
            Area2D(ref a, ref b, ref c, out result);
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
            this.Point1 = point1;
            this.Point2 = point2;
            this.Point3 = point3;
            this.Center = Vector3.Multiply(point1 + point2 + point3, 1.0f / 3.0f);
            this.Plane = new Plane(this.Point1, this.Point2, this.Point3);

            Vector3 n = this.Plane.Normal;
            float absX = (float)Math.Abs(n.X);
            float absY = (float)Math.Abs(n.Y);
            float absZ = (float)Math.Abs(n.Z);

            Vector3 a = new Vector3(absX, absY, absZ);
            if (a.X > a.Y)
            {
                if (a.X > a.Z)
                {
                    this.I1 = 1;
                    this.I2 = 2;
                }
                else
                {
                    this.I1 = 0;
                    this.I2 = 1;
                }
            }
            else
            {
                if (a.Y > a.Z)
                {
                    this.I1 = 0;
                    this.I2 = 2;
                }
                else
                {
                    this.I1 = 0;
                    this.I2 = 1;
                }
            }
        }

        /// <summary>
        /// Text representation
        /// </summary>
        public override string ToString()
        {
            return string.Format("Vertex 1 {0}; Vertex 2 {1}; Vertex 3 {2};", this.Point1, this.Point2, this.Point3);
        }

        /// <summary>
        /// Intersection test between ray and triangle
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <returns>Returns true if ray intersects with this triangle</returns>
        public bool Intersects(ref Ray ray)
        {
            Vector3 position;
            return Intersects(ref ray, out position);
        }
        /// <summary>
        /// Intersection test between ray and triangle
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="distance">Distance from ray origin and intersection point, if any</param>
        /// <returns>Returns true if ray intersects with this triangle</returns>
        public bool Intersects(ref Ray ray, out float distance)
        {
            return Collision.RayIntersectsTriangle(ref ray, ref this.Point1, ref this.Point2, ref this.Point3, out distance);
        }
        /// <summary>
        /// Intersection test between ray and triangle
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="point">Intersection point, if any</param>
        /// <returns>Returns true if ray intersects with this triangle</returns>
        public bool Intersects(ref Ray ray, out Vector3 point)
        {
            return Collision.RayIntersectsTriangle(ref ray, ref this.Point1, ref this.Point2, ref this.Point3, out point);
        }
        /// <summary>
        /// Retrieves the three corners of the triangle.
        /// </summary>
        /// <returns>An array of points representing the three corners of the triangle.</returns>
        public Vector3[] GetCorners()
        {
            return new[]
            {
                this.Point1,
                this.Point2,
                this.Point3,
            };
        }
    }
}
