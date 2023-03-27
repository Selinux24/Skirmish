using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    /// <summary>
    /// 3D Line
    /// </summary>
    public struct Line3D : IVertexList
    {
        /// <summary>
        /// Start point
        /// </summary>
        public Vector3 Point1 { get; set; }
        /// <summary>
        /// End point
        /// </summary>
        public Vector3 Point2 { get; set; }
        /// <summary>
        /// Length
        /// </summary>
        public float Length
        {
            get
            {
                return Vector3.Distance(Point1, Point2);
            }
        }

        /// <summary>
        /// Transform line coordinates
        /// </summary>
        /// <param name="line">Line</param>
        /// <param name="transform">Transformation</param>
        /// <returns>Returns new line</returns>
        public static Line3D Transform(Line3D line, Matrix transform)
        {
            if (transform.IsIdentity)
            {
                return line;
            }

            return new Line3D(
                Vector3.TransformCoordinate(line.Point1, transform),
                Vector3.TransformCoordinate(line.Point2, transform));
        }
        /// <summary>
        /// Transform line list coordinates
        /// </summary>
        /// <param name="lines">Line list</param>
        /// <param name="transform">Transformation</param>
        /// <returns>Returns new line list</returns>
        public static IEnumerable<Line3D> Transform(IEnumerable<Line3D> lines, Matrix transform)
        {
            if (transform.IsIdentity)
            {
                return new List<Line3D>(lines);
            }

            List<Line3D> trnLines = new List<Line3D>();

            foreach (var line in lines)
            {
                trnLines.Add(Transform(line, transform));
            }

            return trnLines;
        }

        public static IEnumerable<Line3D> CreateWiredTriangle(IEnumerable<Triangle> triangleList)
        {
            List<Line3D> lines = new List<Line3D>();

            foreach (var triangle in triangleList)
            {
                lines.AddRange(CreateWiredTriangle(triangle));
            }

            return lines;
        }
        public static IEnumerable<Line3D> CreateWiredTriangle(Triangle triangle)
        {
            return CreateWiredTriangle(triangle.GetVertices());
        }
        public static IEnumerable<Line3D> CreateWiredTriangle(IEnumerable<Vector3> corners)
        {
            int[] indexes = new int[6];

            indexes[0] = 0;
            indexes[1] = 1;

            indexes[2] = 1;
            indexes[3] = 2;

            indexes[4] = 2;
            indexes[5] = 0;

            return CreateFromVertices(corners, indexes);
        }
        public static IEnumerable<Line3D> CreateWiredSquare(IEnumerable<Vector3> corners)
        {
            int[] indexes = new int[8];

            indexes[0] = 0;
            indexes[1] = 1;

            indexes[2] = 1;
            indexes[3] = 2;

            indexes[4] = 2;
            indexes[5] = 3;

            indexes[6] = 3;
            indexes[7] = 0;

            return CreateFromVertices(corners, indexes);
        }
        public static IEnumerable<Line3D> CreateWiredRectangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            List<Line3D> lines = new List<Line3D>
            {
                new Line3D(v0, v1),
                new Line3D(v1, v2),
                new Line3D(v2, v3),
                new Line3D(v3, v0)
            };

            return lines;
        }
        public static IEnumerable<Line3D> CreateWiredPolygon(IEnumerable<Vector3> points)
        {
            int count = points.Count();

            int[] indexes = new int[count * 2];

            int i1 = 0;
            int i2 = 1;
            for (int i = 0; i < count; i++)
            {
                indexes[i1] = i + 0;
                indexes[i2] = i == count - 1 ? 0 : i + 1;

                i1 += 2;
                i2 += 2;
            }

            return CreateFromVertices(points, indexes);
        }
        
        public static IEnumerable<Line3D> CreatePath(IEnumerable<Vector3> path)
        {
            List<Line3D> lines = new List<Line3D>();

            var tmp = path.ToArray();

            for (int i = 0; i < tmp.Length - 1; i++)
            {
                lines.Add(new Line3D(tmp[i], tmp[i + 1]));
            }

            return lines.ToArray();
        }
        public static IEnumerable<Line3D> CreateAxis(Matrix transform, float size)
        {
            List<Line3D> lines = new List<Line3D>();

            Vector3 p = transform.TranslationVector;

            Vector3 up = p + (transform.Up * size);
            Vector3 forward = p + (transform.Forward * size);
            Vector3 left = p + (transform.Left * size);
            Vector3 right = p + (transform.Right * size);

            Vector3 c1 = (forward * 0.8f) + (left * 0.2f);
            Vector3 c2 = (forward * 0.8f) + (right * 0.2f);

            lines.Add(new Line3D(p, up));
            lines.Add(new Line3D(p, forward));
            lines.Add(new Line3D(p, left));

            lines.Add(new Line3D(forward, c1));
            lines.Add(new Line3D(forward, c2));

            return lines;
        }
        public static IEnumerable<Line3D> CreateCross(Vector3 point, float size)
        {
            List<Line3D> lines = new List<Line3D>();

            float h = size * 0.5f;
            lines.Add(new Line3D(point + new Vector3(h, h, h), point - new Vector3(h, h, h)));
            lines.Add(new Line3D(point + new Vector3(h, h, -h), point - new Vector3(h, h, -h)));
            lines.Add(new Line3D(point + new Vector3(-h, h, h), point - new Vector3(-h, h, h)));
            lines.Add(new Line3D(point + new Vector3(-h, h, -h), point - new Vector3(-h, h, -h)));

            return lines;
        }
        public static IEnumerable<Line3D> CreateCrossList(IEnumerable<Vector3> points, float size)
        {
            List<Line3D> lines = new List<Line3D>();

            foreach (var point in points)
            {
                lines.AddRange(CreateCross(point, size));
            }

            return lines;
        }
        public static IEnumerable<Line3D> CreateLineList(IEnumerable<Vector3> points)
        {
            List<Line3D> lines = new List<Line3D>();

            var tmp = points.ToArray();

            Vector3 p0 = tmp[0];

            for (int i = 1; i < tmp.Length; i++)
            {
                Vector3 p1 = tmp[i];

                lines.Add(new Line3D(p0, p1));

                p0 = p1;
            }

            return lines;
        }
        public static IEnumerable<Line3D> CreateArc(Vector3 from, Vector3 to, float h, int points)
        {
            List<Line3D> lines = new List<Line3D>();

            float pad = 0.05f;
            float scale = (1.0f - pad * 2) / points;

            Vector3 d = to - from;
            float len = d.Length();

            EvalArc(from, d, len * h, pad, out Vector3 prev);

            for (int i = 1; i <= points; i++)
            {
                float u = pad + i * scale;
                EvalArc(from, d, len * h, u, out Vector3 pt);

                lines.Add(new Line3D(prev, pt));

                prev = pt;
            }

            return lines;
        }
        public static IEnumerable<Line3D> CreateCircle(Vector3 center, float r, int segments)
        {
            List<Line3D> lines = new List<Line3D>();

            float[] dir = new float[segments * 2];

            for (int i = 0; i < segments; ++i)
            {
                float a = i / (float)segments * MathUtil.TwoPi;
                dir[i * 2 + 0] = (float)Math.Cos(a);
                dir[i * 2 + 1] = (float)Math.Sin(a);
            }

            for (int i = 0, j = segments - 1; i < segments; j = i++)
            {
                Line3D line = new Line3D(
                    new Vector3(center.X + dir[j * 2 + 0] * r, center.Y, center.Z + dir[j * 2 + 1] * r),
                    new Vector3(center.X + dir[i * 2 + 0] * r, center.Y, center.Z + dir[i * 2 + 1] * r));

                lines.Add(line);
            }

            return lines;
        }
        public static IEnumerable<Line3D> CreateArrow(Vector3 position, Vector3 point, float edgeSize)
        {
            List<Line3D> lines = new List<Line3D>();

            float eps = 0.001f;
            if (Vector3.DistanceSquared(point, position) >= eps * eps)
            {
                var az = Vector3.Normalize(position - point);
                var ax = Vector3.Cross(Vector3.Up, az);

                lines.Add(new Line3D(point, new Vector3(point.X + az.X * edgeSize + ax.X * edgeSize / 3, point.Y + az.Y * edgeSize + ax.Y * edgeSize / 3, point.Z + az.Z * edgeSize + ax.Z * edgeSize / 3)));
                lines.Add(new Line3D(point, new Vector3(point.X + az.X * edgeSize - ax.X * edgeSize / 3, point.Y + az.Y * edgeSize - ax.Y * edgeSize / 3, point.Z + az.Z * edgeSize - ax.Z * edgeSize / 3)));
            }

            return lines;
        }

        public static IEnumerable<Line3D> CreateFromVertices(GeometryDescriptor geometry)
        {
            return CreateFromVertices(geometry.Vertices, geometry.Indices);
        }
        public static IEnumerable<Line3D> CreateFromVertices(IEnumerable<Vector3> vertices, IEnumerable<uint> indices)
        {
            return CreateFromVertices(vertices, indices.Select(i => (int)i).ToArray());
        }
        public static IEnumerable<Line3D> CreateFromVertices(IEnumerable<Vector3> vertices, IEnumerable<int> indices)
        {
            if (vertices?.Any() != true)
            {
                return Enumerable.Empty<Line3D>();
            }

            List<Line3D> lines = new List<Line3D>();

            if (indices?.Any() != true)
            {
                // Use vertices only
                for (int i = 0; i < vertices.Count(); i += 2)
                {
                    Line3D l = new Line3D()
                    {
                        Point1 = vertices.ElementAtOrDefault(i),
                        Point2 = vertices.ElementAtOrDefault(i + 1),
                    };

                    lines.Add(l);
                }

                return lines;
            }

            for (int i = 0; i < indices.Count(); i += 2)
            {
                Line3D l = new Line3D()
                {
                    Point1 = vertices.ElementAtOrDefault(indices.ElementAtOrDefault(i)),
                    Point2 = vertices.ElementAtOrDefault(indices.ElementAtOrDefault(i + 1)),
                };

                lines.Add(l);
            }

            return lines;
        }

        /// <summary>
        /// Evaluates arc
        /// </summary>
        /// <param name="v0">First point</param>
        /// <param name="v1">Second point</param>
        /// <param name="h">Height</param>
        /// <param name="u">Evaluation time</param>
        /// <param name="res">Resulting point</param>
        private static void EvalArc(Vector3 v0, Vector3 v1, float h, float u, out Vector3 res)
        {
            res = new Vector3(
                v0.X + v1.X * u,
                v0.Y + v1.Y * u + h * (1 - (u * 2 - 1) * (u * 2 - 1)),
                v0.Z + v1.Z * u);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x1">X coordinate of start point</param>
        /// <param name="y1">Y coordinate of start point</param>
        /// <param name="z1">Z coordinate of start point</param>
        /// <param name="x2">X coordinate of end point</param>
        /// <param name="y2">Y coordinate of end point</param>
        /// <param name="z2">Z coordinate of end point</param>
        public Line3D(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            Point1 = new Vector3(x1, y1, z1);
            Point2 = new Vector3(x2, y2, z2);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p1">Start point</param>
        /// <param name="p2">End point</param>
        public Line3D(Vector3 p1, Vector3 p2)
        {
            Point1 = p1;
            Point2 = p2;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ray">Ray</param>
        public Line3D(Ray ray)
        {
            Point1 = ray.Position;
            Point2 = ray.Position + ray.Direction;
        }

        /// <summary>
        /// Gets vertex position list
        /// </summary>
        /// <returns>Returns the vertex position list</returns>
        public IEnumerable<Vector3> GetVertices()
        {
            return new[]
            {
                Point1,
                Point2,
            };
        }
        /// <summary>
        /// Gets the vertex list stride
        /// </summary>
        /// <returns>Returns the list stride</returns>
        public int GetStride()
        {
            return 2;
        }
        /// <summary>
        /// Gets the vertex list topology
        /// </summary>
        /// <returns>Returns the list topology</returns>
        public Topology GetTopology()
        {
            return Topology.LineList;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"P1({Point1}) -> P2({Point2});";
        }
    }
}
