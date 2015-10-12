using SharpDX;
using System;
using System.Collections.Generic;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Helpers;

    public class Terrain2 : Drawable
    {
        public const int MaxPatchesHighLevel = 4;
        public const int MaxPatchesMediumLevel = 6;
        public const int MaxPatchesLowLevel = 8;
        public const int MaxPatchesDataLoadLevel = 10;

        private HeightMap heightMap = null;
        private IndexBufferDictionary indices = new IndexBufferDictionary();
        private TerrainPatchDictionary patches = new TerrainPatchDictionary();
        private TerrainPatchAssignationDictionary patchAssignation = new TerrainPatchAssignationDictionary();
        private QuadTree quadTree = null;
        private BoundingFrustum frustum = new BoundingFrustum();

        public Terrain2(Game game, TerrainDescription description)
            : base(game)
        {
            ImageContent heightMapImage = new ImageContent()
            {
                Streams = ContentManager.FindContent(description.ContentPath, description.Heightmap.HeightmapFileName),
            };

            //Read heightmap
            this.heightMap = HeightMap.FromStream(heightMapImage.Stream);

            //Get vertices and indices
            VertexData[] vertices;
            uint[] indices;
            this.heightMap.BuildGeometry(
                description.Heightmap.CellSize,
                description.Heightmap.MaximumHeight,
                out vertices, out indices);

            //Initialize Quadtree
            this.quadTree = QuadTree.Build(
                game,
                vertices,
                description);

            //Intialize Indices
            this.InitializeIndices(description.Quadtree.MaxTrianglesPerNode);

            //Initialize patches
            this.InitializePatches(description.Quadtree.MaxTrianglesPerNode);
        }

        public override void Dispose()
        {
            Helper.Dispose(this.indices);
            Helper.Dispose(this.patches);
        }

        private void InitializeIndices(int trianglesPerNode)
        {
            this.indices.Add(LevelOfDetailEnum.High, new Dictionary<IndexBufferShapeEnum, Buffer>());
            this.indices.Add(LevelOfDetailEnum.Medium, new Dictionary<IndexBufferShapeEnum, Buffer>());
            this.indices.Add(LevelOfDetailEnum.Low, new Dictionary<IndexBufferShapeEnum, Buffer>());

            //High level
            for (int i = 0; i < 9; i++)
            {
                IndexBufferShapeEnum shape = (IndexBufferShapeEnum)i;

                uint[] indexList = IndexBufferDictionary.GenerateIndices(shape, trianglesPerNode);
                Buffer buffer = this.Game.Graphics.Device.CreateIndexBufferImmutable(indexList);
                this.indices[LevelOfDetailEnum.High].Add(shape, buffer);
            }

            //Medium level
            for (int i = 0; i < 9; i++)
            {
                IndexBufferShapeEnum shape = (IndexBufferShapeEnum)i;

                uint[] indexList = IndexBufferDictionary.GenerateIndices(shape, trianglesPerNode / 4);
                Buffer buffer = this.Game.Graphics.Device.CreateIndexBufferImmutable(indexList);
                this.indices[LevelOfDetailEnum.Medium].Add(shape, buffer);
            }

            //Low level
            {
                IndexBufferShapeEnum shape = IndexBufferShapeEnum.Full;

                uint[] indexList = IndexBufferDictionary.GenerateIndices(shape, trianglesPerNode / 4 / 4);
                Buffer buffer = this.Game.Graphics.Device.CreateIndexBufferImmutable(indexList);
                this.indices[LevelOfDetailEnum.Low].Add(shape, buffer);
            }
        }

        private void InitializePatches(int trianglesPerNode)
        {
            this.patches.Add(LevelOfDetailEnum.High, new TerrainPatch[MaxPatchesHighLevel]);
            this.patches.Add(LevelOfDetailEnum.Medium, new TerrainPatch[MaxPatchesMediumLevel]);
            this.patches.Add(LevelOfDetailEnum.Low, new TerrainPatch[MaxPatchesLowLevel]);
            this.patches.Add(LevelOfDetailEnum.DataLoad, new TerrainPatch[MaxPatchesDataLoadLevel]);

            for (int i = 0; i < MaxPatchesHighLevel; i++)
            {
                var patch = TerrainPatch.CreatePatch(this.Game, LevelOfDetailEnum.High, trianglesPerNode);
                this.patches[LevelOfDetailEnum.High][i] = patch;
            }

            for (int i = 0; i < MaxPatchesMediumLevel; i++)
            {
                var patch = TerrainPatch.CreatePatch(this.Game, LevelOfDetailEnum.Medium, trianglesPerNode);
                this.patches[LevelOfDetailEnum.Medium][i] = patch;
            }

            for (int i = 0; i < MaxPatchesLowLevel; i++)
            {
                var patch = TerrainPatch.CreatePatch(this.Game, LevelOfDetailEnum.Low, trianglesPerNode);
                this.patches[LevelOfDetailEnum.Low][i] = patch;
            }

            for (int i = 0; i < MaxPatchesDataLoadLevel; i++)
            {
                var patch = TerrainPatch.CreatePatch(this.Game, LevelOfDetailEnum.DataLoad, trianglesPerNode);
                this.patches[LevelOfDetailEnum.DataLoad][i] = patch;
            }
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(GameTime gameTime, Context context)
        {

        }
    }

    public enum LevelOfDetailEnum
    {
        None = 0,
        High = 1,
        Medium = 2,
        Low = 3,
        DataLoad = 4,
    }

    public enum IndexBufferShapeEnum : int
    {
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

    public class IndexBufferDictionary : Dictionary<LevelOfDetailEnum, Dictionary<IndexBufferShapeEnum, Buffer>>
    {
        public static uint[] GenerateIndices(IndexBufferShapeEnum bufferShape, int trianglesPerNode)
        {
            uint[] indices = null;

            bool full = bufferShape == IndexBufferShapeEnum.Full;
            bool side = (
                bufferShape == IndexBufferShapeEnum.SideTop ||
                bufferShape == IndexBufferShapeEnum.SideBottom ||
                bufferShape == IndexBufferShapeEnum.SideLeft ||
                bufferShape == IndexBufferShapeEnum.SideRight);
            bool corner = (
                bufferShape == IndexBufferShapeEnum.CornerTopLeft ||
                bufferShape == IndexBufferShapeEnum.CornerBottomLeft ||
                bufferShape == IndexBufferShapeEnum.CornerTopRight ||
                bufferShape == IndexBufferShapeEnum.CornerBottomRight);

            if (full)
            {
                indices = GenerateFull(trianglesPerNode);
            }
            else if (side)
            {
                indices = GenerateSide(bufferShape, trianglesPerNode);
            }
            else if (corner)
            {
                indices = GenerateCorner(bufferShape, trianglesPerNode);
            }

            return indices;
        }

        private static uint[] GenerateFull(int trianglesPerNode)
        {
            int nodes = trianglesPerNode / 2;
            uint size = (uint)Math.Sqrt(nodes);

            uint[] indices = new uint[trianglesPerNode * 3];

            int index = 0;

            for (uint y = 0; y < size; y++)
            {
                for (uint x = 0; x < size; x++)
                {
                    uint indexCRow = ((y + 0) * size) + x;
                    uint indexNRow = ((y + 1) * size) + x;

                    //Tri 1
                    indices[index++] = indexCRow;
                    indices[index++] = indexCRow + 1;
                    indices[index++] = indexNRow;

                    //Tri 2
                    indices[index++] = indexCRow + 1;
                    indices[index++] = indexNRow;
                    indices[index++] = indexNRow + 1;
                }
            }

            return indices;
        }

        private static uint[] GenerateSide(IndexBufferShapeEnum bufferShape, int trianglesPerNode)
        {
            uint nodes = (uint)trianglesPerNode / 2;
            uint size = (uint)Math.Sqrt(nodes);
            uint triangles = ((nodes - size) * 2) + ((size / 2) * 3);

            uint lY = bufferShape == IndexBufferShapeEnum.SideTop ? (uint)1 : (uint)0;
            uint lX = bufferShape == IndexBufferShapeEnum.SideLeft ? (uint)1 : (uint)0;
            uint rY = bufferShape == IndexBufferShapeEnum.SideBottom ? (uint)1 : (uint)0;
            uint rX = bufferShape == IndexBufferShapeEnum.SideRight ? (uint)1 : (uint)0;

            uint[] indices = new uint[triangles * 3];

            int index = 0;

            //Compute regular nodes
            for (uint y = 0 + lY; y < size - rY; y++)
            {
                for (uint x = 0 + lX; x < size - rX; x++)
                {
                    uint indexCRow = ((y + 0) * size) + x;
                    uint indexNRow = ((y + 1) * size) + x;

                    //Tri 1
                    indices[index++] = indexCRow;
                    indices[index++] = indexCRow + 1;
                    indices[index++] = indexNRow;

                    //Tri 2
                    indices[index++] = indexCRow + 1;
                    indices[index++] = indexNRow;
                    indices[index++] = indexNRow + 1;
                }
            }

            //Compute side


            return indices;
        }

        private static uint[] GenerateCorner(IndexBufferShapeEnum bufferShape, int trianglesPerNode)
        {
            int nodes = trianglesPerNode / 2;
            int size = (int)Math.Sqrt(nodes);
            int triangles = ((size - 1) * (size - 1) * 2) + ((size - 1) * 3) + 4;

            uint[] indices = new uint[triangles * 3];


            return indices;
        }
    }

    public class TerrainPatch : IDisposable
    {
        public static TerrainPatch CreatePatch(Game game, LevelOfDetailEnum detail, int trianglesPerNode)
        {
            int triangleCount = 0;

            if (detail == LevelOfDetailEnum.High) triangleCount = trianglesPerNode;
            else if (detail == LevelOfDetailEnum.Medium) triangleCount = trianglesPerNode / 4;
            else if (detail == LevelOfDetailEnum.Low) triangleCount = trianglesPerNode / 16;

            if (triangleCount > 0)
            {
                //Vertices
                int vertices = (int)Math.Pow((Math.Sqrt(triangleCount / 4) + 1), 2);
                int indices = triangleCount * 3;

                VertexPositionNormalTextureTangent[] vertexData = new VertexPositionNormalTextureTangent[vertices];

                return new TerrainPatch()
                {
                    LevelOfDetail = detail,
                    Current = null,
                    VertexBuffer = game.Graphics.Device.CreateVertexBufferWrite(vertexData),
                };
            }
            else
            {
                return new TerrainPatch()
                {
                    LevelOfDetail = detail,
                    Current = null,
                };
            }
        }

        public LevelOfDetailEnum LevelOfDetail = LevelOfDetailEnum.None;
        public QuadTreeNode Current;
        public Buffer VertexBuffer;
        public Dictionary<int, Drawable[]> Drawables = new Dictionary<int, Drawable[]>();

        public TerrainPatch()
        {

        }

        public void Dispose()
        {
            Helper.Dispose(this.VertexBuffer);
            Helper.Dispose(this.Drawables);
        }
    }

    public class TerrainPatchDictionary : Dictionary<LevelOfDetailEnum, TerrainPatch[]>
    {

    }

    public class TerrainPatchAssignationDictionary : Dictionary<LevelOfDetailEnum, Dictionary<int, int>>
    {

    }
}
