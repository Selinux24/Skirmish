using SharpDX;
using System;
using System.Collections.Generic;
using Buffer = SharpDX.Direct3D11.Buffer;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;
    using Engine.Helpers;

    public class Terrain2 : Drawable
    {
        #region Index buffers

        /// <summary>
        /// Index buffer dictionary by level of detail and shape type
        /// </summary>
        class IndexBufferDictionary : Dictionary<LevelOfDetailEnum, Dictionary<IndexBufferShapeEnum, IndexBuffer>>
        {

        }
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        class IndexBuffer
        {
            /// <summary>
            /// Buffer
            /// </summary>
            public Buffer Buffer;
            /// <summary>
            /// Index count in buffer
            /// </summary>
            public int Count;
        }

        #endregion

        #region Terrain patches

        /// <summary>
        /// Terrain patch action enumeration
        /// </summary>
        enum TerrainPatchActionEnum
        {
            /// <summary>
            /// No action
            /// </summary>
            None,
            /// <summary>
            /// Draw patch
            /// </summary>
            Draw,
            /// <summary>
            /// Load data in patch
            /// </summary>
            Load,
        }
        /// <summary>
        /// Terrain patch dictionary by level of detail
        /// </summary>
        class TerrainPatchDictionary : Dictionary<LevelOfDetailEnum, TerrainPatch[]>
        {
            /// <summary>
            /// Game
            /// </summary>
            public readonly Game Game = null;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="game">Game</param>
            public TerrainPatchDictionary(Game game)
                : base()
            {
                this.Game = game;
            }
            
            /// <summary>
            /// Resets all patches state
            /// </summary>
            public void Reset()
            {
                foreach (var item in this.Values)
                {
                    if (item != null && item.Length > 0)
                    {
                        for (int i = 0; i < item.Length; i++)
                        {
                            item[i].Reset();
                        }
                    }
                }
            }
            /// <summary>
            /// Draw patches
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <param name="textures">Textures</param>
            /// <param name="normalMap">Normal map</param>
            public void Draw(DrawContext context, ShaderResourceView textures, ShaderResourceView normalMap)
            {
                EffectTerrain effect = DrawerPool.EffectTerrain;
                if (effect != null)
                {
                    this.Game.Graphics.SetDepthStencilZEnabled();
                    this.Game.Graphics.SetRasterizerDefault();

                    #region Per frame update

                    if (context.DrawerMode == DrawerModesEnum.Forward)
                    {
                        effect.UpdatePerFrame(
                            context.World,
                            context.ViewProjection,
                            context.EyePosition,
                            context.Lights,
                            context.ShadowMap,
                            context.FromLightViewProjection);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        effect.UpdatePerFrame(
                            context.World,
                            context.ViewProjection);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        effect.UpdatePerFrame(
                            context.World,
                            context.ViewProjection);
                    }

                    #endregion

                    #region Per object update

                    if (context.DrawerMode == DrawerModesEnum.Forward)
                    {
                        effect.UpdatePerObject(Material.Default, textures, normalMap);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        effect.UpdatePerObject(Material.Default, textures, normalMap);
                    }

                    #endregion

                    var technique = effect.GetTechnique(VertexTypes.Terrain, DrawingStages.Drawing, context.DrawerMode);

                    this.Game.Graphics.DeviceContext.InputAssembler.InputLayout = effect.GetInputLayout(technique);
                    this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

                    foreach (var lod in this.Keys)
                    {
                        foreach (var item in this[lod])
                        {
                            if (item.Action == TerrainPatchActionEnum.Draw)
                            {
                                item.Draw(context, technique);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Terrain patch
        /// </summary>
        /// <remarks>
        /// Holds the necessary information to render a portion of terrain using an arbitrary level of detail
        /// </remarks>
        class TerrainPatch : IDisposable
        {
            /// <summary>
            /// Creates a new patch of the specified level of detail
            /// </summary>
            /// <typeparam name="T">Vertex type</typeparam>
            /// <param name="game">Game</param>
            /// <param name="lod">Level of detail</param>
            /// <param name="trianglesPerNode">Triangles per node</param>
            /// <returns>Returns the new generated patch</returns>
            public static TerrainPatch CreatePatch<T>(Game game, LevelOfDetailEnum lod, int trianglesPerNode) where T : struct, IVertexData
            {
                int triangleCount = 0;

                if (lod == LevelOfDetailEnum.High) triangleCount = trianglesPerNode;
                else if (lod == LevelOfDetailEnum.Medium) triangleCount = trianglesPerNode / 4;
                else if (lod == LevelOfDetailEnum.Low) triangleCount = trianglesPerNode / 16;

                if (triangleCount > 0)
                {
                    //Vertices
                    int vertices = (int)Math.Pow((Math.Sqrt(triangleCount / 2) + 1), 2);

                    T[] vertexData = new T[vertices];

                    var vertexBuffer = game.Graphics.Device.CreateVertexBufferWrite(vertexData);
                    var vertexBufferBinding = new[]
                {
                    new VertexBufferBinding(vertexBuffer, default(T).Stride, 0),
                };

                    return new TerrainPatch(game)
                    {
                        vertexBuffer = vertexBuffer,
                        vertexBufferBinding = vertexBufferBinding,
                    };
                }
                else
                {
                    return new TerrainPatch(game);
                }
            }

            /// <summary>
            /// Index buffer
            /// </summary>
            private Buffer indexBuffer = null;
            /// <summary>
            /// Index count
            /// </summary>
            private int indexCount = 0;
            /// <summary>
            /// Vertex buffer
            /// </summary>
            private Buffer vertexBuffer = null;
            /// <summary>
            /// Vertex buffer binding
            /// </summary>
            private VertexBufferBinding[] vertexBufferBinding = null;
            /// <summary>
            /// Current quadtree node
            /// </summary>
            private QuadTreeNode current = null;

            /// <summary>
            /// Game
            /// </summary>
            public readonly Game Game = null;
            /// <summary>
            /// Patch action
            /// </summary>
            public TerrainPatchActionEnum Action { get; private set; }
            /// <summary>
            /// Level of detail
            /// </summary>
            public LevelOfDetailEnum LevelOfDetail { get; private set; }

            /// <summary>
            /// Cosntructor
            /// </summary>
            /// <param name="game">Game</param>
            public TerrainPatch(Game game)
            {
                this.Game = game;
            }
            /// <summary>
            /// Releases created resources
            /// </summary>
            public void Dispose()
            {
                Helper.Dispose(this.vertexBuffer);
            }

            /// <summary>
            /// Resets patch
            /// </summary>
            public void Reset()
            {
                this.Action = TerrainPatchActionEnum.None;
            }
            /// <summary>
            /// Sets vertex data
            /// </summary>
            /// <param name="action">Action</param>
            /// <param name="lod">Level of detail</param>
            /// <param name="node">Node to attach to the vertex buffer</param>
            public void SetVertexData(TerrainPatchActionEnum action, LevelOfDetailEnum lod, QuadTreeNode node)
            {
                if (this.vertexBuffer != null)
                {
                    this.Action = action;
                    this.LevelOfDetail = lod;

                    if (this.current != node)
                    {
                        this.current = node;

                        if (this.current != null)
                        {
                            var data = this.current.GetVertexData(VertexTypes.Terrain, lod);

                            VertexData.WriteVertexBuffer(
                                this.Game.Graphics.DeviceContext,
                                this.vertexBuffer,
                                data);
                        }
                    }
                }
                else
                {
                    this.Action = TerrainPatchActionEnum.None;
                }
            }
            /// <summary>
            /// Sets index data
            /// </summary>
            /// <param name="buffer">Index buffer description</param>
            public void SetIndexData(IndexBuffer buffer)
            {
                if (buffer != null)
                {
                    this.indexBuffer = buffer.Buffer;
                    this.indexCount = buffer.Count;
                }
                else
                {
                    this.indexBuffer = null;
                    this.indexCount = 0;
                }
            }
            /// <summary>
            /// Draw the patch
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <param name="technique">Technique</param>
            public void Draw(DrawContext context, EffectTechnique technique)
            {
                //Sets vertex and index buffer
                this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
                this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

                for (int p = 0; p < technique.Description.PassCount; p++)
                {
                    technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                    this.Game.Graphics.DeviceContext.DrawIndexed(this.indexCount, 0, 0);

                    Counters.DrawCallsPerFrame++;
                    Counters.InstancesPerFrame++;
                }
            }

            /// <summary>
            /// Test current patch versus other to find the connection shape
            /// </summary>
            /// <param name="terrainPatch">Other terrain patch</param>
            /// <returns>Returns connection shape</returns>
            public IndexBufferShapeEnum Test(TerrainPatch terrainPatch)
            {
                if (this.current != null && terrainPatch.current != null)
                {
                    if (this.LevelOfDetail == terrainPatch.LevelOfDetail)
                    {
                        return IndexBufferShapeEnum.Full;
                    }
                    else
                    {
                        if (this.current.TopNeighbour == terrainPatch.current) return IndexBufferShapeEnum.SideTop;
                        else if (this.current.BottomNeighbour == terrainPatch.current) return IndexBufferShapeEnum.SideBottom;
                        else if (this.current.LeftNeighbour == terrainPatch.current) return IndexBufferShapeEnum.SideLeft;
                        else if (this.current.RightNeighbour == terrainPatch.current) return IndexBufferShapeEnum.SideRight;
                    }
                }

                return IndexBufferShapeEnum.None;
            }
        }

        #endregion

        /// <summary>
        /// Maximum number of patches in high level of detail
        /// </summary>
        public const int MaxPatchesHighLevel = 4;
        /// <summary>
        /// Maximum number of patches in medium level
        /// </summary>
        public const int MaxPatchesMediumLevel = 12;
        /// <summary>
        /// Maximum number of patches in low level
        /// </summary>
        public const int MaxPatchesLowLevel = 20;
        /// <summary>
        /// Maximum number of patches in minimum level
        /// </summary>
        public const int MaxPatchesMinimumLevel = 28;

        /// <summary>
        /// Height map
        /// </summary>
        private HeightMap heightMap = null;
        /// <summary>
        /// Index dictionary
        /// </summary>
        private IndexBufferDictionary indices = null;
        /// <summary>
        /// Patch dictionary
        /// </summary>
        private TerrainPatchDictionary patches = null;
        /// <summary>
        /// Terrain textures
        /// </summary>
        private ShaderResourceView textures = null;
        /// <summary>
        /// Terrain normal maps
        /// </summary>
        private ShaderResourceView normalMap = null;
        /// <summary>
        /// Quadtree
        /// </summary>
        private QuadTree quadTree = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Terrain description</param>
        public Terrain2(Game game, TerrainDescription description)
            : base(game)
        {
            ImageContent terrainTextures = new ImageContent()
            {
                Streams = ContentManager.FindContent(description.ContentPath, description.Heightmap.Textures),
            };
            ImageContent normalMapTextures = new ImageContent()
            {
                Streams = ContentManager.FindContent(description.ContentPath, description.Heightmap.NormalMap),
            };
            ImageContent heightMapImage = new ImageContent()
            {
                Streams = ContentManager.FindContent(description.ContentPath, description.Heightmap.HeightmapFileName),
            };
            ImageContent colorMapImage = new ImageContent()
            {
                Streams = !string.IsNullOrEmpty(description.Heightmap.ColormapFileName) ?
                    ContentManager.FindContent(description.ContentPath, description.Heightmap.ColormapFileName) :
                    null,
            };

            //Read textures
            this.textures = game.Graphics.Device.LoadTextureArray(terrainTextures.Streams);
            //Read normal map
            this.normalMap = game.Graphics.Device.LoadTexture(normalMapTextures.Stream);
            //Read heightmap
            this.heightMap = HeightMap.FromStream(heightMapImage.Stream, colorMapImage.Stream);

            //Get vertices and indices from heightmap
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

            //Intialize index dictionary
            this.InitializeIndices(description.Quadtree.MaxTrianglesPerNode);

            //Initialize patch dictionary
            this.InitializePatches<VertexTerrain>(description.Quadtree.MaxTrianglesPerNode);

            //Set drawing parameters for renderer
            this.Opaque = true;
            this.DeferredEnabled = true;
        }
        /// <summary>
        /// Release of resources
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.textures);
            Helper.Dispose(this.normalMap);
            Helper.Dispose(this.indices);
            Helper.Dispose(this.patches);
        }
        /// <summary>
        /// Initialize index dictionary
        /// </summary>
        /// <param name="trianglesPerNode">Triangles per node</param>
        private void InitializeIndices(int trianglesPerNode)
        {
            this.indices = new IndexBufferDictionary();

            this.indices.Add(LevelOfDetailEnum.High, new Dictionary<IndexBufferShapeEnum, IndexBuffer>());
            this.indices.Add(LevelOfDetailEnum.Medium, new Dictionary<IndexBufferShapeEnum, IndexBuffer>());
            this.indices.Add(LevelOfDetailEnum.Low, new Dictionary<IndexBufferShapeEnum, IndexBuffer>());

            //High level
            for (int i = 0; i < 9; i++)
            {
                IndexBufferShapeEnum shape = (IndexBufferShapeEnum)i;

                uint[] indexList = GeometryUtil.GenerateIndices(shape, trianglesPerNode);
                IndexBuffer buffer = new IndexBuffer()
                {
                    Buffer = this.Game.Graphics.Device.CreateIndexBufferImmutable(indexList),
                    Count = indexList.Length,
                };
                this.indices[LevelOfDetailEnum.High].Add(shape, buffer);
            }

            //Medium level
            for (int i = 0; i < 9; i++)
            {
                IndexBufferShapeEnum shape = (IndexBufferShapeEnum)i;

                uint[] indexList = GeometryUtil.GenerateIndices(shape, trianglesPerNode / 4);
                IndexBuffer buffer = new IndexBuffer()
                {
                    Buffer = this.Game.Graphics.Device.CreateIndexBufferImmutable(indexList),
                    Count = indexList.Length,
                };
                this.indices[LevelOfDetailEnum.Medium].Add(shape, buffer);
            }

            //Low level
            {
                IndexBufferShapeEnum shape = IndexBufferShapeEnum.Full;

                uint[] indexList = GeometryUtil.GenerateIndices(shape, trianglesPerNode / 4 / 4);
                IndexBuffer buffer = new IndexBuffer()
                {
                    Buffer = this.Game.Graphics.Device.CreateIndexBufferImmutable(indexList),
                    Count = indexList.Length,
                };
                this.indices[LevelOfDetailEnum.Low].Add(shape, buffer);
            }
        }
        /// <summary>
        /// Initialize patch dictionary
        /// </summary>
        /// <typeparam name="T">Vertext type</typeparam>
        /// <param name="trianglesPerNode">Triangles per node</param>
        private void InitializePatches<T>(int trianglesPerNode) where T : struct, IVertexData
        {
            this.patches = new TerrainPatchDictionary(this.Game);

            this.patches.Add(LevelOfDetailEnum.High, new TerrainPatch[MaxPatchesHighLevel]);
            this.patches.Add(LevelOfDetailEnum.Medium, new TerrainPatch[MaxPatchesMediumLevel]);
            this.patches.Add(LevelOfDetailEnum.Low, new TerrainPatch[MaxPatchesLowLevel]);
            this.patches.Add(LevelOfDetailEnum.Minimum, new TerrainPatch[MaxPatchesMinimumLevel]);

            for (int i = 0; i < MaxPatchesHighLevel; i++)
            {
                var patch = TerrainPatch.CreatePatch<T>(this.Game, LevelOfDetailEnum.High, trianglesPerNode);
                this.patches[LevelOfDetailEnum.High][i] = patch;
            }

            for (int i = 0; i < MaxPatchesMediumLevel; i++)
            {
                var patch = TerrainPatch.CreatePatch<T>(this.Game, LevelOfDetailEnum.High, trianglesPerNode);
                this.patches[LevelOfDetailEnum.Medium][i] = patch;
            }

            for (int i = 0; i < MaxPatchesLowLevel; i++)
            {
                var patch = TerrainPatch.CreatePatch<T>(this.Game, LevelOfDetailEnum.High, trianglesPerNode);
                this.patches[LevelOfDetailEnum.Low][i] = patch;
            }

            for (int i = 0; i < MaxPatchesMinimumLevel; i++)
            {
                var patch = TerrainPatch.CreatePatch<T>(this.Game, LevelOfDetailEnum.Minimum, trianglesPerNode);
                this.patches[LevelOfDetailEnum.Minimum][i] = patch;
            }
        }

        /// <summary>
        /// Updates the state of the terrain components
        /// </summary>
        /// <param name="context">Update context</param>
        public override void Update(UpdateContext context)
        {
            QuadTreeNode[] visibleNodes = this.quadTree.GetNodesInVolume(ref context.Frustum);

            if (visibleNodes != null && visibleNodes.Length > 0)
            {
                //Sort by distance to eye position
                Array.Sort(visibleNodes, (n1, n2) =>
                {
                    float d1 = Vector3.DistanceSquared(n1.Center, context.EyePosition);
                    float d2 = Vector3.DistanceSquared(n2.Center, context.EyePosition);

                    return d1.CompareTo(d2);
                });

                //Assign level of detail by order
                int patchesHighLevel = 0;
                int patchesMediumLevel = 0;
                int patchesLowLevel = 0;
                int patchesDataLoadLevel = 0;

                //Reset actions
                this.patches.Reset();

                TerrainPatch[] patchList = new TerrainPatch[visibleNodes.Length];

                //Assign level of detail bases in distance to eye position
                //TODO: validate distance too...
                for (int i = 0; i < visibleNodes.Length; i++)
                {
                    if (patchesHighLevel < MaxPatchesHighLevel)
                    {
                        patchList[i] = this.patches[LevelOfDetailEnum.High][patchesHighLevel];
                        patchList[i].SetVertexData(
                            TerrainPatchActionEnum.Draw,
                            LevelOfDetailEnum.High,
                            visibleNodes[i]);

                        patchesHighLevel++;
                    }
                    else if (patchesMediumLevel < MaxPatchesMediumLevel)
                    {
                        patchList[i] = this.patches[LevelOfDetailEnum.Medium][patchesMediumLevel];
                        patchList[i].SetVertexData(
                            TerrainPatchActionEnum.Draw,
                            LevelOfDetailEnum.Medium,
                            visibleNodes[i]);

                        patchesMediumLevel++;
                    }
                    else if (patchesLowLevel < MaxPatchesLowLevel)
                    {
                        patchList[i] = this.patches[LevelOfDetailEnum.Low][patchesLowLevel];
                        patchList[i].SetVertexData(
                            TerrainPatchActionEnum.Draw,
                            LevelOfDetailEnum.Low,
                            visibleNodes[i]);

                        patchesLowLevel++;
                    }
                    else if (patchesDataLoadLevel < MaxPatchesMinimumLevel)
                    {
                        patchList[i] = this.patches[LevelOfDetailEnum.Minimum][patchesDataLoadLevel];
                        patchList[i].SetVertexData(
                            TerrainPatchActionEnum.Load,
                            LevelOfDetailEnum.Minimum,
                            visibleNodes[i]);

                        patchesDataLoadLevel++;
                    }
                    else
                    {
                        visibleNodes[i].Cull = true;
                    }
                }

                for (int a = 0; a < patchList.Length; a++)
                {
                    if (patchList[a].LevelOfDetail != LevelOfDetailEnum.None)
                    {
                        IndexBufferShapeEnum t0 = IndexBufferShapeEnum.Full;

                        for (int b = a + 1; b < patchList.Length; b++)
                        {
                            IndexBufferShapeEnum t1 = patchList[a].Test(patchList[b]);
                            if (t1 != IndexBufferShapeEnum.None)
                            {
                                if (t0 == IndexBufferShapeEnum.Full)
                                {
                                    t0 = t1;
                                }
                                else
                                {
                                    if (t0 == IndexBufferShapeEnum.SideTop && t1 == IndexBufferShapeEnum.SideLeft)
                                    {
                                        t0 = IndexBufferShapeEnum.CornerTopLeft;
                                        break;
                                    }
                                    else if (t0 == IndexBufferShapeEnum.SideTop && t1 == IndexBufferShapeEnum.SideRight)
                                    {
                                        t0 = IndexBufferShapeEnum.CornerTopRight;
                                        break;
                                    }
                                    else if (t0 == IndexBufferShapeEnum.SideBottom && t1 == IndexBufferShapeEnum.SideLeft)
                                    {
                                        t0 = IndexBufferShapeEnum.CornerBottomLeft;
                                        break;
                                    }
                                    else if (t0 == IndexBufferShapeEnum.SideBottom && t1 == IndexBufferShapeEnum.SideRight)
                                    {
                                        t0 = IndexBufferShapeEnum.CornerBottomRight;
                                        break;
                                    }
                                    else if (t0 == IndexBufferShapeEnum.SideLeft && t1 == IndexBufferShapeEnum.SideTop)
                                    {
                                        t0 = IndexBufferShapeEnum.CornerTopLeft;
                                        break;
                                    }
                                    else if (t0 == IndexBufferShapeEnum.SideLeft && t1 == IndexBufferShapeEnum.SideBottom)
                                    {
                                        t0 = IndexBufferShapeEnum.CornerBottomLeft;
                                        break;
                                    }
                                    else if (t0 == IndexBufferShapeEnum.SideRight && t1 == IndexBufferShapeEnum.SideTop)
                                    {
                                        t0 = IndexBufferShapeEnum.CornerTopRight;
                                        break;
                                    }
                                    else if (t0 == IndexBufferShapeEnum.SideRight && t1 == IndexBufferShapeEnum.SideBottom)
                                    {
                                        t0 = IndexBufferShapeEnum.CornerBottomRight;
                                        break;
                                    }
                                    else
                                    {
                                        t0 = t1;
                                    }
                                }
                            }
                        }

                        if (t0 != IndexBufferShapeEnum.None)
                        {
                            patchList[a].SetIndexData(this.indices[patchList[a].LevelOfDetail][t0]);
                        }
                        else
                        {
                            patchList[a].SetIndexData(null);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Draws the terrain components
        /// </summary>
        /// <param name="context">Draw context</param>
        public override void Draw(DrawContext context)
        {
            this.patches.Draw(context, this.textures, this.normalMap);
        }

        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="position">Ground position if exists</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindTopGroundPosition(float x, float z, out Vector3 position)
        {
            Triangle tri;
            return FindTopGroundPosition(x, z, out position, out tri);
        }
        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindTopGroundPosition(float x, float z, out Vector3 position, out Triangle triangle)
        {
            BoundingBox bbox = this.GetBoundingBox();

            Ray ray = new Ray()
            {
                Position = new Vector3(x, bbox.Maximum.Y + 0.1f, z),
                Direction = Vector3.Down,
            };

            return this.PickNearest(ref ray, out position, out triangle);
        }
        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="position">Ground position if exists</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindFirstGroundPosition(float x, float z, out Vector3 position)
        {
            Triangle tri;
            return FindFirstGroundPosition(x, z, out position, out tri);
        }
        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindFirstGroundPosition(float x, float z, out Vector3 position, out Triangle triangle)
        {
            BoundingBox bbox = this.GetBoundingBox();

            Ray ray = new Ray()
            {
                Position = new Vector3(x, bbox.Maximum.Y + 0.1f, z),
                Direction = Vector3.Down,
            };

            return this.PickFirst(ref ray, out position, out triangle);
        }
        /// <summary>
        /// Gets ground positions giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="positions">Ground positions if exists</param>
        /// <returns>Returns true if ground positions found</returns>
        public bool FindAllGroundPosition(float x, float z, out Vector3[] positions)
        {
            Triangle[] triangles;
            return FindAllGroundPosition(x, z, out positions, out triangles);
        }
        /// <summary>
        /// Gets all ground positions giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="positions">Ground positions if exists</param>
        /// <param name="triangles">Triangles found</param>
        /// <returns>Returns true if ground positions found</returns>
        public bool FindAllGroundPosition(float x, float z, out Vector3[] positions, out Triangle[] triangles)
        {
            BoundingBox bbox = this.GetBoundingBox();

            Ray ray = new Ray()
            {
                Position = new Vector3(x, bbox.Maximum.Y + 0.01f, z),
                Direction = Vector3.Down,
            };

            return this.PickAll(ref ray, out positions, out triangles);
        }
        /// <summary>
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="position">Picked position if exists</param>
        /// <param name="triangle">Picked triangle if exists</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickNearest(ref Ray ray, out Vector3 position, out Triangle triangle)
        {
            return this.quadTree.PickNearest(ref ray, out position, out triangle);
        }
        /// <summary>
        /// Pick first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="position">Picked position if exists</param>
        /// <param name="triangle">Picked triangle if exists</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickFirst(ref Ray ray, out Vector3 position, out Triangle triangle)
        {
            return this.quadTree.PickFirst(ref ray, out position, out triangle);
        }
        /// <summary>
        /// Pick all positions
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="positions">Picked positions if exists</param>
        /// <param name="triangles">Picked triangles if exists</param>
        /// <returns>Returns true if picked positions found</returns>
        public bool PickAll(ref Ray ray, out Vector3[] positions, out Triangle[] triangles)
        {
            return this.quadTree.PickAll(ref ray, out positions, out triangles);
        }

        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public BoundingSphere GetBoundingSphere()
        {
            return this.quadTree.BoundingSphere;
        }
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        public BoundingBox GetBoundingBox()
        {
            return this.quadTree.BoundingBox;
        }
    }
}
