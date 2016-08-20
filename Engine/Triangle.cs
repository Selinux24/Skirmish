using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D;
using Engine.Common;

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
        /// 
        /// </summary>
        /// <param name="topology"></param>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public static Triangle[] ComputeTriangleList(PrimitiveTopology topology, BoundingBox bbox)
        {
            List<Triangle> triangleList = new List<Triangle>();

            if (topology == PrimitiveTopology.TriangleList || topology == PrimitiveTopology.TriangleListWithAdjacency)
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

        public static Triangle[] ComputeTriangleList(PrimitiveTopology topology, BoundingCylinder cylinder, int segments)
        {
            List<Triangle> triangleList = new List<Triangle>();

            if (topology == PrimitiveTopology.TriangleList || topology == PrimitiveTopology.TriangleListWithAdjacency)
            {
                List<Vector3> verts = new List<Vector3>();

                //verts
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j <= segments; j++)
                    {
                        float theta = ((float)j / (float)segments) * 2 * (float)Math.PI;
                        float st = (float)Math.Sin(theta), ct = (float)Math.Cos(theta);

                        verts.Add(cylinder.Position + new Vector3(cylinder.Radius * st, cylinder.Height * i, cylinder.Radius * ct));
                        verts.Add(cylinder.Position + (i == 0 ? -Vector3.UnitY : Vector3.UnitY));
                    }
                }

                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j <= segments; j++)
                    {
                        float theta = ((float)j / (float)segments) * 2 * (float)Math.PI;
                        float st = (float)Math.Sin(theta), ct = (float)Math.Cos(theta);

                        verts.Add(cylinder.Position + new Vector3(cylinder.Radius * st, cylinder.Height * i, cylinder.Radius * ct));
                        verts.Add(cylinder.Position + new Vector3(st, 0, ct));
                    }
                }

                //inds
                int start = 0;

                //bottom cap
                for (int i = 1; i < segments - 1; i++)
                {
                    triangleList.Add(new Triangle(verts[start], verts[start + i + 1], verts[start + i]));
                }

                start = segments + 1;

                //top cap
                for (int i = 1; i < segments - 1; i++)
                {
                    triangleList.Add(new Triangle(verts[start], verts[start + i], verts[start + i + 1]));
                }

                start += segments + 1;

                //edge
                for (int i = 0; i <= segments; i++)
                {
                    triangleList.Add(new Triangle(verts[start + i], verts[start + segments + i + 1], verts[start + segments + i]));
                    triangleList.Add(new Triangle(verts[start + i], verts[start + i + 1], verts[start + segments + i + 1]));
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
        /// <param name="distance">Result distance to picked position</param>
        /// <returns>Returns first intersection if exists</returns>
        public static bool IntersectFirst(ref Ray ray, Triangle[] triangles, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            position = Vector3.Zero;
            triangle = new Triangle();
            distance = float.MaxValue;

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
                    float d;
                    if (tri.Intersects(ref ray, out pos, out d))
                    {
                        position = pos;
                        triangle = tri;
                        distance = d;

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
        /// <param name="distance">Result distance to picked position</param>
        /// <returns>Returns nearest intersection if exists</returns>
        public static bool IntersectNearest(ref Ray ray, Triangle[] triangles, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            position = Vector3.Zero;
            triangle = new Triangle();
            distance = float.MaxValue;

            Vector3[] pickedPositions;
            Triangle[] pickedTriangles;
            float[] pickedDistances;
            if (IntersectAll(ref ray, triangles, facingOnly, out pickedPositions, out pickedTriangles, out pickedDistances))
            {
                float distanceMin = float.MaxValue;

                for (int i = 0; i < pickedPositions.Length; i++)
                {
                    float dist = pickedDistances[i];
                    if (dist < distanceMin)
                    {
                        distanceMin = dist;
                        position = pickedPositions[i];
                        triangle = pickedTriangles[i];
                        distance = pickedDistances[i];
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
        /// <param name="pickedDistances">Distances to picked positions</param>
        /// <returns>Returns all intersections if exists</returns>
        public static bool IntersectAll(ref Ray ray, Triangle[] triangles, bool facingOnly, out Vector3[] pickedPositions, out Triangle[] pickedTriangles, out float[] pickedDistances)
        {
            SortedDictionary<float, Vector3> pickedPositionList = new SortedDictionary<float, Vector3>();
            SortedDictionary<float, Triangle> pickedTriangleList = new SortedDictionary<float, Triangle>();
            SortedDictionary<float, float> pickedDistancesList = new SortedDictionary<float, float>();

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
                    float d;
                    if (t.Intersects(ref ray, out pos, out d))
                    {
                        //Avoid duplicate picked positions
                        if (!pickedPositionList.ContainsValue(pos))
                        {
                            float k = d;
                            while (pickedPositionList.ContainsKey(k))
                            {
                                //Avoid duplicate distance keys
                                k += 0.001f;
                            }

                            pickedPositionList.Add(k, pos);
                            pickedTriangleList.Add(k, t);
                            pickedDistancesList.Add(k, d);
                        }
                    }
                }
            }

            if (pickedPositionList.Values.Count > 0)
            {
                pickedPositions = new Vector3[pickedPositionList.Values.Count];
                pickedTriangles = new Triangle[pickedTriangleList.Values.Count];
                pickedDistances = new float[pickedDistancesList.Values.Count];

                pickedPositionList.Values.CopyTo(pickedPositions, 0);
                pickedTriangleList.Values.CopyTo(pickedTriangles, 0);
                pickedDistancesList.Values.CopyTo(pickedDistances, 0);

                return true;
            }
            else
            {
                pickedPositions = null;
                pickedTriangles = null;
                pickedDistances = null;

                return false;
            }
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
            float distance;
            return GeometryUtil.RayIntersectsTriangle(ref ray, ref this.Point1, ref this.Point2, ref this.Point3, out distance);
        }
        /// <summary>
        /// Intersection test between ray and triangle
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="distance">Distance from ray origin and intersection point, if any</param>
        /// <returns>Returns true if ray intersects with this triangle</returns>
        public bool Intersects(ref Ray ray, out float distance)
        {
            return GeometryUtil.RayIntersectsTriangle(ref ray, ref this.Point1, ref this.Point2, ref this.Point3, out distance);
        }
        /// <summary>
        /// Intersection test between ray and triangle
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="point">Intersection point, if any</param>
        /// <param name="distance">Distance from ray origin and intersection point, if any</param>
        /// <returns>Returns true if ray intersects with this triangle</returns>
        public bool Intersects(ref Ray ray, out Vector3 point, out float distance)
        {
            return GeometryUtil.RayIntersectsTriangle(ref ray, ref this.Point1, ref this.Point2, ref this.Point3, out point, out distance);
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
