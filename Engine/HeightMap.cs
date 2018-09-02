using SharpDX;
using System;
using System.Drawing;
using System.IO;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Height map
    /// </summary>
    public class HeightMap : IDisposable
    {
        /// <summary>
        /// Generates a new height map from a bitmap stream
        /// </summary>
        /// <param name="heightData">Height data stream</param>
        /// <param name="colorData">Color data stream</param>
        /// <returns>Returns the new generated height map</returns>
        public static HeightMap FromStream(Stream heightData, Stream colorData)
        {
            Bitmap heightBitmap = Bitmap.FromStream(heightData) as Bitmap;

            Bitmap colorBitmap = null;
            if (colorData != null)
            {
                colorBitmap = Bitmap.FromStream(colorData) as Bitmap;

                if (colorBitmap.Height != heightBitmap.Height || colorBitmap.Width != heightBitmap.Width)
                {
                    throw new EngineException("Height map and color map must have the same size");
                }
            }

            var heights = new float[heightBitmap.Height + 1, heightBitmap.Width + 1];
            var colors = new Color4[heightBitmap.Height + 1, heightBitmap.Width + 1];

            using (colorBitmap)
            using (heightBitmap)
            {
                for (int h = 0; h < heightBitmap.Height + 1; h++)
                {
                    int hh = h < heightBitmap.Height ? h : h - 1;

                    for (int w = 0; w < heightBitmap.Width + 1; w++)
                    {
                        int ww = w < heightBitmap.Width ? w : w - 1;

                        var height = heightBitmap.GetPixel(hh, ww);
                        var color = colorBitmap != null ? colorBitmap.GetPixel(hh, ww) : System.Drawing.Color.Gray;

                        heights[w, h] = (float)height.B / 255f;
                        colors[w, h] = new SharpDX.Color(color.R, color.G, color.B, color.A);
                    }
                }
            }

            return new HeightMap(heights, colors);
        }
        /// <summary>
        /// Generates the height map normals
        /// </summary>
        /// <param name="cellSize">Cell size</param>
        /// <returns>Returns the generated normals array</returns>
        private static void ComputeHeightMapNormals(VertexData[] vertList, long width, long depth)
        {
            for (long x = 0; x < depth; x++)
            {
                for (long y = 0; y < width; y++)
                {
                    long index1 = (y * width) + x;

                    Vector3 normal;
                    Vector3 tangent;
                    Vector3 binormal;

                    if (x == 0 || y == 0 || x == (depth - 1) || y == (width - 1))
                    {
                        // The vertices in the borders have always the up normal
                        normal = Vector3.UnitY;
                        tangent = Vector3.UnitX;
                        binormal = Vector3.UnitZ;
                    }
                    else
                    {
                        Vector3 norm1;
                        Vector3 norm2;
                        Vector3 norm3;
                        Vector3 norm4;
                        Vector3 norm5;
                        Vector3 norm6;

                        Vector3 tan1;
                        Vector3 tan2;
                        Vector3 tan3;
                        Vector3 tan4;
                        Vector3 tan5;
                        Vector3 tan6;

                        Vector3 biNorm1;
                        Vector3 biNorm2;
                        Vector3 biNorm3;
                        Vector3 biNorm4;
                        Vector3 biNorm5;
                        Vector3 biNorm6;

                        long index2;
                        long index3;
                        VertexData pos1 = vertList[index1];
                        VertexData pos2;
                        VertexData pos3;

                        index2 = ((y - 1) * width) + x;
                        index3 = (y * width) + (x - 1);
                        pos2 = vertList[index2];
                        pos3 = vertList[index3];
                        GeometryUtil.ComputeNormals(
                            pos1.Position.Value, pos3.Position.Value, pos2.Position.Value,
                            pos1.Texture.Value, pos3.Texture.Value, pos2.Texture.Value,
                            out tan1, out biNorm1, out norm1);

                        index2 = (y * width) + (x - 1);
                        index3 = ((y + 1) * width) + (x - 1);
                        pos2 = vertList[index2];
                        pos3 = vertList[index3];
                        GeometryUtil.ComputeNormals(
                            pos1.Position.Value, pos3.Position.Value, pos2.Position.Value,
                            pos1.Texture.Value, pos3.Texture.Value, pos2.Texture.Value,
                            out tan2, out biNorm2, out norm2);

                        index2 = ((y + 1) * width) + (x - 1);
                        index3 = ((y + 1) * width) + x;
                        pos2 = vertList[index2];
                        pos3 = vertList[index3];
                        GeometryUtil.ComputeNormals(
                            pos1.Position.Value, pos3.Position.Value, pos2.Position.Value,
                            pos1.Texture.Value, pos3.Texture.Value, pos2.Texture.Value,
                            out tan3, out biNorm3, out norm3);

                        index2 = ((y + 1) * width) + x;
                        index3 = (y * width) + (x + 1);
                        pos2 = vertList[index2];
                        pos3 = vertList[index3];
                        GeometryUtil.ComputeNormals(
                            pos1.Position.Value, pos3.Position.Value, pos2.Position.Value,
                            pos1.Texture.Value, pos3.Texture.Value, pos2.Texture.Value,
                            out tan4, out biNorm4, out norm4);

                        index2 = (y * width) + (x + 1);
                        index3 = ((y - 1) * width) + (x + 1);
                        pos2 = vertList[index2];
                        pos3 = vertList[index3];
                        GeometryUtil.ComputeNormals(
                            pos1.Position.Value, pos3.Position.Value, pos2.Position.Value,
                            pos1.Texture.Value, pos3.Texture.Value, pos2.Texture.Value,
                            out tan5, out biNorm5, out norm5);

                        index2 = ((y - 1) * width) + (x + 1);
                        index3 = ((y - 1) * width) + x;
                        pos2 = vertList[index2];
                        pos3 = vertList[index3];
                        GeometryUtil.ComputeNormals(
                            pos1.Position.Value, pos3.Position.Value, pos2.Position.Value,
                            pos1.Texture.Value, pos3.Texture.Value, pos2.Texture.Value,
                            out tan6, out biNorm6, out norm6);

                        Vector3 norm = (norm1 + norm2 + norm3 + norm4 + norm5 + norm6) / 6.0f;
                        Vector3 tang = (tan1 + tan2 + tan3 + tan4 + tan5 + tan6) / 6.0f;
                        Vector3 binorm = (biNorm1 + biNorm2 + biNorm3 + biNorm4 + biNorm5 + biNorm6) / 6.0f;

                        normal = Vector3.Normalize(norm);
                        tangent = Vector3.Normalize(tang);
                        binormal = Vector3.Normalize(binorm);
                    }

                    vertList[index1].Normal = normal;
                    vertList[index1].Tangent = tangent;
                    vertList[index1].BiNormal = binormal;
                }
            }
        }

        /// <summary>
        /// Heights
        /// </summary>
        private float[,] m_HeightData;
        /// <summary>
        /// Color map data
        /// </summary>
        private Color4[,] m_ColorData;

        /// <summary>
        /// Minimum height
        /// </summary>
        public float Min { get; private set; }
        /// <summary>
        /// Maximum height
        /// </summary>
        public float Max { get; private set; }
        /// <summary>
        /// Width
        /// </summary>
        public long Width
        {
            get
            {
                if (this.m_HeightData != null)
                {
                    return this.m_HeightData.GetLongLength(0);
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
                if (this.m_HeightData != null)
                {
                    return this.m_HeightData.GetLongLength(1);
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
                if (this.m_HeightData != null)
                {
                    return this.m_HeightData.LongLength;
                }

                return 0;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="heightData">Height map data</param>
        /// <param name="colorData">Color map data</param>
        public HeightMap(float[,] heightData, Color4[,] colorData)
        {
            this.m_HeightData = heightData;
            this.m_ColorData = colorData;

            foreach (int height in heightData)
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
        /// Destructor
        /// </summary>
        ~HeightMap()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_ColorData = null;
                this.m_HeightData = null;
            }
        }

        /// <summary>
        /// Generates the vertex data from the height map
        /// </summary>
        /// <param name="cellSize">Cell size</param>
        /// <param name="cellHeight">Cell height</param>
        /// <param name="vertices">Result vertices</param>
        /// <param name="indices">Result indices</param>
        public void BuildGeometry(float cellSize, float cellHeight, out VertexData[] vertices, out uint[] indices)
        {
            float totalWidth = cellSize * (this.Width - 1);
            float totalDepth = cellSize * (this.Depth - 1);

            long vertexCountX = this.Width;
            long vertexCountZ = this.Depth;

            vertices = new VertexData[vertexCountX * vertexCountZ];
            indices = new uint[(vertexCountX - 1) * (vertexCountZ - 1) * 2 * 3];

            long vertexCount = 0;

            for (long depth = 0; depth < vertexCountZ; depth++)
            {
                for (long width = 0; width < vertexCountX; width++)
                {
                    float posX = (depth * cellSize) - (totalDepth * 0.5f);
                    float posY = this.m_HeightData[depth, width] * cellHeight;
                    float posZ = (width * cellSize) - (totalWidth * 0.5f);

                    float tu = width * cellSize / totalWidth;
                    float tv = depth * cellSize / totalDepth;

                    VertexData newVertex = new VertexData()
                    {
                        Position = new Vector3(posX, posY, posZ),
                        Texture = new Vector2(tu, tv),
                        Color = this.m_ColorData[depth, width],
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
                    indices[indexCount++] = (uint)index3;
                    indices[indexCount++] = (uint)index2;

                    indices[indexCount++] = (uint)index2;
                    indices[indexCount++] = (uint)index3;
                    indices[indexCount++] = (uint)index4;
                }
            }

            ComputeHeightMapNormals(vertices, this.Width, this.Depth);
        }
        /// <summary>
        /// Gets the number of triangles of the note for the specified partition level
        /// </summary>
        /// <param name="partitionLevel">Partition level</param>
        /// <returns>Returns the number of triangles of the note for the specified partition level</returns>
        public int CalcTrianglesPerNode(int partitionLevel)
        {
            int side = ((int)Math.Sqrt(this.DataLength) - 1) / ((int)Math.Pow(2, partitionLevel));

            return side * side * 2;
        }
    }
}
