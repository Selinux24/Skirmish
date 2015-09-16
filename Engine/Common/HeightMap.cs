using SharpDX;
using System.Collections.Generic;

namespace Engine.Common
{
    /// <summary>
    /// Height map
    /// </summary>
    public class HeightMap
    {
        /// <summary>
        /// Generates a new height map from a bitmap
        /// </summary>
        /// <param name="bitmapData">Bitmap buffer</param>
        /// <param name="bitmapHeight">Height in pixels</param>
        /// <param name="bitmapWidth">Width in pixels</param>
        /// <param name="cellScale">Cell scale</param>
        /// <returns>Returns the new generated height map</returns>
        public static HeightMap FromData(byte[] bitmapData, int bitmapHeight, int bitmapWidth, float cellScale)
        {
            int height = bitmapHeight + 1;
            int width = bitmapWidth + 1;
            int stride = bitmapData.Length / (bitmapWidth * bitmapHeight);
            int size = bitmapData.Length / stride;

            float[,] result = new float[height, width];

            int dataX = 0;
            int dataZ = 0;
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    dataX = x;
                    dataZ = z;

                    if (x == width - 1)
                    {
                        //Max width reached
                        dataX = x - 1;
                    }

                    if (z == height - 1)
                    {
                        //Max height reached
                        dataZ = z - 1;
                    }

                    int index = ((dataX * (width - 1)) + dataZ) * stride;

                    result[x, z] = (float)(bitmapData[index] & 0x000000FF) * cellScale;
                }
            }

            return new HeightMap(result);
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
        public int Width
        {
            get
            {
                if (this.m_Data != null)
                {
                    return this.m_Data.GetLength(0);
                }

                return 0;
            }
        }
        /// <summary>
        /// Depth
        /// </summary>
        public int Depth
        {
            get
            {
                if (this.m_Data != null)
                {
                    return this.m_Data.GetLength(1);
                }

                return 0;
            }
        }
        /// <summary>
        /// Gets the total height count
        /// </summary>
        public int DataLength
        {
            get
            {
                if (this.m_Data != null)
                {
                    return this.m_Data.Length;
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

            foreach (float height in data)
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
        public VertexData[] BuildVertices(float cellSize)
        {
            // Contador de vértices
            int vertexCountX = this.Width;
            int vertexCountZ = this.Depth;
            int vertexCount = vertexCountX * vertexCountZ;

            // Crear los vértices
            List<VertexData> vertList = new List<VertexData>(vertexCount);
            Vector3[,] normals = this.CreateNormals(cellSize);

            for (int width = 0; width < vertexCountX; width++)
            {
                for (int deep = 0; deep < vertexCountZ; deep++)
                {
                    VertexData newVertex = new VertexData();

                    float posX = width * cellSize;
                    float posY = this.m_Data[deep, width];
                    float posZ = deep * cellSize;

                    newVertex.Position = new Vector3(posX, posY, posZ);
                    newVertex.Normal = normals[deep, width];
                    newVertex.Texture = new Vector2(width / 10.0f, deep / 10.0f);

                    vertList.Add(newVertex);
                }
            }

            return vertList.ToArray();
        }
        /// <summary>
        /// Generates the height map normals
        /// </summary>
        /// <param name="cellSize">Cell size</param>
        /// <returns>Returns the generated normals array</returns>
        private Vector3[,] CreateNormals(float cellSize)
        {
            int width = this.Width;
            int deep = this.Depth;

            Vector3[,] normals = new Vector3[width, deep];

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < deep; x++)
                {
                    if (x == 0 || y == 0 || x == (deep - 1) || y == (width - 1))
                    {
                        // The vertices in the borders have always the up normal
                        normals[y, x] = Vector3.Up;
                    }
                    else
                    {
                        // Get vertex position to calculate normals
                        Vector3 pos = new Vector3(cellSize * x, this.m_Data[y, x], cellSize * y);

                        Vector3 pos2;
                        Vector3 pos3;
                        Vector3 norm1;
                        Vector3 norm2;
                        Vector3 norm3;
                        Vector3 norm4;
                        Vector3 norm5;
                        Vector3 norm6;

                        pos2 = new Vector3(cellSize * (x), this.m_Data[y - 1, x], cellSize * (y - 1));
                        pos3 = new Vector3(cellSize * (x - 1), this.m_Data[y, x - 1], cellSize * (y));
                        pos2 -= pos;
                        pos3 -= pos;
                        pos2.Normalize();
                        pos3.Normalize();
                        norm1 = Vector3.Cross(pos2, pos3);

                        pos2 = new Vector3(cellSize * (x - 1), this.m_Data[y, x - 1], cellSize * (y));
                        pos3 = new Vector3(cellSize * (x - 1), this.m_Data[y + 1, x - 1], cellSize * (y + 1));
                        pos2 -= pos;
                        pos3 -= pos;
                        pos2.Normalize();
                        pos3.Normalize();
                        norm2 = Vector3.Cross(pos2, pos3);

                        pos2 = new Vector3(cellSize * (x - 1), this.m_Data[y + 1, x - 1], cellSize * (y + 1));
                        pos3 = new Vector3(cellSize * (x), this.m_Data[y + 1, x], cellSize * (y + 1));
                        pos2 -= pos;
                        pos3 -= pos;
                        pos2.Normalize();
                        pos3.Normalize();
                        norm3 = Vector3.Cross(pos2, pos3);

                        pos2 = new Vector3(cellSize * (x), this.m_Data[y + 1, x], cellSize * (y + 1));
                        pos3 = new Vector3(cellSize * (x + 1), this.m_Data[y, x + 1], cellSize * (y));
                        pos2 -= pos;
                        pos3 -= pos;
                        pos2.Normalize();
                        pos3.Normalize();
                        norm4 = Vector3.Cross(pos2, pos3);

                        pos2 = new Vector3(cellSize * (x + 1), this.m_Data[y, x + 1], cellSize * (y));
                        pos3 = new Vector3(cellSize * (x + 1), this.m_Data[y - 1, x + 1], cellSize * (y - 1));
                        pos2 -= pos;
                        pos3 -= pos;
                        pos2.Normalize();
                        pos3.Normalize();
                        norm5 = Vector3.Cross(pos2, pos3);

                        pos2 = new Vector3(cellSize * (x + 1), this.m_Data[y - 1, x + 1], cellSize * (y - 1));
                        pos3 = new Vector3(cellSize * (x), this.m_Data[y - 1, x], cellSize * (y - 1));
                        pos2 -= pos;
                        pos3 -= pos;
                        pos2.Normalize();
                        pos3.Normalize();
                        norm6 = Vector3.Cross(pos2, pos3);

                        Vector3 norm = (norm1 + norm2 + norm3 + norm4 + norm5 + norm6) / 6.0f;

                        normals[y, x] = Vector3.Normalize(norm);
                    }
                }
            }

            return normals;
        }
    }
}
