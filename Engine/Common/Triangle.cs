using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D;

namespace Engine.Common
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
                return Helpers.Helper.Angle(this.Normal, Vector3.Up);
            }
        }

        /// <summary>
        /// Generate a triangle list from vertices
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <returns>Return the triangle list</returns>
        public static Triangle[] ComputeTriangleList(PrimitiveTopology topology, VertexData[] vertices)
        {
            List<Triangle> triangleList = new List<Triangle>();

            if (topology == PrimitiveTopology.TriangleList || topology == PrimitiveTopology.TriangleListWithAdjacency)
            {
                for (int i = 0; i < vertices.Length; i += 3)
                {
                    Triangle tri = new Triangle(
                        vertices[i + 0].Position.Value,
                        vertices[i + 1].Position.Value,
                        vertices[i + 2].Position.Value);

                    triangleList.Add(tri);
                }
            }
            else if (topology == PrimitiveTopology.TriangleStrip || topology == PrimitiveTopology.TriangleStripWithAdjacency)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new Exception("Bad topology for triangle");
            }

            return triangleList.ToArray();
        }
        /// <summary>
        /// Generate a triangle list from vertices and indices
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        /// <returns>Return the triangle list</returns>
        public static Triangle[] ComputeTriangleList(PrimitiveTopology topology, VertexData[] vertices, uint[] indices)
        {
            List<Triangle> triangleList = new List<Triangle>();

            if (topology == PrimitiveTopology.TriangleList || topology == PrimitiveTopology.TriangleListWithAdjacency)
            {
                for (int i = 0; i < indices.Length; i += 3)
                {
                    Triangle tri = new Triangle(
                        vertices[indices[i + 0]].Position.Value,
                        vertices[indices[i + 1]].Position.Value,
                        vertices[indices[i + 2]].Position.Value);

                    triangleList.Add(tri);
                }
            }
            else if (topology == PrimitiveTopology.TriangleStrip || topology == PrimitiveTopology.TriangleStripWithAdjacency)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new Exception("Bad topology for triangle");
            }

            return triangleList.ToArray();
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
        /// Tests if point is in triangle
        /// </summary>
        /// <param name="tri">Triangle</param>
        /// <param name="point">Point</param>
        /// <returns>Returns true if point is in triangle</returns>
        public static bool PointInTriangle(Triangle tri, Vector3 point)
        {
            Vector3 u = new Vector3(
                Triangle.PointFromVector(point, tri.I1) - Triangle.PointFromVector(tri.Point1, tri.I1),
                Triangle.PointFromVector(tri.Point2, tri.I1) - Triangle.PointFromVector(tri.Point1, tri.I1),
                Triangle.PointFromVector(tri.Point3, tri.I1) - Triangle.PointFromVector(tri.Point1, tri.I1));

            Vector3 v = new Vector3(
                Triangle.PointFromVector(point, tri.I2) - Triangle.PointFromVector(tri.Point1, tri.I2),
                Triangle.PointFromVector(tri.Point2, tri.I2) - Triangle.PointFromVector(tri.Point1, tri.I2),
                Triangle.PointFromVector(tri.Point3, tri.I2) - Triangle.PointFromVector(tri.Point1, tri.I2));

            float a, b, c;
            if (u.Y == 0.0f)
            {
                b = u.X / u.Z;
                if (b >= 0.0f && b <= 1.0f)
                {
                    c = (b * v.Z);
                    a = (v.X - c) / v.Y;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                b = ((v.X * u.Y) - (u.X * v.Y)) / ((v.Z * u.Y) - (u.Z * v.Y));
                if (b >= 0.0f && b <= 1.0f)
                {
                    c = (b * u.Z);
                    a = (u.X - c) / u.Y;
                }
                else
                {
                    return false;
                }
            }

            return (a >= 0 && (a + b) <= 1);
        }
        /// <summary>
        /// Gest vector component value by index
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <param name="index">Index. 0, 1, 2 for components x, y, z</param>
        /// <returns>Return specified component value</returns>
        public static float PointFromVector(Vector3 vector, int index)
        {
            if (index == 0)
            {
                return vector.X;
            }
            else if (index == 1)
            {
                return vector.Y;
            }
            else if (index == 2)
            {
                return vector.Z;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Gets closest point in triangle from another point
        /// </summary>
        /// <param name="tri">Triangle</param>
        /// <param name="point">Point</param>
        /// <returns>Returns closest point in triangle from another point</returns>
        public static Vector3 ClosestPointInTriangle(Triangle tri, Vector3 point)
        {
            Vector3 diff = tri.Point1 - point;
            Vector3 edge1 = tri.Point2 - tri.Point1;
            Vector3 edge2 = tri.Point3 - tri.Point1;

            float diffLength = diff.LengthSquared();
            float edge1Length = edge1.LengthSquared();
            float edge2Length = edge2.LengthSquared();

            float edgesProj = Vector3.Dot(edge1, edge2);
            float edge1Proj = Vector3.Dot(diff, edge1);
            float edge2Proj = Vector3.Dot(diff, edge2);

            float determinant = Math.Abs(edge1Length * edge2Length - edgesProj * edgesProj);
            float s = edgesProj * edge2Proj - edge2Length * edge1Proj;
            float t = edgesProj * edge1Proj - edge1Length * edge2Proj;

            if (s + t <= determinant)
            {
                if (s < 0.0f)
                {
                    if (t < 0.0f)
                    {
                        //Región 4
                        if (edge1Proj < 0.0f)
                        {
                            t = 0.0f;
                            if (-edge1Proj >= edge1Length)
                            {
                                s = 1.0f;
                            }
                            else
                            {
                                s = -edge1Proj / edge1Length;
                            }
                        }
                        else
                        {
                            s = 0.0f;
                            if (edge2Proj >= 0.0f)
                            {
                                t = 0.0f;
                            }
                            else if (-edge2Proj >= edge2Length)
                            {
                                t = 1.0f;
                            }
                            else
                            {
                                t = -edge2Proj / edge2Length;
                            }
                        }
                    }
                    else
                    {
                        //Región 3
                        s = 0.0f;
                        if (edge2Proj >= 0.0f)
                        {
                            t = 0.0f;
                        }
                        else if (-edge2Proj >= edge2Length)
                        {
                            t = 0.0f;
                        }
                        else
                        {
                            t = -edge2Proj / edge2Length;
                        }
                    }
                }
                else if (t < 0.0f)
                {
                    //Región 5
                    t = 0.0f;
                    if (edge1Proj >= 0.0f)
                    {
                        s = 0.0f;
                    }
                    else if (-edge1Proj >= edge1Length)
                    {
                        s = 1.0f;
                    }
                    else
                    {
                        s = -edge1Proj / edge1Length;
                    }
                }
                else
                {
                    //Región 0

                    //Mínimo punto interior
                    float fInvDet = 1.0f / determinant;
                    s *= fInvDet;
                    t *= fInvDet;
                }
            }
            else
            {
                if (s < 0.0f)
                {
                    //Región 2
                    float tmp0 = edgesProj + edge1Proj;
                    float tmp1 = edge2Length + edge2Proj;
                    if (tmp1 > tmp0)
                    {
                        float numer = tmp1 - tmp0;
                        float denom = edge1Length - 2.0f * edgesProj + edge2Length;
                        if (numer >= denom)
                        {
                            s = 1.0f;
                            t = 0.0f;
                        }
                        else
                        {
                            s = numer / denom;
                            t = 1.0f - s;
                        }
                    }
                    else
                    {
                        s = 0.0f;
                        if (tmp1 <= 0.0f)
                        {
                            t = 1.0f;
                        }
                        else if (edge2Proj >= 0.0f)
                        {
                            t = 0.0f;
                        }
                        else
                        {
                            t = -edge2Proj / edge2Length;
                        }
                    }
                }
                else if (t < 0.0f)
                {
                    //Región 6
                    float tmp0 = edgesProj + edge2Proj;
                    float tmp1 = edge1Length + edge1Proj;
                    if (tmp1 > tmp0)
                    {
                        float numer = tmp1 - tmp0;
                        float denom = edge1Length - 2.0f * edgesProj + edge2Length;
                        if (numer >= denom)
                        {
                            t = 1.0f;
                            s = 0.0f;
                        }
                        else
                        {
                            t = numer / denom;
                            s = 1.0f - t;
                        }
                    }
                    else
                    {
                        t = 0.0f;
                        if (tmp1 <= 0.0f)
                        {
                            s = 1.0f;
                        }
                        else if (edge1Proj >= 0.0f)
                        {
                            s = 0.0f;
                        }
                        else
                        {
                            s = -edge1Proj / edge1Length;
                        }
                    }
                }
                else
                {
                    //Región 1
                    float numer = edge2Length + edge2Proj - edgesProj - edge1Proj;
                    if (numer <= 0.0f)
                    {
                        s = 0.0f;
                        t = 1.0f;
                    }
                    else
                    {
                        float denom = edge1Length - 2.0f * edgesProj + edge2Length;
                        if (numer >= denom)
                        {
                            s = 1.0f;
                            t = 0.0f;
                        }
                        else
                        {
                            s = numer / denom;
                            t = 1.0f - s;
                        }
                    }
                }
            }

            return tri.Point1 + s * edge1 + t * edge2;
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
            distance = 0;

            Vector3 point;

            if (ray.Intersects(ref this.Plane, out point))
            {
                if (PointInTriangle(this, point))
                {
                    distance = Vector3.Distance(ray.Position, point);

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Intersection test between ray and triangle
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="point">Intersection point, if any</param>
        /// <returns>Returns true if ray intersects with this triangle</returns>
        public bool Intersects(ref Ray ray, out Vector3 point)
        {
            if (ray.Intersects(ref this.Plane, out point))
            {
                if (PointInTriangle(this, point))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
