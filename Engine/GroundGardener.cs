using SharpDX;
using SharpDX.Direct3D;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Collections;
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Ground garden planter
    /// </summary>
    public class GroundGardener : Drawable, UseMaterials, IDisposable
    {
        #region Helper classes

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
            /// Foliage generated data
            /// </summary>
            private VertexBillboard[] foliageData = null;

            /// <summary>
            /// Foliage populating flag
            /// </summary>
            public bool Planting { get; protected set; }
            /// <summary>
            /// Foliage populated flag
            /// </summary>
            public bool Planted { get; protected set; }
            /// <summary>
            /// Gets the node to wich this patch is currently assigned
            /// </summary>
            public QuadTreeNode CurrentNode { get; protected set; }
            /// <summary>
            /// Foliage map channel
            /// </summary>
            public int Channel { get; protected set; }
            /// <summary>
            /// Returns true if the path has foliage data
            /// </summary>
            public bool HasData
            {
                get
                {
                    return this.foliageData != null && this.foliageData.Length > 0;
                }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            public FoliagePatch()
            {
                this.Planted = false;
                this.Planting = false;

                this.CurrentNode = null;
                this.Channel = -1;
            }
            /// <summary>
            /// Dispose
            /// </summary>
            public void Dispose()
            {
                this.foliageData = null;
            }

            /// <summary>
            /// Launchs foliage population asynchronous task
            /// </summary>
            /// <param name="scene">Scene</param>
            /// <param name="node">Foliage Node</param>
            /// <param name="map">Foliage map</param>
            /// <param name="description">Terrain vegetation description</param>
            /// <param name="delay">Task delay</param>
            public void Plant(Scene scene, QuadTreeNode node, FoliageMap map, FoliageMapChannel description, int delay)
            {
                if (!this.Planting)
                {
                    //Start planting task
                    this.CurrentNode = node;
                    this.Channel = description.Index;
                    this.Planting = true;

                    var task = Task.Run(async () =>
                    {
                        await Task.Delay(delay);

                        return PlantTask(scene, node, map, description);
                    });

                    task.ContinueWith((t) =>
                    {
                        this.Planting = false;
                        this.Planted = true;
                        this.foliageData = t.Result;
                    });
                }
            }
            /// <summary>
            /// Asynchronous planting task
            /// </summary>
            /// <param name="scene">Scene</param>
            /// <param name="node">Node to process</param>
            /// <param name="map">Foliage map</param>
            /// <param name="description">Vegetation task</param>
            /// <returns>Returns generated vertex data</returns>
            private static VertexBillboard[] PlantTask(Scene scene, QuadTreeNode node, FoliageMap map, FoliageMapChannel description)
            {
                List<VertexBillboard> vertexData = new List<VertexBillboard>(MAX);

                if (node != null)
                {
                    BoundingBox gbbox = scene.GetBoundingBox();

                    Vector2 min = new Vector2(gbbox.Minimum.X, gbbox.Minimum.Z);
                    Vector2 max = new Vector2(gbbox.Maximum.X, gbbox.Maximum.Z);

                    Random rnd = new Random(description.Seed);
                    BoundingBox bbox = node.BoundingBox;
                    int count = (int)Math.Min(MAX, MAX * description.Saturation);
                    int iterations = MAX * 2;

                    //Number of points
                    while (count > 0 && iterations > 0)
                    {
                        iterations--;

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
                            Vector3 intersectionPoint;
                            Triangle t;
                            float d;
                            if (scene.FindTopGroundPosition(pos.X, pos.Z, out intersectionPoint, out t, out d))
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

                                    count--;
                                }
                            }
                        }
                        else
                        {
                            count--;
                        }
                    }
                }

                return vertexData.ToArray();
            }
            /// <summary>
            /// Get foliage data
            /// </summary>
            /// <param name="eyePosition">Eye position</param>
            /// <param name="transparent">Use transparency</param>
            /// <returns>Returns the foliage data ordered by distance to eye position. Far first if transparency specified, near first otherwise</returns>
            /// <remarks>Returns the foliage data</remarks>
            public VertexBillboard[] GetData(Vector3 eyePosition, bool transparent)
            {
                //Sort data
                Array.Sort(this.foliageData, (f1, f2) =>
                {
                    float d1 = Vector3.DistanceSquared(f1.Position, eyePosition);
                    float d2 = Vector3.DistanceSquared(f2.Position, eyePosition);

                    var res = d1.CompareTo(d2);

                    return transparent ? -res : res;
                });

                return this.foliageData;
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
            /// Delta
            /// </summary>
            public Vector3 Delta;
            /// <summary>
            /// Foliage textures
            /// </summary>
            public EngineShaderResourceView Textures;
            /// <summary>
            /// Foliage normal maps
            /// </summary>
            public EngineShaderResourceView NormalMaps;
            /// <summary>
            /// Foliage texture count
            /// </summary>
            public uint TextureCount;
            /// <summary>
            /// Foliage normal map count
            /// </summary>
            public uint NormalMapCount;
            /// <summary>
            /// Foliage start radius
            /// </summary>
            public float StartRadius;
            /// <summary>
            /// Foliage end radius
            /// </summary>
            public float EndRadius;
            /// <summary>
            /// Wind effect
            /// </summary>
            public float WindEffect;
            /// <summary>
            /// Gometry output count
            /// </summary>
            public int Count;

            /// <summary>
            /// Dispose
            /// </summary>
            public void Dispose()
            {
                Helper.Dispose(this.Textures);
                Helper.Dispose(this.NormalMaps);
            }
        }
        /// <summary>
        /// Foliage buffer
        /// </summary>
        class FoliageBuffer : IDisposable
        {
            /// <summary>
            /// Foliage buffer id static counter
            /// </summary>
            private static int ID = 0;

            /// <summary>
            /// Vertex count
            /// </summary>
            private int vertexDrawCount = 0;

            /// <summary>
            /// Game
            /// </summary>
            protected readonly Game Game = null;

            /// <summary>
            /// Buffer id
            /// </summary>
            public readonly int Id = 0;
            /// <summary>
            /// Foliage attached to buffer flag
            /// </summary>
            public bool Attached { get; protected set; }
            /// <summary>
            /// Current attached patch
            /// </summary>
            public FoliagePatch CurrentPatch { get; protected set; }
            /// <summary>
            /// Vertex buffer descriptor
            /// </summary>
            public BufferDescriptor VertexBuffer = null;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="game">Game</param>
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="name">Name</param>
            public FoliageBuffer(Game game, BufferManager bufferManager, string name)
            {
                this.Game = game;
                this.Id = ++ID;
                this.Attached = false;
                this.CurrentPatch = null;
                this.VertexBuffer = bufferManager.Add(string.Format("{1}.{0}", this.Id, name), new VertexBillboard[FoliagePatch.MAX], true, 0);
            }
            /// <summary>
            /// Dispose
            /// </summary>
            public void Dispose()
            {

            }

            /// <summary>
            /// Attachs the specified patch to buffer
            /// </summary>
            /// <param name="eyePosition">Eye position</param>
            /// <param name="transparent">The billboards were transparent</param>
            /// <param name="patch">Patch</param>
            /// <param name="bufferManager">Buffer manager</param>
            public void AttachFoliage(Vector3 eyePosition, bool transparent, FoliagePatch patch, BufferManager bufferManager)
            {
                this.vertexDrawCount = 0;
                this.Attached = false;
                this.CurrentPatch = null;

                if (patch.HasData)
                {
                    var data = patch.GetData(eyePosition, transparent);

                    //Attach data
                    bufferManager.WriteBuffer(
                        this.VertexBuffer.Slot,
                        this.VertexBuffer.Offset,
                        data);

                    this.vertexDrawCount = data.Length;
                    this.Attached = true;
                    this.CurrentPatch = patch;
                }
            }
            /// <summary>
            /// Draw foliage shadows
            /// </summary>
            /// <param name="context">Context</param>
            /// <param name="technique">Technique</param>
            public void DrawFoliageShadows(DrawContextShadows context, EngineEffectTechnique technique)
            {
                if (this.vertexDrawCount > 0)
                {
                    var graphics = this.Game.Graphics;

                    for (int p = 0; p < technique.PassCount; p++)
                    {
                        graphics.EffectPassApply(technique, p, 0);

                        graphics.Draw(this.vertexDrawCount, this.VertexBuffer.Offset);
                    }
                }
            }
            /// <summary>
            /// Draws the foliage data
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <param name="technique">Technique</param>
            public void DrawFoliage(DrawContext context, EngineEffectTechnique technique)
            {
                if (this.vertexDrawCount > 0)
                {
                    var graphics = this.Game.Graphics;

                    Counters.InstancesPerFrame++;
                    Counters.PrimitivesPerFrame += this.vertexDrawCount / 3;

                    for (int p = 0; p < technique.PassCount; p++)
                    {
                        graphics.EffectPassApply(technique, p, 0);

                        graphics.Draw(this.vertexDrawCount, this.VertexBuffer.Offset);
                    }
                }
            }

            /// <summary>
            /// Gets the text representation of the buffer
            /// </summary>
            /// <returns>Returns the text representation of the buffer</returns>
            public override string ToString()
            {
                return string.Format("{0} => Attached: {1}; HasPatch: {2}", this.Id, this.Attached, this.CurrentPatch != null);
            }
        }

        #endregion

        /// <summary>
        /// Maximum number of active buffer for foliage drawing
        /// </summary>
        public const int MaxFoliageBuffers = 32;
        /// <summary>
        /// Maximum number of cached patches for foliage data
        /// </summary>
        public const int MaxFoliagePatches = MaxFoliageBuffers * 2;

        /// <summary>
        /// Foliage patches list
        /// </summary>
        private Dictionary<QuadTreeNode, List<FoliagePatch>> foliagePatches = new Dictionary<QuadTreeNode, List<FoliagePatch>>();
        /// <summary>
        /// Foliage buffer list
        /// </summary>
        private List<FoliageBuffer> foliageBuffers = new List<FoliageBuffer>();
        /// <summary>
        /// Wind total time
        /// </summary>
        private float windTime = 0;
        /// <summary>
        /// Random texture
        /// </summary>
        private EngineShaderResourceView textureRandom = null;
        /// <summary>
        /// Folliage map for vegetation planting task
        /// </summary>
        private FoliageMap foliageMap = null;
        /// <summary>
        /// Foliage map channels for vegetation planting task
        /// </summary>
        private FoliageMapChannel[] foliageMapChannels = null;
        /// <summary>
        /// Material
        /// </summary>
        private MeshMaterial material;
        /// <summary>
        /// Foliage visible sphere
        /// </summary>
        private BoundingSphere foliageSphere;
        /// <summary>
        /// Foliage node size
        /// </summary>
        private float foliageNodeSize;
        /// <summary>
        /// Foliage quadtree
        /// </summary>
        private QuadTree foliageQuadtree;
        /// <summary>
        /// Last visible node collection
        /// </summary>
        private QuadTreeNode[] visibleNodes;

        /// <summary>
        /// Material
        /// </summary>
        public MeshMaterial[] Materials
        {
            get
            {
                return new[] { this.material };
            }
        }
        /// <summary>
        /// Current active planting tasks
        /// </summary>
        public int PlantingTasks { get; private set; }

        /// <summary>
        /// Wind direction
        /// </summary>
        public Vector3 WindDirection;
        /// <summary>
        /// Wind strength
        /// </summary>
        public float WindStrength;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="description">Description</param>
        public GroundGardener(Scene scene, GroundGardenerDescription description) :
            base(scene, description)
        {
            this.textureRandom = this.Game.ResourceManager.CreateResource(Guid.NewGuid(), 1024, -1, 1, 24);

            List<FoliageMapChannel> foliageChannels = new List<FoliageMapChannel>();

            if (description != null)
            {
                this.foliageSphere = new BoundingSphere(Vector3.Zero, description.VisibleRadius);
                this.foliageNodeSize = description.NodeSize;

                //Material
                this.material = new MeshMaterial()
                {
                    Material = description.Material != null ? description.Material.GetMaterial() : Material.Default
                };

                //Read foliage textures
                string contentPath = description.ContentPath;

                if (!string.IsNullOrEmpty(description.VegetationMap))
                {
                    var foliageMapImage = new ImageContent()
                    {
                        Streams = ContentManager.FindContent(contentPath, description.VegetationMap),
                    };
                    this.foliageMap = FoliageMap.FromStream(foliageMapImage.Stream);
                }

                for (int i = 0; i < description.Channels.Length; i++)
                {
                    var channel = description.Channels[i];
                    if (channel != null && channel.Enabled)
                    {
                        EngineShaderResourceView foliageTextures = null;
                        EngineShaderResourceView foliageNormalMaps = null;
                        int textureCount = channel.VegetationTextures != null ? channel.VegetationTextures.Length : 0;
                        int normalMapCount = channel.VegetationNormalMaps != null ? channel.VegetationNormalMaps.Length : 0;

                        if (normalMapCount != 0 && normalMapCount != textureCount)
                        {
                            throw new EngineException("Normal map arrays must have the same slices than diffuse texture arrays");
                        }

                        if (textureCount > 0)
                        {
                            var image = new ImageContent()
                            {
                                Streams = ContentManager.FindContent(contentPath, channel.VegetationTextures),
                            };

                            foliageTextures = this.Game.ResourceManager.CreateResource(image);
                        }

                        if (normalMapCount > 0)
                        {
                            var image = new ImageContent()
                            {
                                Streams = ContentManager.FindContent(contentPath, channel.VegetationNormalMaps),
                            };

                            foliageNormalMaps = this.Game.ResourceManager.CreateResource(image);
                        }

                        foliageChannels.Add(
                            new FoliageMapChannel()
                            {
                                Index = i,
                                Seed = channel.Seed,
                                Saturation = channel.Saturation,
                                MinSize = channel.MinSize,
                                MaxSize = channel.MaxSize,
                                Delta = channel.Delta,
                                StartRadius = channel.StartRadius,
                                EndRadius = channel.EndRadius,
                                TextureCount = (uint)textureCount,
                                NormalMapCount = (uint)normalMapCount,
                                Textures = foliageTextures,
                                NormalMaps = foliageNormalMaps,
                                WindEffect = channel.WindEffect,
                                Count = channel.Count,
                            });
                    }
                }

                this.foliageMapChannels = foliageChannels.ToArray();

                for (int i = 0; i < MaxFoliageBuffers; i++)
                {
                    this.foliageBuffers.Add(new FoliageBuffer(this.Game, this.BufferManager, description.Name));
                }
            }
        }
        /// <summary>
        /// Resource disposal
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.foliageBuffers);
            Helper.Dispose(this.foliagePatches);
            Helper.Dispose(this.foliageMap);
            Helper.Dispose(this.foliageMapChannels);

            Helper.Dispose(this.textureRandom);
        }

        /// <summary>
        /// Updates the gardener
        /// </summary>
        /// <param name="context">Update context</param>
        public override void Update(UpdateContext context)
        {
            this.windTime += context.GameTime.ElapsedSeconds * this.WindStrength;

            if (this.foliageQuadtree == null)
            {
                BoundingBox bbox = this.Scene.GetBoundingBox();

                float x = bbox.GetX();
                float z = bbox.GetZ();

                float max = x < z ? z : x;

                int levels = Math.Max(1, (int)(max / this.foliageNodeSize));
                levels = Math.Min(6, levels);

                this.foliageQuadtree = new QuadTree(bbox, levels);
            }

            this.foliageSphere.Center = context.EyePosition;

            this.visibleNodes = this.GetFoliageNodes(context.Frustum, this.foliageSphere);

            if (this.visibleNodes != null && this.visibleNodes.Length > 0)
            {
                bool transparent = this.Description.AlphaEnabled;

                //Sort fartest first
                Array.Sort(this.visibleNodes, (f1, f2) =>
                {
                    float d1 = Vector3.DistanceSquared(f1.Center, context.EyePosition);
                    float d2 = Vector3.DistanceSquared(f2.Center, context.EyePosition);

                    var res = d1.CompareTo(d2);

                    return transparent ? -res : res;
                });

                #region Assign foliage patches

                /*
                 * Foreach high lod visible node, look for planted foliagePatches.
                 * - If planted, see if they were assigned to a foliageBuffer
                 *   - If assigned, do nothing
                 *   - If not, add to toAssign list
                 * - If not planted, launch planting task y do nothing more. The node will be processed next time
                 */

                List<FoliagePatch> toAssign = new List<FoliagePatch>();

                int count = this.foliageMapChannels.Length;

                foreach (var node in this.visibleNodes)
                {
                    if (!this.foliagePatches.ContainsKey(node))
                    {
                        this.foliagePatches.Add(node, new List<FoliagePatch>());

                        for (int i = 0; i < count; i++)
                        {
                            this.foliagePatches[node].Add(new FoliagePatch());
                        }
                    }

                    var fPatchList = this.foliagePatches[node];

                    for (int i = 0; i < fPatchList.Count; i++)
                    {
                        var fPatch = fPatchList[i];

                        if (!fPatch.Planted)
                        {
                            fPatch.Plant(this.Scene, node, this.foliageMap, this.foliageMapChannels[i], i * 100);
                        }
                        else
                        {
                            if (!this.foliageBuffers.Exists(b => b.CurrentPatch == fPatch))
                            {
                                if (fPatch.HasData)
                                {
                                    toAssign.Add(fPatch);
                                }
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
                        (b.CurrentPatch == null) ||
                        (b.CurrentPatch != null && !Array.Exists(this.visibleNodes, n => n == b.CurrentPatch.CurrentNode)));

                    if (freeBuffers.Count > 0)
                    {
                        //Sort free first and fartest first
                        freeBuffers.Sort((f1, f2) =>
                        {
                            float d1 = f1.CurrentPatch == null ? -1 : Vector3.DistanceSquared(f1.CurrentPatch.CurrentNode.Center, context.EyePosition);
                            float d2 = f2.CurrentPatch == null ? -1 : Vector3.DistanceSquared(f2.CurrentPatch.CurrentNode.Center, context.EyePosition);

                            return -d1.CompareTo(d2);
                        });

                        while (toAssign.Count > 0 && freeBuffers.Count > 0)
                        {
                            freeBuffers[0].AttachFoliage(context.EyePosition, this.Description.AlphaEnabled, toAssign[0], this.BufferManager);

                            toAssign.RemoveAt(0);
                            freeBuffers.RemoveAt(0);
                        }
                    }
                }

                //Free unussed patches
                if (this.foliagePatches.Keys.Count > MaxFoliagePatches)
                {
                    var nodes = this.foliagePatches.Keys.ToArray();
                    var notVisible = Array.FindAll(nodes, n => !Array.Exists(this.visibleNodes, v => v == n));
                    if (notVisible.Length > 0)
                    {
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
                }

                #endregion
            }

            this.PlantingTasks = 0;

            foreach (var q in this.foliagePatches)
            {
                this.PlantingTasks += q.Value.FindAll(f => f.Planting == true).Count;
            }
        }
        /// <summary>
        /// Draws the gardener shadows
        /// </summary>
        /// <param name="context">Context</param>
        public override void DrawShadows(DrawContextShadows context)
        {
            if (this.visibleNodes != null && this.visibleNodes.Length > 0)
            {
                var graphics = this.Game.Graphics;

                graphics.SetBlendDefaultAlpha();

                foreach (var item in this.visibleNodes)
                {
                    var buffers = this.foliageBuffers.FindAll(b => b.CurrentPatch != null && b.CurrentPatch.CurrentNode == item);
                    if (buffers.Count > 0)
                    {
                        foreach (var buffer in buffers)
                        {
                            var vegetationTechnique = this.SetTechniqueVegetationShadowMap(context, buffer.CurrentPatch.Channel);
                            if (vegetationTechnique != null)
                            {
                                this.BufferManager.SetInputAssembler(
                                    vegetationTechnique,
                                    buffer.VertexBuffer.Slot,
                                    PrimitiveTopology.PointList);

                                buffer.DrawFoliageShadows(context, vegetationTechnique);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Draws the gardener
        /// </summary>
        /// <param name="context">Drawing context</param>
        public override void Draw(DrawContext context)
        {
            var mode = context.DrawerMode;
            var graphics = this.Game.Graphics;

            if ((mode.HasFlag(DrawerModesEnum.OpaqueOnly) && !this.Description.AlphaEnabled) ||
                (mode.HasFlag(DrawerModesEnum.TransparentOnly) && this.Description.AlphaEnabled))
            {
                if (this.visibleNodes != null && this.visibleNodes.Length > 0)
                {
                    graphics.SetBlendDefaultAlpha();

                    foreach (var item in this.visibleNodes)
                    {
                        var buffers = this.foliageBuffers.FindAll(b => b.CurrentPatch != null && b.CurrentPatch.CurrentNode == item);
                        if (buffers.Count > 0)
                        {
                            foreach (var buffer in buffers)
                            {
                                var vegetationTechnique = this.SetTechniqueVegetationDefault(context, buffer.CurrentPatch.Channel);
                                if (vegetationTechnique != null)
                                {
                                    this.BufferManager.SetInputAssembler(
                                        vegetationTechnique,
                                        buffer.VertexBuffer.Slot,
                                        PrimitiveTopology.PointList);

                                    buffer.DrawFoliage(context, vegetationTechnique);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets thecnique for vegetation drawing with forward renderer
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="channel">Channel</param>
        /// <returns>Returns the selected technique</returns>
        private EngineEffectTechnique SetTechniqueVegetationDefault(DrawContext context, int channel)
        {
            var channelData = this.foliageMapChannels[channel];

            var effect = DrawerPool.EffectDefaultFoliage;

            #region Per frame update

            effect.UpdatePerFrame(
                context.ViewProjection,
                context.EyePosition,
                context.Lights,
                context.ShadowMaps,
                context.ShadowMapLow.Texture,
                context.ShadowMapHigh.Texture,
                context.ShadowMapLow.FromLightViewProjectionArray[0],
                context.ShadowMapHigh.FromLightViewProjectionArray[0],
                this.WindDirection,
                this.WindStrength * channelData.WindEffect,
                this.windTime * channelData.WindEffect,
                channelData.Delta,
                this.textureRandom,
                channelData.StartRadius,
                channelData.EndRadius,
                this.material.ResourceIndex,
                channelData.TextureCount,
                channelData.NormalMapCount,
                channelData.Textures,
                channelData.NormalMaps);

            #endregion

            if (channelData.Count == 1) return effect.ForwardFoliage4;
            if (channelData.Count == 2) return effect.ForwardFoliage8;
            if (channelData.Count == 4) return effect.ForwardFoliage16;
            else return null;
        }
        /// <summary>
        /// Sets thecnique for vegetation drawing in shadow mapping
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="channel">Channel</param>
        /// <returns>Returns the selected technique</returns>
        private EngineEffectTechnique SetTechniqueVegetationShadowMap(DrawContextShadows context, int channel)
        {
            var channelData = this.foliageMapChannels[channel];

            var effect = DrawerPool.EffectShadowFoliage;

            #region Per frame update

            effect.UpdatePerFrame(
                context.ViewProjection,
                context.EyePosition,
                this.WindDirection,
                this.WindStrength * channelData.WindEffect,
                this.windTime * channelData.WindEffect,
                channelData.Delta,
                this.textureRandom);

            #endregion

            #region Per object update

            effect.UpdatePerObject(
                channelData.StartRadius,
                channelData.EndRadius,
                channelData.TextureCount,
                channelData.Textures);

            #endregion

            if (channelData.Count == 1) return effect.ShadowMapFoliage4;
            if (channelData.Count == 2) return effect.ShadowMapFoliage8;
            if (channelData.Count == 4) return effect.ShadowMapFoliage16;
            else return null;
        }

        /// <summary>
        /// Sets wind parameters
        /// </summary>
        /// <param name="direction">Direction</param>
        /// <param name="strength">Strength</param>
        public void SetWind(Vector3 direction, float strength)
        {
            this.WindDirection = direction;
            this.WindStrength = strength;
        }

        /// <summary>
        /// Gets the node list suitable for foliage planting
        /// </summary>
        /// <param name="frustum">Camera frustum</param>
        /// <param name="sph">Foliagle bounding sphere</param>
        /// <returns>Returns a node list</returns>
        private QuadTreeNode[] GetFoliageNodes(BoundingFrustum frustum, BoundingSphere sph)
        {
            var visibleNodes = this.foliageQuadtree.GetNodesInVolume(ref sph);
            if (visibleNodes != null && visibleNodes.Length > 0)
            {
                return Array.FindAll(visibleNodes, n => frustum.Contains(ref n.BoundingBox) != ContainmentType.Disjoint);
            }

            return null;
        }
    }
}
