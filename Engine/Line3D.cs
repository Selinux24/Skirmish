using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// 3D Line
    /// </summary>
    public struct Line3D
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
                return Vector3.Distance(this.Point1, this.Point2);
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
        public static Line3D[] Transform(Line3D[] lines, Matrix transform)
        {
            Line3D[] trnLines = new Line3D[lines.Length];

            for (int i = 0; i < lines.Length; i++)
            {
                trnLines[i] = Transform(lines[i], transform);
            }

            return trnLines;
        }

        public static Line3D[] CreateWiredTriangle(Triangle[] triangleList)
        {
            List<Line3D> lines = new List<Line3D>();

            for (int i = 0; i < triangleList.Length; i++)
            {
                lines.AddRange(CreateWiredTriangle(triangleList[i]));
            }

            return lines.ToArray();
        }
        public static Line3D[] CreateWiredTriangle(Triangle triangle)
        {
            return CreateWiredTriangle(triangle.GetVertices());
        }
        public static Line3D[] CreateWiredTriangle(Vector3[] corners)
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
        public static Line3D[] CreateWiredSquare(Vector3[] corners)
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
        public static Line3D[] CreateWiredPolygon(Vector3[] points)
        {
            int[] indexes = new int[points.Length * 2];

            int i1 = 0;
            int i2 = 1;
            for (int i = 0; i < points.Length; i++)
            {
                indexes[i1] = i + 0;
                indexes[i2] = i == points.Length - 1 ? 0 : i + 1;

                i1 += 2;
                i2 += 2;
            }

            return CreateFromVertices(points, indexes);
        }
        public static Line3D[] CreateWiredBox(BoundingBox[] bboxList)
        {
            List<Line3D> lines = new List<Line3D>();

            for (int i = 0; i < bboxList.Length; i++)
            {
                lines.AddRange(CreateWiredBox(bboxList[i]));
            }

            return lines.ToArray();
        }
        public static Line3D[] CreateWiredBox(BoundingBox bbox)
        {
            return CreateWiredBox(bbox.GetCorners());
        }
        public static Line3D[] CreateWiredBox(OrientedBoundingBox[] obboxList)
        {
            List<Line3D> lines = new List<Line3D>();

            for (int i = 0; i < obboxList.Length; i++)
            {
                lines.AddRange(CreateWiredBox(obboxList[i]));
            }

            return lines.ToArray();
        }
        public static Line3D[] CreateWiredBox(OrientedBoundingBox obbox)
        {
            return CreateWiredBox(obbox.GetCorners());
        }
        public static Line3D[] CreateWiredBox(Vector3[] corners)
        {
            List<int> indexes = new List<int>(24)
            {
                0,
                1,
                0,
                3,
                1,
                2,
                3,
                2,

                4,
                5,
                4,
                7,
                5,
                6,
                7,
                6,

                0,
                4,
                1,
                5,
                2,
                6,
                3,
                7
            };

            return CreateFromVertices(corners, indexes.ToArray());
        }
        public static Line3D[] CreateWiredSphere(BoundingSphere[] bsphList, int sliceCount, int stackCount)
        {
            List<Line3D> lines = new List<Line3D>();

            for (int i = 0; i < bsphList.Length; i++)
            {
                lines.AddRange(CreateWiredSphere(bsphList[i], sliceCount, stackCount));
            }

            return lines.ToArray();
        }
        public static Line3D[] CreateWiredSphere(BoundingSphere bsph, int sliceCount, int stackCount)
        {
            return CreateWiredSphere(bsph.Center, bsph.Radius, sliceCount, stackCount);
        }
        public static Line3D[] CreateWiredSphere(Vector3 center, float radius, int sliceCount, int stackCount)
        {
            List<Vector3> vertList = new List<Vector3>();
            List<int> indexList = new List<int>();

            //North pole
            vertList.Add(new Vector3(0.0f, +radius, 0.0f) + center);

            float phiStep = MathUtil.Pi / (stackCount + 1);
            float thetaStep = 2.0f * MathUtil.Pi / sliceCount;

            //Compute vertices for each stack ring (do not count the poles as rings).
            for (int st = 1; st < (stackCount + 1); ++st)
            {
                float phi = st * phiStep;

                //Vertices of ring.
                for (int sl = 0; sl <= sliceCount; ++sl)
                {
                    float theta = sl * thetaStep;

                    //Spherical to cartesian
                    Vector3 position = new Vector3(
                        radius * (float)Math.Sin(phi) * (float)Math.Cos(theta),
                        radius * (float)Math.Cos(phi),
                        radius * (float)Math.Sin(phi) * (float)Math.Sin(theta));

                    indexList.Add(vertList.Count);
                    indexList.Add(sl == sliceCount ? vertList.Count - sliceCount : vertList.Count + 1);

                    vertList.Add(position + center);
                }
            }

            //South pole
            vertList.Add(new Vector3(0.0f, -radius, 0.0f) + center);

            return CreateFromVertices(vertList.ToArray(), indexList.ToArray());
        }
        public static Line3D[] CreateWiredConeAngle(float cupAngle, float height, int sliceCount)
        {
            float baseRadius = (float)Math.Tan(cupAngle) * height;

            return CreateWiredConeBaseRadius(baseRadius, height, sliceCount);
        }
        public static Line3D[] CreateWiredConeBaseRadius(float baseRadius, float height, int sliceCount)
        {
            List<Vector3> vertList = new List<Vector3>();
            List<int> indexList = new List<int>();

            vertList.Add(new Vector3(0.0f, 0.0f, 0.0f));
            vertList.Add(new Vector3(0.0f, -height, 0.0f));

            float thetaStep = MathUtil.TwoPi / (float)sliceCount;

            for (int sl = 0; sl < sliceCount; sl++)
            {
                float theta = sl * thetaStep;

                Vector3 position = new Vector3(
                    baseRadius * (float)Math.Sin(MathUtil.PiOverTwo) * (float)Math.Cos(theta),
                    -height,
                    baseRadius * (float)Math.Sin(MathUtil.PiOverTwo) * (float)Math.Sin(theta));

                vertList.Add(position);
            }

            for (int index = 0; index < sliceCount; index++)
            {
                indexList.Add(0);
                indexList.Add(index + 2);

                indexList.Add(1);
                indexList.Add(index + 2);

                indexList.Add(index + 2);
                indexList.Add(index == sliceCount - 1 ? 2 : index + 3);
            }

            return CreateFromVertices(vertList.ToArray(), indexList.ToArray());
        }
        public static Line3D[] CreateWiredCylinder(BoundingCylinder cylinder, int segments)
        {
            List<Line3D> resultList = new List<Line3D>();

            List<Vector3> verts = new List<Vector3>();

            //verts
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < segments; j++)
                {
                    float theta = ((float)j / (float)segments) * 2 * (float)Math.PI;
                    float st = (float)Math.Sin(theta), ct = (float)Math.Cos(theta);

                    verts.Add(cylinder.Position + new Vector3(cylinder.Radius * st, cylinder.Height * i, cylinder.Radius * ct));
                }
            }

            for (int i = 0; i < segments; i++)
            {
                if (i == segments - 1)
                {
                    resultList.Add(new Line3D(verts[i], verts[0]));
                    resultList.Add(new Line3D(verts[i + segments], verts[0 + segments]));

                    resultList.Add(new Line3D(verts[i], verts[i + segments]));
                }
                else
                {
                    resultList.Add(new Line3D(verts[i], verts[i + 1]));
                    resultList.Add(new Line3D(verts[i + segments], verts[i + 1 + segments]));

                    resultList.Add(new Line3D(verts[i], verts[i + segments]));
                }
            }

            return resultList.ToArray();
        }
        public static Line3D[] CreateWiredFrustum(BoundingFrustum frustum)
        {
            return CreateWiredBox(frustum.GetCorners());
        }
        public static Line3D[] CreateWiredPyramid(BoundingFrustum frustum)
        {
            FrustumCameraParams prms = frustum.GetCameraParams();
            Vector3[] corners = frustum.GetCorners();

            Vector3[] vertices = new Vector3[5];

            vertices[0] = prms.Position;
            vertices[1] = corners[4];
            vertices[2] = corners[5];
            vertices[3] = corners[6];
            vertices[4] = corners[7];

            List<int> indexes = new List<int>(16)
            {
                0,
                1,
                0,
                2,
                0,
                3,
                0,
                4,

                1,
                2,
                2,
                3,
                3,
                4,
                4,
                1
            };

            return CreateFromVertices(vertices, indexes.ToArray());
        }
        public static Line3D[] CreatePath(Vector3[] path)
        {
            List<Line3D> lines = new List<Line3D>();

            for (int i = 0; i < path.Length - 1; i++)
            {
                lines.Add(new Line3D(path[i], path[i + 1]));
            }

            return lines.ToArray();
        }
        public static Line3D[] CreateAxis(Matrix transform, float size)
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

            return lines.ToArray();
        }
        public static Line3D[] CreateCrossList(Vector3[] points, float size)
        {
            List<Line3D> lines = new List<Line3D>();

            for (int i = 0; i < points.Length; i++)
            {
                Vector3 p = points[i];

                float h = size * 0.5f;
                lines.Add(new Line3D(p + new Vector3(h, h, h), p - new Vector3(h, h, h)));
                lines.Add(new Line3D(p + new Vector3(h, h, -h), p - new Vector3(h, h, -h)));
                lines.Add(new Line3D(p + new Vector3(-h, h, h), p - new Vector3(-h, h, h)));
                lines.Add(new Line3D(p + new Vector3(-h, h, -h), p - new Vector3(-h, h, -h)));
            }

            return lines.ToArray();
        }
        public static Line3D[] CreateLineList(Vector3[] points)
        {
            List<Line3D> lines = new List<Line3D>();

            Vector3 p0 = points[0];

            for (int i = 1; i < points.Length; i++)
            {
                Vector3 p1 = points[i];

                lines.Add(new Line3D(p0, p1));

                p0 = p1;
            }

            return lines.ToArray();
        }
        public static Line3D[] CreateArc(Vector3 from, Vector3 to, float h, int points)
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

            return lines.ToArray();
        }
        public static Line3D[] CreateCircle(Vector3 center, float r, int segments)
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

            return lines.ToArray();
        }
        public static Line3D[] CreateArrow(Vector3 p, Vector3 q, float s)
        {
            List<Line3D> lines = new List<Line3D>();

            float eps = 0.001f;
            if (Vector3.DistanceSquared(p, q) >= eps * eps)
            {
                var az = Vector3.Normalize(q - p);
                var ax = Vector3.Cross(Vector3.Up, az);

                lines.Add(new Line3D(p, new Vector3(p.X + az.X * s + ax.X * s / 3, p.Y + az.Y * s + ax.Y * s / 3, p.Z + az.Z * s + ax.Z * s / 3)));
                lines.Add(new Line3D(p, new Vector3(p.X + az.X * s - ax.X * s / 3, p.Y + az.Y * s - ax.Y * s / 3, p.Z + az.Z * s - ax.Z * s / 3)));
            }

            return lines.ToArray();
        }

        private static Line3D[] CreateFromVertices(Vector3[] vertices, int[] indices)
        {
            List<Line3D> lines = new List<Line3D>();

            for (int i = 0; i < indices.Length; i += 2)
            {
                Line3D l = new Line3D()
                {
                    Point1 = vertices[indices[i + 0]],
                    Point2 = vertices[indices[i + 1]],
                };

                lines.Add(l);
            }

            return lines.ToArray();
        }
        /// <summary>
        /// Eval arc
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
            this.Point1 = new Vector3(x1, y1, z1);
            this.Point2 = new Vector3(x2, y2, z2);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p1">Start point</param>
        /// <param name="p2">End point</param>
        public Line3D(Vector3 p1, Vector3 p2)
        {
            this.Point1 = p1;
            this.Point2 = p2;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ray">Ray</param>
        public Line3D(Ray ray)
        {
            this.Point1 = ray.Position;
            this.Point2 = ray.Position + ray.Direction;
        }

        /// <summary>
        /// Text representation
        /// </summary>
        public override string ToString()
        {
            return string.Format("Vertex 1 {0}; Vertex 2 {1};", this.Point1, this.Point2);
        }
    }
}
