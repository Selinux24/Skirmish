using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.Common
{
    /// <summary>
    /// Geometry utilities
    /// </summary>
    public static class GeometryUtil
    {
        /// <summary>
        /// Generates a index for a triangle soup quad with the specified shape
        /// </summary>
        /// <param name="bufferShape">Buffer shape</param>
        /// <param name="triangles">Triangle count</param>
        /// <returns>Returns the generated index list</returns>
        public static uint[] GenerateIndices(IndexBufferShapes bufferShape, int triangles)
        {
            return GenerateIndices(LevelOfDetail.High, bufferShape, triangles);
        }
        /// <summary>
        /// Generates a index for a triangle soup quad with the specified shape
        /// </summary>
        /// <param name="lod">Level of detail</param>
        /// <param name="bufferShape">Buffer shape</param>
        /// <param name="triangles">Triangle count</param>
        /// <returns>Returns the generated index list</returns>
        public static uint[] GenerateIndices(LevelOfDetail lod, IndexBufferShapes bufferShape, int triangles)
        {
            uint offset = (uint)lod;
            uint fullSide = (uint)Math.Sqrt(triangles / 2f);

            int tris = triangles / (int)Math.Pow(offset, 2);

            int nodes = tris / 2;
            uint side = (uint)Math.Sqrt(nodes);
            uint sideLoss = side / 2;

            bool topSide =
                bufferShape == IndexBufferShapes.CornerTopLeft ||
                bufferShape == IndexBufferShapes.CornerTopRight ||
                bufferShape == IndexBufferShapes.SideTop;

            bool bottomSide =
                bufferShape == IndexBufferShapes.CornerBottomLeft ||
                bufferShape == IndexBufferShapes.CornerBottomRight ||
                bufferShape == IndexBufferShapes.SideBottom;

            bool leftSide =
                bufferShape == IndexBufferShapes.CornerBottomLeft ||
                bufferShape == IndexBufferShapes.CornerTopLeft ||
                bufferShape == IndexBufferShapes.SideLeft;

            bool rightSide =
                bufferShape == IndexBufferShapes.CornerBottomRight ||
                bufferShape == IndexBufferShapes.CornerTopRight ||
                bufferShape == IndexBufferShapes.SideRight;

            uint totalTriangles = (uint)tris;
            if (topSide) totalTriangles -= sideLoss;
            if (bottomSide) totalTriangles -= sideLoss;
            if (leftSide) totalTriangles -= sideLoss;
            if (rightSide) totalTriangles -= sideLoss;

            List<uint> indices = new List<uint>((int)totalTriangles * 3);

            for (uint y = 1; y < side; y += 2)
            {
                for (uint x = 1; x < side; x += 2)
                {
                    uint indexPRow = (((y - 1) * offset) * (fullSide + 1)) + (x * offset);
                    uint indexCRow = (((y + 0) * offset) * (fullSide + 1)) + (x * offset);
                    uint indexNRow = (((y + 1) * offset) * (fullSide + 1)) + (x * offset);

                    //Top side
                    if (y == 1 && topSide)
                    {
                        //Top
                        indices.Add(indexCRow);
                        indices.Add(indexPRow - (1 * offset));
                        indices.Add(indexPRow + (1 * offset));
                    }
                    else
                    {
                        //Top left
                        indices.Add(indexCRow);
                        indices.Add(indexPRow - (1 * offset));
                        indices.Add(indexPRow);
                        //Top right
                        indices.Add(indexCRow);
                        indices.Add(indexPRow);
                        indices.Add(indexPRow + (1 * offset));
                    }

                    //Bottom side
                    if (y == side - 1 && bottomSide)
                    {
                        //Bottom only
                        indices.Add(indexCRow);
                        indices.Add(indexNRow + (1 * offset));
                        indices.Add(indexNRow - (1 * offset));
                    }
                    else
                    {
                        //Bottom left
                        indices.Add(indexCRow);
                        indices.Add(indexNRow);
                        indices.Add(indexNRow - (1 * offset));
                        //Bottom right
                        indices.Add(indexCRow);
                        indices.Add(indexNRow + (1 * offset));
                        indices.Add(indexNRow);
                    }

                    //Left side
                    if (x == 1 && leftSide)
                    {
                        //Left only
                        indices.Add(indexCRow);
                        indices.Add(indexNRow - (1 * offset));
                        indices.Add(indexPRow - (1 * offset));
                    }
                    else
                    {
                        //Left top
                        indices.Add(indexCRow);
                        indices.Add(indexCRow - (1 * offset));
                        indices.Add(indexPRow - (1 * offset));
                        //Left bottom
                        indices.Add(indexCRow);
                        indices.Add(indexNRow - (1 * offset));
                        indices.Add(indexCRow - (1 * offset));
                    }

                    //Right side
                    if (x == side - 1 && rightSide)
                    {
                        //Right only
                        indices.Add(indexCRow);
                        indices.Add(indexPRow + (1 * offset));
                        indices.Add(indexNRow + (1 * offset));
                    }
                    else
                    {
                        //Right top
                        indices.Add(indexCRow);
                        indices.Add(indexPRow + (1 * offset));
                        indices.Add(indexCRow + (1 * offset));
                        //Right bottom
                        indices.Add(indexCRow);
                        indices.Add(indexCRow + (1 * offset));
                        indices.Add(indexNRow + (1 * offset));
                    }
                }
            }

            return indices.ToArray();
        }
        /// <summary>
        /// Toggle coordinates from left-handed to right-handed and vice versa
        /// </summary>
        /// <typeparam name="T">Index type</typeparam>
        /// <param name="indices">Indices in a triangle list topology</param>
        /// <returns>Returns a new array</returns>
        public static T[] ChangeCoordinate<T>(T[] indices)
        {
            T[] res = new T[indices.Length];

            for (int i = 0; i < indices.Length; i += 3)
            {
                res[i + 0] = indices[i + 0];
                res[i + 1] = indices[i + 2];
                res[i + 2] = indices[i + 1];
            }

            return res;
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
            CreateSprite(position, width, height, formWidth, formHeight, 0, 0, 0, out vertices, out Vector2[] uvs, out indices);
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
            CreateSprite(position, width, height, formWidth, formHeight, 0, 0, 0, out vertices, out uvs, out indices);
        }
        /// <summary>
        /// Creates a sprite of VertexPositionTexture VertexData
        /// </summary>
        /// <param name="position">Sprite position</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="formWidth">Render form width</param>
        /// <param name="formHeight">Render form height</param>
        /// <param name="texU">Texture U</param>
        /// <param name="texV">Texture V</param>
        /// <param name="texSize">Texture total size</param>
        /// <param name="vertices">Result vertices</param>
        /// <param name="indices">Result indices</param>
        /// <param name="uvs">Result texture uvs</param>
        public static void CreateSprite(Vector2 position, float width, float height, float formWidth, float formHeight, float texU, float texV, float texSize, out Vector3[] vertices, out Vector2[] uvs, out uint[] indices)
        {
            vertices = new Vector3[4];
            uvs = new Vector2[4];

            float left = (formWidth * 0.5f * -1f) + position.X;
            float right = left + width;
            float top = (formHeight * 0.5f) - position.Y;
            float bottom = top - height;

            //Texture map
            float u0 = texSize > 0 ? (texU) / texSize : 0;
            float v0 = texSize > 0 ? (texV) / texSize : 0;
            float u1 = texSize > 0 ? (texU + width) / texSize : 1;
            float v1 = texSize > 0 ? (texV + height) / texSize : 1;

            vertices[0] = new Vector3(left, top, 0.0f);
            uvs[0] = new Vector2(u0, v0);

            vertices[1] = new Vector3(right, bottom, 0.0f);
            uvs[1] = new Vector2(u1, v1);

            vertices[2] = new Vector3(left, bottom, 0.0f);
            uvs[2] = new Vector2(u0, v1);

            vertices[3] = new Vector3(right, top, 0.0f);
            uvs[3] = new Vector2(u1, v0);

            indices = new uint[6];

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;

            indices[3] = 0;
            indices[4] = 3;
            indices[5] = 1;
        }
        /// <summary>
        /// Creates a sprite of VertexPositionTexture VertexData
        /// </summary>
        /// <param name="position">Sprite position</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="formWidth">Render form width</param>
        /// <param name="formHeight">Render form height</param>
        /// <param name="uvMap">UV map</param>
        /// <param name="vertices">Result vertices</param>
        /// <param name="indices">Result indices</param>
        /// <param name="uvs">Result texture uvs</param>
        public static void CreateSprite(Vector2 position, float width, float height, float formWidth, float formHeight, Vector4 uvMap, out Vector3[] vertices, out Vector2[] uvs, out uint[] indices)
        {
            vertices = new Vector3[4];
            uvs = new Vector2[4];

            float left = (formWidth * 0.5f * -1f) + position.X;
            float right = left + width;
            float top = (formHeight * 0.5f) - position.Y;
            float bottom = top - height;

            //Texture map
            float u0 = uvMap.X;
            float v0 = uvMap.Y;
            float u1 = uvMap.Z;
            float v1 = uvMap.W;

            vertices[0] = new Vector3(left, top, 0.0f);
            uvs[0] = new Vector2(u0, v0);

            vertices[1] = new Vector3(right, bottom, 0.0f);
            uvs[1] = new Vector2(u1, v1);

            vertices[2] = new Vector3(left, bottom, 0.0f);
            uvs[2] = new Vector2(u0, v1);

            vertices[3] = new Vector3(right, top, 0.0f);
            uvs[3] = new Vector2(u1, v0);

            indices = new uint[6];

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;

            indices[3] = 0;
            indices[4] = 3;
            indices[5] = 1;
        }
        /// <summary>
        /// Creates a screen of VertexPositionTexture VertexData
        /// </summary>
        /// <param name="form">Form</param>
        /// <param name="vertices">Resulting positions</param>
        /// <param name="indices">Resulting indices</param>
        public static void CreateScreen(EngineForm form, out Vector3[] vertices, out uint[] indices)
        {
            CreateScreen(form, out vertices, out Vector2[] uvs, out indices);
        }
        /// <summary>
        /// Creates a screen of VertexPositionTexture VertexData
        /// </summary>
        /// <param name="renderWidth">Render area width</param>
        /// <param name="renderHeight">Render area height</param>
        /// <param name="vertices">Resulting positions</param>
        /// <param name="indices">Resulting indices</param>
        public static void CreateScreen(int renderWidth, int renderHeight, out Vector3[] vertices, out uint[] indices)
        {
            CreateScreen(renderWidth, renderHeight, out vertices, out Vector2[] uvs, out indices);
        }
        /// <summary>
        /// Creates a screen of VertexPositionTexture VertexData
        /// </summary>
        /// <param name="form">Form</param>
        /// <param name="vertices">Resulting positions</param>
        /// <param name="uvs">Resulting uv coordinates</param>
        /// <param name="indices">Resulting indices</param>
        public static void CreateScreen(EngineForm form, out Vector3[] vertices, out Vector2[] uvs, out uint[] indices)
        {
            CreateScreen(form.RenderWidth, form.RenderHeight, out vertices, out uvs, out indices);
        }
        /// <summary>
        /// Creates a screen of VertexPositionTexture VertexData
        /// </summary>
        /// <param name="renderWidth">Render area width</param>
        /// <param name="renderHeight">Render area height</param>
        /// <param name="vertices">Resulting positions</param>
        /// <param name="uvs">Resulting uv coordinates</param>
        /// <param name="indices">Resulting indices</param>
        public static void CreateScreen(int renderWidth, int renderHeight, out Vector3[] vertices, out Vector2[] uvs, out uint[] indices)
        {
            vertices = new Vector3[4];
            uvs = new Vector2[4];
            indices = new uint[6];

            float width = renderWidth;
            float height = renderHeight;

            float left = ((width / 2) * -1);
            float right = left + width;
            float top = (height / 2);
            float bottom = top - height;

            vertices[0] = new Vector3(left, top, 0.0f);
            uvs[0] = new Vector2(0.0f, 0.0f);

            vertices[1] = new Vector3(right, bottom, 0.0f);
            uvs[1] = new Vector2(1.0f, 1.0f);

            vertices[2] = new Vector3(left, bottom, 0.0f);
            uvs[2] = new Vector2(0.0f, 1.0f);

            vertices[3] = new Vector3(right, top, 0.0f);
            uvs[3] = new Vector2(1.0f, 0.0f);

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

                texture.X = theta / MathUtil.TwoPi;
                texture.Y = 1f;

                vertList.Add(position);
            }

            for (uint index = 0; index < sliceCount; index++)
            {
                indexList.Add(0);
                indexList.Add(index == sliceCount - 1 ? 2 : index + 3);
                indexList.Add(index + 2);

                indexList.Add(1);
                indexList.Add(index + 2);
                indexList.Add(index == sliceCount - 1 ? 2 : index + 3);
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
            CreateSphere(radius, sliceCount, stackCount, out vertices, out Vector3[] normals, out Vector2[] uvs, out Vector3[] tangents, out Vector3[] binormals, out indices);
        }
        /// <summary>
        /// Creates a sphere of VertexPositionNormalTextureTangent VertexData
        /// </summary>
        /// <param name="radius">Radius</param>
        /// <param name="sliceCount">Slice count</param>
        /// <param name="stackCount">Stack count</param>
        /// <param name="vertices">Result vertices</param>
        /// <param name="normals">Result normals</param>
        /// <param name="uvs">Result texture uvs</param>
        /// <param name="indices">Result indices</param>
        public static void CreateSphere(float radius, uint sliceCount, uint stackCount, out Vector3[] vertices, out Vector3[] normals, out Vector2[] uvs, out uint[] indices)
        {
            CreateSphere(radius, sliceCount, stackCount, out vertices, out normals, out uvs, out Vector3[] tangents, out Vector3[] binormals, out indices);
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

            sliceCount--;
            stackCount++;

            #region Positions

            //North pole
            vertList.Add(new Vector3(0.0f, radius, 0.0f));
            normList.Add(new Vector3(0.0f, 1.0f, 0.0f));
            tangList.Add(new Vector3(0.0f, 0.0f, 1.0f));
            binmList.Add(new Vector3(1.0f, 0.0f, 0.0f));
            uvList.Add(new Vector2(0.0f, 0.0f));

            float phiStep = MathUtil.Pi / stackCount;
            float thetaStep = 2.0f * MathUtil.Pi / sliceCount;

            for (int st = 1; st <= stackCount - 1; ++st)
            {
                float phi = st * phiStep;

                for (int sl = 0; sl <= sliceCount; ++sl)
                {
                    float theta = sl * thetaStep;

                    float x = (float)Math.Sin(phi) * (float)Math.Cos(theta);
                    float y = (float)Math.Cos(phi);
                    float z = (float)Math.Sin(phi) * (float)Math.Sin(theta);

                    float tX = -(float)Math.Sin(phi) * (float)Math.Sin(theta);
                    float tY = 0.0f;
                    float tZ = +(float)Math.Sin(phi) * (float)Math.Cos(theta);

                    Vector3 position = radius * new Vector3(x, y, z);
                    Vector3 normal = new Vector3(x, y, z);
                    Vector3 tangent = Vector3.Normalize(new Vector3(tX, tY, tZ));
                    Vector3 binormal = Vector3.Cross(normal, tangent);

                    float u = theta / MathUtil.Pi * 2f;
                    float v = phi / MathUtil.Pi;

                    Vector2 texture = new Vector2(u, v);

                    vertList.Add(position);
                    normList.Add(normal);
                    tangList.Add(tangent);
                    binmList.Add(binormal);
                    uvList.Add(texture);
                }
            }

            //South pole
            vertList.Add(new Vector3(0.0f, -radius, 0.0f));
            normList.Add(new Vector3(0.0f, -1.0f, 0.0f));
            tangList.Add(new Vector3(0.0f, 0.0f, -1.0f));
            binmList.Add(new Vector3(-1.0f, 0.0f, 0.0f));
            uvList.Add(new Vector2(0.0f, 1.0f));

            #endregion

            List<uint> indexList = new List<uint>();

            #region Indexes

            for (uint index = 1; index <= sliceCount; ++index)
            {
                indexList.Add(0);
                indexList.Add(index + 1);
                indexList.Add(index);
            }

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

            uint southPoleIndex = (uint)vertList.Count - 1;

            baseIndex = southPoleIndex - ringVertexCount;

            for (uint index = 0; index < sliceCount; ++index)
            {
                indexList.Add(southPoleIndex);
                indexList.Add(baseIndex + index);
                indexList.Add(baseIndex + index + 1);
            }

            #endregion

            vertices = vertList.ToArray();
            normals = normList.ToArray();
            tangents = tangList.ToArray();
            binormals = binmList.ToArray();
            uvs = uvList.ToArray();
            indices = indexList.ToArray();
        }
        /// <summary>
        /// Creates a XZ plane of position normal texture data
        /// </summary>
        /// <param name="size">Plane size</param>
        /// <param name="height">Plane height</param>
        /// <param name="vertices">Gets the plane vertices</param>
        /// <param name="normals">Gets the plane normals</param>
        /// <param name="uvs">Gets the plane uvs</param>
        /// <param name="indices">Gets the plane indices</param>
        public static void CreateXZPlane(float size, float height, out Vector3[] vertices, out Vector3[] normals, out Vector2[] uvs, out uint[] indices)
        {
            vertices = new Vector3[]
            {
                new Vector3(-size*0.5f, +height, -size*0.5f),
                new Vector3(-size*0.5f, +height, +size*0.5f),
                new Vector3(+size*0.5f, +height, -size*0.5f),
                new Vector3(+size*0.5f, +height, +size*0.5f),
            };

            normals = new Vector3[]
            {
                Vector3.Up,
                Vector3.Up,
                Vector3.Up,
                Vector3.Up,
            };

            uvs = new Vector2[]
            {
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, size),
                new Vector2(size, 0.0f),
                new Vector2(size, size),
            };

            indices = new uint[]
            {
                0, 1, 2,
                1, 3, 2,
            };
        }
        /// <summary>
        /// Creates a curve plane
        /// </summary>
        /// <param name="size">Quad size</param>
        /// <param name="textureRepeat">Texture repeat</param>
        /// <param name="planeWidth">Plane width</param>
        /// <param name="planeTop">Plane top</param>
        /// <param name="planeBottom">Plane bottom</param>
        /// <param name="vertices">Result vertices</param>
        /// <param name="uvs">Result texture uvs</param>
        /// <param name="indices">Result indices</param>
        public static void CreateCurvePlane(int size, int textureRepeat, float planeWidth, float planeTop, float planeBottom, out Vector3[] vertices, out Vector2[] uvs, out uint[] indices)
        {
            vertices = new Vector3[(size + 1) * (size + 1)];
            uvs = new Vector2[(size + 1) * (size + 1)];

            // Determine the size of each quad on the sky plane.
            float quadSize = planeWidth / (float)size;

            // Calculate the radius of the sky plane based on the width.
            float radius = planeWidth * 0.5f;

            // Calculate the height constant to increment by.
            float constant = (planeTop - planeBottom) / (radius * radius);

            // Calculate the texture coordinate increment value.
            float textureDelta = (float)textureRepeat / (float)size;

            // Loop through the sky plane and build the coordinates based on the increment values given.
            for (int j = 0; j <= size; j++)
            {
                for (int i = 0; i <= size; i++)
                {
                    // Calculate the vertex coordinates.
                    float positionX = (-0.5f * planeWidth) + ((float)i * quadSize);
                    float positionZ = (-0.5f * planeWidth) + ((float)j * quadSize);
                    float positionY = planeTop - (constant * ((positionX * positionX) + (positionZ * positionZ)));

                    // Calculate the texture coordinates.
                    float tu = (float)i * textureDelta;
                    float tv = (float)j * textureDelta;

                    // Calculate the index into the sky plane array to add this coordinate.
                    int ix = j * (size + 1) + i;

                    // Add the coordinates to the sky plane array.
                    vertices[ix] = new Vector3(positionX, positionY, positionZ);
                    uvs[ix] = new Vector2(tu, tv);
                }
            }


            // Create the index array.
            List<uint> indexList = new List<uint>((size + 1) * (size + 1) * 6);

            // Load the vertex and index array with the sky plane array data.
            for (int j = 0; j < size; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    int index1 = j * (size + 1) + i;
                    int index2 = j * (size + 1) + (i + 1);
                    int index3 = (j + 1) * (size + 1) + i;
                    int index4 = (j + 1) * (size + 1) + (i + 1);

                    // Triangle 1 - Upper Left
                    indexList.Add((uint)index1);

                    // Triangle 1 - Upper Right
                    indexList.Add((uint)index2);

                    // Triangle 1 - Bottom Left
                    indexList.Add((uint)index3);

                    // Triangle 2 - Bottom Left
                    indexList.Add((uint)index3);

                    // Triangle 2 - Upper Right
                    indexList.Add((uint)index2);

                    // Triangle 2 - Bottom Right
                    indexList.Add((uint)index4);
                }
            }

            indices = indexList.ToArray();
        }

        /// <summary>
        /// Compute normal of three points in the same plane
        /// </summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">point 2</param>
        /// <param name="p3">point 3</param>
        /// <param name="normal">Resulting normal</param>
        public static void ComputeNormal(Vector3 p1, Vector3 p2, Vector3 p3, out Vector3 normal)
        {
            var p = new Plane(p1, p2, p3);

            normal = p.Normal;
        }
        /// <summary>
        /// Calculate tangent, normal and binormals of triangle vertices
        /// </summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <param name="p3">Point 3</param>
        /// <param name="uv1">Texture uv 1</param>
        /// <param name="uv2">Texture uv 2</param>
        /// <param name="uv3">Texture uv 3</param>
        /// <param name="tangent">Tangen result</param>
        /// <param name="binormal">Binormal result</param>
        /// <param name="normal">Normal result</param>
        public static void ComputeNormals(Vector3 p1, Vector3 p2, Vector3 p3, Vector2 uv1, Vector2 uv2, Vector2 uv3, out Vector3 tangent, out Vector3 binormal, out Vector3 normal)
        {
            // Calculate the two vectors for the face.
            Vector3 vector1 = p2 - p1;
            Vector3 vector2 = p3 - p1;

            // Calculate the tu and tv texture space vectors.
            Vector2 tuVector = new Vector2(uv2.X - uv1.X, uv3.X - uv1.X);
            Vector2 tvVector = new Vector2(uv2.Y - uv1.Y, uv3.Y - uv1.Y);

            // Calculate the denominator of the tangent / binormal equation.
            var den = 1.0f / (tuVector[0] * tvVector[1] - tuVector[1] * tvVector[0]);

            // Calculate the cross products and multiply by the coefficient to get the tangent and binormal.
            tangent.X = (tvVector[1] * vector1.X - tvVector[0] * vector2.X) * den;
            tangent.Y = (tvVector[1] * vector1.Y - tvVector[0] * vector2.Y) * den;
            tangent.Z = (tvVector[1] * vector1.Z - tvVector[0] * vector2.Z) * den;

            binormal.X = (tuVector[0] * vector2.X - tuVector[1] * vector1.X) * den;
            binormal.Y = (tuVector[0] * vector2.Y - tuVector[1] * vector1.Y) * den;
            binormal.Z = (tuVector[0] * vector2.Z - tuVector[1] * vector1.Z) * den;

            tangent.Normalize();
            binormal.Normalize();

            // Calculate the cross product of the tangent and binormal which will give the normal vector.
            normal = Vector3.Cross(tangent, binormal);
        }

        /// <summary>
        /// Generates a bounding box from a vertex list item list
        /// </summary>
        /// <param name="vertexListItems">Vertex list item list</param>
        /// <returns>Returns the minimum bounding box that contains all the specified vertex list item list</returns>
        public static BoundingBox CreateBoundingBox<T>(IEnumerable<T> vertexListItems) where T : IVertexList
        {
            var bbox = new BoundingBox();

            int index = 0;
            foreach (var item in vertexListItems)
            {
                var tbox = BoundingBox.FromPoints(item.GetVertices());

                if (index == 0)
                {
                    bbox = tbox;
                }
                else
                {
                    bbox = BoundingBox.Merge(bbox, tbox);
                }

                index++;
            }

            return bbox;
        }
        /// <summary>
        /// Generates a bounding sphere from a vertex list item list
        /// </summary>
        /// <param name="vertexListItems">Vertex list item list</param>
        /// <returns>Returns the minimum bounding sphere that contains all the specified vertex list item list</returns>
        public static BoundingSphere CreateBoundingSphere<T>(T[] vertexListItems) where T : IVertexList
        {
            BoundingSphere bsph = new BoundingSphere();

            for (int i = 0; i < vertexListItems.Length; i++)
            {
                BoundingSphere tsph = BoundingSphere.FromPoints(vertexListItems[i].GetVertices());

                if (i == 0)
                {
                    bsph = tsph;
                }
                else
                {
                    bsph = BoundingSphere.Merge(bsph, tsph);
                }
            }

            return bsph;
        }
    }
}
