using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SharpDX;
using Buffer = SharpDX.Direct3D11.Buffer;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Collections;
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;
    using Engine.Helpers;
    using Engine.PathFinding;

    public class Terrain : Ground
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
            public PickingQuadTreeNode[] VisibleNodes;
            /// <summary>
            /// Foliage generation description
            /// </summary>
            public GroundDescription.VegetationDescription FoliageDescription;
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
            /// Normal map textures for terrain
            /// </summary>
            public ShaderResourceView TerrainNormalMaps;
            /// <summary>
            /// Gets or sets whether use alpha mapping or not
            /// </summary>
            public bool UseAlphaMap;
            /// <summary>
            /// Alpha map texture
            /// </summary>
            public ShaderResourceView AlphaMap;
            /// <summary>
            /// Color textures for alpha mapping
            /// </summary>
            public ShaderResourceView ColorTextures;
            /// <summary>
            /// Gets or sets whether use slope texturing or not
            /// </summary>
            public bool UseSlopes;
            /// <summary>
            /// Slope ranges
            /// </summary>
            public Vector2 SlopeRanges;
            /// <summary>
            /// Low resolution textures for terrain
            /// </summary>
            public ShaderResourceView TerraintexturesLR;
            /// <summary>
            /// High resolution textures for terrain
            /// </summary>
            public ShaderResourceView TerraintexturesHR;
            /// <summary>
            /// Lerping proportion between alhpa mapping and slope texturing
            /// </summary>
            public float Proportion;

            /// <summary>
            /// Foliage textures
            /// </summary>
            public ShaderResourceView FoliageTextures;
            /// <summary>
            /// Foliage texture count
            /// </summary>
            public uint FoliageTextureCount;
            /// <summary>
            /// Foliage end radius
            /// </summary>
            public float FoliageEndRadius;
            /// <summary>
            /// Wind direction
            /// </summary>
            public Vector3 WindDirection;
            /// <summary>
            /// Wind strength
            /// </summary>
            public float WindStrength;

            public float Time;
            /// <summary>
            /// Random texture
            /// </summary>
            public ShaderResourceView RandomTexture;
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
            /// 
            /// </summary>
            private Dictionary<LevelOfDetailEnum, List<PickingQuadTreeNode>> tmp = null;

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

                this.tmp = new Dictionary<LevelOfDetailEnum, List<PickingQuadTreeNode>>();
                tmp.Add(LevelOfDetailEnum.High, new List<PickingQuadTreeNode>());
                tmp.Add(LevelOfDetailEnum.Medium, new List<PickingQuadTreeNode>());
                tmp.Add(LevelOfDetailEnum.Low, new List<PickingQuadTreeNode>());
                tmp.Add(LevelOfDetailEnum.Minimum, new List<PickingQuadTreeNode>());
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
            /// Gets the currently assigned patch of the specified node
            /// </summary>
            /// <param name="node">Node to find</param>
            /// <returns>Returns the currently assigned patch of the specified node if exists</returns>
            private TerrainPatch GetCurrent(PickingQuadTreeNode node)
            {
                foreach (var item in this.patches.Values)
                {
                    var n = Array.Find(item, i => i.Current == node);

                    if (n != null) return n;
                }

                return null;
            }
            /// <summary>
            /// Gets the next free patch of the specified level of detail
            /// </summary>
            /// <param name="lod">Level of detail</param>
            /// <returns>Returns the next free patch of the specified level of detail</returns>
            private TerrainPatch GetFree(LevelOfDetailEnum lod)
            {
                //Find free patch
                return Array.Find(this.patches[lod], p => p.Current == null);
            }
            /// <summary>
            /// Gets the visible patches in a flat array
            /// </summary>
            /// <returns>Returns the visible patches in a flat array</returns>
            private TerrainPatch[] FlatList()
            {
                List<TerrainPatch> list = new List<TerrainPatch>();

                foreach (var item in this.patches.Values)
                {
                    list.AddRange(Array.FindAll(item, i => i.Visible == true));
                }

                return list.ToArray();
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
                    this.tmp[LevelOfDetailEnum.High].Clear();
                    this.tmp[LevelOfDetailEnum.Medium].Clear();
                    this.tmp[LevelOfDetailEnum.Low].Clear();
                    this.tmp[LevelOfDetailEnum.Minimum].Clear();

                    Vector3 eyePosition = context.BaseContext.EyePosition;

                    //Sort by distance to eye position
                    Array.Sort(visibleNodes, (n1, n2) =>
                    {
                        float d1 = Vector3.DistanceSquared(n1.Center, eyePosition);
                        float d2 = Vector3.DistanceSquared(n2.Center, eyePosition);

                        return d1.CompareTo(d2);
                    });

                    //Get nearest node center
                    var startPoint = visibleNodes[0].Center;
                    //Get node side size
                    float side = visibleNodes[0].BoundingBox.Maximum.X - visibleNodes[0].BoundingBox.Minimum.X;
                    //Apply thales theorem to get the distance to a corner neighbour node
                    float maxDistance = (float)Math.Sqrt((side * side) * 2f);

                    //Assign level of detail by distance, making quads from start point node
                    for (int i = 0; i < visibleNodes.Length; i++)
                    {
                        var distance = Vector3.Distance(startPoint, visibleNodes[i].Center);

                        if (distance <= maxDistance)
                        {
                            tmp[LevelOfDetailEnum.High].Add(visibleNodes[i]);
                        }
                        else if (distance <= (maxDistance * 2f))
                        {
                            tmp[LevelOfDetailEnum.Medium].Add(visibleNodes[i]);
                        }
                        else if (distance <= (maxDistance * 4f))
                        {
                            tmp[LevelOfDetailEnum.Low].Add(visibleNodes[i]);
                        }
                        else
                        {
                            //tmp[LevelOfDetailEnum.Minimum].Add(visibleNodes[i]);
                        }
                    }

                    int changes = 0;

                    //Discard nodes (nodes in patches dictionary not in visible nodes)
                    foreach (LevelOfDetailEnum currentLod in this.patches.Keys)
                    {
                        foreach (var patch in this.patches[currentLod])
                        {
                            if (patch.Current != null && !Array.Exists(visibleNodes, n => n == patch.Current))
                            {
                                //Discard node
                                patch.Visible = false;
                                patch.SetVertexData(null);

                                changes++;
                            }
                        }
                    }

                    //Modified lod nodes (node moved between lods)
                    foreach (LevelOfDetailEnum newLod in tmp.Keys)
                    {
                        foreach (var node in tmp[newLod])
                        {
                            //Node exists int patches dictionary and has distinct lod
                            var currentPatch = this.GetCurrent(node);
                            if (currentPatch != null)
                            {
                                if (currentPatch.LevelOfDetail != newLod)
                                {
                                    //Discard node
                                    currentPatch.Visible = false;
                                    currentPatch.SetVertexData(null);

                                    //Add node
                                    var freePatch = this.GetFree(newLod);
                                    if (freePatch != null)
                                    {
                                        freePatch.Visible = true;
                                        freePatch.SetVertexData(node);
                                        if (context.FoliageDescription != null)
                                        {
                                            freePatch.CopyFoliageData(currentPatch);
                                        }
                                    }

                                    changes++;
                                }
                                else
                                {
                                    currentPatch.Visible = true;
                                }
                            }
                        }
                    }

                    //Add nodes
                    foreach (LevelOfDetailEnum newLod in tmp.Keys)
                    {
                        foreach (var node in tmp[newLod])
                        {
                            var currentPatch = this.GetCurrent(node);
                            if (currentPatch == null)
                            {
                                var freePatch = this.GetFree(newLod);
                                if (freePatch != null)
                                {
                                    freePatch.Visible = true;
                                    freePatch.SetVertexData(node);
                                    if (context.FoliageDescription != null)
                                    {
                                        freePatch.Plant(context.FoliageDescription);
                                    }

                                    changes++;
                                }
                            }
                        }
                    }

                    if (changes > 0)
                    {
                        var patchList = this.FlatList();

                        //Set node connections to select index buffer shapes
                        for (int a = 0; a < patchList.Length; a++)
                        {
                            IndexBufferShapeEnum t0;

                            if (patchList[a].LevelOfDetail == LevelOfDetailEnum.None || patchList[a].LevelOfDetail == LevelOfDetailEnum.Minimum)
                            {
                                t0 = IndexBufferShapeEnum.None;
                            }
                            else if (patchList[a].LevelOfDetail == LevelOfDetailEnum.Low)
                            {
                                t0 = IndexBufferShapeEnum.Full;
                            }
                            else
                            {
                                t0 = IndexBufferShapeEnum.Full;

                                for (int b = a + 1; b < patchList.Length; b++)
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
                            context.BaseContext.Frustum,
                            context.BaseContext.Lights,
                            context.BaseContext.ShadowMapStatic,
                            context.BaseContext.ShadowMapDynamic,
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
                            context.TerrainNormalMaps,
                            context.UseAlphaMap,
                            context.AlphaMap,
                            context.ColorTextures,
                            context.UseSlopes,
                            context.SlopeRanges,
                            context.TerraintexturesLR,
                            context.TerraintexturesHR,
                            context.Proportion);
                    }
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        effect.UpdatePerObject(
                            Material.Default,
                            context.TerrainNormalMaps,
                            context.UseAlphaMap,
                            context.AlphaMap,
                            context.ColorTextures,
                            context.UseSlopes,
                            context.SlopeRanges,
                            context.TerraintexturesLR,
                            context.TerraintexturesHR,
                            context.Proportion);
                    }

                    #endregion

                    var technique = effect.GetTechnique(VertexTypes.Terrain, false, DrawingStages.Drawing, context.BaseContext.DrawerMode);

                    this.game.Graphics.DeviceContext.InputAssembler.InputLayout = effect.GetInputLayout(technique);
                    Counters.IAInputLayoutSets++;
                    this.game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
                    Counters.IAPrimitiveTopologySets++;

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
                            context.BaseContext.Frustum,
                            context.BaseContext.Lights,
                            context.BaseContext.ShadowMapStatic,
                            context.BaseContext.ShadowMapDynamic,
                            context.BaseContext.FromLightViewProjection,
                            context.WindDirection,
                            context.WindStrength,
                            context.Time,
                            context.RandomTexture);
                    }
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        effect.UpdatePerFrame(
                            context.BaseContext.World,
                            context.BaseContext.ViewProjection,
                            context.BaseContext.EyePosition,
                            context.BaseContext.Frustum,
                            context.BaseContext.Lights,
                            context.BaseContext.ShadowMapStatic,
                            context.BaseContext.ShadowMapDynamic,
                            context.BaseContext.FromLightViewProjection,
                            context.WindDirection,
                            context.WindStrength,
                            context.Time,
                            context.RandomTexture);
                    }
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        effect.UpdatePerFrame(
                            context.BaseContext.World,
                            context.BaseContext.ViewProjection,
                            context.BaseContext.EyePosition,
                            new BoundingFrustum(),
                            null,
                            null,
                            null,
                            Matrix.Identity,
                            context.WindDirection,
                            context.WindStrength,
                            context.Time,
                            context.RandomTexture);
                    }

                    #endregion

                    #region Per object update

                    if (context.BaseContext.DrawerMode == DrawerModesEnum.Forward)
                    {
                        effect.UpdatePerObject(Material.Default, 0, context.FoliageEndRadius, context.FoliageTextureCount, context.FoliageTextures);
                    }
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        effect.UpdatePerObject(Material.Default, 0, context.FoliageEndRadius, context.FoliageTextureCount, context.FoliageTextures);
                    }
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        effect.UpdatePerObject(Material.Default, 0, context.FoliageEndRadius, context.FoliageTextureCount, context.FoliageTextures);
                    }

                    #endregion

                    var technique = effect.GetTechnique(VertexTypes.Billboard, false, DrawingStages.Drawing, context.BaseContext.DrawerMode);

                    this.game.Graphics.DeviceContext.InputAssembler.InputLayout = effect.GetInputLayout(technique);
                    Counters.IAInputLayoutSets++;
                    this.game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.PointList;
                    Counters.IAPrimitiveTopologySets++;

                    foreach (var lod in this.patches.Keys)
                    {
                        foreach (var item in this.patches[lod])
                        {
                            if (item.Visible)
                            {
                                item.DrawFoliage(context.BaseContext, technique);
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

                    //Foliage buffer
                    {
                        VertexBillboard[] vertexData = new VertexBillboard[MAX];

                        patch.foliageBuffer = game.Graphics.Device.CreateVertexBufferWrite(vertexData);
                        patch.foliageBufferBinding = new[]
                        {
                            new VertexBufferBinding(patch.foliageBuffer, default(VertexBillboard).Stride, 0),
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
            /// Gets the vertex data for buffer writing
            /// </summary>
            /// <param name="vertexType">Vertex type</param>
            /// <param name="lod">Level of detail</param>
            /// <returns>Returns the vertex data for buffer writing</returns>
            private static IVertexData[] PrepareVertexData(VertexData[] vertices, VertexTypes vertexType, LevelOfDetailEnum lod)
            {
                var data = VertexData.Convert(vertexType, vertices, null, null, Matrix.Identity);

                int range = (int)lod;
                if (range > 1)
                {
                    int side = (int)Math.Sqrt(data.Length);

                    List<IVertexData> data2 = new List<IVertexData>();

                    for (int y = 0; y < side; y += range)
                    {
                        for (int x = 0; x < side; x += range)
                        {
                            int index = (y * side) + x;

                            data2.Add(data[index]);
                        }
                    }

                    data = data2.ToArray();
                }

                return data;
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
            /// Vertex buffer with foliage data
            /// </summary>
            private Buffer foliageBuffer = null;
            /// <summary>
            /// Foliage positions
            /// </summary>
            private int foliageCount = 0;
            /// <summary>
            /// Vertex buffer binding for foliage buffer
            /// </summary>
            private VertexBufferBinding[] foliageBufferBinding = null;
            /// <summary>
            /// Foliage populated flag
            /// </summary>
            private bool foliagePlanted = false;
            /// <summary>
            /// Foliage populating flag
            /// </summary>
            private bool foliagePlanting = false;
            /// <summary>
            /// Foliage attached to buffer flag
            /// </summary>
            private bool foliageAttached = false;
            /// <summary>
            /// Foliage generated data
            /// </summary>
            private IVertexData[] foliageData = null;

            /// <summary>
            /// Game
            /// </summary>
            public readonly Game Game = null;
            /// <summary>
            /// Level of detail
            /// </summary>
            public LevelOfDetailEnum LevelOfDetail { get; private set; }
            /// <summary>
            /// Current quadtree node
            /// </summary>
            public PickingQuadTreeNode Current { get; private set; }
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
            public void SetVertexData(PickingQuadTreeNode node)
            {
                if (this.vertexBuffer != null)
                {
                    if (this.Current != node)
                    {
                        this.Current = node;

                        if (this.Current != null)
                        {
                            var data = PrepareVertexData(this.Current.Vertices, VertexTypes.Terrain, this.LevelOfDetail);

                            //if (this.LevelOfDetail == LevelOfDetailEnum.High)
                            //{
                            //    Array.ForEach(data, d => d.SetChannelValue(VertexDataChannels.Color, new Color4(0.5f, 0.5f, 1f, 1f)));
                            //}
                            //else if (this.LevelOfDetail == LevelOfDetailEnum.Medium)
                            //{
                            //    Array.ForEach(data, d => d.SetChannelValue(VertexDataChannels.Color, new Color4(0.5f, 1f, 0.5f, 1f)));
                            //}
                            //else if (this.LevelOfDetail == LevelOfDetailEnum.Low)
                            //{
                            //    Array.ForEach(data, d => d.SetChannelValue(VertexDataChannels.Color, new Color4(1f, 0.5f, 0.5f, 1f)));
                            //}

                            VertexData.WriteVertexBuffer(
                                this.Game.Graphics.DeviceContext,
                                this.vertexBuffer,
                                data);
                        }
                    }
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
                    Counters.IAVertexBuffersSets++;
                    this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);
                    Counters.IAIndexBufferSets++;

                    for (int p = 0; p < technique.Description.PassCount; p++)
                    {
                        technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                        this.Game.Graphics.DeviceContext.DrawIndexed(this.indexCount, 0, 0);

                        Counters.DrawCallsPerFrame++;
                        Counters.InstancesPerFrame++;
                        Counters.TrianglesPerFrame += this.indexCount / 3;
                    }
                }
            }
            /// <summary>
            /// Draw the patch foliage
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <param name="technique">Technique</param>
            public void DrawFoliage(DrawContext context, EffectTechnique technique)
            {
                if (this.foliageCount > 0)
                {
                    this.AttachFoliage();

                    //Sets vertex and index buffer
                    this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, this.foliageBufferBinding);
                    Counters.IAVertexBuffersSets++;
                    this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(null, SharpDX.DXGI.Format.R32_UInt, 0);
                    Counters.IAIndexBufferSets++;

                    for (int p = 0; p < technique.Description.PassCount; p++)
                    {
                        technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                        this.Game.Graphics.DeviceContext.Draw(this.foliageCount, 0);

                        Counters.DrawCallsPerFrame++;
                        Counters.InstancesPerFrame++;
                        Counters.TrianglesPerFrame += this.foliageCount / 3;
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
                if (this.Current != null && terrainPatch != null && terrainPatch.Current != null)
                {
                    if (this.LevelOfDetail == terrainPatch.LevelOfDetail)
                    {
                        return IndexBufferShapeEnum.Full;
                    }
                    else
                    {
                        if (this.Current.TopNeighbour == terrainPatch.Current) return IndexBufferShapeEnum.SideTop;
                        else if (this.Current.BottomNeighbour == terrainPatch.Current) return IndexBufferShapeEnum.SideBottom;
                        else if (this.Current.LeftNeighbour == terrainPatch.Current) return IndexBufferShapeEnum.SideLeft;
                        else if (this.Current.RightNeighbour == terrainPatch.Current) return IndexBufferShapeEnum.SideRight;
                    }
                }

                return IndexBufferShapeEnum.None;
            }
            /// <summary>
            /// Launchs foliage population asynchronous task
            /// </summary>
            /// <param name="description">Terrain vegetation description</param>
            public void Plant(GroundDescription.VegetationDescription description)
            {
                if (this.Current != null)
                {
                    this.foliageAttached = false;
                    this.foliagePlanted = false;

                    if (!this.foliagePlanting)
                    {
                        //Start planting task
                        this.foliagePlanting = true;

                        Task<VertexData[]> t = Task.Factory.StartNew<VertexData[]>(() => PlantTask(this.Current, description));

                        t.ContinueWith(task => PlantThreadCompleted(task.Result));
                    }
                }
            }
            /// <summary>
            /// Copies the foliage data from one patch to another
            /// </summary>
            /// <param name="patch">The other patch from copy data to</param>
            public void CopyFoliageData(TerrainPatch patch)
            {
                this.foliageCount = patch.foliageCount;
                this.foliageData = patch.foliageData;
                this.foliagePlanting = false;
                this.foliagePlanted = true;
                this.foliageAttached = false;
            }

            /// <summary>
            /// Asynchronous planting task
            /// </summary>
            /// <param name="node">Node to process</param>
            /// <param name="description">Vegetation task</param>
            /// <returns>Returns generated vertex data</returns>
            private static VertexData[] PlantTask(PickingQuadTreeNode node, GroundDescription.VegetationDescription description)
            {
                List<VertexData> vertexData = new List<VertexData>(MAX);

                if (node != null)
                {
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
                                float d;
                                if (tri.Intersects(ref ray, out intersectionPoint, out d))
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
                }

                return vertexData.ToArray();
            }
            /// <summary>
            /// Planting task completed
            /// </summary>
            /// <param name="vData">Vertex data generated in asynchronous task</param>
            private void PlantThreadCompleted(VertexData[] vData)
            {
                this.foliageCount = vData.Length;
                this.foliageData = VertexData.Convert(VertexTypes.Billboard, vData, null, null, Matrix.Identity);
                this.foliagePlanting = false;
                this.foliagePlanted = true;
                this.foliageAttached = false;
            }
            /// <summary>
            /// Attachs the foliage data to the vertex buffer
            /// </summary>
            private void AttachFoliage()
            {
                if (!this.foliageAttached)
                {
                    if (this.foliagePlanted && this.foliageData != null && this.foliageData.Length > 0)
                    {
                        //Attach data
                        VertexData.WriteVertexBuffer(
                            this.Game.Graphics.DeviceContext,
                            this.foliageBuffer,
                            this.foliageData);
                    }

                    this.foliageAttached = true;
                }
            }

            /// <summary>
            /// Gets the instance text representation
            /// </summary>
            /// <returns>Returns the instance text representation</returns>
            public override string ToString()
            {
                return string.Format("LOD: {0}; Visible: {1}; Indices: {2}; Foliage: {3}; {4}",
                    this.LevelOfDetail,
                    this.Visible,
                    this.indexCount,
                    this.foliageCount,
                    this.Current);
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
        /// Color textures for alpha map
        /// </summary>
        private ShaderResourceView colorTextures = null;
        /// <summary>
        /// Alpha map
        /// </summary>
        private ShaderResourceView alphaMap = null;
        /// <summary>
        /// Random texture
        /// </summary>
        private ShaderResourceView textureRandom = null;
        /// <summary>
        /// Slope ranges
        /// </summary>
        private Vector2 slopeRanges = Vector2.Zero;
        /// <summary>
        /// Foliage textures
        /// </summary>
        private ShaderResourceView foliageTextures = null;
        /// <summary>
        /// Foliage texture count
        /// </summary>
        private uint foliageTextureCount = 0;
        /// <summary>
        /// Wind total time
        /// </summary>
        private float windTime = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Terrain description</param>
        public Terrain(Game game, GroundDescription description)
            : base(game, description)
        {
            #region Read heightmap

            if (this.Description.Heightmap != null)
            {
                string contentPath = Path.Combine(this.Description.ContentPath, this.Description.Heightmap.ContentPath);

                ImageContent heightMapImage = new ImageContent()
                {
                    Streams = ContentManager.FindContent(contentPath, this.Description.Heightmap.HeightmapFileName),
                };
                ImageContent colorMapImage = new ImageContent()
                {
                    Streams = ContentManager.FindContent(contentPath, this.Description.Heightmap.ColormapFileName),
                };

                this.heightMap = HeightMap.FromStream(heightMapImage.Stream, colorMapImage.Stream);
            }

            #endregion

            #region Read terrain textures

            if (this.Description.Textures != null)
            {
                string contentPath = Path.Combine(this.Description.ContentPath, this.Description.Textures.ContentPath);

                ImageContent normalMapTextures = new ImageContent()
                {
                    Streams = ContentManager.FindContent(contentPath, this.Description.Textures.NormalMaps),
                };

                this.terrainNormalMaps = game.Graphics.Device.LoadTextureArray(normalMapTextures.Streams);

                if (this.Description.Textures.UseSlopes)
                {
                    ImageContent terrainTexturesLR = new ImageContent()
                    {
                        Streams = ContentManager.FindContent(contentPath, this.Description.Textures.TexturesLR),
                    };
                    ImageContent terrainTexturesHR = new ImageContent()
                    {
                        Streams = ContentManager.FindContent(contentPath, this.Description.Textures.TexturesHR),
                    };
                    
                    this.terrainTexturesLR = game.Graphics.Device.LoadTextureArray(terrainTexturesLR.Streams);
                    this.terrainTexturesHR = game.Graphics.Device.LoadTextureArray(terrainTexturesHR.Streams);
                    this.slopeRanges = this.Description.Textures.SlopeRanges;
                }

                if (this.Description.Textures.UseAlphaMapping)
                {
                    ImageContent colors = new ImageContent()
                    {
                        Streams = ContentManager.FindContent(contentPath, this.Description.Textures.ColorTextures),
                    };
                    ImageContent alphaMap = new ImageContent()
                    {
                        Streams = ContentManager.FindContent(contentPath, this.Description.Textures.AlphaMap),
                    };

                    this.colorTextures = game.Graphics.Device.LoadTextureArray(colors.Streams);
                    this.alphaMap = game.Graphics.Device.LoadTexture(alphaMap.Stream);
                }
            }

            #endregion

            #region Read foliage textures

            if (this.Description.Vegetation != null)
            {
                //Read foliage textures
                string contentPath = Path.Combine(this.Description.ContentPath, this.Description.Vegetation.ContentPath);

                ImageContent foliageTextures = new ImageContent()
                {
                    Streams = ContentManager.FindContent(contentPath, this.Description.Vegetation.VegetarionTextures),
                };

                this.foliageTextures = game.Graphics.Device.LoadTextureArray(foliageTextures.Streams);
                this.foliageTextureCount = (uint)foliageTextures.Count;
            }

            #endregion

            #region Random texture generation

            this.textureRandom = game.Graphics.Device.CreateRandomTexture(1024, 24);

            #endregion

            //Initialize patch dictionary
            int trisPerNode = this.heightMap.CalcTrianglesPerNode(this.Description.Quadtree.MaximumDepth);
            this.patches = new TerrainPatchDictionary(game, trisPerNode);

            //Initialize update context
            this.updateContext = new TerrainUpdateContext()
            {
                FoliageDescription = this.Description.Vegetation,
            };

            //Initialize draw context
            this.drawContext = new TerrainDrawContext()
            {
                TerrainNormalMaps = this.terrainNormalMaps,

                UseAlphaMap = this.Description.Textures.UseAlphaMapping,
                AlphaMap = this.alphaMap,
                ColorTextures = this.colorTextures,

                UseSlopes = this.Description.Textures.UseSlopes,
                SlopeRanges = this.slopeRanges,
                TerraintexturesLR = this.terrainTexturesLR,
                TerraintexturesHR = this.terrainTexturesHR,

                Proportion = this.Description.Textures.Proportion,

                FoliageTextureCount = this.foliageTextureCount,
                FoliageTextures = this.foliageTextures,
                FoliageEndRadius = this.Description.Vegetation != null ? this.Description.Vegetation.EndRadius : 0,
                RandomTexture = this.textureRandom,
            };

            //Set drawing parameters for renderer
            this.Opaque = true;
            this.DeferredEnabled = true;

            if (!this.Description.DelayGeneration)
            {
                this.UpdateInternals();
            }
        }
        /// <summary>
        /// Release of resources
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.terrainTexturesLR);
            Helper.Dispose(this.terrainTexturesHR);
            Helper.Dispose(this.terrainNormalMaps);
            Helper.Dispose(this.foliageTextures);
            Helper.Dispose(this.textureRandom);
            Helper.Dispose(this.colorTextures);
            Helper.Dispose(this.alphaMap);
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
                this.updateContext.VisibleNodes = this.pickingQuadtree.GetNodesInVolume(ref context.Frustum);

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
                this.windTime += context.GameTime.ElapsedSeconds * this.drawContext.WindStrength;

                this.drawContext.BaseContext = context;
                this.drawContext.Time = this.windTime;

                this.patches.Draw(this.drawContext);
            }
        }

        /// <summary>
        /// Updates internal objects
        /// </summary>
        public override void UpdateInternals()
        {
            //Get vertices and indices from heightmap
            VertexData[] vertices;
            uint[] indices;
            this.heightMap.BuildGeometry(
                this.Description.Heightmap.CellSize,
                this.Description.Heightmap.MaximumHeight,
                out vertices, out indices);

            //Initialize Quadtree
            this.pickingQuadtree = new PickingQuadTree(
                vertices,
                this.Description.Quadtree.MaximumDepth);

            //Intialize Pathfinding Graph
            if (this.Description != null && this.Description.PathFinder != null)
            {
                this.navigationGraph = PathFinder.Build(this.Description.PathFinder.Settings, vertices, indices);
            }
        }
        /// <summary>
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="position">Picked position if exists</param>
        /// <param name="triangle">Picked triangle if exists</param>
        /// <param name="distance">Distance to position</param>
        /// <returns>Returns true if picked position found</returns>
        public override bool PickNearest(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            return this.pickingQuadtree.PickNearest(ref ray, facingOnly, out position, out triangle, out distance);
        }
        /// <summary>
        /// Pick first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="position">Picked position if exists</param>
        /// <param name="triangle">Picked triangle if exists</param>
        /// <param name="distance">Distance to position</param>
        /// <returns>Returns true if picked position found</returns>
        public override bool PickFirst(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            return this.pickingQuadtree.PickFirst(ref ray, facingOnly, out position, out triangle, out distance);
        }
        /// <summary>
        /// Pick all positions
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="positions">Picked positions if exists</param>
        /// <param name="triangles">Picked triangles if exists</param>
        /// <param name="distances">Distances to positions</param>
        /// <returns>Returns true if picked positions found</returns>
        public override bool PickAll(ref Ray ray, bool facingOnly, out Vector3[] positions, out Triangle[] triangles, out float[] distances)
        {
            return this.pickingQuadtree.PickAll(ref ray, facingOnly, out positions, out triangles, out distances);
        }
        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public override BoundingSphere GetBoundingSphere()
        {
            return this.pickingQuadtree.BoundingSphere;
        }
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        public override BoundingBox GetBoundingBox()
        {
            return this.pickingQuadtree.BoundingBox;
        }

        /// <summary>
        /// Sets wind parameters
        /// </summary>
        /// <param name="direction">Direction</param>
        /// <param name="strength">Strength</param>
        public void SetWind(Vector3 direction, float strength)
        {
            if (this.drawContext != null)
            {
                this.drawContext.WindDirection = direction;
                this.drawContext.WindStrength = strength;
            }
        }

        /// <summary>
        /// Gets terrain bounding boxes at specified level
        /// </summary>
        /// <param name="level">Level</param>
        /// <returns>Returns terrain bounding boxes</returns>
        public BoundingBox[] GetBoundingBoxes(int level = 0)
        {
            return this.pickingQuadtree.GetBoundingBoxes(level);
        }
        /// <summary>
        /// Gets the path finder grid nodes
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <returns>Returns the path finder grid nodes</returns>
        public IGraphNode[] GetNodes(Agent agent)
        {
            IGraphNode[] nodes = null;

            if (this.navigationGraph != null)
            {
                nodes = this.navigationGraph.GetNodes(agent);
            }

            return nodes;
        }
    }
}
