using System.Collections.Generic;
using System.IO;
using System;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX;
using SharpDX.Direct3D;

namespace Engine
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Content;

    public class Terrain2 : Drawable
    {
        public const int MaxPatchesHighLevel = 4;
        public const int MaxPatchesMediumLevel = 6;
        public const int MaxPatchesLowLevel = 8;
        public const int MaxPatchesDataLoadLevel = 10;

        private HeightMap heightMap = null;
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

            this.heightMap = HeightMap.FromStream(heightMapImage.Stream);

            VertexData[] vertices;
            uint[] indices;
            this.heightMap.BuildGeometry(
                description.Heightmap.CellSize,
                description.Heightmap.MaximumHeight,
                out vertices, out indices);

            //Initialize Quadtree
            this.quadTree = QuadTree.Build(game, vertices, indices, description);

            //Initialize patches
            this.InitializePatches(description.Quadtree.MaxTrianglesPerNode);
        }

        public override void Dispose()
        {
            Helper.Dispose(this.patches);
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
                uint[] indexData = new uint[indices];

                return new TerrainPatch()
                {
                    LevelOfDetail = detail,
                    Current = null,
                    VertexBuffer = game.Graphics.Device.CreateVertexBufferWrite(vertexData),
                    IndexBuffer = game.Graphics.Device.CreateIndexBufferImmutable(indexData),
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
        public Buffer IndexBuffer;
        public Dictionary<int, Drawable[]> Drawables = new Dictionary<int, Drawable[]>();

        public TerrainPatch()
        {

        }

        public void Dispose()
        {
            Helper.Dispose(this.VertexBuffer);
            Helper.Dispose(this.IndexBuffer);
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
