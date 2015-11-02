using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
        #region Update and Draw context for terrain

        /// <summary>
        /// Update context
        /// </summary>
        public class TerrainUpdateContext
        {
            /// <summary>
            /// General update context
            /// </summary>
            public UpdateContext BaseContext;
            /// <summary>
            /// Visible Nodes
            /// </summary>
            public QuadTreeNode[] VisibleNodes;
            /// <summary>
            /// Folliage generation description
            /// </summary>
            public TerrainDescription.VegetationDescription FolliageDescription;
        }

        /// <summary>
        /// Draw context
        /// </summary>
        public class TerrainDrawContext
        {
            /// <summary>
            /// General draw context
            /// </summary>
            public DrawContext BaseContext;
            /// <summary>
            /// Low resolution textures for terrain
            /// </summary>
            public ShaderResourceView TerraintexturesLR;
            /// <summary>
            /// High resolution textures for terrain
            /// </summary>
            public ShaderResourceView TerraintexturesHR;
            /// <summary>
            /// Normal map textures for terrain
            /// </summary>
            public ShaderResourceView TerrainNormalMaps;
            /// <summary>
            /// Slope ranges
            /// </summary>
            public Vector2 SlopeRanges;
            /// <summary>
            /// Folliage textures
            /// </summary>
            public ShaderResourceView FolliageTextures;
            /// <summary>
            /// Folliage texture count
            /// </summary>
            public uint FolliageTextureCount;
            /// <summary>
            /// Folliage end radius
            /// </summary>
            public float FolliageEndRadius;
        }

        #endregion

        #region Terrain patches

        /// <summary>
        /// Terrain patch dictionary by level of detail
        /// </summary>
        class TerrainPatchDictionary : IDisposable
        {
            /// <summary>
            /// Maximum number of patches in high level of detail
            /// </summary>
            public const int MaxPatchesHighLevel = 6;
            /// <summary>
            /// Maximum number of patches in medium level
            /// </summary>
            public const int MaxPatchesMediumLevel = 7;
            /// <summary>
            /// Maximum number of patches in low level
            /// </summary>
            public const int MaxPatchesLowLevel = 20;
            /// <summary>
            /// Maximum number of patches in minimum level
            /// </summary>
            public const int MaxPatchesMinimumLevel = 13;

            /// <summary>
            /// Game
            /// </summary>
            private Game game = null;
            /// <summary>
            /// Patches dictionary
            /// </summary>
            private Dictionary<LevelOfDetailEnum, TerrainPatch[]> patches = null;
            /// <summary>
            /// Index dictionary
            /// </summary>
            private Dictionary<LevelOfDetailEnum, Dictionary<IndexBufferShapeEnum, IndexBuffer>> indices = null;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="game">Game</param>
            /// <param name="trianglesPerNode">Triangles per node</param>
            public TerrainPatchDictionary(Game game, int trianglesPerNode)
                : base()
            {
                this.game = game;

                this.InitializePatches(trianglesPerNode);

                this.InitializeIndices(trianglesPerNode);
            }
            /// <summary>
            /// Resource disposal
            /// </summary>
            public void Dispose()
            {
                Helper.Dispose(this.patches);
                Helper.Dispose(this.indices);
            }

            /// <summary>
            /// Initialize patch dictionary
            /// </summary>
            /// <param name="trianglesPerNode">Triangles per node</param>
            private void InitializePatches(int trianglesPerNode)
            {
                this.patches = new Dictionary<LevelOfDetailEnum, TerrainPatch[]>();

                this.patches.Add(LevelOfDetailEnum.High, new TerrainPatch[MaxPatchesHighLevel]);
                this.patches.Add(LevelOfDetailEnum.Medium, new TerrainPatch[MaxPatchesMediumLevel]);
                this.patches.Add(LevelOfDetailEnum.Low, new TerrainPatch[MaxPatchesLowLevel]);
                this.patches.Add(LevelOfDetailEnum.Minimum, new TerrainPatch[MaxPatchesMinimumLevel]);

                for (int i = 0; i < MaxPatchesHighLevel; i++)
                {
                    var patch = TerrainPatch.CreatePatch(this.game, LevelOfDetailEnum.High, trianglesPerNode);
                    this.patches[LevelOfDetailEnum.High][i] = patch;
                }

                for (int i = 0; i < MaxPatchesMediumLevel; i++)
                {
                    var patch = TerrainPatch.CreatePatch(this.game, LevelOfDetailEnum.Medium, trianglesPerNode);
                    this.patches[LevelOfDetailEnum.Medium][i] = patch;
                }

                for (int i = 0; i < MaxPatchesLowLevel; i++)
                {
                    var patch = TerrainPatch.CreatePatch(this.game, LevelOfDetailEnum.Low, trianglesPerNode);
                    this.patches[LevelOfDetailEnum.Low][i] = patch;
                }

                for (int i = 0; i < MaxPatchesMinimumLevel; i++)
                {
                    var patch = TerrainPatch.CreatePatch(this.game, LevelOfDetailEnum.Minimum, trianglesPerNode);
                    this.patches[LevelOfDetailEnum.Minimum][i] = patch;
                }
            }
            /// <summary>
            /// Initialize index dictionary
            /// </summary>
            /// <param name="trianglesPerNode">Triangles per node</param>
            private void InitializeIndices(int trianglesPerNode)
            {
                this.indices = new Dictionary<LevelOfDetailEnum, Dictionary<IndexBufferShapeEnum, IndexBuffer>>();

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
                        Buffer = this.game.Graphics.Device.CreateIndexBufferImmutable(indexList),
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
                        Buffer = this.game.Graphics.Device.CreateIndexBufferImmutable(indexList),
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
                        Buffer = this.game.Graphics.Device.CreateIndexBufferImmutable(indexList),
                        Count = indexList.Length,
                    };
                    this.indices[LevelOfDetailEnum.Low].Add(shape, buffer);
                }
            }

            /// <summary>
            /// Updates dictionary
            /// </summary>
            /// <param name="context">Update context</param>
            public void Update(TerrainUpdateContext context)
            {
                var visibleNodes = context.VisibleNodes;

                if (visibleNodes != null && visibleNodes.Length > 0)
                {
                    //Reset visibility for all patches
                    foreach (var patch in this.patches.Values)
                    {
                        for (int i = 0; i < patch.Length; i++)
                        {
                            patch[i].Visible = false;
                        }
                    }

                    Vector3 eyePosition = context.BaseContext.EyePosition;

                    //Sort by distance to eye position
                    Array.Sort(visibleNodes, (n1, n2) =>
                    {
                        float d1 = Vector3.DistanceSquared(n1.Center, eyePosition);
                        float d2 = Vector3.DistanceSquared(n2.Center, eyePosition);

                        return d1.CompareTo(d2);
                    });

                    //Assign level of detail by order
                    int patchesHighLevel = 0;
                    int patchesMediumLevel = 0;
                    int patchesLowLevel = 0;
                    int patchesDataLoadLevel = 0;

                    TerrainPatch[] patchList = new TerrainPatch[visibleNodes.Length];

                    //Assign level of detail bases in distance to eye position
                    //TODO: Prefer current node assignation to prevent folliage reloading in lod change
                    for (int i = 0; i < visibleNodes.Length; i++)
                    {
                        var current = visibleNodes[i];

                        float dist = Vector3.Distance(eyePosition, current.Center);

                        if (patchesHighLevel < MaxPatchesHighLevel)
                        {
                            patchList[i] = this.patches[LevelOfDetailEnum.High][patchesHighLevel++];
                            patchList[i].SetVertexData(current);
                            patchList[i].Visible = true;
                        }
                        else if (patchesMediumLevel < MaxPatchesMediumLevel)
                        {
                            patchList[i] = this.patches[LevelOfDetailEnum.Medium][patchesMediumLevel++];
                            patchList[i].SetVertexData(current);
                            patchList[i].Visible = true;
                        }
                        else if (patchesLowLevel < MaxPatchesLowLevel)
                        {
                            patchList[i] = this.patches[LevelOfDetailEnum.Low][patchesLowLevel++];
                            patchList[i].SetVertexData(current);
                            patchList[i].Visible = true;
                        }
                        else if (patchesDataLoadLevel < MaxPatchesMinimumLevel)
                        {
                            patchList[i] = this.patches[LevelOfDetailEnum.Minimum][patchesDataLoadLevel++];
                            patchList[i].SetVertexData(current);
                        }
                    }

                    //var patch = this.patches.FindPatch(LevelOfDetailEnum.High, current);
                    //if (patch != null)
                    //{
                    //    patchList[i] = patch;
                    //}
                    //else
                    //{
                    //    patchList[i] = this.patches[LevelOfDetailEnum.High][patchesHighLevel];
                    //}

                    int assignedPatches = patchesHighLevel + patchesMediumLevel + patchesLowLevel + patchesDataLoadLevel;
                    if (assignedPatches > 0)
                    {
                        for (int a = 0; a < assignedPatches; a++)
                        {
                            if (patchList[a].LevelOfDetail != LevelOfDetailEnum.None &&
                                patchList[a].LevelOfDetail != LevelOfDetailEnum.Minimum)
                            {
                                IndexBufferShapeEnum t0 = IndexBufferShapeEnum.Full;

                                for (int b = a + 1; b < assignedPatches; b++)
                                {
                                    if (patchList[b].LevelOfDetail != LevelOfDetailEnum.None &&
                                        patchList[b].LevelOfDetail != LevelOfDetailEnum.Minimum)
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
                                }

                                if (t0 != IndexBufferShapeEnum.None)
                                {
                                    patchList[a].SetIndexData(this.indices[patchList[a].LevelOfDetail][t0]);
                                }
                                else
                                {
                                    patchList[a].SetIndexData(null);
                                }

                                if (context.FolliageDescription != null)
                                {
                                    if (patchList[a].LevelOfDetail == LevelOfDetailEnum.High ||
                                        patchList[a].LevelOfDetail == LevelOfDetailEnum.Medium)
                                    {
                                        patchList[a].Plant(context.FolliageDescription);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            /// <summary>
            /// Draw patches
            /// </summary>
            /// <param name="context">Drawing context</param>
            public void Draw(TerrainDrawContext context)
            {
                this.game.Graphics.SetDepthStencilZEnabled();

                {
                    EffectTerrain effect = DrawerPool.EffectTerrain;

                    this.game.Graphics.SetBlendDefault();

                    #region Per frame update

                    if (context.BaseContext.DrawerMode == DrawerModesEnum.Forward)
                    {
                        effect.UpdatePerFrame(
                            context.BaseContext.World,
                            context.BaseContext.ViewProjection,
                            context.BaseContext.EyePosition,
                            context.BaseContext.Lights,
                            context.BaseContext.ShadowMap,
                            context.BaseContext.FromLightViewProjection);
                    }
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        effect.UpdatePerFrame(
                            context.BaseContext.World,
                            context.BaseContext.ViewProjection);
                    }
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        effect.UpdatePerFrame(
                            context.BaseContext.World,
                            context.BaseContext.ViewProjection);
                    }

                    #endregion

                    #region Per object update

                    if (context.BaseContext.DrawerMode == DrawerModesEnum.Forward)
                    {
                        effect.UpdatePerObject(
                            Material.Default, 
                            context.TerraintexturesLR, 
                            context.TerraintexturesHR, 
                            context.TerrainNormalMaps,
                            context.SlopeRanges);
                    }
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        effect.UpdatePerObject(
                            Material.Default, 
                            context.TerraintexturesLR, 
                            context.TerraintexturesHR, 
                            context.TerrainNormalMaps,
                            context.SlopeRanges);
                    }

                    #endregion

                    var technique = effect.GetTechnique(VertexTypes.Terrain, DrawingStages.Drawing, context.BaseContext.DrawerMode);

                    this.game.Graphics.DeviceContext.InputAssembler.InputLayout = effect.GetInputLayout(technique);
                    this.game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

                    foreach (var lod in this.patches.Keys)
                    {
                        foreach (var item in this.patches[lod])
                        {
                            if (item.Visible)
                            {
                                item.DrawTerrain(context.BaseContext, technique);
                            }
                        }
                    }
                }

                {
                    EffectBillboard effect = DrawerPool.EffectBillboard;

                    if (context.BaseContext.DrawerMode == DrawerModesEnum.Forward)
                    {
                        this.game.Graphics.SetBlendTransparent();
                    }
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        this.game.Graphics.SetBlendDeferredComposerTransparent();
                    }
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        this.game.Graphics.SetBlendTransparent();
                    }

                    #region Per frame update

                    if (context.BaseContext.DrawerMode == DrawerModesEnum.Forward)
                    {
                        effect.UpdatePerFrame(
                            context.BaseContext.World,
                            context.BaseContext.ViewProjection,
                            context.BaseContext.EyePosition,
                            context.BaseContext.Lights,
                            context.BaseContext.ShadowMap,
                            context.BaseContext.FromLightViewProjection);
                    }
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        effect.UpdatePerFrame(
                            context.BaseContext.World,
                            context.BaseContext.ViewProjection,
                            context.BaseContext.EyePosition,
                            context.BaseContext.Lights,
                            context.BaseContext.ShadowMap,
                            context.BaseContext.FromLightViewProjection);
                    }
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        effect.UpdatePerFrame(
                            context.BaseContext.World,
                            context.BaseContext.ViewProjection,
                            context.BaseContext.EyePosition);
                    }

                    #endregion

                    #region Per object update

                    if (context.BaseContext.DrawerMode == DrawerModesEnum.Forward)
                    {
                        effect.UpdatePerObject(Material.Default, 0, context.FolliageEndRadius, context.FolliageTextureCount, context.FolliageTextures);
                    }
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        effect.UpdatePerObject(Material.Default, 0, context.FolliageEndRadius, context.FolliageTextureCount, context.FolliageTextures);
                    }
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        effect.UpdatePerObject(Material.Default, 0, context.FolliageEndRadius, context.FolliageTextureCount, context.FolliageTextures);
                    }

                    #endregion

                    var technique = effect.GetTechnique(VertexTypes.Billboard, DrawingStages.Drawing, context.BaseContext.DrawerMode);

                    this.game.Graphics.DeviceContext.InputAssembler.InputLayout = effect.GetInputLayout(technique);
                    this.game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.PointList;

                    foreach (var lod in this.patches.Keys)
                    {
                        foreach (var item in this.patches[lod])
                        {
                            if (item.Visible)
                            {
                                item.DrawFolliage(context.BaseContext, technique);
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
            const int MAX = 1024 * 10;

            /// <summary>
            /// Creates a new patch of the specified level of detail
            /// </summary>
            /// <typeparam name="T">Terrain vertext type</typeparam>
            /// <typeparam name="F">Folliage vertex type</typeparam>
            /// <param name="game">Game</param>
            /// <param name="lod">Level of detail</param>
            /// <param name="trianglesPerNode">Triangles per node</param>
            /// <returns>Returns the new generated patch</returns>
            public static TerrainPatch CreatePatch(Game game, LevelOfDetailEnum lod, int trianglesPerNode)
            {
                int triangleCount = 0;

                if (lod == LevelOfDetailEnum.High) triangleCount = trianglesPerNode;
                else if (lod == LevelOfDetailEnum.Medium) triangleCount = trianglesPerNode / 4;
                else if (lod == LevelOfDetailEnum.Low) triangleCount = trianglesPerNode / 16;

                if (triangleCount > 0)
                {
                    var patch = new TerrainPatch(game, lod);

                    //Terrain buffer
                    {
                        int vertices = (int)Math.Pow((Math.Sqrt(triangleCount / 2) + 1), 2);

                        VertexTerrain[] vertexData = new VertexTerrain[vertices];

                        patch.vertexBuffer = game.Graphics.Device.CreateVertexBufferWrite(vertexData);
                        patch.vertexBufferBinding = new[]
                        {
                            new VertexBufferBinding(patch.vertexBuffer, default(VertexTerrain).Stride, 0),
                        };
                    }

                    //Folliage buffer
                    {
                        VertexBillboard[] vertexData = new VertexBillboard[MAX];

                        patch.folliageBuffer = game.Graphics.Device.CreateVertexBufferWrite(vertexData);
                        patch.folliageBufferBinding = new[]
                        {
                            new VertexBufferBinding(patch.folliageBuffer, default(VertexBillboard).Stride, 0),
                        };
                    }

                    return patch;
                }
                else
                {
                    return new TerrainPatch(game, lod);
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
            /// Vertex buffer with folliage data
            /// </summary>
            private Buffer folliageBuffer = null;
            /// <summary>
            /// Folliage positions
            /// </summary>
            private int folliageCount = 0;
            /// <summary>
            /// Vertex buffer binding for folliage buffer
            /// </summary>
            private VertexBufferBinding[] folliageBufferBinding = null;
            /// <summary>
            /// Current quadtree node
            /// </summary>
            private QuadTreeNode current = null;
            /// <summary>
            /// Folliage populated flag
            /// </summary>
            private bool folliagePlanted = false;
            /// <summary>
            /// Folliage populating flag
            /// </summary>
            private bool folliagePlanting = false;
            /// <summary>
            /// Folliage attached to buffer flag
            /// </summary>
            private bool folliageAttached = false;
            /// <summary>
            /// Folliage generated data
            /// </summary>
            private IVertexData[] folliageData = null;

            /// <summary>
            /// Game
            /// </summary>
            public readonly Game Game = null;
            /// <summary>
            /// Level of detail
            /// </summary>
            public LevelOfDetailEnum LevelOfDetail { get; private set; }
            /// <summary>
            /// Gets or sets if patch is visible
            /// </summary>
            public bool Visible { get; set; }

            /// <summary>
            /// Cosntructor
            /// </summary>
            /// <param name="game">Game</param>
            /// <param name="lod">Level of detail</param>
            public TerrainPatch(Game game, LevelOfDetailEnum lod)
            {
                this.Game = game;
                this.LevelOfDetail = lod;
            }
            /// <summary>
            /// Releases created resources
            /// </summary>
            public void Dispose()
            {
                Helper.Dispose(this.vertexBuffer);
            }

            /// <summary>
            /// Sets vertex data
            /// </summary>
            /// <param name="node">Node to attach to the vertex buffer</param>
            public void SetVertexData(QuadTreeNode node)
            {
                if (this.vertexBuffer != null)
                {
                    if (this.current != node)
                    {
                        this.folliagePlanting = false;
                        this.folliageAttached = false;
                        this.folliagePlanted = false;
                        this.folliageData = null;
                        this.folliageCount = 0;

                        this.current = node;

                        if (this.current != null)
                        {
                            var data = this.current.GetVertexData(VertexTypes.Terrain, this.LevelOfDetail);

                            if (this.LevelOfDetail == LevelOfDetailEnum.High)
                            {
                                Array.ForEach(data, d => d.SetChannelValue(VertexDataChannels.Color, new Color4(0.5f, 0.5f, 1f, 1f)));
                            }
                            else if (this.LevelOfDetail == LevelOfDetailEnum.Medium)
                            {
                                Array.ForEach(data, d => d.SetChannelValue(VertexDataChannels.Color, new Color4(0.5f, 1f, 0.5f, 1f)));
                            }
                            else if (this.LevelOfDetail == LevelOfDetailEnum.Low)
                            {
                                Array.ForEach(data, d => d.SetChannelValue(VertexDataChannels.Color, new Color4(1f, 0.5f, 0.5f, 1f)));
                            }

                            VertexData.WriteVertexBuffer(
                                this.Game.Graphics.DeviceContext,
                                this.vertexBuffer,
                                data);
                        }
                    }
                }
                else
                {

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
            /// Draw the patch terrain
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <param name="technique">Technique</param>
            public void DrawTerrain(DrawContext context, EffectTechnique technique)
            {
                if (this.indexCount > 0)
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
            }
            /// <summary>
            /// Draw the patch folliage
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <param name="technique">Technique</param>
            public void DrawFolliage(DrawContext context, EffectTechnique technique)
            {
                if (this.folliageCount > 0)
                {
                    //Sets vertex and index buffer
                    this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, this.folliageBufferBinding);
                    this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(null, SharpDX.DXGI.Format.R32_UInt, 0);

                    for (int p = 0; p < technique.Description.PassCount; p++)
                    {
                        technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                        this.Game.Graphics.DeviceContext.Draw(this.folliageCount, 0);

                        Counters.DrawCallsPerFrame++;
                        Counters.InstancesPerFrame++;
                    }
                }
            }

            /// <summary>
            /// Test current patch versus other to find the connection shape
            /// </summary>
            /// <param name="terrainPatch">Other terrain patch</param>
            /// <returns>Returns connection shape</returns>
            public IndexBufferShapeEnum Test(TerrainPatch terrainPatch)
            {
                if (this.current != null && terrainPatch != null && terrainPatch.current != null)
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
            /// <summary>
            /// Launchs folliage population asynchronous task
            /// </summary>
            /// <param name="description">Terrain vegetation description</param>
            public void Plant(TerrainDescription.VegetationDescription description)
            {
                if (this.current != null)
                {
                    if (!this.folliagePlanted)
                    {
                        if (!this.folliagePlanting)
                        {
                            //Start planting task
                            this.folliagePlanting = true;

                            Task<VertexData[]> t = Task.Factory.StartNew<VertexData[]>(() => PlantTask(this.current, description));

                            t.ContinueWith(task => PlantThreadCompleted(task.Result));
                        }
                    }
                    else if (!this.folliageAttached)
                    {
                        if (this.folliageData != null && this.folliageData.Length > 0)
                        {
                            //Attach data
                            VertexData.WriteVertexBuffer(
                                this.Game.Graphics.DeviceContext,
                                this.folliageBuffer,
                                this.folliageData);
                        }

                        this.folliageAttached = true;
                    }
                }
            }

            /// <summary>
            /// Asynchronous planting task
            /// </summary>
            /// <param name="node">Node to process</param>
            /// <param name="description">Vegetation task</param>
            /// <returns>Returns generated vertex data</returns>
            private static VertexData[] PlantTask(QuadTreeNode node, TerrainDescription.VegetationDescription description)
            {
                List<VertexData> vertexData = new List<VertexData>(MAX);

                var triangles = node.Triangles;
                if (triangles != null && triangles.Length > 0)
                {
                    Random rnd = new Random(description.Seed);
                    BoundingBox bbox = node.BoundingBox;
                    float density = description.Saturation;
                    int count = 0;

                    for (int i = 0; i < triangles.Length; i++)
                    {
                        var tri = triangles[i];
                        BoundingBox tribox = BoundingBox.FromPoints(tri.GetCorners());

                        float triCount = 0;
                        float maxCount = tri.Area * density * (tri.Normal.Y);

                        while (triCount < maxCount && count < MAX)
                        {
                            Vector3 pos = new Vector3(
                                rnd.NextFloat(tribox.Minimum.X, tribox.Maximum.X),
                                bbox.Maximum.Y + 1f,
                                rnd.NextFloat(tribox.Minimum.Z, tribox.Maximum.Z));

                            Ray ray = new Ray(pos, Vector3.Down);

                            Vector3 intersectionPoint;
                            if (tri.Intersects(ref ray, out intersectionPoint))
                            {
                                vertexData.Add(new VertexData()
                                {
                                    Position = intersectionPoint,
                                    Size = new Vector2(
                                        rnd.NextFloat(description.MinSize.X, description.MaxSize.X),
                                        rnd.NextFloat(description.MinSize.Y, description.MaxSize.Y)),
                                });

                                triCount++;
                                count++;
                            }
                        }

                        if (count >= MAX)
                        {
                            break;
                        }
                    }
                }

                return vertexData.ToArray();
            }
            /// <summary>
            /// Planting task completed
            /// </summary>
            /// <param name="vData">Vertex data generated in asynchronous task</param>
            private void PlantThreadCompleted(VertexData[] vData)
            {
                this.folliageCount = vData.Length;
                this.folliageData = VertexData.Convert(VertexTypes.Billboard, vData, null, null, Matrix.Identity);
                this.folliagePlanting = false;
                this.folliagePlanted = true;
                this.folliageAttached = false;
            }
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

        /// <summary>
        /// Height map
        /// </summary>
        private HeightMap heightMap = null;
        /// <summary>
        /// Quadtree
        /// </summary>
        private QuadTree quadTree = null;
        /// <summary>
        /// Patch dictionary
        /// </summary>
        private TerrainPatchDictionary patches = null;
        /// <summary>
        /// Terrain update context
        /// </summary>
        private TerrainUpdateContext updateContext = null;
        /// <summary>
        /// Terrain draw context
        /// </summary>
        private TerrainDrawContext drawContext = null;
        /// <summary>
        /// Terrain low res textures
        /// </summary>
        private ShaderResourceView terrainTexturesLR = null;
        /// <summary>
        /// Terrain high res textures
        /// </summary>
        private ShaderResourceView terrainTexturesHR = null;
        /// <summary>
        /// Terrain normal maps
        /// </summary>
        private ShaderResourceView terrainNormalMaps = null;
        /// <summary>
        /// Slope ranges
        /// </summary>
        private Vector2 slopeRanges = Vector2.Zero;
        /// <summary>
        /// Folliage textures
        /// </summary>
        private ShaderResourceView folliageTextures = null;
        /// <summary>
        /// Folliage texture count
        /// </summary>
        private uint folliageTextureCount = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Terrain description</param>
        public Terrain2(Game game, TerrainDescription description)
            : base(game)
        {
            #region Read heightmap

            if (description.Heightmap != null)
            {
                string contentPath = Path.Combine(description.ContentPath, description.Heightmap.ContentPath);

                ImageContent heightMapImage = new ImageContent()
                {
                    Streams = ContentManager.FindContent(contentPath, description.Heightmap.HeightmapFileName),
                };
                ImageContent colorMapImage = new ImageContent()
                {
                    Streams = ContentManager.FindContent(contentPath, description.Heightmap.ColormapFileName),
                };

                this.heightMap = HeightMap.FromStream(heightMapImage.Stream, colorMapImage.Stream);
            }

            #endregion

            #region Read terrain textures

            if (description.Textures != null)
            {
                string contentPath = Path.Combine(description.ContentPath, description.Textures.ContentPath);

                ImageContent terrainTexturesLR = new ImageContent()
                {
                    Streams = ContentManager.FindContent(contentPath, description.Textures.TexturesLR),
                };
                ImageContent terrainTexturesHR = new ImageContent()
                {
                    Streams = ContentManager.FindContent(contentPath, description.Textures.TexturesHR),
                };
                ImageContent normalMapTextures = new ImageContent()
                {
                    Streams = ContentManager.FindContent(contentPath, description.Textures.NormalMaps),
                };

                this.terrainTexturesLR = game.Graphics.Device.LoadTextureArray(terrainTexturesLR.Streams);
                this.terrainTexturesHR = game.Graphics.Device.LoadTextureArray(terrainTexturesHR.Streams);
                this.terrainNormalMaps = game.Graphics.Device.LoadTextureArray(normalMapTextures.Streams);
                this.slopeRanges = description.Textures.SlopeRanges;
            }

            #endregion

            #region Read folliage textures

            if (description.Vegetation != null)
            {
                //Read folliage textures
                string contentPath = Path.Combine(description.ContentPath, description.Vegetation.ContentPath);

                ImageContent folliageTextures = new ImageContent()
                {
                    Streams = ContentManager.FindContent(contentPath, description.Vegetation.VegetarionTextures),
                };

                this.folliageTextures = game.Graphics.Device.LoadTextureArray(folliageTextures.Streams);
                this.folliageTextureCount = (uint)folliageTextures.Count;
            }

            #endregion

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

            //Initialize patch dictionary
            this.patches = new TerrainPatchDictionary(
                game, 
                description.Quadtree.MaxTrianglesPerNode);

            //Initialize update context
            this.updateContext = new TerrainUpdateContext()
            {
                FolliageDescription = description.Vegetation,
            };

            //Initialize draw context
            this.drawContext = new TerrainDrawContext()
            {
                TerraintexturesLR = this.terrainTexturesLR,
                TerraintexturesHR = this.terrainTexturesHR,
                TerrainNormalMaps = this.terrainNormalMaps,
                SlopeRanges = this.slopeRanges,
                FolliageTextureCount = this.folliageTextureCount,
                FolliageTextures = this.folliageTextures,
                FolliageEndRadius = description.Vegetation != null ? description.Vegetation.EndRadius : 0,
            };

            //Set drawing parameters for renderer
            this.Opaque = true;
            this.DeferredEnabled = true;
        }
        /// <summary>
        /// Release of resources
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.terrainTexturesLR);
            Helper.Dispose(this.terrainTexturesHR);
            Helper.Dispose(this.terrainNormalMaps);
            Helper.Dispose(this.folliageTextures);
            Helper.Dispose(this.patches);
        }

        /// <summary>
        /// Updates the state of the terrain components
        /// </summary>
        /// <param name="context">Update context</param>
        public override void Update(UpdateContext context)
        {
            if (this.patches != null)
            {
                this.updateContext.BaseContext = context;
                this.updateContext.VisibleNodes = this.quadTree.GetNodesInVolume(ref context.Frustum);

                this.patches.Update(this.updateContext);
            }
        }
        /// <summary>
        /// Draws the terrain components
        /// </summary>
        /// <param name="context">Draw context</param>
        public override void Draw(DrawContext context)
        {
            if (this.patches != null)
            {
                this.drawContext.BaseContext = context;

                this.patches.Draw(this.drawContext);
            }
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
