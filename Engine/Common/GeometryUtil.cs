using System;
using System.Collections.Generic;
using SharpDX;

namespace Engine.Common
{
    public static class GeometryUtil
    {
        public static Line[] CreateWiredTriangle(Triangle[] triangleList)
        {
            List<Line> lines = new List<Line>();

            for (int i = 0; i < triangleList.Length; i++)
            {
                lines.AddRange(CreateWiredTriangle(triangleList[i]));
            }

            return lines.ToArray();
        }
        public static Line[] CreateWiredTriangle(Triangle triangle)
        {
            return CreateWiredTriangle(triangle.GetCorners());
        }
        public static Line[] CreateWiredTriangle(Vector3[] corners)
        {
            List<Line> lines = new List<Line>();

            int[] indexes = new int[6];

            indexes[0] = 0;
            indexes[1] = 1;

            indexes[2] = 1;
            indexes[3] = 2;

            indexes[4] = 2;
            indexes[5] = 0;

            return CreateFromVertices(corners, indexes);
        }
        public static Line[] CreateWiredBox(BoundingBox[] bboxList)
        {
            List<Line> lines = new List<Line>();

            for (int i = 0; i < bboxList.Length; i++)
            {
                lines.AddRange(CreateWiredBox(bboxList[i]));
            }

            return lines.ToArray();
        }
        public static Line[] CreateWiredBox(BoundingBox bbox)
        {
            return CreateWiredBox(bbox.GetCorners());
        }
        public static Line[] CreateWiredBox(OrientedBoundingBox[] obboxList)
        {
            List<Line> lines = new List<Line>();

            for (int i = 0; i < obboxList.Length; i++)
            {
                lines.AddRange(CreateWiredBox(obboxList[i]));
            }

            return lines.ToArray();
        }
        public static Line[] CreateWiredBox(OrientedBoundingBox obbox)
        {
            return CreateWiredBox(obbox.GetCorners());
        }
        public static Line[] CreateWiredBox(Vector3[] corners)
        {
            List<Line> lines = new List<Line>();

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
        public static Line[] CreateWiredSphere(BoundingSphere[] bsphList, int sliceCount, int stackCount)
        {
            List<Line> lines = new List<Line>();

            for (int i = 0; i < bsphList.Length; i++)
            {
                lines.AddRange(CreateWiredSphere(bsphList[i], sliceCount, stackCount));
            }

            return lines.ToArray();
        }
        public static Line[] CreateWiredSphere(BoundingSphere bsph, int sliceCount, int stackCount)
        {
            return CreateWiredSphere(bsph.Center, bsph.Radius, sliceCount, stackCount);
        }
        public static Line[] CreateWiredSphere(Vector3 center, float radius, int sliceCount, int stackCount)
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
        public static Line[] CreateWiredFrustum(BoundingFrustum frustum)
        {
            return CreateWiredBox(frustum.GetCorners());
        }
        public static Line[] CreatePath(Vector3[] path)
        {
            List<Line> lines = new List<Line>();

            for (int i = 0; i < path.Length - 1; i++)
            {
                lines.Add(new Line(path[i], path[i + 1]));
            }

            return lines.ToArray();
        }
        public static Line[] CreateAxis(Matrix transform, float size)
        {
            List<Line> lines = new List<Line>();

            Vector3 up = transform.TranslationVector + (transform.Up * size);
            Vector3 forward = transform.TranslationVector + (transform.Forward * size);
            Vector3 left = transform.TranslationVector + (transform.Left * size);

            lines.Add(new Line(transform.TranslationVector, up));
            lines.Add(new Line(transform.TranslationVector, forward));
            lines.Add(new Line(transform.TranslationVector, left));

            return lines.ToArray();
        }
        public static Line[] CreateCrossList(Vector3[] points, float size)
        {
            List<Line> lines = new List<Line>();

            for (int i = 0; i < points.Length; i++)
            {
                Vector3 p = points[i];

                float h = size * 0.5f;
                lines.Add(new Line(p + new Vector3(h, h, h), p - new Vector3(h, h, h)));
                lines.Add(new Line(p + new Vector3(h, h, -h), p - new Vector3(h, h, -h)));
                lines.Add(new Line(p + new Vector3(-h, h, h), p - new Vector3(-h, h, h)));
                lines.Add(new Line(p + new Vector3(-h, h, -h), p - new Vector3(-h, h, -h)));
            }

            return lines.ToArray();
        }

        private static Line[] CreateFromVertices(Vector3[] vertices, int[] indices)
        {
            List<Line> lines = new List<Line>();

            for (int i = 0; i < indices.Length; i += 2)
            {
                Line l = new Line()
                {
                    Point1 = vertices[indices[i + 0]],
                    Point2 = vertices[indices[i + 1]],
                };

                lines.Add(l);
            }

            return lines.ToArray();
        }
    }
}
