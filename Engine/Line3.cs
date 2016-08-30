using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// 3D Line
    /// </summary>
    public struct Line3
    {
        /// <summary>
        /// Start point
        /// </summary>
        public Vector3 Point1;
        /// <summary>
        /// End point
        /// </summary>
        public Vector3 Point2;
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
        public static Line3 Transform(Line3 line, Matrix transform)
        {
            return new Line3(
                Vector3.TransformCoordinate(line.Point1, transform),
                Vector3.TransformCoordinate(line.Point2, transform));
        }
        /// <summary>
        /// Transform line list coordinates
        /// </summary>
        /// <param name="lines">Line list</param>
        /// <param name="transform">Transformation</param>
        /// <returns>Returns new line list</returns>
        public static Line3[] Transform(Line3[] lines, Matrix transform)
        {
            Line3[] trnLines = new Line3[lines.Length];

            for (int i = 0; i < lines.Length; i++)
            {
                trnLines[i] = Transform(lines[i], transform);
            }

            return trnLines;
        }

        public static Line3[] CreateWiredTriangle(Triangle[] triangleList)
        {
            List<Line3> lines = new List<Line3>();

            for (int i = 0; i < triangleList.Length; i++)
            {
                lines.AddRange(CreateWiredTriangle(triangleList[i]));
            }

            return lines.ToArray();
        }
        public static Line3[] CreateWiredTriangle(Triangle triangle)
        {
            return CreateWiredTriangle(triangle.GetCorners());
        }
        public static Line3[] CreateWiredTriangle(Vector3[] corners)
        {
            List<Line3> lines = new List<Line3>();

            int[] indexes = new int[6];

            indexes[0] = 0;
            indexes[1] = 1;

            indexes[2] = 1;
            indexes[3] = 2;

            indexes[4] = 2;
            indexes[5] = 0;

            return CreateFromVertices(corners, indexes);
        }
        public static Line3[] CreateWiredSquare(Vector3[] corners)
        {
            List<Line3> lines = new List<Line3>();

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
        public static Line3[] CreateWiredPolygon(Vector3[] points)
        {
            List<Line3> lines = new List<Line3>();

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
        public static Line3[] CreateWiredBox(BoundingBox[] bboxList)
        {
            List<Line3> lines = new List<Line3>();

            for (int i = 0; i < bboxList.Length; i++)
            {
                lines.AddRange(CreateWiredBox(bboxList[i]));
            }

            return lines.ToArray();
        }
        public static Line3[] CreateWiredBox(BoundingBox bbox)
        {
            return CreateWiredBox(bbox.GetCorners());
        }
        public static Line3[] CreateWiredBox(OrientedBoundingBox[] obboxList)
        {
            List<Line3> lines = new List<Line3>();

            for (int i = 0; i < obboxList.Length; i++)
            {
                lines.AddRange(CreateWiredBox(obboxList[i]));
            }

            return lines.ToArray();
        }
        public static Line3[] CreateWiredBox(OrientedBoundingBox obbox)
        {
            return CreateWiredBox(obbox.GetCorners());
        }
        public static Line3[] CreateWiredBox(Vector3[] corners)
        {
            int[] indexes = new int[24];

            int index = 0;

            indexes[index++] = 0; indexes[index++] = 1;
            indexes[index++] = 0; indexes[index++] = 3;
            indexes[index++] = 1; indexes[index++] = 2;
            indexes[index++] = 3; indexes[index++] = 2;

            indexes[index++] = 4; indexes[index++] = 5;
            indexes[index++] = 4; indexes[index++] = 7;
            indexes[index++] = 5; indexes[index++] = 6;
            indexes[index++] = 7; indexes[index++] = 6;

            indexes[index++] = 0; indexes[index++] = 4;
            indexes[index++] = 1; indexes[index++] = 5;
            indexes[index++] = 2; indexes[index++] = 6;
            indexes[index++] = 3; indexes[index++] = 7;

            return CreateFromVertices(corners, indexes);
        }
        public static Line3[] CreateWiredSphere(BoundingSphere[] bsphList, int sliceCount, int stackCount)
        {
            List<Line3> lines = new List<Line3>();

            for (int i = 0; i < bsphList.Length; i++)
            {
                lines.AddRange(CreateWiredSphere(bsphList[i], sliceCount, stackCount));
            }

            return lines.ToArray();
        }
        public static Line3[] CreateWiredSphere(BoundingSphere bsph, int sliceCount, int stackCount)
        {
            return CreateWiredSphere(bsph.Center, bsph.Radius, sliceCount, stackCount);
        }
        public static Line3[] CreateWiredSphere(Vector3 center, float radius, int sliceCount, int stackCount)
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
        public static Line3[] CreateWiredCone(Vector3 center, float radius, int sliceCount, float height)
        {
            List<Vector3> vertList = new List<Vector3>();
            List<int> indexList = new List<int>();

            vertList.Add(new Vector3(0.0f, height, 0.0f) + center);
            vertList.Add(new Vector3(0.0f, 0.0f, 0.0f) + center);

            float thetaStep = MathUtil.TwoPi / (float)sliceCount;

            for (int sl = 0; sl < sliceCount; sl++)
            {
                float theta = sl * thetaStep;

                Vector3 position = new Vector3(
                    radius * (float)Math.Sin(MathUtil.PiOverTwo) * (float)Math.Cos(theta),
                    0.0f,
                    radius * (float)Math.Sin(MathUtil.PiOverTwo) * (float)Math.Sin(theta));

                vertList.Add(position + center);
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
        public static Line3[] CreateWiredCylinder(BoundingCylinder cylinder, int segments)
        {
            List<Line3> resultList = new List<Line3>();

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
                    resultList.Add(new Line3(verts[i], verts[0]));
                    resultList.Add(new Line3(verts[i + segments], verts[0 + segments]));

                    resultList.Add(new Line3(verts[i], verts[i + segments]));
                }
                else
                {
                    resultList.Add(new Line3(verts[i], verts[i + 1]));
                    resultList.Add(new Line3(verts[i + segments], verts[i + 1 + segments]));

                    resultList.Add(new Line3(verts[i], verts[i + segments]));
                }
            }

            return resultList.ToArray();
        }
        public static Line3[] CreateWiredFrustum(BoundingFrustum frustum)
        {
            return CreateWiredBox(frustum.GetCorners());
        }
        public static Line3[] CreateWiredPyramid(BoundingFrustum frustum)
        {
            FrustumCameraParams prms = frustum.GetCameraParams();
            Vector3[] corners = frustum.GetCorners();

            Vector3[] vertices = new Vector3[5];

            vertices[0] = prms.Position;
            vertices[1] = corners[4];
            vertices[2] = corners[5];
            vertices[3] = corners[6];
            vertices[4] = corners[7];

            int[] indexes = new int[16];

            int index = 0;

            indexes[index++] = 0; indexes[index++] = 1;
            indexes[index++] = 0; indexes[index++] = 2;
            indexes[index++] = 0; indexes[index++] = 3;
            indexes[index++] = 0; indexes[index++] = 4;

            indexes[index++] = 1; indexes[index++] = 2;
            indexes[index++] = 2; indexes[index++] = 3;
            indexes[index++] = 3; indexes[index++] = 4;
            indexes[index++] = 4; indexes[index++] = 1;

            return CreateFromVertices(vertices, indexes);
        }
        public static Line3[] CreatePath(Vector3[] path)
        {
            List<Line3> lines = new List<Line3>();

            for (int i = 0; i < path.Length - 1; i++)
            {
                lines.Add(new Line3(path[i], path[i + 1]));
            }

            return lines.ToArray();
        }
        public static Line3[] CreateAxis(Matrix transform, float size)
        {
            List<Line3> lines = new List<Line3>();

            Vector3 p = transform.TranslationVector;

            Vector3 up = p + (transform.Up * size);
            Vector3 forward = p + (transform.Forward * size);
            Vector3 left = p + (transform.Left * size);
            Vector3 right = p + (transform.Right * size);

            Vector3 c1 = (forward * 0.8f) + (left * 0.2f);
            Vector3 c2 = (forward * 0.8f) + (right * 0.2f);

            lines.Add(new Line3(p, up));
            lines.Add(new Line3(p, forward));
            lines.Add(new Line3(p, left));

            lines.Add(new Line3(forward, c1));
            lines.Add(new Line3(forward, c2));

            return lines.ToArray();
        }
        public static Line3[] CreateCrossList(Vector3[] points, float size)
        {
            List<Line3> lines = new List<Line3>();

            for (int i = 0; i < points.Length; i++)
            {
                Vector3 p = points[i];

                float h = size * 0.5f;
                lines.Add(new Line3(p + new Vector3(h, h, h), p - new Vector3(h, h, h)));
                lines.Add(new Line3(p + new Vector3(h, h, -h), p - new Vector3(h, h, -h)));
                lines.Add(new Line3(p + new Vector3(-h, h, h), p - new Vector3(-h, h, h)));
                lines.Add(new Line3(p + new Vector3(-h, h, -h), p - new Vector3(-h, h, -h)));
            }

            return lines.ToArray();
        }
        public static Line3[] CreateLineList(Vector3[] points)
        {
            List<Line3> lines = new List<Line3>();

            Vector3 p0 = points[0];

            for (int i = 1; i < points.Length; i++)
            {
                Vector3 p1 = points[i];

                lines.Add(new Line3(p0, p1));

                p0 = p1;
            }

            return lines.ToArray();
        }

        private static Line3[] CreateFromVertices(Vector3[] vertices, int[] indices)
        {
            List<Line3> lines = new List<Line3>();

            for (int i = 0; i < indices.Length; i += 2)
            {
                Line3 l = new Line3()
                {
                    Point1 = vertices[indices[i + 0]],
                    Point2 = vertices[indices[i + 1]],
                };

                lines.Add(l);
            }

            return lines.ToArray();
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
        public Line3(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            this.Point1 = new Vector3(x1, y1, z1);
            this.Point2 = new Vector3(x2, y2, z2);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p1">Start point</param>
        /// <param name="p2">End point</param>
        public Line3(Vector3 p1, Vector3 p2)
        {
            this.Point1 = p1;
            this.Point2 = p2;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ray">Ray</param>
        public Line3(Ray ray)
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
