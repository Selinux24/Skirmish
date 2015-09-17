using SharpDX;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Engine.Common
{
    /// <summary>
    /// Height map
    /// </summary>
    public class HeightMap
    {
        /// <summary>
        /// Generates a new height map from a bitmap stream
        /// </summary>
        /// <param name="stream">Bitmap stream</param>
        /// <returns>Returns the new generated height map</returns>
        public static HeightMap FromStream(Stream stream)
        {
            using (var bitmap = Bitmap.FromStream(stream) as Bitmap)
            {
                var result = new float[bitmap.Height, bitmap.Width];

                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        var color = bitmap.GetPixel(x, y);

                        result[x, y] = (float)color.B / 255f;
                    }
                }

                return new HeightMap(result);
            }
        }

        /// <summary>
        /// Heights
        /// </summary>
        private float[,] m_Data;
        /// <summary>
        /// Minimum height
        /// </summary>
        public readonly float Min;
        /// <summary>
        /// Maximum height
        /// </summary>
        public readonly float Max;

        /// <summary>
        /// Width
        /// </summary>
        public long Width
        {
            get
            {
                if (this.m_Data != null)
                {
                    return this.m_Data.GetLongLength(0);
                }

                return 0;
            }
        }
        /// <summary>
        /// Depth
        /// </summary>
        public long Depth
        {
            get
            {
                if (this.m_Data != null)
                {
                    return this.m_Data.GetLongLength(1);
                }

                return 0;
            }
        }
        /// <summary>
        /// Gets the total height count
        /// </summary>
        public long DataLength
        {
            get
            {
                if (this.m_Data != null)
                {
                    return this.m_Data.LongLength;
                }

                return 0;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Height map data</param>
        public HeightMap(float[,] data)
        {
            this.m_Data = data;

            foreach (int height in data)
            {
                if (height < this.Min)
                {
                    this.Min = height;
                }

                if (height > this.Max)
                {
                    this.Max = height;
                }
            }
        }

        /// <summary>
        /// Generates the vertex data from the height map
        /// </summary>
        /// <param name="cellSize">Cell size</param>
        /// <returns>Returns the generated vertex data array</returns>
        public void BuildGeometry(float cellSize, float cellHeight, out VertexData[] vertices, out uint[] indices)
        {
            long vertexCountX = this.Width;
            long vertexCountZ = this.Depth;

            vertices = new VertexData[vertexCountX * vertexCountZ];
            indices = new uint[(vertexCountX - 1) * (vertexCountZ - 1) * 2 * 3];

            long vertexCount = 0;

            for (long width = 0; width < vertexCountX; width++)
            {
                for (long depth = 0; depth < vertexCountZ; depth++)
                {
                    float posX = width * cellSize;
                    float posY = this.m_Data[depth, width] * cellHeight;
                    float posZ = depth * cellSize;

                    VertexData newVertex = new VertexData()
                    {
                        Position = new Vector3(posX, posY, posZ),
                        Texture = new Vector2(width / 10.0f, depth / 10.0f),
                    };

                    vertices[vertexCount++] = newVertex;
                }
            }

            long indexCount = 0;

            for (long depth = 0; depth < vertexCountZ - 1; depth++)
            {
                for (long width = 0; width < vertexCountX - 1; width++)
                {
                    long index1 = (vertexCountZ * (depth + 0)) + (width + 0); // top left
                    long index2 = (vertexCountZ * (depth + 0)) + (width + 1); // top right
                    long index3 = (vertexCountZ * (depth + 1)) + (width + 0); // bottom left
                    long index4 = (vertexCountZ * (depth + 1)) + (width + 1); // bottom right

                    indices[indexCount++] = (uint)index1;
                    indices[indexCount++] = (uint)index2;
                    indices[indexCount++] = (uint)index3;

                    indices[indexCount++] = (uint)index2;
                    indices[indexCount++] = (uint)index4;
                    indices[indexCount++] = (uint)index3;
                }
            }

            ComputeNormals(vertices, this.Width, this.Depth);
        }
        /// <summary>
        /// Generates the height map normals
        /// </summary>
        /// <param name="cellSize">Cell size</param>
        /// <returns>Returns the generated normals array</returns>
        private static void ComputeNormals(VertexData[] vertList, long width, long depth)
        {
            for (long y = 0; y < width; y++)
            {
                for (long x = 0; x < depth; x++)
                {
                    long index1 = (y * width) + x;

                    VertexData pos1 = vertList[index1];

                    if (x == 0 || y == 0 || x == (depth - 1) || y == (width - 1))
                    {
                        // The vertices in the borders have always the up normal
                        pos1.Normal = Vector3.Up;
                    }
                    else
                    {
                        Vector3 tangent;
                        Vector3 binormal;

                        Vector3 norm1;
                        Vector3 norm2;
                        Vector3 norm3;
                        Vector3 norm4;
                        Vector3 norm5;
                        Vector3 norm6;

                        long index2;
                        long index3;
                        VertexData pos2;
                        VertexData pos3;

                        index2 = ((y - 1) * width) + x;
                        index3 = (y * width) + (x - 1);
                        pos2 = vertList[index2];
                        pos3 = vertList[index3];
                        VertexData.ComputeNormals(pos1, pos2, pos3, out tangent, out binormal, out norm1);

                        index2 = (y * width) + (x - 1);
                        index3 = ((y + 1) * width) + (x - 1);
                        pos2 = vertList[index2];
                        pos3 = vertList[index3];
                        VertexData.ComputeNormals(pos1, pos2, pos3, out tangent, out binormal, out norm2);

                        index2 = ((y + 1) * width) + (x - 1);
                        index3 = ((y + 1) * width) + x;
                        pos2 = vertList[index2];
                        pos3 = vertList[index3];
                        VertexData.ComputeNormals(pos1, pos2, pos3, out tangent, out binormal, out norm3);

                        index2 = ((y + 1) * width) + x;
                        index3 = (y * width) + (x + 1);
                        pos2 = vertList[index2];
                        pos3 = vertList[index3];
                        VertexData.ComputeNormals(pos1, pos2, pos3, out tangent, out binormal, out norm4);

                        index2 = (y * width) + (x + 1);
                        index3 = ((y - 1) * width) + (x + 1);
                        pos2 = vertList[index2];
                        pos3 = vertList[index3];
                        VertexData.ComputeNormals(pos1, pos2, pos3, out tangent, out binormal, out norm5);

                        index2 = ((y - 1) * width) + (x + 1);
                        index3 = ((y - 1) * width) + x;
                        pos2 = vertList[index2];
                        pos3 = vertList[index3];
                        VertexData.ComputeNormals(pos1, pos2, pos3, out tangent, out binormal, out norm6);

                        Vector3 norm = (norm1 + norm2 + norm3 + norm4 + norm5 + norm6) / 6.0f;

                        pos1.Normal = Vector3.Normalize(norm);
                    }
                }
            }
        }
    }
}
