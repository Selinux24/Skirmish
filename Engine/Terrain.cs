using SharpDX;
using SharpDX.Direct3D;
using System;
using System.Collections.Generic;
using System.IO;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine
{
    using Engine.Collections;
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;
    using Engine.PathFinding;

    /// <summary>
    /// Terrain class
    /// </summary>
    public class Terrain : Ground, UseMaterials
    {
        #region Terrain patches

        /// <summary>
        /// Terrain patch dictionary by level of detail
        /// </summary>
        class TerrainPatchDictionary : IDisposable
        {
            /// <summary>
            /// Maximum number of patches in high level of detail
            /// </summary>
            public const int MaxPatchesHighLevel = 9;
            /// <summary>
            /// Maximum number of patches in medium level
            /// </summary>
            public const int MaxPatchesMediumLevel = 16;
            /// <summary>
            /// Maximum number of patches in low level
            /// </summary>
            public const int MaxPatchesLowLevel = 24;
            /// <summary>
            /// Maximum number of patches in minimum level
            /// </summary>
            public const int MaxPatchesMinimumLevel = 24;

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
            private Dictionary<LevelOfDetailEnum, Dictionary<IndexBufferShapeEnum, TerrainIndexBuffer>> indices = null;
            /// <summary>
            /// 
            /// </summary>
            private Dictionary<LevelOfDetailEnum, List<PickingQuadTreeNode>> tmp = null;

            /// <summary>
            /// Height map
            /// </summary>
            private HeightMap heightMap = null;
            /// <summary>
            /// Heightmap cell size
            /// </summary>
            private float heightMapCellSize;
            /// <summary>
            /// Heightmap maximum height
            /// </summary>
            private float heightMapHeight;
            /// <summary>
            /// Heightmap texture resolution
            /// </summary>
            private float textureResolution;

            /// <summary>
            /// Gets or sets whether use alpha mapping or not
            /// </summary>
            private bool useAlphaMap;
            /// <summary>
            /// Gets or sets whether use slope texturing or not
            /// </summary>
            private bool useSlopes;
            /// <summary>
            /// Lerping proportion between alhpa mapping and slope texturing
            /// </summary>
            private float proportion;
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
            /// Terrain specular maps
            /// </summary>
            private ShaderResourceView terrainSpecularMaps = null;
            /// <summary>
            /// Color textures for alpha map
            /// </summary>
            private ShaderResourceView colorTextures = null;
            /// <summary>
            /// Alpha map
            /// </summary>
            private ShaderResourceView alphaMap = null;
            /// <summary>
            /// Slope ranges
            /// </summary>
            private Vector2 slopeRanges = Vector2.Zero;

            /// <summary>
            /// Terrain material
            /// </summary>
            public MeshMaterial TerrainMaterial { get; protected set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="game">Game</param>
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="description">Heightmap description</param>
            /// <param name="groundDescription">Ground description</param>
            public TerrainPatchDictionary(Game game, BufferManager bufferManager, HeightmapDescription description, GroundDescription groundDescription)
                : base()
            {
                this.game = game;

                #region Read heightmap

                {
                    string contentPath = description.ContentPath;

                    ImageContent heightMapImage = new ImageContent()
                    {
                        Streams = ContentManager.FindContent(contentPath, description.HeightmapFileName),
                    };
                    ImageContent colorMapImage = new ImageContent()
                    {
                        Streams = ContentManager.FindContent(contentPath, description.ColormapFileName),
                    };

                    this.heightMap = HeightMap.FromStream(heightMapImage.Stream, colorMapImage.Stream);
                    this.heightMapCellSize = description.CellSize;
                    this.heightMapHeight = description.MaximumHeight;
                    this.textureResolution = description.TextureResolution;
                }

                #endregion

                #region Read terrain data

                {
                    string contentPath = Path.Combine(description.ContentPath, description.Textures.ContentPath);

                    this.TerrainMaterial = new MeshMaterial()
                    {
                        Material = description.Material != null ? description.Material.GetMaterial() : Material.Default
                    };

                    ImageContent normalMapTextures = new ImageContent()
                    {
                        Streams = ContentManager.FindContent(contentPath, description.Textures.NormalMaps),
                    };
                    this.terrainNormalMaps = game.ResourceManager.CreateResource(normalMapTextures);

                    ImageContent specularMapTextures = new ImageContent()
                    {
                        Streams = ContentManager.FindContent(contentPath, description.Textures.SpecularMaps),
                    };
                    this.terrainSpecularMaps = game.ResourceManager.CreateResource(specularMapTextures);

                    if (description.Textures.UseSlopes)
                    {
                        ImageContent terrainTexturesLR = new ImageContent()
                        {
                            Streams = ContentManager.FindContent(contentPath, description.Textures.TexturesLR),
                        };
                        ImageContent terrainTexturesHR = new ImageContent()
                        {
                            Streams = ContentManager.FindContent(contentPath, description.Textures.TexturesHR),
                        };

                        this.terrainTexturesLR = game.ResourceManager.CreateResource(terrainTexturesLR);
                        this.terrainTexturesHR = game.ResourceManager.CreateResource(terrainTexturesHR);
                        this.slopeRanges = description.Textures.SlopeRanges;
                    }

                    if (description.Textures.UseAlphaMapping)
                    {
                        ImageContent colors = new ImageContent()
                        {
                            Streams = ContentManager.FindContent(contentPath, description.Textures.ColorTextures),
                        };
                        ImageContent alphaMap = new ImageContent()
                        {
                            Streams = ContentManager.FindContent(contentPath, description.Textures.AlphaMap),
                        };

                        this.colorTextures = game.ResourceManager.CreateResource(colors);
                        this.alphaMap = game.ResourceManager.CreateResource(alphaMap);
                    }

                    this.useAlphaMap = description.Textures.UseAlphaMapping;
                    this.useSlopes = description.Textures.UseSlopes;
                    this.proportion = description.Textures.Proportion;
                }

                #endregion

                int trianglesPerNode = this.heightMap.CalcTrianglesPerNode(groundDescription.Quadtree.MaximumDepth);

                this.InitializeTerrainPatches(groundDescription.Name, bufferManager, trianglesPerNode);
                this.InitializeTerrainIndices(groundDescription.Name, bufferManager, trianglesPerNode);

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
                Helper.Dispose(this.tmp);

                Helper.Dispose(this.heightMap);

                Helper.Dispose(this.terrainTexturesLR);
                Helper.Dispose(this.terrainTexturesHR);
                Helper.Dispose(this.terrainNormalMaps);
                Helper.Dispose(this.terrainSpecularMaps);
                Helper.Dispose(this.colorTextures);
                Helper.Dispose(this.alphaMap);

                Helper.Dispose(this.TerrainMaterial);
            }

            /// <summary>
            /// Initialize patch dictionary
            /// </summary>
            /// <param name="name">Name</param>
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="trianglesPerNode">Triangles per node</param>
            private void InitializeTerrainPatches(string name, BufferManager bufferManager, int trianglesPerNode)
            {
                this.patches = new Dictionary<LevelOfDetailEnum, TerrainPatch[]>();

                this.patches.Add(LevelOfDetailEnum.High, new TerrainPatch[MaxPatchesHighLevel]);
                this.patches.Add(LevelOfDetailEnum.Medium, new TerrainPatch[MaxPatchesMediumLevel]);
                this.patches.Add(LevelOfDetailEnum.Low, new TerrainPatch[MaxPatchesLowLevel]);
                this.patches.Add(LevelOfDetailEnum.Minimum, new TerrainPatch[MaxPatchesMinimumLevel]);

                int id = 0;

                for (int i = 0; i < MaxPatchesHighLevel; i++)
                {
                    var patch = TerrainPatch.CreatePatch(this.game, bufferManager, string.Format("{0}.{1}", name, ++id), LevelOfDetailEnum.High, trianglesPerNode);
                    this.patches[LevelOfDetailEnum.High][i] = patch;
                }

                for (int i = 0; i < MaxPatchesMediumLevel; i++)
                {
                    var patch = TerrainPatch.CreatePatch(this.game, bufferManager, string.Format("{0}.{1}", name, ++id), LevelOfDetailEnum.Medium, trianglesPerNode);
                    this.patches[LevelOfDetailEnum.Medium][i] = patch;
                }

                for (int i = 0; i < MaxPatchesLowLevel; i++)
                {
                    var patch = TerrainPatch.CreatePatch(this.game, bufferManager, string.Format("{0}.{1}", name, ++id), LevelOfDetailEnum.Low, trianglesPerNode);
                    this.patches[LevelOfDetailEnum.Low][i] = patch;
                }

                for (int i = 0; i < MaxPatchesMinimumLevel; i++)
                {
                    var patch = TerrainPatch.CreatePatch(this.game, bufferManager, string.Format("{0}.{1}", name, ++id), LevelOfDetailEnum.Minimum, trianglesPerNode);
                    this.patches[LevelOfDetailEnum.Minimum][i] = patch;
                }
            }
            /// <summary>
            /// Initialize index dictionary
            /// </summary>
            /// <param name="name">Name</param>
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="trianglesPerNode">Triangles per node</param>
            private void InitializeTerrainIndices(string name, BufferManager bufferManager, int trianglesPerNode)
            {
                this.indices = new Dictionary<LevelOfDetailEnum, Dictionary<IndexBufferShapeEnum, TerrainIndexBuffer>>();

                this.indices.Add(LevelOfDetailEnum.High, new Dictionary<IndexBufferShapeEnum, TerrainIndexBuffer>());
                this.indices.Add(LevelOfDetailEnum.Medium, new Dictionary<IndexBufferShapeEnum, TerrainIndexBuffer>());
                this.indices.Add(LevelOfDetailEnum.Low, new Dictionary<IndexBufferShapeEnum, TerrainIndexBuffer>());

                int id = 0;

                //High level
                for (int i = 0; i < 9; i++)
                {
                    IndexBufferShapeEnum shape = (IndexBufferShapeEnum)i;

                    uint[] indexList = GeometryUtil.GenerateIndices(shape, trianglesPerNode);
                    int ibOffset;
                    int ibSlot;
                    bufferManager.Add(string.Format("{0}.{1}", name, ++id), indexList, false, out ibOffset, out ibSlot);

                    TerrainIndexBuffer buffer = new TerrainIndexBuffer()
                    {
                        Slot = ibSlot,
                        Offset = ibOffset,
                        Count = indexList.Length,
                    };
                    this.indices[LevelOfDetailEnum.High].Add(shape, buffer);
                }

                //Medium level
                for (int i = 0; i < 9; i++)
                {
                    IndexBufferShapeEnum shape = (IndexBufferShapeEnum)i;

                    uint[] indexList = GeometryUtil.GenerateIndices(shape, trianglesPerNode / 4);
                    int ibOffset;
                    int ibSlot;
                    bufferManager.Add(string.Format("{0}.{1}", name, ++id), indexList, false, out ibOffset, out ibSlot);

                    TerrainIndexBuffer buffer = new TerrainIndexBuffer()
                    {
                        Slot = ibSlot,
                        Offset = ibOffset,
                        Count = indexList.Length,
                    };
                    this.indices[LevelOfDetailEnum.Medium].Add(shape, buffer);
                }

                //Low level
                {
                    IndexBufferShapeEnum shape = IndexBufferShapeEnum.Full;

                    uint[] indexList = GeometryUtil.GenerateIndices(shape, trianglesPerNode / 4 / 4);
                    int ibOffset;
                    int ibSlot;
                    bufferManager.Add(string.Format("{0}.{1}", name, ++id), indexList, false, out ibOffset, out ibSlot);

                    TerrainIndexBuffer buffer = new TerrainIndexBuffer()
                    {
                        Slot = ibSlot,
                        Offset = ibOffset,
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
            /// <param name="visibleNodes">Visible nodes</param>
            public void Update(UpdateContext context, PickingQuadTreeNode[] visibleNodes, BufferManager bufferManager)
            {
                if (visibleNodes != null && visibleNodes.Length > 0)
                {
                    this.tmp[LevelOfDetailEnum.High].Clear();
                    this.tmp[LevelOfDetailEnum.Medium].Clear();
                    this.tmp[LevelOfDetailEnum.Low].Clear();
                    this.tmp[LevelOfDetailEnum.Minimum].Clear();

                    Vector3 eyePosition = context.EyePosition;

                    //Sort by distance to eye position
                    Array.Sort(visibleNodes, (n1, n2) =>
                    {
                        float d1 = Vector3.DistanceSquared(n1.Center, eyePosition);
                        float d2 = Vector3.DistanceSquared(n2.Center, eyePosition);

                        return d1.CompareTo(d2);
                    });

                    #region Assign terrain patches

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
                                patch.SetVertexData(null, bufferManager);

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
                                    currentPatch.SetVertexData(null, bufferManager);

                                    //Add node
                                    var freePatch = this.GetFree(newLod);
                                    if (freePatch != null)
                                    {
                                        freePatch.Visible = true;
                                        freePatch.SetVertexData(node, bufferManager);
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
                                    freePatch.SetVertexData(node, bufferManager);

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

                    #endregion
                }
            }
            /// <summary>
            /// Draw patches
            /// </summary>
            /// <param name="context">Drawing context</param>
            public void Draw(DrawContext context, BufferManager bufferManager)
            {
                var terrainTechnique = this.SetTechniqueTerrain(context);
                if (terrainTechnique != null)
                {
                    foreach (var lod in this.patches.Keys)
                    {
                        foreach (var item in this.patches[lod])
                        {
                            if (item.Visible)
                            {
                                bufferManager.SetInputAssembler(terrainTechnique, item.VertexBufferSlot, PrimitiveTopology.TriangleList);

                                bufferManager.SetIndexBuffer(item.IndexBufferSlot);

                                item.DrawTerrain(context, terrainTechnique);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Sets thecnique for terrain drawing
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <returns>Returns the selected technique</returns>
            private EffectTechnique SetTechniqueTerrain(DrawContext context)
            {
                if (context.DrawerMode == DrawerModesEnum.Forward) return this.SetTechniqueTerrainDefault(context);
                if (context.DrawerMode == DrawerModesEnum.Deferred) return this.SetTechniqueTerrainDeferred(context);
                if (context.DrawerMode == DrawerModesEnum.ShadowMap) return this.SetTechniqueTerrainShadowMap(context);
                else return null;
            }
            /// <summary>
            /// Sets thecnique for terrain drawing with forward renderer
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <returns>Returns the selected technique</returns>
            private EffectTechnique SetTechniqueTerrainDefault(DrawContext context)
            {
                EffectDefaultTerrain effect = DrawerPool.EffectDefaultTerrain;

                this.game.Graphics.SetBlendDefault();

                #region Per frame update

                effect.UpdatePerFrame(
                    context.World,
                    context.ViewProjection,
                    this.textureResolution,
                    context.EyePosition,
                    context.Lights,
                    context.ShadowMaps,
                    context.ShadowMapStatic,
                    context.ShadowMapDynamic,
                    context.FromLightViewProjection);

                #endregion

                #region Per object update

                effect.UpdatePerObject(
                    this.terrainNormalMaps,
                    this.terrainSpecularMaps,
                    this.useAlphaMap,
                    this.alphaMap,
                    this.colorTextures,
                    this.useSlopes,
                    this.slopeRanges,
                    this.terrainTexturesLR,
                    this.terrainTexturesHR,
                    this.proportion,
                    (uint)this.TerrainMaterial.ResourceIndex);

                #endregion

                return effect.GetTechnique(VertexTypes.Terrain, false, DrawingStages.Drawing, context.DrawerMode);
            }
            /// <summary>
            /// Sets thecnique for terrain drawing with deferred renderer
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <returns>Returns the selected technique</returns>
            private EffectTechnique SetTechniqueTerrainDeferred(DrawContext context)
            {
                EffectDeferredTerrain effect = DrawerPool.EffectDeferredTerrain;

                this.game.Graphics.SetBlendDefault();

                #region Per frame update

                effect.UpdatePerFrame(
                    context.World,
                    context.ViewProjection,
                    this.textureResolution);

                #endregion

                #region Per object update

                effect.UpdatePerObject(
                    (uint)this.TerrainMaterial.ResourceIndex,
                    this.terrainNormalMaps,
                    this.terrainSpecularMaps,
                    this.useAlphaMap,
                    this.alphaMap,
                    this.colorTextures,
                    this.useSlopes,
                    this.slopeRanges,
                    this.terrainTexturesLR,
                    this.terrainTexturesHR,
                    this.proportion);

                #endregion

                return effect.GetTechnique(VertexTypes.Terrain, false, DrawingStages.Drawing, context.DrawerMode);
            }
            /// <summary>
            /// Sets thecnique for terrain drawing in shadow mapping
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <returns>Returns the selected technique</returns>
            private EffectTechnique SetTechniqueTerrainShadowMap(DrawContext context)
            {
                EffectShadowTerrain effect = DrawerPool.EffectShadowTerrain;

                this.game.Graphics.SetBlendDefault();

                #region Per frame update

                effect.UpdatePerFrame(
                    context.World,
                    context.ViewProjection);

                #endregion

                return effect.GetTechnique(VertexTypes.Terrain, false, DrawingStages.Drawing, context.DrawerMode);
            }

            /// <summary>
            /// Build geometry
            /// </summary>
            /// <param name="vertices">Geometry vertices</param>
            /// <param name="indices">Geometry indices</param>
            public void BuildGeometry(out VertexData[] vertices, out uint[] indices)
            {
                this.heightMap.BuildGeometry(
                    this.heightMapCellSize,
                    this.heightMapHeight,
                    out vertices, out indices);
            }
            /// <summary>
            /// Gets visible nodes by level of detail
            /// </summary>
            /// <param name="levelOfDetail">Level of detail</param>
            /// <returns>Returns a visible nodes list by level of detail</returns>
            public PickingQuadTreeNode[] GetVisibleNodes(LevelOfDetailEnum levelOfDetail)
            {
                return this.tmp[levelOfDetail].ToArray();
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
            /// <param name="game">Game</param>
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="id">Identifier</param>
            /// <param name="lod">Level of detail</param>
            /// <param name="trianglesPerNode">Triangles per node</param>
            /// <returns>Returns the new generated patch</returns>
            public static TerrainPatch CreatePatch(Game game, BufferManager bufferManager, string id, LevelOfDetailEnum lod, int trianglesPerNode)
            {
                int triangleCount = 0;

                if (lod == LevelOfDetailEnum.High) triangleCount = trianglesPerNode;
                else if (lod == LevelOfDetailEnum.Medium) triangleCount = trianglesPerNode / 4;
                else if (lod == LevelOfDetailEnum.Low) triangleCount = trianglesPerNode / 16;

                if (triangleCount > 0)
                {
                    var patch = new TerrainPatch(game, lod);

                    int vertices = (int)Math.Pow((Math.Sqrt(triangleCount / 2) + 1), 2);

                    VertexTerrain[] vertexData = new VertexTerrain[vertices];

                    bufferManager.Add(
                        id,
                        vertexData,
                        true,
                        0,
                        out patch.VertexBufferOffset,
                        out patch.VertexBufferSlot);

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
            /// Vertex buffer slot
            /// </summary>
            public int VertexBufferSlot;
            /// <summary>
            /// Vertex buffer offset
            /// </summary>
            public int VertexBufferOffset;
            /// <summary>
            /// Index buffer slot
            /// </summary>
            public int IndexBufferSlot;
            /// <summary>
            /// Index buffer offset
            /// </summary>
            public int IndexBufferOffset;
            /// <summary>
            /// Indexes count
            /// </summary>
            public int IndexCount = 0;

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

            }

            /// <summary>
            /// Sets vertex data
            /// </summary>
            /// <param name="node">Node to attach to the vertex buffer</param>
            public void SetVertexData(PickingQuadTreeNode node, BufferManager bufferManager)
            {
                if (bufferManager != null)
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

                            bufferManager.WriteBuffer(this.VertexBufferSlot, this.VertexBufferOffset, data);
                        }
                    }
                }
            }
            /// <summary>
            /// Sets index data
            /// </summary>
            /// <param name="buffer">Index buffer description</param>
            public void SetIndexData(TerrainIndexBuffer buffer)
            {
                if (buffer != null)
                {
                    this.IndexBufferSlot = buffer.Slot;
                    this.IndexBufferOffset = buffer.Offset;
                    this.IndexCount = buffer.Count;
                }
                else
                {
                    this.IndexBufferSlot = -1;
                    this.IndexBufferOffset = -1;
                    this.IndexCount = 0;
                }
            }
            /// <summary>
            /// Draw the patch terrain
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <param name="technique">Technique</param>
            public void DrawTerrain(DrawContext context, EffectTechnique technique)
            {
                if (this.IndexCount > 0)
                {
                    if (context.DrawerMode != DrawerModesEnum.ShadowMap)
                    {
                        Counters.InstancesPerFrame++;
                        Counters.PrimitivesPerFrame += this.IndexCount / 3;
                    }

                    for (int p = 0; p < technique.Description.PassCount; p++)
                    {
                        technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                        this.Game.Graphics.DeviceContext.DrawIndexed(this.IndexCount, this.IndexBufferOffset, this.VertexBufferOffset);

                        Counters.DrawCallsPerFrame++;
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
            /// Gets the instance text representation
            /// </summary>
            /// <returns>Returns the instance text representation</returns>
            public override string ToString()
            {
                return string.Format("LOD: {0}; Visible: {1}; Indices: {2}; {3}",
                    this.LevelOfDetail,
                    this.Visible,
                    this.IndexCount,
                    this.Current);
            }
        }
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        class TerrainIndexBuffer
        {
            /// <summary>
            /// Buffer Slot
            /// </summary>
            public int Slot;
            /// <summary>
            /// Buffer Offset
            /// </summary>
            public int Offset;
            /// <summary>
            /// Index count in buffer
            /// </summary>
            public int Count;
        }

        #endregion

        /// <summary>
        /// Patch dictionary
        /// </summary>
        private TerrainPatchDictionary patches = null;

        /// <summary>
        /// Gets the used material list
        /// </summary>
        public virtual MeshMaterial[] Materials
        {
            get
            {
                return new[] { this.patches.TerrainMaterial };
            }
        }
        /// <summary>
        /// Maximum number of instances
        /// </summary>
        public override int MaxInstances
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="content">Heightmap content</param>
        /// <param name="description">Terrain description</param>
        public Terrain(Game game, BufferManager bufferManager, HeightmapDescription content, GroundDescription description)
            : base(game, bufferManager, description)
        {
            //Initialize patch dictionary
            this.patches = new TerrainPatchDictionary(game, this.BufferManager, content, description);

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
                var visibleNodes = this.pickingQuadtree.GetNodesInVolume(ref context.Frustum);

                this.patches.Update(context, visibleNodes, this.BufferManager);
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
                this.patches.Draw(context, this.BufferManager);
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
            this.patches.BuildGeometry(
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

        /// <summary>
        /// Gets the visible nodes collection
        /// </summary>
        /// <returns>Returns a list of visible nodes</returns>
        public override PickingQuadTreeNode[] GetVisibleNodes()
        {
            return this.patches.GetVisibleNodes(LevelOfDetailEnum.High);
        }
    }

    /// <summary>
    /// Map grid
    /// </summary>
    public class MapGrid
    {
        /// <summary>
        /// High resolution nodes
        /// </summary>
        public MapGridNode[] NodesHigh = new MapGridNode[9];
        /// <summary>
        /// Medium resolution nodes
        /// </summary>
        public MapGridNode[] NodesMedium = new MapGridNode[16];
        /// <summary>
        /// Low resolution nodes
        /// </summary>
        public MapGridNode[] NodesLow = new MapGridNode[24];
        /// <summary>
        /// Minimum resolution nodes
        /// </summary>
        public MapGridNode[] NodesMinimum = new MapGridNode[32];

        /// <summary>
        /// Updates map from quad-tree and position
        /// </summary>
        /// <param name="tree">Quadtree</param>
        /// <param name="position">Position</param>
        public void Update(PickingQuadTree tree, Vector3 position)
        {
            
        }
    }
    /// <summary>
    /// Map grid node
    /// </summary>
    public class MapGridNode
    {
        /// <summary>
        /// Level of detail
        /// </summary>
        public LevelOfDetailEnum LevelOfDetail;
        /// <summary>
        /// Vertices
        /// </summary>
        public VertexTerrain[] Vertices;
        /// <summary>
        /// Shape
        /// </summary>
        public IndexBufferShapeEnum Shape;
        /// <summary>
        /// Vertex buffer slot
        /// </summary>
        public int VertexBufferSlot;
        /// <summary>
        /// Vertex buffer offset
        /// </summary>
        public int VertexBufferOffset;
        /// <summary>
        /// Index buffer slot
        /// </summary>
        public int IndexBufferSlot;
        /// <summary>
        /// Index buffer offset
        /// </summary>
        public int IndexBufferOffset;
        /// <summary>
        /// Indexes count
        /// </summary>
        public int IndexCount = 0;

        /// <summary>
        /// Changes level of detail
        /// </summary>
        /// <param name="newLOD">New level of detail</param>
        public void ChangeLOD(LevelOfDetailEnum newLOD)
        {
            if (newLOD > this.LevelOfDetail)
            {
                //Downgrade
            }
            else
            {
                //Upgrade
            }
        }
    }
}
