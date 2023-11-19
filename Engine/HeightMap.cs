using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Height map
    /// </summary>
    public class HeightMap : IDisposable
    {
        /// <summary>
        /// Generates a new height map from a height map description
        /// </summary>
        /// <param name="description">Height map description</param>
        /// <returns>Returns the new generated height map</returns>
        public static HeightMap FromDescription(HeightmapDescription description)
        {
            if (description.Heightmap != null)
            {
                return FromMap(description.Heightmap, description.Colormap, description.UseFalloff, description.FalloffCurve.X, description.FalloffCurve.Y);
            }

            if (string.IsNullOrEmpty(description.HeightmapFileName))
            {
                throw new EngineException("No heightmap found in description.");
            }

            Stream heightMapData = ContentManager.FindContent(description.ContentPath, description.HeightmapFileName).FirstOrDefault();

            Stream colorMapData = null;
            if (!string.IsNullOrEmpty(description.ColormapFileName))
            {
                colorMapData = ContentManager.FindContent(description.ContentPath, description.ColormapFileName).FirstOrDefault();
            }

            return FromStream(heightMapData, colorMapData, description.UseFalloff, description.FalloffCurve.X, description.FalloffCurve.Y);
        }
        /// <summary>
        /// Generates a new height map from a bitmap stream
        /// </summary>
        /// <param name="heightData">Height data stream</param>
        /// <param name="colorData">Color data stream</param>
        /// <param name="useFalloff">Use falloff map</param>
        /// <param name="falloffCurveA">Falloff curve A</param>
        /// <param name="falloffCurveB">Falloff curve B</param>
        /// <returns>Returns the new generated height map</returns>
        private static HeightMap FromStream(Stream heightData, Stream colorData, bool useFalloff = false, float falloffCurveA = 0f, float falloffCurveB = 0f)
        {
            Image heightBitmap = Game.Images.FromStream(heightData);

            Image colorBitmap = default;
            if (colorData != null)
            {
                colorBitmap = Game.Images.FromStream(colorData);

                if (colorBitmap.Height != heightBitmap.Height || colorBitmap.Width != heightBitmap.Width)
                {
                    throw new EngineException("Height map and color map must have the same size");
                }
            }

            ReadImages(heightBitmap, colorBitmap, out var heights, out var colors);

            return FromMap(heights, colors, useFalloff, falloffCurveA, falloffCurveB);
        }
        /// <summary>
        /// Read images into data maps
        /// </summary>
        /// <param name="heightBitmap">Height image</param>
        /// <param name="colorBitmap">Color image</param>
        /// <param name="heights">Returns the height map</param>
        /// <param name="colors">Returns the color map</param>
        private static void ReadImages(Image heightBitmap, Image? colorBitmap, out float[,] heights, out Color4[,] colors)
        {
            heights = new float[heightBitmap.Height + 1, heightBitmap.Width + 1];
            colors = new Color4[heightBitmap.Height + 1, heightBitmap.Width + 1];

            for (int h = 0; h < heightBitmap.Height + 1; h++)
            {
                int hh = h < heightBitmap.Height ? h : h - 1;

                for (int w = 0; w < heightBitmap.Width + 1; w++)
                {
                    int ww = w < heightBitmap.Width ? w : w - 1;

                    //Flip coordinates
                    heights[h, w] = heightBitmap.GetPixel(ww, hh).Blue;
                    colors[h, w] = colorBitmap?.GetPixel(ww, hh) ?? Color.Gray;
                }
            }
        }
        /// <summary>
        /// Generates a new hight map from map data
        /// </summary>
        /// <param name="heights">Height map</param>
        /// <param name="colors">Color map</param>
        /// <param name="useFalloff">Use falloff map</param>
        /// <param name="falloffCurveA">Falloff curve A</param>
        /// <param name="falloffCurveB">Falloff curve B</param>
        /// <returns>Returns the new generated height map</returns>
        private static HeightMap FromMap(float[,] heights, Color4[,] colors, bool useFalloff = false, float falloffCurveA = 0f, float falloffCurveB = 0f)
        {
            float[,] falloffMap = null;
            if (useFalloff)
            {
                falloffMap = GenerateFalloff(heights.GetLength(0), heights.GetLength(1), falloffCurveA, falloffCurveB);
            }

            return new HeightMap(heights, colors, falloffMap);
        }
        /// <summary>
        /// Generates a falloff map
        /// </summary>
        /// <param name="width">Map width</param>
        /// <param name="height">Map height</param>
        /// <param name="a">Curve param A</param>
        /// <param name="b">Curve param B</param>
        public static float[,] GenerateFalloff(int width, int height, float a, float b)
        {
            float[,] res = new float[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float pX = x / (float)width * 2 - 1;
                    float pY = y / (float)width * 2 - 1;

                    float value = Math.Max(Math.Abs(pX), Math.Abs(pY));

                    res[x, y] = EvaluateFalloff(value, a, b);
                }
            }

            return res;
        }
        /// <summary>
        /// Evaluates the falloff curve function
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="a">Curve param A</param>
        /// <param name="b">Curve param B</param>
        private static float EvaluateFalloff(float value, float a, float b)
        {
            return (float)(Math.Pow(value, a) / (Math.Pow(value, a) + Math.Pow(b - b * value, a)));
        }
        /// <summary>
        /// Generates the height map normals
        /// </summary>
        /// <param name="vertList">Vertex list</param>
        /// <param name="width">Width</param>
        /// <param name="depth">Depth</param>
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
                        long index2;
                        long index3;
                        VertexData pos1 = vertList[index1];
                        VertexData pos2;
                        VertexData pos3;

                        index2 = ((y - 1) * width) + x;
                        index3 = (y * width) + (x - 1);
                        pos2 = vertList[index2];
                        pos3 = vertList[index3];
                        var n1 = GeometryUtil.ComputeNormals(
                            pos1.Position.Value, pos3.Position.Value, pos2.Position.Value,
                            pos1.Texture.Value, pos3.Texture.Value, pos2.Texture.Value);

                        index2 = (y * width) + (x - 1);
                        index3 = ((y + 1) * width) + (x - 1);
                        pos2 = vertList[index2];
                        pos3 = vertList[index3];
                        var n2 = GeometryUtil.ComputeNormals(
                            pos1.Position.Value, pos3.Position.Value, pos2.Position.Value,
                            pos1.Texture.Value, pos3.Texture.Value, pos2.Texture.Value);

                        index2 = ((y + 1) * width) + (x - 1);
                        index3 = ((y + 1) * width) + x;
                        pos2 = vertList[index2];
                        pos3 = vertList[index3];
                        var n3 = GeometryUtil.ComputeNormals(
                            pos1.Position.Value, pos3.Position.Value, pos2.Position.Value,
                            pos1.Texture.Value, pos3.Texture.Value, pos2.Texture.Value);

                        index2 = ((y + 1) * width) + x;
                        index3 = (y * width) + (x + 1);
                        pos2 = vertList[index2];
                        pos3 = vertList[index3];
                        var n4 = GeometryUtil.ComputeNormals(
                            pos1.Position.Value, pos3.Position.Value, pos2.Position.Value,
                            pos1.Texture.Value, pos3.Texture.Value, pos2.Texture.Value);

                        index2 = (y * width) + (x + 1);
                        index3 = ((y - 1) * width) + (x + 1);
                        pos2 = vertList[index2];
                        pos3 = vertList[index3];
                        var n5 = GeometryUtil.ComputeNormals(
                            pos1.Position.Value, pos3.Position.Value, pos2.Position.Value,
                            pos1.Texture.Value, pos3.Texture.Value, pos2.Texture.Value);

                        index2 = ((y - 1) * width) + (x + 1);
                        index3 = ((y - 1) * width) + x;
                        pos2 = vertList[index2];
                        pos3 = vertList[index3];
                        var n6 = GeometryUtil.ComputeNormals(
                            pos1.Position.Value, pos3.Position.Value, pos2.Position.Value,
                            pos1.Texture.Value, pos3.Texture.Value, pos2.Texture.Value);

                        Vector3 norm = (n1.Normal + n2.Normal + n3.Normal + n4.Normal + n5.Normal + n6.Normal) / 6.0f;
                        Vector3 tang = (n1.Tangent + n2.Tangent + n3.Tangent + n4.Tangent + n5.Tangent + n6.Tangent) / 6.0f;
                        Vector3 binorm = (n1.Binormal + n2.Binormal + n3.Binormal + n4.Binormal + n5.Binormal + n6.Binormal) / 6.0f;

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
        /// Falloff map mada
        /// </summary>
        private float[,] m_FalloffData;

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
                if (m_HeightData != null)
                {
                    return m_HeightData.GetLongLength(0);
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
                if (m_HeightData != null)
                {
                    return m_HeightData.GetLongLength(1);
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
                if (m_HeightData != null)
                {
                    return m_HeightData.LongLength;
                }

                return 0;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="heightData">Height map data</param>
        /// <param name="colorData">Color map data</param>
        /// <param name="falloffData">Falloff map data</param>
        HeightMap(float[,] heightData, Color4[,] colorData, float[,] falloffData)
        {
            m_HeightData = heightData;
            m_ColorData = colorData;
            m_FalloffData = falloffData;

            Min = float.MaxValue;
            Max = float.MinValue;

            foreach (var height in heightData)
            {
                if (height < Min)
                {
                    Min = height;
                }

                if (height > Max)
                {
                    Max = height;
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
                m_ColorData = null;
                m_HeightData = null;
                m_FalloffData = null;
            }
        }

        /// <summary>
        /// Generates the vertex data from the height map
        /// </summary>
        /// <param name="cellSize">Cell size</param>
        /// <param name="cellHeight">Cell height</param>
        /// <param name="heightCurve">Height curve</param>
        /// <param name="textureScale">Texture scale</param>
        /// <param name="textureDisplacement">Texture displacement</param>
        /// <returns>Returns a vertex list, and a index list</returns>
        public async Task<(IEnumerable<VertexData> Vertices, IEnumerable<uint> Indices)> BuildGeometry(float cellSize, float cellHeight, Curve heightCurve, float textureScale, Vector2 textureDisplacement)
        {
            var vertTask = Task.Run(() =>
            {
                VertexData[] vertArray = new VertexData[Width * Depth];

                Parallel.For(0, Depth, (depth) =>
                {
                    for (long width = 0; width < Width; width++)
                    {
                        var (Index, Vertex) = BuildVertex(depth, width, cellSize, cellHeight, heightCurve, textureScale, textureDisplacement);

                        vertArray[Index] = Vertex;
                    }
                });

                ComputeHeightMapNormals(vertArray, Width, Depth);

                return vertArray;
            });

            var indexTask = Task.Run(() =>
            {
                uint[] indexArray = new uint[(Width - 1) * (Depth - 1) * 6];

                Parallel.For(0, Depth - 1, (depth) =>
                {
                    for (long width = 0; width < Width - 1; width++)
                    {
                        var (Index, Quad) = BuildQuad(depth, width);

                        Array.Copy(Quad, 0, indexArray, Index, 6);
                    }
                });

                return indexArray;
            });

            return (await vertTask, await indexTask);
        }
        /// <summary>
        /// Builds a vertex
        /// </summary>
        /// <param name="depth">Depth</param>
        /// <param name="width">Width</param>
        /// <param name="cellSize">Cell size</param>
        /// <param name="cellHeight">Cell height</param>
        /// <param name="heightCurve">Height curve</param>
        /// <param name="textureScale">Texture scale</param>
        /// <param name="textureDisplacement">Texture displacement</param>
        /// <returns>Returns a vertex</returns>
        private (long Index, VertexData Vertex) BuildVertex(long depth, long width, float cellSize, float cellHeight, Curve heightCurve, float textureScale, Vector2 textureDisplacement)
        {
            float totalWidth = cellSize * (Width - 1);
            float totalDepth = cellSize * (Depth - 1);

            long vertexIndex = (depth * Depth) + width;

            float h = heightCurve.Evaluate(m_HeightData[depth, width]);
            if (m_FalloffData != null)
            {
                h = MathUtil.Clamp(h - m_FalloffData[depth, width], 0, 1);
            }

            Color4 c = m_ColorData != null ? m_ColorData[depth, width] : Color4.Lerp(Color4.Black, Color4.White, h);

            float posX = (depth * cellSize) - (totalDepth * 0.5f);
            float posY = h * cellHeight;
            float posZ = (width * cellSize) - (totalWidth * 0.5f);

            float tu = width * cellSize / totalWidth;
            float tv = depth * cellSize / totalDepth;

            var newVertex = new VertexData()
            {
                Position = new Vector3(posX, posY, posZ),
                Texture = (new Vector2(tu, tv) + textureDisplacement) / textureScale,
                Color = c,
            };

            return (vertexIndex, newVertex);
        }
        /// <summary>
        /// Builds a quad of indexes
        /// </summary>
        /// <param name="depth">Depth</param>
        /// <param name="width">Width</param>
        /// <returns>Returns a quad</returns>
        private (long Index, uint[] Quad) BuildQuad(long depth, long width)
        {
            long vertexIndex = ((depth * (Depth - 1)) + width) * 6;

            long index1 = (Depth * (depth + 0)) + (width + 0); // top left
            long index2 = (Depth * (depth + 0)) + (width + 1); // top right
            long index3 = (Depth * (depth + 1)) + (width + 0); // bottom left
            long index4 = (Depth * (depth + 1)) + (width + 1); // bottom right

            uint[] quad = new[]
            {
                (uint)index1,
                (uint)index2,
                (uint)index3,
                (uint)index2,
                (uint)index4,
                (uint)index3,
            };

            return (vertexIndex, quad);
        }
        /// <summary>
        /// Gets the number of triangles of the note for the specified partition level
        /// </summary>
        /// <param name="partitionLevel">Partition level</param>
        /// <returns>Returns the number of triangles of the note for the specified partition level</returns>
        public int CalcTrianglesPerNode(int partitionLevel)
        {
            int side = ((int)Math.Sqrt(DataLength) - 1) / ((int)Math.Pow(2, partitionLevel));

            return side * side * 2;
        }
    }
}
