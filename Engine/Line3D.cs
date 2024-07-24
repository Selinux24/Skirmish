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
        public readonly float Length
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

            return new(
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
            if (lines?.Any() != true)
            {
                return [];
            }

            if (transform.IsIdentity)
            {
                return Enumerable.AsEnumerable(lines);
            }

            return lines
                .Select(l => Transform(l, transform))
                .AsEnumerable();
        }

        public static IEnumerable<Line3D> CreateLineList(IEnumerable<Vector3> path)
        {
            List<Line3D> lines = [];

            var tmp = path.ToArray();

            for (int i = 0; i < tmp.Length - 1; i++)
            {
                lines.Add(new(tmp[i], tmp[i + 1]));
            }

            return lines;
        }
        public static IEnumerable<Line3D> CreateTriangle(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            int[] indexes = [0, 1, 1, 2, 2, 0];
            return CreateFromVertices([v0, v1, v2], indexes);
        }
        public static IEnumerable<Line3D> CreateTriangle(Triangle triangle)
        {
            return CreateTriangle(triangle.Point1, triangle.Point2, triangle.Point3);
        }
        public static IEnumerable<Line3D> CreateTriangle(IEnumerable<Triangle> triangleList)
        {
            if (triangleList?.Any() != true)
            {
                return [];
            }

            return triangleList
                .SelectMany(CreateTriangle)
                .AsEnumerable();
        }
        public static IEnumerable<Line3D> CreateSquare(IEnumerable<Vector3> corners)
        {
            int[] indexes = [0, 1, 1, 2, 2, 3, 3, 0];

            return CreateFromVertices(corners, indexes);
        }
        public static IEnumerable<Line3D> CreateRectangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            return
            [
                new(v0, v1),
                new(v1, v2),
                new(v2, v3),
                new(v3, v0)
            ];
        }
        public static IEnumerable<Line3D> CreatePyramid(Vector3 center, float width, float height, float depth)
        {
            var geometry = GeometryUtil.CreatePyramid(Topology.LineList, center, width, height, depth);

            return CreateFromVertices(geometry);
        }
        public static IEnumerable<Line3D> CreateBox(Vector3 center, float width, float height, float depth)
        {
            var geometry = GeometryUtil.CreateBox(Topology.LineList, center, width, height, depth);

            return CreateFromVertices(geometry);
        }
        public static IEnumerable<Line3D> CreateBox(BoundingBox bbox)
        {
            var geometry = GeometryUtil.CreateBox(Topology.LineList, bbox);

            return CreateFromVertices(geometry);
        }
        public static IEnumerable<Line3D> CreateBox(OrientedBoundingBox obb)
        {
            var geometry = GeometryUtil.CreateBox(Topology.LineList, obb);

            return CreateFromVertices(geometry);
        }
        public static IEnumerable<Line3D> CreateBoxes(IEnumerable<BoundingBox> bboxes)
        {
            var geometry = GeometryUtil.CreateBoxes(Topology.LineList, bboxes);

            return CreateFromVertices(geometry);
        }
        public static IEnumerable<Line3D> CreateBoxes(IEnumerable<OrientedBoundingBox> obbs)
        {
            var geometry = GeometryUtil.CreateBoxes(Topology.LineList, obbs);

            return CreateFromVertices(geometry);
        }
        public static IEnumerable<Line3D> CreateSphere(BoundingSphere sph, int sliceCount, int stackCount)
        {
            var geometry = GeometryUtil.CreateSphere(Topology.LineList, sph, sliceCount, stackCount);

            return CreateFromVertices(geometry);
        }
        public static IEnumerable<Line3D> CreateSphere(Vector3 center, float radius, int sliceCount, int stackCount)
        {
            var geometry = GeometryUtil.CreateSphere(Topology.LineList, center, radius, sliceCount, stackCount);

            return CreateFromVertices(geometry);
        }
        public static IEnumerable<Line3D> CreateSpheres(IEnumerable<BoundingSphere> sphList, int sliceCount, int stackCount)
        {
            var geometry = GeometryUtil.CreateSpheres(Topology.LineList, sphList, sliceCount, stackCount);

            return CreateFromVertices(geometry);
        }
        public static IEnumerable<Line3D> CreateFrustum(BoundingFrustum frustum)
        {
            var geometry = GeometryUtil.CreateFrustum(Topology.LineList, frustum);

            return CreateFromVertices(geometry);
        }
        public static IEnumerable<Line3D> CreateConeCupAngle(float cupAngle, float height, int sliceCount)
        {
            var geometry = GeometryUtil.CreateConeCupAngle(Topology.LineList, cupAngle, height, sliceCount);

            return CreateFromVertices(geometry);
        }
        public static IEnumerable<Line3D> CreateConeBaseRadius(float baseRadius, float height, int sliceCount)
        {
            var geometry = GeometryUtil.CreateConeBaseRadius(Topology.LineList, baseRadius, height, sliceCount);

            return CreateFromVertices(geometry);
        }
        public static IEnumerable<Line3D> CreatePolygon(IEnumerable<Vector3> points)
        {
            var geometry = GeometryUtil.CreatePolygon(Topology.LineList, points);

            return CreateFromVertices(geometry);
        }
        public static IEnumerable<Line3D> CreateCylinder(BoundingCylinder cylinder, int sliceCount)
        {
            var geometry = GeometryUtil.CreateCylinder(Topology.LineList, cylinder, sliceCount);

            return CreateFromVertices(geometry);
        }
        public static IEnumerable<Line3D> CreateCylinder(Vector3 center, float radius, float height, int sliceCount)
        {
            var geometry = GeometryUtil.CreateCylinder(Topology.LineList, center, radius, height, sliceCount);

            return CreateFromVertices(geometry);
        }
        public static IEnumerable<Line3D> CreateCapsule(Vector3 center, float radius, float height, int sliceCount, int stackCount)
        {
            var geometry = GeometryUtil.CreateCapsule(Topology.LineList, center, radius, height, sliceCount, stackCount);

            return CreateFromVertices(geometry);
        }
        public static IEnumerable<Line3D> CreateCircle(Vector3 center, float r, int segments)
        {
            List<Line3D> lines = [];

            float[] dir = new float[segments * 2];

            for (int i = 0; i < segments; ++i)
            {
                float a = i / (float)segments * MathUtil.TwoPi;
                dir[i * 2 + 0] = MathF.Cos(a);
                dir[i * 2 + 1] = MathF.Sin(a);
            }

            for (int i = 0, j = segments - 1; i < segments; j = i++)
            {
                Line3D line = new(
                    new(center.X + dir[j * 2 + 0] * r, center.Y, center.Z + dir[j * 2 + 1] * r),
                    new(center.X + dir[i * 2 + 0] * r, center.Y, center.Z + dir[i * 2 + 1] * r));

                lines.Add(line);
            }

            return lines;
        }
        public static IEnumerable<Line3D> CreateCircle(IEnumerable<(Vector3 center, float r)> circles, int segments)
        {
            if (circles?.Any() != true)
            {
                return [];
            }

            return circles
                .SelectMany(c => CreateCircle(c.center, c.r, segments))
                .AsEnumerable();
        }
        public static IEnumerable<Line3D> CreateAxis(Matrix transform, float size)
        {
            List<Line3D> lines = [];

            var p = transform.TranslationVector;

            var up = p + (transform.Up * size);
            var forward = p + (transform.Forward * size);
            var left = p + (transform.Left * size);
            var right = p + (transform.Right * size);

            var c1 = (forward * 0.8f) + (left * 0.2f);
            var c2 = (forward * 0.8f) + (right * 0.2f);

            lines.Add(new(p, up));
            lines.Add(new(p, forward));
            lines.Add(new(p, left));

            lines.Add(new(forward, c1));
            lines.Add(new(forward, c2));

            return lines;
        }
        public static IEnumerable<Line3D> CreateAxis(IEnumerable<Matrix> transforms, float size)
        {
            if (transforms?.Any() != true)
            {
                return [];
            }

            return transforms
                .SelectMany(t => CreateAxis(t, size))
                .AsEnumerable();
        }
        public static IEnumerable<Line3D> CreateCross(Vector3 point, float size)
        {
            List<Line3D> lines = [];

            float h = size * 0.5f;
            lines.Add(new(point + new Vector3(h, h, h), point - new Vector3(h, h, h)));
            lines.Add(new(point + new Vector3(h, h, -h), point - new Vector3(h, h, -h)));
            lines.Add(new(point + new Vector3(-h, h, h), point - new Vector3(-h, h, h)));
            lines.Add(new(point + new Vector3(-h, h, -h), point - new Vector3(-h, h, -h)));

            return lines;
        }
        public static IEnumerable<Line3D> CreateCross(IEnumerable<Vector3> points, float size)
        {
            if (points?.Any() != true)
            {
                return [];
            }

            return points
                .SelectMany(p => CreateCross(p, size))
                .AsEnumerable();
        }
        public static IEnumerable<Line3D> CreateArc(Vector3 from, Vector3 to, float h, int points)
        {
            List<Line3D> lines = [];

            float pad = 0.05f;
            float scale = (1.0f - pad * 2) / points;

            var d = to - from;
            float len = d.Length();
            var prev = from;

            for (int i = 0; i <= points; i++)
            {
                float u = pad + i * scale;
                EvalArc(from, d, len * h, u, out var pt);

                lines.Add(new(prev, pt));

                prev = pt;
            }

            lines.Add(new(prev, to));

            return lines;
        }
        public static IEnumerable<Line3D> CreateArc(IEnumerable<(Vector3 from, Vector3 to)> arcs, float h, int points)
        {
            if (arcs?.Any() != true)
            {
                return [];
            }

            return arcs
                .SelectMany(a => CreateArc(a.from, a.to, h, points))
                .AsEnumerable();
        }
        public static IEnumerable<Line3D> CreateArrow(Vector3 point, Vector3 direction, float edgeSize)
        {
            return CreateArrow(point, direction, Vector3.Up, edgeSize);
        }
        public static IEnumerable<Line3D> CreateArrow(Vector3 point, Vector3 direction, Vector3 up, float edgeSize)
        {
            var lines = new List<Line3D>();

            var az = Vector3.Normalize(direction);
            var ax = Vector3.Cross(up, az);
            az *= edgeSize;
            ax *= edgeSize / 3f;

            Vector3 pl = new(point.X - az.X + ax.X, point.Y - az.Y + ax.Y, point.Z - az.Z + ax.Z);
            Vector3 pr = new(point.X - az.X - ax.X, point.Y - az.Y - ax.Y, point.Z - az.Z - ax.Z);

            lines.Add(new(point, pl));
            lines.Add(new(point, pr));

            return lines;
        }
        public static IEnumerable<Line3D> CreateArrow(IEnumerable<(Vector3 position, Vector3 point)> arrows, float edgeSize)
        {
            if (arrows?.Any() != true)
            {
                return [];
            }

            return arrows
                .SelectMany(a => CreateArrow(a.point, a.point, edgeSize))
                .AsEnumerable();
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
                return [];
            }

            var vArray = vertices.ToArray();

            List<Line3D> lines = [];

            if (indices?.Any() != true)
            {
                // Use vertices only
                for (int i = 0; i < vArray.Length; i += 2)
                {
                    lines.Add(new()
                    {
                        Point1 = vArray[i],
                        Point2 = vArray[i + 1],
                    });
                }

                return lines;
            }

            var iArray = indices.ToArray();

            for (int i = 0; i < iArray.Length; i += 2)
            {
                lines.Add(new()
                {
                    Point1 = vArray[iArray[i]],
                    Point2 = vArray[iArray[i + 1]],
                });
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
            res = new(
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
            Point1 = new(x1, y1, z1);
            Point2 = new(x2, y2, z2);
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

        /// <inheritdoc/>
        public readonly IEnumerable<Vector3> GetVertices()
        {
            yield return Point1;
            yield return Point2;
        }
        /// <inheritdoc/>
        public readonly int GetStride()
        {
            return 2;
        }
        /// <inheritdoc/>
        public readonly Topology GetTopology()
        {
            return Topology.LineList;
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"P1({Point1}) -> P2({Point2});";
        }
    }
}
