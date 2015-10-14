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
            uint[] indices = GenerateDiamond(bufferShape, trianglesPerNode);

            return indices;
        }

        private static uint[] GenerateDiamond(IndexBufferShapeEnum bufferShape, int trianglesPerNode)
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
                    uint indexPRow = ((y - 1) * side) + x;
                    uint indexCRow = ((y + 0) * side) + x;
                    uint indexNRow = ((y + 1) * side) + x;

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
                        indices[index++] = indexPRow - 1;
                        indices[index++] = indexNRow - 1;
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

                VertexPositionNormalTextureTangent[] vertexData = new VertexPositionNormalTextureTangent[vertices];

                return new TerrainPatch(game)
                {
                    LevelOfDetail = detail,
                    Current = null,
                    VertexBuffer = game.Graphics.Device.CreateVertexBufferWrite(vertexData),
                };
            }
            else
            {
                return new TerrainPatch(game)
                {
                    LevelOfDetail = detail,
                    Current = null,
                };
            }
        }

        public readonly Game Game;
        public LevelOfDetailEnum LevelOfDetail = LevelOfDetailEnum.None;
        public QuadTreeNode Current;
        public Buffer VertexBuffer;
        public Dictionary<int, Drawable[]> Drawables = new Dictionary<int, Drawable[]>();

        public TerrainPatch(Game game)
        {
            this.Game = game;
        }

        public void Dispose()
        {
            Helper.Dispose(this.VertexBuffer);
            Helper.Dispose(this.Drawables);
        }

        public void WriteData(VertexPositionNormalTextureTangent[] vertexData)
        {
            this.Game.Graphics.DeviceContext.WriteBuffer(
                this.VertexBuffer,
                vertexData);
        }
    }

    public class TerrainPatchDictionary : Dictionary<LevelOfDetailEnum, TerrainPatch[]>
    {

    }

    public class TerrainPatchAssignationDictionary : Dictionary<LevelOfDetailEnum, Dictionary<int, int>>
    {

    }
}
