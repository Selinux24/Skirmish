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

        /// <summary>
        /// Creates a line list
        /// </summary>
        /// <param name="lines">Line list</param>
        /// <param name="vertices">Result vertices</param>
        public static void CreateLineList(Line3D[] lines, out Vector3[] vertices)
        {
            List<Vector3> data = new List<Vector3>();

            for (int i = 0; i < lines.Length; i++)
            {
                data.Add(lines[i].Point1);
                data.Add(lines[i].Point2);
            }

            vertices = data.ToArray();
        }
        /// <summary>
        /// Creates a line list
        /// </summary>
        /// <param name="lines">Line list</param>
        /// <param name="vertices">Result vertices</param>
        /// <param name="indices">Result indices</param>
        public static void CreateLineList(Line3D[] lines, out Vector3[] vertices, out uint[] indices)
        {
            List<Vector3> vData = new List<Vector3>();
            List<uint> iData = new List<uint>();

            for (int i = 0; i < lines.Length; i++)
            {
                var p1 = lines[i].Point1;
                var p2 = lines[i].Point2;

                var i1 = vData.IndexOf(p1);
                var i2 = vData.IndexOf(p2);

                if (i1 >= 0)
                {
                    iData.Add((uint)i1);
                }
                else
                {
                    vData.Add(p1);
                    iData.Add((uint)vData.Count - 1);
                }

                if (i2 >= 0)
                {
                    iData.Add((uint)i2);
                }
                else
                {
                    vData.Add(p2);
                    iData.Add((uint)vData.Count - 1);
                }
            }

            vertices = vData.ToArray();
            indices = iData.ToArray();
        }
        /// <summary>
        /// Creates a triangle list
        /// </summary>
        /// <param name="triangles">Triangle list</param>
        /// <param name="vertices">Result vertices</param>
        public static void CreateTriangleList(Triangle[] triangles, out Vector3[] vertices)
        {
            List<Vector3> vData = new List<Vector3>();

            for (int i = 0; i < triangles.Length; i++)
            {
                vData.Add(triangles[i].Point1);
                vData.Add(triangles[i].Point2);
                vData.Add(triangles[i].Point3);
            }

            vertices = vData.ToArray();
        }
        /// <summary>
        /// Creates a triangle list
        /// </summary>
        /// <param name="triangles">Triangle list</param>
        /// <param name="vertices">Result vertices</param>
        /// <param name="indices">Result indices</param>
        public static void CreateTriangleList(Triangle[] triangles, out Vector3[] vertices, out uint[] indices)
        {
            List<Vector3> vData = new List<Vector3>();
            List<uint> iData = new List<uint>();

            for (int i = 0; i < triangles.Length; i++)
            {
                var p1 = triangles[i].Point1;
                var p2 = triangles[i].Point2;
                var p3 = triangles[i].Point3;

                var i1 = vData.IndexOf(p1);
                var i2 = vData.IndexOf(p2);
                var i3 = vData.IndexOf(p3);

                if (i1 >= 0)
                {
                    iData.Add((uint)i1);
                }
                else
                {
                    vData.Add(p1);
                    iData.Add((uint)vData.Count - 1);
                }

                if (i2 >= 0)
                {
                    iData.Add((uint)i2);
                }
                else
                {
                    vData.Add(p2);
                    iData.Add((uint)vData.Count - 1);
                }

                if (i3 >= 0)
                {
                    iData.Add((uint)i3);
                }
                else
                {
                    vData.Add(p3);
                    iData.Add((uint)vData.Count - 1);
                }
            }

            vertices = vData.ToArray();
            indices = iData.ToArray();
        }
        /// <summary>
        /// Creates a sprite of VertexPositionTexture VertexData
        /// </summary>
        /// <param name="position">Sprite position</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="formWidth">Render form width</param>
        /// <param name="formHeight">Render form height</param>
        /// <param name="vertices">Result vertices</param>
        /// <param name="indices">Result indices</param>
        public static void CreateSprite(Vector2 position, float width, float height, float formWidth, float formHeight, out Vector3[] vertices, out uint[] indices)
        {
            Vector2[] uvs;
            CreateSprite(position, width, height, formWidth, formHeight, out vertices, out uvs, out indices);
        }
        /// <summary>
        /// Creates a sprite of VertexPositionTexture VertexData
        /// </summary>
        /// <param name="position">Sprite position</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="formWidth">Render form width</param>
        /// <param name="formHeight">Render form height</param>
        /// <param name="vertices">Result vertices</param>
        /// <param name="indices">Result indices</param>
        /// <param name="uvs">Result texture uvs</param>
        public static void CreateSprite(Vector2 position, float width, float height, float formWidth, float formHeight, out Vector3[] vertices, out Vector2[] uvs, out uint[] indices)
        {
            vertices = new Vector3[4];
            uvs = new Vector2[4];

            float left = (formWidth * 0.5f * -1f) + position.X;
            float right = left + width;
            float top = (formHeight * 0.5f) - position.Y;
            float bottom = top - height;

            vertices[0] = new Vector3(left, top, 0.0f);
            uvs[0] = Vector2.Zero;

            vertices[1] = new Vector3(right, bottom, 0.0f);
            uvs[1] = Vector2.One;

            vertices[2] = new Vector3(left, bottom, 0.0f);
            uvs[2] = Vector2.UnitY;

            vertices[3] = new Vector3(right, top, 0.0f);
            uvs[3] = Vector2.UnitX;

            indices = new uint[6];

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;

            indices[3] = 0;
            indices[4] = 3;
            indices[5] = 1;
        }
        /// <summary>
        /// Creates a box of VertexPosition VertexData
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="depth">Depth</param>
        /// <param name="v">Result vertices</param>
        /// <param name="i">Result indices</param>
        public static void CreateBox(float width, float height, float depth, out Vector3[] vertices, out uint[] indices)
        {
            vertices = new Vector3[24];
            indices = new uint[36];

            float w2 = 0.5f * width;
            float h2 = 0.5f * height;
            float d2 = 0.5f * depth;

            // Fill in the front face vertex data.
            vertices[0] = new Vector3(-w2, -h2, -d2);
            vertices[1] = new Vector3(-w2, +h2, -d2);
            vertices[2] = new Vector3(+w2, +h2, -d2);
            vertices[3] = new Vector3(+w2, -h2, -d2);

            // Fill in the back face vertex data.
            vertices[4] = new Vector3(-w2, -h2, +d2);
            vertices[5] = new Vector3(+w2, -h2, +d2);
            vertices[6] = new Vector3(+w2, +h2, +d2);
            vertices[7] = new Vector3(-w2, +h2, +d2);

            // Fill in the top face vertex data.
            vertices[8] = new Vector3(-w2, +h2, -d2);
            vertices[9] = new Vector3(-w2, +h2, +d2);
            vertices[10] = new Vector3(+w2, +h2, +d2);
            vertices[11] = new Vector3(+w2, +h2, -d2);

            // Fill in the bottom face vertex data.
            vertices[12] = new Vector3(-w2, -h2, -d2);
            vertices[13] = new Vector3(+w2, -h2, -d2);
            vertices[14] = new Vector3(+w2, -h2, +d2);
            vertices[15] = new Vector3(-w2, -h2, +d2);

            // Fill in the left face vertex data.
            vertices[16] = new Vector3(-w2, -h2, +d2);
            vertices[17] = new Vector3(-w2, +h2, +d2);
            vertices[18] = new Vector3(-w2, +h2, -d2);
            vertices[19] = new Vector3(-w2, -h2, -d2);

            // Fill in the right face vertex data.
            vertices[20] = new Vector3(+w2, -h2, -d2);
            vertices[21] = new Vector3(+w2, +h2, -d2);
            vertices[22] = new Vector3(+w2, +h2, +d2);
            vertices[23] = new Vector3(+w2, -h2, +d2);

            // Fill in the front face index data
            indices[0] = 0; indices[1] = 1; indices[2] = 2;
            indices[3] = 0; indices[4] = 2; indices[5] = 3;

            // Fill in the back face index data
            indices[6] = 4; indices[7] = 5; indices[8] = 6;
            indices[9] = 4; indices[10] = 6; indices[11] = 7;

            // Fill in the top face index data
            indices[12] = 8; indices[13] = 9; indices[14] = 10;
            indices[15] = 8; indices[16] = 10; indices[17] = 11;

            // Fill in the bottom face index data
            indices[18] = 12; indices[19] = 13; indices[20] = 14;
            indices[21] = 12; indices[22] = 14; indices[23] = 15;

            // Fill in the left face index data
            indices[24] = 16; indices[25] = 17; indices[26] = 18;
            indices[27] = 16; indices[28] = 18; indices[29] = 19;

            // Fill in the right face index data
            indices[30] = 20; indices[31] = 21; indices[32] = 22;
            indices[33] = 20; indices[34] = 22; indices[35] = 23;
        }
        /// <summary>
        /// Creates a cone of VertexPositionNormalTextureTangent VertexData
        /// </summary>
        /// <param name="radius">The base radius</param>
        /// <param name="sliceCount">The base slice count</param>
        /// <param name="height">Cone height</param>
        /// <param name="vertices">Result vertices</param>
        /// <param name="indices">Result indices</param>
        public static void CreateCone(float radius, uint sliceCount, float height, out Vector3[] vertices, out uint[] indices)
        {
            List<Vector3> vertList = new List<Vector3>();
            List<uint> indexList = new List<uint>();

            vertList.Add(new Vector3(0.0f, 0.0f, 0.0f));

            vertList.Add(new Vector3(0.0f, -height, 0.0f));

            float thetaStep = MathUtil.TwoPi / (float)sliceCount;

            for (int sl = 0; sl < sliceCount; sl++)
            {
                float theta = sl * thetaStep;

                Vector3 position;
                Vector3 normal;
                Vector3 tangent;
                Vector3 binormal;
                Vector2 texture;

                // spherical to cartesian
                position.X = radius * (float)Math.Sin(MathUtil.PiOverTwo) * (float)Math.Cos(theta);
                position.Y = -height;
                position.Z = radius * (float)Math.Sin(MathUtil.PiOverTwo) * (float)Math.Sin(theta);

                normal = position;
                normal.Normalize();

                // Partial derivative of P with respect to theta
                tangent.X = -radius * (float)Math.Sin(MathUtil.PiOverTwo) * (float)Math.Sin(theta);
                tangent.Y = 0.0f;
                tangent.Z = +radius * (float)Math.Sin(MathUtil.PiOverTwo) * (float)Math.Cos(theta);
                tangent.Normalize();

                binormal = tangent;

                texture.X = theta / MathUtil.TwoPi;
                texture.Y = 1f;

                vertList.Add(position);
            }

            for (uint index = 0; index < sliceCount; index++)
            {
                indexList.Add(0);
                indexList.Add(index + 2);
                indexList.Add(index == sliceCount - 1 ? 2 : index + 3);

                indexList.Add(1);
                indexList.Add(index == sliceCount - 1 ? 2 : index + 3);
                indexList.Add(index + 2);
            }

            vertices = vertList.ToArray();
            indices = indexList.ToArray();
        }
        /// <summary>
        /// Creates a sphere of VertexPositionNormalTextureTangent VertexData
        /// </summary>
        /// <param name="radius">Radius</param>
        /// <param name="sliceCount">Slice count</param>
        /// <param name="stackCount">Stack count</param>
        /// <param name="vertices">Result vertices</param>
        /// <param name="indices">Result indices</param>
        public static void CreateSphere(float radius, uint sliceCount, uint stackCount, out Vector3[] vertices, out uint[] indices)
        {
            Vector3[] normals;
            Vector3[] tangents;
            Vector3[] binormals;
            Vector2[] uvs;
            CreateSphere(radius, sliceCount, stackCount, out vertices, out normals, out uvs, out tangents, out binormals, out indices);
        }
        /// <summary>
        /// Creates a sphere of VertexPositionNormalTextureTangent VertexData
        /// </summary>
        /// <param name="radius">Radius</param>
        /// <param name="sliceCount">Slice count</param>
        /// <param name="stackCount">Stack count</param>
        /// <param name="vertices">Result vertices</param>
        /// <param name="normals">Result normals</param>
        /// <param name="tangents">Result tangents</param>
        /// <param name="binormals">Result binormals</param>
        /// <param name="uvs">Result texture uvs</param>
        /// <param name="indices">Result indices</param>
        public static void CreateSphere(float radius, uint sliceCount, uint stackCount, out Vector3[] vertices, out Vector3[] normals, out Vector2[] uvs, out Vector3[] tangents, out Vector3[] binormals, out uint[] indices)
        {
            List<Vector3> vertList = new List<Vector3>();
            List<Vector3> normList = new List<Vector3>();
            List<Vector3> tangList = new List<Vector3>();
            List<Vector3> binmList = new List<Vector3>();
            List<Vector2> uvList = new List<Vector2>();
            List<uint> ixList = new List<uint>();

            //
            // Compute the vertices stating at the top pole and moving down the stacks.
            //

            // Poles: note that there will be texture coordinate distortion as there is
            // not a unique point on the texture map to assign to the pole when mapping
            // a rectangular texture onto a sphere.

            vertList.Add(new Vector3(0.0f, +radius, 0.0f));
            normList.Add(new Vector3(0.0f, +1.0f, 0.0f));
            tangList.Add(new Vector3(1.0f, 0.0f, 0.0f));
            binmList.Add(new Vector3(1.0f, 0.0f, 0.0f));
            uvList.Add(new Vector2(0.0f, 0.0f));

            float phiStep = MathUtil.Pi / stackCount;
            float thetaStep = 2.0f * MathUtil.Pi / sliceCount;

            // Compute vertices for each stack ring (do not count the poles as rings).
            for (int st = 1; st <= stackCount - 1; ++st)
            {
                float phi = st * phiStep;

                // Vertices of ring.
                for (int sl = 0; sl <= sliceCount; ++sl)
                {
                    float theta = sl * thetaStep;

                    Vector3 position;
                    Vector3 normal;
                    Vector3 tangent;
                    Vector3 binormal;
                    Vector2 texture;

                    // spherical to cartesian
                    position.X = radius * (float)Math.Sin(phi) * (float)Math.Cos(theta);
                    position.Y = radius * (float)Math.Cos(phi);
                    position.Z = radius * (float)Math.Sin(phi) * (float)Math.Sin(theta);

                    normal = position;
                    normal.Normalize();

                    // Partial derivative of P with respect to theta
                    tangent.X = -radius * (float)Math.Sin(phi) * (float)Math.Sin(theta);
                    tangent.Y = 0.0f;
                    tangent.Z = +radius * (float)Math.Sin(phi) * (float)Math.Cos(theta);
                    //tangent.W = 0.0f;
                    tangent.Normalize();

                    binormal = tangent;

                    texture.X = theta / MathUtil.Pi * 2f;
                    texture.Y = phi / MathUtil.Pi;

                    vertList.Add(position);
                    normList.Add(normal);
                    tangList.Add(tangent);
                    binmList.Add(binormal);
                    uvList.Add(texture);
                }
            }

            vertList.Add(new Vector3(0.0f, -radius, 0.0f));
            normList.Add(new Vector3(0.0f, -1.0f, 0.0f));
            tangList.Add(new Vector3(1.0f, 0.0f, 0.0f));
            binmList.Add(new Vector3(1.0f, 0.0f, 0.0f));
            uvList.Add(new Vector2(0.0f, 1.0f));

            List<uint> indexList = new List<uint>();

            for (uint index = 1; index <= sliceCount; ++index)
            {
                indexList.Add(0);
                indexList.Add(index + 1);
                indexList.Add(index);
            }

            //
            // Compute indices for inner stacks (not connected to poles).
            //

            // Offset the indices to the index of the first vertex in the first ring.
            // This is just skipping the top pole vertex.
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

            //
            // Compute indices for bottom stack.  The bottom stack was written last to the vertex buffer
            // and connects the bottom pole to the bottom ring.
            //

            // South pole vertex was added last.
            uint southPoleIndex = (uint)vertList.Count - 1;

            // Offset the indices to the index of the first vertex in the last ring.
            baseIndex = southPoleIndex - ringVertexCount;

            for (uint index = 0; index < sliceCount; ++index)
            {
                indexList.Add(southPoleIndex);
                indexList.Add(baseIndex + index);
                indexList.Add(baseIndex + index + 1);
            }

            vertices = vertList.ToArray();
            normals = normList.ToArray();
            tangents = tangList.ToArray();
            binormals = binmList.ToArray();
            uvs = uvList.ToArray();
            indices = indexList.ToArray();
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
