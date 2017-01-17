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
    using Engine.Collections;
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;
    using Engine.Helpers;
    using Engine.PathFinding;

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
            /// Maximum number of active buffer for foliage drawing
            /// </summary>
            public const int MaxFoliageBuffers = MaxPatchesHighLevel * 3;
            /// <summary>
            /// Maximum number of cached patches for foliage data
            /// </summary>
            public const int MaxFoliagePatches = MaxFoliageBuffers * 2;

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
            /// Foliage patches list
            /// </summary>
            private Dictionary<PickingQuadTreeNode, List<FoliagePatch>> foliagePatches = new Dictionary<PickingQuadTreeNode, List<FoliagePatch>>();
            /// <summary>
            /// Foliage buffer list
            /// </summary>
            private List<FoliageBuffer> foliageBuffers = new List<FoliageBuffer>();

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
            /// Wind total time
            /// </summary>
            private float windTime = 0;

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
            /// Random texture
            /// </summary>
            private ShaderResourceView textureRandom = null;
            /// <summary>
            /// Slope ranges
            /// </summary>
            private Vector2 slopeRanges = Vector2.Zero;

            /// <summary>
            /// Folliage map for vegetation planting task
            /// </summary>
            private FoliageMap foliageMap = null;
            /// <summary>
            /// Foliage map channels for vegetation planting task
            /// </summary>
            private FoliageMapChannel[] foliageMapChannels = null;

            /// <summary>
            /// Wind direction
            /// </summary>
            public Vector3 WindDirection;
            /// <summary>
            /// Wind strength
            /// </summary>
            public float WindStrength;
            /// <summary>
            /// Terrain material
            /// </summary>
            public MeshMaterial TerrainMaterial { get; protected set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="game">Game</param>
            /// <param name="trianglesPerNode">Triangles per node</param>
            /// <param name="foliageMap">Foliage map</param>
            /// <param name="foliageChannels">Foliage channels</param>
            public TerrainPatchDictionary(Game game, HeightmapDescription description, GroundDescription groundDescription)
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

                #region Read foliage data

                List<FoliageMapChannel> foliageChannels = new List<FoliageMapChannel>();

                if (groundDescription.Vegetation != null)
                {
                    //Read foliage textures
                    string contentPath = groundDescription.Vegetation.ContentPath;

                    ImageContent foliageMapImage = new ImageContent()
                    {
                        Streams = ContentManager.FindContent(contentPath, groundDescription.Vegetation.VegetationMap),
                    };
                    this.foliageMap = FoliageMap.FromStream(foliageMapImage.Stream);

                    for (int i = 0; i < groundDescription.Vegetation.Channels.Length; i++)
                    {
                        var channel = groundDescription.Vegetation.Channels[i];
                        if (channel != null && channel.VegetarionTextures != null && channel.VegetarionTextures.Length > 0)
                        {
                            ImageContent foliageTextures = new ImageContent()
                            {
                                Streams = ContentManager.FindContent(contentPath, channel.VegetarionTextures),
                            };

                            foliageChannels.Add(
                                new FoliageMapChannel()
                                {
                                    Index = i,
                                    Seed = channel.Seed,
                                    Saturation = channel.Saturation,
                                    MinSize = channel.MinSize,
                                    MaxSize = channel.MaxSize,
                                    EndRadius = channel.EndRadius,
                                    TextureCount = (uint)foliageTextures.Count,
                                    Textures = game.ResourceManager.CreateResource(foliageTextures),
                                    ToggleUV = channel.ToggleUV,
                                    WindEffect = channel.WindEffect,
                                });
                        }
                    }

                    this.foliageMapChannels = foliageChannels.ToArray();

                    for (int i = 0; i < MaxFoliageBuffers; i++)
                    {
                        this.foliageBuffers.Add(new FoliageBuffer(game));
                    }
                }

                #endregion

                #region Random texture generation

                this.textureRandom = game.ResourceManager.CreateResource(Guid.NewGuid(), 1024, -1, 1, 24);

                #endregion

                int trianglesPerNode = this.heightMap.CalcTrianglesPerNode(groundDescription.Quadtree.MaximumDepth);

                this.InitializeTerrainPatches(trianglesPerNode);
                this.InitializeTerrainIndices(trianglesPerNode);

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

                Helper.Dispose(this.foliageBuffers);
                Helper.Dispose(this.foliagePatches);
                Helper.Dispose(this.foliageMap);
                Helper.Dispose(this.foliageMapChannels);

                Helper.Dispose(this.terrainTexturesLR);
                Helper.Dispose(this.terrainTexturesHR);
                Helper.Dispose(this.terrainNormalMaps);
                Helper.Dispose(this.terrainSpecularMaps);
                Helper.Dispose(this.colorTextures);
                Helper.Dispose(this.alphaMap);
                Helper.Dispose(this.textureRandom);

                Helper.Dispose(this.TerrainMaterial);
            }

            /// <summary>
            /// Initialize patch dictionary
            /// </summary>
            /// <param name="trianglesPerNode">Triangles per node</param>
            private void InitializeTerrainPatches(int trianglesPerNode)
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
            private void InitializeTerrainIndices(int trianglesPerNode)
            {
                this.indices = new Dictionary<LevelOfDetailEnum, Dictionary<IndexBufferShapeEnum, TerrainIndexBuffer>>();

                this.indices.Add(LevelOfDetailEnum.High, new Dictionary<IndexBufferShapeEnum, TerrainIndexBuffer>());
                this.indices.Add(LevelOfDetailEnum.Medium, new Dictionary<IndexBufferShapeEnum, TerrainIndexBuffer>());
                this.indices.Add(LevelOfDetailEnum.Low, new Dictionary<IndexBufferShapeEnum, TerrainIndexBuffer>());

                //High level
                for (int i = 0; i < 9; i++)
                {
                    IndexBufferShapeEnum shape = (IndexBufferShapeEnum)i;

                    uint[] indexList = GeometryUtil.GenerateIndices(shape, trianglesPerNode);
                    TerrainIndexBuffer buffer = new TerrainIndexBuffer()
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
                    TerrainIndexBuffer buffer = new TerrainIndexBuffer()
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
                    TerrainIndexBuffer buffer = new TerrainIndexBuffer()
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
            /// <param name="visibleNodes">Visible nodes</param>
            public void Update(UpdateContext context, PickingQuadTreeNode[] visibleNodes)
            {
                this.windTime += context.GameTime.ElapsedSeconds * this.WindStrength;

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

                    #region Assign foliage patches

                    /*
                     * Foreach high lod visible node, look for planted foliagePatches.
                     * - If planted, see if they were assigned to a foliageBuffer
                     *   - If assigned, do nothing
                     *   - If not, add to toAssign list
                     * - If not planted, launch planting task y do nothing more. The node will be processed next time
                     */

                    List<FoliagePatch> toAssign = new List<FoliagePatch>();

                    var highNodes = tmp[LevelOfDetailEnum.High];

                    foreach (var node in highNodes)
                    {
                        if (!this.foliagePatches.ContainsKey(node))
                        {
                            this.foliagePatches.Add(node, new List<FoliagePatch>());

                            for (int i = 0; i < 3; i++)
                            {
                                this.foliagePatches[node].Add(new FoliagePatch(this.game));
                            }
                        }

                        var fPatchList = this.foliagePatches[node];

                        for (int i = 0; i < fPatchList.Count; i++)
                        {
                            var fPatch = fPatchList[i];

                            if (!fPatch.Planted)
                            {
                                fPatch.Plant(node, this.foliageMap, this.foliageMapChannels[i]);
                            }
                            else
                            {
                                if (!this.foliageBuffers.Exists(b => b.CurrentPatch == fPatch))
                                {
                                    toAssign.Add(fPatch);
                                }
                            }
                        }
                    }

                    /*
                     * For each node to assign
                     * - Look for a free buffer. It's free if unassigned or assigned to not visible node
                     *   - If free buffer found, assign
                     *   - If not, look for a buffer to free, fartests from camera first
                     */

                    if (toAssign.Count > 0)
                    {
                        //Sort nearest first
                        toAssign.Sort((f1, f2) =>
                        {
                            float d1 = Vector3.DistanceSquared(f1.CurrentNode.Center, context.EyePosition);
                            float d2 = Vector3.DistanceSquared(f2.CurrentNode.Center, context.EyePosition);

                            return d1.CompareTo(d2);
                        });

                        var freeBuffers = this.foliageBuffers.FindAll(b =>
                            b.CurrentPatch == null ||
                            (b.CurrentPatch != null && !highNodes.Contains(b.CurrentPatch.CurrentNode)));

                        if (freeBuffers.Count > 0)
                        {
                            //Sort free first and fartest first
                            freeBuffers.Sort((f1, f2) =>
                            {
                                float d1 = f1.CurrentPatch == null ? -1 : Vector3.DistanceSquared(f1.CurrentPatch.CurrentNode.Center, context.EyePosition);
                                float d2 = f2.CurrentPatch == null ? -1 : Vector3.DistanceSquared(f2.CurrentPatch.CurrentNode.Center, context.EyePosition);

                                return d2.CompareTo(d1);
                            });

                            while (toAssign.Count > 0 && freeBuffers.Count > 0)
                            {
                                freeBuffers[0].AttachFoliage(toAssign[0]);

                                toAssign.RemoveAt(0);
                                freeBuffers.RemoveAt(0);
                            }
                        }
                    }

                    //Free unussed patches
                    if (this.foliagePatches.Keys.Count > MaxFoliagePatches)
                    {
                        var nodes = this.foliagePatches.Keys.ToArray();
                        var notVisible = Array.FindAll(nodes, n => !Array.Exists(visibleNodes, v => v == n));
                        Array.Sort(notVisible, (n1, n2) =>
                        {
                            float d1 = Vector3.DistanceSquared(n1.Center, context.EyePosition);
                            float d2 = Vector3.DistanceSquared(n2.Center, context.EyePosition);

                            return d2.CompareTo(d1);
                        });

                        int toDelete = this.foliagePatches.Keys.Count - MaxFoliagePatches;
                        for (int i = 0; i < toDelete; i++)
                        {
                            this.foliagePatches.Remove(notVisible[i]);
                        }
                    }

                    #endregion
                }
            }
            /// <summary>
            /// Draw patches
            /// </summary>
            /// <param name="context">Drawing context</param>
            public void Draw(DrawContext context)
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
                                item.DrawTerrain(context, terrainTechnique);
                            }
                        }
                    }
                }

                var items = Array.FindAll(this.patches[LevelOfDetailEnum.High], p => p.Visible == true);
                if (items.Length > 0)
                {
                    foreach (var item in items)
                    {
                        var buffers = this.foliageBuffers.FindAll(b => b.CurrentPatch != null && b.CurrentPatch.CurrentNode == item.Current);
                        if (buffers.Count > 0)
                        {
                            foreach (var buffer in buffers)
                            {
                                var vegetationTechnique = this.SetTechniqueVegetation(context, buffer.CurrentPatch.Channel);
                                if (vegetationTechnique != null)
                                {
                                    buffer.DrawFoliage(context, vegetationTechnique);
                                }
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

                var technique = effect.GetTechnique(VertexTypes.Terrain, false, DrawingStages.Drawing, context.DrawerMode);

                this.game.Graphics.DeviceContext.InputAssembler.InputLayout = effect.GetInputLayout(technique);
                Counters.IAInputLayoutSets++;
                this.game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
                Counters.IAPrimitiveTopologySets++;

                return technique;
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
                    context.ViewProjection);

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

                var technique = effect.GetTechnique(VertexTypes.Terrain, false, DrawingStages.Drawing, context.DrawerMode);

                this.game.Graphics.DeviceContext.InputAssembler.InputLayout = effect.GetInputLayout(technique);
                Counters.IAInputLayoutSets++;
                this.game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
                Counters.IAPrimitiveTopologySets++;

                return technique;
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

                var technique = effect.GetTechnique(VertexTypes.Terrain, false, DrawingStages.Drawing, context.DrawerMode);

                this.game.Graphics.DeviceContext.InputAssembler.InputLayout = effect.GetInputLayout(technique);
                Counters.IAInputLayoutSets++;
                this.game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
                Counters.IAPrimitiveTopologySets++;

                return technique;
            }
            /// <summary>
            /// Sets thecnique for vegetation drawing
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <param name="channel">Channel</param>
            /// <returns>Returns the selected technique</returns>
            private EffectTechnique SetTechniqueVegetation(DrawContext context, int channel)
            {
                if (context.DrawerMode == DrawerModesEnum.Forward) return this.SetTechniqueVegetationDefault(context, channel);
                if (context.DrawerMode == DrawerModesEnum.ShadowMap) return this.SetTechniqueVegetationShadowMap(context, channel);
                else return null;
            }
            /// <summary>
            /// Sets thecnique for vegetation drawing with forward renderer
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <param name="channel">Channel</param>
            /// <returns>Returns the selected technique</returns>
            private EffectTechnique SetTechniqueVegetationDefault(DrawContext context, int channel)
            {
                EffectDefaultBillboard effect = DrawerPool.EffectDefaultBillboard;

                this.game.Graphics.SetBlendTransparent();

                #region Per frame update

                effect.UpdatePerFrame(
                    context.World,
                    context.ViewProjection,
                    context.EyePosition,
                    context.Lights,
                    context.ShadowMaps,
                    context.ShadowMapStatic,
                    context.ShadowMapDynamic,
                    context.FromLightViewProjection,
                    this.WindDirection,
                    this.WindStrength * this.foliageMapChannels[channel].WindEffect,
                    this.windTime * this.foliageMapChannels[channel].WindEffect,
                    this.textureRandom,
                    0,
                    this.foliageMapChannels[channel].EndRadius,
                    this.foliageMapChannels[channel].TextureCount,
                    0,
                    this.foliageMapChannels[channel].ToggleUV,
                    this.foliageMapChannels[channel].Textures);

                #endregion

                var technique = effect.GetTechnique(VertexTypes.Billboard, false, DrawingStages.Drawing, context.DrawerMode);

                this.game.Graphics.DeviceContext.InputAssembler.InputLayout = effect.GetInputLayout(technique);
                Counters.IAInputLayoutSets++;
                this.game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.PointList;
                Counters.IAPrimitiveTopologySets++;

                return technique;
            }
            /// <summary>
            /// Sets thecnique for vegetation drawing in shadow mapping
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <param name="channel">Channel</param>
            /// <returns>Returns the selected technique</returns>
            private EffectTechnique SetTechniqueVegetationShadowMap(DrawContext context, int channel)
            {
                EffectShadowBillboard effect = DrawerPool.EffectShadowBillboard;

                this.game.Graphics.SetBlendTransparent();

                #region Per frame update

                effect.UpdatePerFrame(
                    context.World,
                    context.ViewProjection,
                    context.EyePosition,
                    this.WindDirection,
                    this.WindStrength * this.foliageMapChannels[channel].WindEffect,
                    this.windTime * this.foliageMapChannels[channel].WindEffect,
                    this.textureRandom);

                #endregion

                #region Per object update

                effect.UpdatePerObject(
                    0,
                    this.foliageMapChannels[channel].EndRadius,
                    this.foliageMapChannels[channel].TextureCount,
                    this.foliageMapChannels[channel].ToggleUV,
                    this.foliageMapChannels[channel].Textures);

                #endregion

                var technique = effect.GetTechnique(VertexTypes.Billboard, false, DrawingStages.Drawing, context.DrawerMode);

                this.game.Graphics.DeviceContext.InputAssembler.InputLayout = effect.GetInputLayout(technique);
                Counters.IAInputLayoutSets++;
                this.game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.PointList;
                Counters.IAPrimitiveTopologySets++;

                return technique;
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

                    int vertices = (int)Math.Pow((Math.Sqrt(triangleCount / 2) + 1), 2);

                    VertexTerrain[] vertexData = new VertexTerrain[vertices];

                    patch.vertexBuffer = game.Graphics.Device.CreateVertexBufferWrite(vertexData);
                    patch.vertexBufferBinding = new[]
                    {
                        new VertexBufferBinding(patch.vertexBuffer, default(VertexTerrain).GetStride(), 0),
                    };

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
                Helper.Dispose(this.indexBuffer);
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

                            this.Game.Graphics.DeviceContext.WriteVertexBuffer(this.vertexBuffer, data);
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
                        Counters.PrimitivesPerFrame += this.indexCount / 3;
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
                    this.indexCount,
                    this.Current);
            }
        }
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        class TerrainIndexBuffer : IDisposable
        {
            /// <summary>
            /// Buffer
            /// </summary>
            public Buffer Buffer;
            /// <summary>
            /// Index count in buffer
            /// </summary>
            public int Count;

            /// <summary>
            /// Dispose
            /// </summary>
            public void Dispose()
            {
                Helper.Dispose(this.Buffer);
            }
        }
        /// <summary>
        /// Foliage patch
        /// </summary>
        class FoliagePatch : IDisposable
        {
            /// <summary>
            /// Maximum number of elements in patch
            /// </summary>
            public const int MAX = 1024 * 8;

            /// <summary>
            /// Game
            /// </summary>
            protected readonly Game Game = null;
            /// <summary>
            /// Foliage populating flag
            /// </summary>
            protected bool Planting = false;

            /// <summary>
            /// Foliage populated flag
            /// </summary>
            public bool Planted = false;
            /// <summary>
            /// Foliage generated data
            /// </summary>
            public VertexBillboard[] FoliageData = null;
            /// <summary>
            /// Gets the node to wich this patch is currently assigned
            /// </summary>
            public PickingQuadTreeNode CurrentNode { get; protected set; }
            /// <summary>
            /// Foliage map channel
            /// </summary>
            public int Channel { get; protected set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="game">Game</param>
            public FoliagePatch(Game game)
            {
                this.Game = game;

                this.CurrentNode = null;
                this.Channel = -1;
            }

            /// <summary>
            /// Launchs foliage population asynchronous task
            /// </summary>
            /// <param name="node">Node</param>
            /// <param name="map">Foliage map</param>
            /// <param name="description">Terrain vegetation description</param>
            public void Plant(PickingQuadTreeNode node, FoliageMap map, FoliageMapChannel description)
            {
                if (!this.Planting)
                {
                    //Start planting task
                    this.CurrentNode = node;
                    this.Channel = description.Index;
                    this.Planting = true;

                    var t = Task.Factory.StartNew<VertexBillboard[]>(() => PlantTask(node, map, description), TaskCreationOptions.PreferFairness);

                    t.ContinueWith(task => PlantThreadCompleted(task.Result));
                }
            }
            /// <summary>
            /// Asynchronous planting task
            /// </summary>
            /// <param name="node">Node to process</param>
            /// <param name="map">Foliage map</param>
            /// <param name="description">Vegetation task</param>
            /// <returns>Returns generated vertex data</returns>
            private static VertexBillboard[] PlantTask(PickingQuadTreeNode node, FoliageMap map, FoliageMapChannel description)
            {
                List<VertexBillboard> vertexData = new List<VertexBillboard>(MAX);

                if (node != null)
                {
                    var root = node.QuadTree;
                    Vector2 min = new Vector2(root.BoundingBox.Minimum.X, root.BoundingBox.Minimum.Z);
                    Vector2 max = new Vector2(root.BoundingBox.Maximum.X, root.BoundingBox.Maximum.Z);

                    Random rnd = new Random(description.Seed);
                    BoundingBox bbox = node.BoundingBox;
                    int count = (int)Math.Min(MAX, MAX * description.Saturation);

                    //Number of points
                    while (count > 0)
                    {
                        Vector3 pos = new Vector3(
                            rnd.NextFloat(bbox.Minimum.X, bbox.Maximum.X),
                            bbox.Maximum.Y + 1f,
                            rnd.NextFloat(bbox.Minimum.Z, bbox.Maximum.Z));

                        bool plant = false;
                        if (map != null)
                        {
                            Color4 c = map.GetRelative(pos, min, max);

                            if (c[description.Index] > 0)
                            {
                                plant = rnd.NextFloat(0, 1) < (c[description.Index]);
                            }
                        }
                        else
                        {
                            plant = true;
                        }

                        if (plant)
                        {
                            Ray ray = new Ray(pos, Vector3.Down);

                            Vector3 intersectionPoint;
                            Triangle t;
                            if (node.PickFirst(ref ray, out intersectionPoint, out t))
                            {
                                if (t.Normal.Y > 0.5f)
                                {
                                    vertexData.Add(new VertexBillboard()
                                    {
                                        Position = intersectionPoint,
                                        Size = new Vector2(
                                            rnd.NextFloat(description.MinSize.X, description.MaxSize.X),
                                            rnd.NextFloat(description.MinSize.Y, description.MaxSize.Y)),
                                    });
                                }
                            }
                        }

                        count--;
                    }
                }

                return vertexData.ToArray();
            }
            /// <summary>
            /// Planting task completed
            /// </summary>
            /// <param name="vData">Vertex data generated in asynchronous task</param>
            private void PlantThreadCompleted(VertexBillboard[] vData)
            {
                this.Planting = false;
                this.Planted = true;
                this.FoliageData = vData;
            }

            /// <summary>
            /// Dispose
            /// </summary>
            public void Dispose()
            {
                this.FoliageData = null;
            }
        }
        /// <summary>
        /// Foliage map channel
        /// </summary>
        class FoliageMapChannel : IDisposable
        {
            /// <summary>
            /// Channel index
            /// </summary>
            public int Index;
            /// <summary>
            /// Random seed
            /// </summary>
            public int Seed;
            /// <summary>
            /// Point saturation
            /// </summary>
            public float Saturation;
            /// <summary>
            /// Billboard minimum size
            /// </summary>
            public Vector2 MinSize;
            /// <summary>
            /// Billboard maximum size
            /// </summary>
            public Vector2 MaxSize;
            /// <summary>
            /// Foliage textures
            /// </summary>
            public ShaderResourceView Textures;
            /// <summary>
            /// Foliage texture count
            /// </summary>
            public uint TextureCount;
            /// <summary>
            /// Toggles UV horizontal coordinate by primitive ID
            /// </summary>
            public bool ToggleUV;
            /// <summary>
            /// Foliage end radius
            /// </summary>
            public float EndRadius;
            /// <summary>
            /// Wind effect
            /// </summary>
            public float WindEffect;

            /// <summary>
            /// Dispose
            /// </summary>
            public void Dispose()
            {
                Helper.Dispose(this.Textures);
            }
        }
        /// <summary>
        /// Foliage buffer
        /// </summary>
        class FoliageBuffer : IDisposable
        {
            /// <summary>
            /// Vertex buffer with foliage data
            /// </summary>
            private Buffer buffer = null;
            /// <summary>
            /// Vertex count
            /// </summary>
            private int vertexCount = 0;
            /// <summary>
            /// Vertex buffer binding for foliage buffer
            /// </summary>
            private VertexBufferBinding[] bufferBinding = null;

            /// <summary>
            /// Game
            /// </summary>
            protected readonly Game Game = null;

            /// <summary>
            /// Foliage attached to buffer flag
            /// </summary>
            public bool Attached = false;
            /// <summary>
            /// Current attached patch
            /// </summary>
            public FoliagePatch CurrentPatch { get; protected set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="game">Game</param>
            public FoliageBuffer(Game game)
            {
                this.Game = game;

                VertexBillboard[] vertexData = new VertexBillboard[FoliagePatch.MAX];

                this.buffer = this.Game.Graphics.Device.CreateVertexBufferWrite(vertexData);
                this.bufferBinding = new[]
                {
                    new VertexBufferBinding(this.buffer, default(VertexBillboard).GetStride(), 0),
                };
            }
            /// <summary>
            /// Dispose
            /// </summary>
            public void Dispose()
            {
                Helper.Dispose(this.buffer);
            }

            /// <summary>
            /// Attachs the specified patch to buffer
            /// </summary>
            /// <param name="patch">Patch</param>
            public void AttachFoliage(FoliagePatch patch)
            {
                this.vertexCount = 0;
                this.Attached = false;
                this.CurrentPatch = null;

                if (patch.FoliageData != null && patch.FoliageData.Length > 0)
                {
                    //Attach data
                    this.Game.Graphics.DeviceContext.WriteBuffer(this.buffer, patch.FoliageData);

                    this.vertexCount = patch.FoliageData.Length;
                    this.Attached = true;
                    this.CurrentPatch = patch;
                }
            }
            /// <summary>
            /// Draws the foliage data
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <param name="technique">Technique</param>
            public void DrawFoliage(DrawContext context, EffectTechnique technique)
            {
                if (this.vertexCount > 0)
                {
                    //Sets vertex and index buffer
                    this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, this.bufferBinding);
                    Counters.IAVertexBuffersSets++;
                    this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(null, SharpDX.DXGI.Format.R32_UInt, 0);
                    Counters.IAIndexBufferSets++;

                    for (int p = 0; p < technique.Description.PassCount; p++)
                    {
                        technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                        this.Game.Graphics.DeviceContext.Draw(this.vertexCount, 0);

                        Counters.DrawCallsPerFrame++;
                        Counters.InstancesPerFrame++;
                        Counters.PrimitivesPerFrame += this.vertexCount;
                    }
                }
            }
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
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="content">Heightmap content</param>
        /// <param name="description">Terrain description</param>
        public Terrain(Game game, HeightmapDescription content, GroundDescription description)
            : base(game, description)
        {
            //Initialize patch dictionary
            this.patches = new TerrainPatchDictionary(game, content, description);

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

                this.patches.Update(context, visibleNodes);
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
                this.patches.Draw(context);
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
        /// Sets wind parameters
        /// </summary>
        /// <param name="direction">Direction</param>
        /// <param name="strength">Strength</param>
        public void SetWind(Vector3 direction, float strength)
        {
            if (this.patches != null)
            {
                this.patches.WindDirection = direction;
                this.patches.WindStrength = strength;
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
