using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.Common
{
    public static class GeometryUtil
    {
        public static uint[] GenerateIndices(IndexBufferShapeEnum bufferShape, int trianglesPerNode)
        {
            int nodes = trianglesPerNode / 2;
            uint side = (uint)Math.Sqrt(nodes);
            uint sideLoss = side / 2;

            bool topSide =
                bufferShape == IndexBufferShapeEnum.CornerTopLeft ||
                bufferShape == IndexBufferShapeEnum.CornerTopRight ||
                bufferShape == IndexBufferShapeEnum.SideTop;

            bool bottomSide =
                bufferShape == IndexBufferShapeEnum.CornerBottomLeft ||
                bufferShape == IndexBufferShapeEnum.CornerBottomRight ||
                bufferShape == IndexBufferShapeEnum.SideBottom;

            bool leftSide =
                bufferShape == IndexBufferShapeEnum.CornerBottomLeft ||
                bufferShape == IndexBufferShapeEnum.CornerTopLeft ||
                bufferShape == IndexBufferShapeEnum.SideLeft;

            bool rightSide =
                bufferShape == IndexBufferShapeEnum.CornerBottomRight ||
                bufferShape == IndexBufferShapeEnum.CornerTopRight ||
                bufferShape == IndexBufferShapeEnum.SideRight;

            uint totalTriangles = (uint)trianglesPerNode;
            if (topSide) totalTriangles -= sideLoss;
            if (bottomSide) totalTriangles -= sideLoss;
            if (leftSide) totalTriangles -= sideLoss;
            if (rightSide) totalTriangles -= sideLoss;

            uint[] indices = new uint[totalTriangles * 3];

            int index = 0;

            for (uint y = 1; y < side; y += 2)
            {
                for (uint x = 1; x < side; x += 2)
                {
                    uint indexPRow = ((y - 1) * (side + 1)) + x;
                    uint indexCRow = ((y + 0) * (side + 1)) + x;
                    uint indexNRow = ((y + 1) * (side + 1)) + x;

                    //Top side
                    if (y == 1 && topSide)
                    {
                        //Top
                        indices[index++] = indexCRow;
                        indices[index++] = indexPRow - 1;
                        indices[index++] = indexPRow + 1;
                    }
                    else
                    {
                        //Top left
                        indices[index++] = indexCRow;
                        indices[index++] = indexPRow - 1;
                        indices[index++] = indexPRow;
                        //Top right
                        indices[index++] = indexCRow;
                        indices[index++] = indexPRow;
                        indices[index++] = indexPRow + 1;
                    }

                    //Bottom side
                    if (y == side - 1 && bottomSide)
                    {
                        //Bottom only
                        indices[index++] = indexCRow;
                        indices[index++] = indexNRow + 1;
                        indices[index++] = indexNRow - 1;
                    }
                    else
                    {
                        //Bottom left
                        indices[index++] = indexCRow;
                        indices[index++] = indexNRow;
                        indices[index++] = indexNRow - 1;
                        //Bottom right
                        indices[index++] = indexCRow;
                        indices[index++] = indexNRow + 1;
                        indices[index++] = indexNRow;
                    }

                    //Left side
                    if (x == 1 && leftSide)
                    {
                        //Left only
                        indices[index++] = indexCRow;
                        indices[index++] = indexNRow - 1;
                        indices[index++] = indexPRow - 1;
                    }
                    else
                    {
                        //Left top
                        indices[index++] = indexCRow;
                        indices[index++] = indexCRow - 1;
                        indices[index++] = indexPRow - 1;
                        //Left bottom
                        indices[index++] = indexCRow;
                        indices[index++] = indexNRow - 1;
                        indices[index++] = indexCRow - 1;
                    }

                    //Right side
                    if (x == side - 1 && rightSide)
                    {
                        //Right only
                        indices[index++] = indexCRow;
                        indices[index++] = indexPRow + 1;
                        indices[index++] = indexNRow + 1;
                    }
                    else
                    {
                        //Right top
                        indices[index++] = indexCRow;
                        indices[index++] = indexPRow + 1;
                        indices[index++] = indexCRow + 1;
                        //Right bottom
                        indices[index++] = indexCRow;
                        indices[index++] = indexCRow + 1;
                        indices[index++] = indexNRow + 1;
                    }
                }
            }

            return indices;
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

        public static bool Intersects(Vector2 p11, Vector2 p12, Vector2 p21, Vector2 p22)
        {
            if ((p11.X == p21.X) && (p11.Y == p21.Y)) return false;
            if ((p11.X == p22.X) && (p11.Y == p22.Y)) return false;
            if ((p12.X == p21.X) && (p12.Y == p21.Y)) return false;
            if ((p12.X == p22.X) && (p12.Y == p22.Y)) return false;

            Vector2 v1ort = new Vector2(p12.Y - p11.Y, p11.X - p12.X);
            Vector2 v2ort = new Vector2(p22.Y - p21.Y, p21.X - p22.X);

            Vector2 v;
            v = p21 - p11;
            float dot21 = v.X * v1ort.X + v.Y * v1ort.Y;
            v = p22 - p11;
            float dot22 = v.X * v1ort.X + v.Y * v1ort.Y;

            if (dot21 * dot22 > 0) return false;

            v = p11 - p21;
            float dot11 = v.X * v2ort.X + v.Y * v2ort.Y;
            v = p12 - p21;
            float dot12 = v.X * v2ort.X + v.Y * v2ort.Y;

            if (dot11 * dot12 > 0) return false;

            return true;
        }
        public static bool IsInside(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
        {
            if (IsConvex(p1, p, p2)) return false;
            if (IsConvex(p2, p, p3)) return false;
            if (IsConvex(p3, p, p1)) return false;
            return true;
        }
        public static bool InCone(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
        {
            if (IsConvex(p1, p2, p3))
            {
                if (!IsConvex(p1, p2, p)) return false;
                if (!IsConvex(p2, p3, p)) return false;
                return true;
            }
            else
            {
                if (IsConvex(p1, p2, p)) return true;
                if (IsConvex(p2, p3, p)) return true;
                return false;
            }
        }
        public static bool IsReflex(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return ((p3.Y - p1.Y) * (p2.X - p1.X) - (p3.X - p1.X) * (p2.Y - p1.Y)) < 0;
        }
        public static bool IsConvex(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return ((p3.Y - p1.Y) * (p2.X - p1.X) - (p3.X - p1.X) * (p2.Y - p1.Y)) > 0;
        }
    }

    public enum IndexBufferShapeEnum : int
    {
        None = -1,
        Full = 0,
        SideTop = 1,
        SideBottom = 2,
        SideLeft = 3,
        SideRight = 4,
        CornerTopLeft = 5,
        CornerBottomLeft = 6,
        CornerTopRight = 7,
        CornerBottomRight = 8,
    }
}
