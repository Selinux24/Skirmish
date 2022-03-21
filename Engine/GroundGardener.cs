using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public sealed class GroundGardener : Drawable<GroundGardenerDescription>, IUseMaterials
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
            private IEnumerable<VertexBillboard> foliageData = new VertexBillboard[] { };

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
                    return foliageData.Any();
                }
            }

            /// <summary>
            /// Planting task
            /// </summary>
            /// <param name="scene">Scene</param>
            /// <param name="node">Node to process</param>
            /// <param name="map">Foliage map</param>
            /// <param name="description">Vegetation task</param>
            /// <param name="gbbox">Relative bounding box to plant</param>
            /// <returns>Returns generated vertex data</returns>
            private static IEnumerable<VertexBillboard> PlantNode(Scene scene, QuadTreeNode node, FoliageMap map, FoliageMapChannel description, BoundingBox gbbox)
            {
                if (node == null)
                {
                    return Enumerable.Empty<VertexBillboard>();
                }

                List<VertexBillboard> vertexData = new List<VertexBillboard>(MAX);

                Random rnd = new Random(description.Seed);
                var bbox = node.BoundingBox;
                int count = (int)Math.Min(MAX, MAX * description.Saturation);

                Parallel.For(0, count, (index) =>
                {
                    var v = CalculatePoint(scene, map, description, gbbox, bbox, rnd);
                    if (v.HasValue)
                    {
                        vertexData.Add(v.Value);
                    }
                });

                return vertexData;
            }
            /// <summary>
            /// Calculates a planting point
            /// </summary>
            /// <param name="scene">Scene</param>
            /// <param name="map">Foliage map</param>
            /// <param name="description">Vegetation task</param>
            /// <param name="gbbox">Relative bounding box to plant</param>
            /// <param name="bbox">Node box</param>
            /// <param name="rnd">Randomizer</param>
            /// <returns>Returns the planting point</returns>
            private static VertexBillboard? CalculatePoint(Scene scene, FoliageMap map, FoliageMapChannel description, BoundingBox gbbox, BoundingBox bbox, Random rnd)
            {
                VertexBillboard? result = null;

                Vector2 min = new Vector2(gbbox.Minimum.X, gbbox.Minimum.Z);
                Vector2 max = new Vector2(gbbox.Maximum.X, gbbox.Maximum.Z);

                //Attempts
                for (int i = 0; i < 3; i++)
                {
                    Vector3 pos = new Vector3(
                        rnd.NextFloat(bbox.Minimum.X, bbox.Maximum.X),
                        bbox.Maximum.Y + 1f,
                        rnd.NextFloat(bbox.Minimum.Z, bbox.Maximum.Z));

                    bool plant = false;
                    if (map != null)
                    {
                        var c = map.GetRelative(pos, min, max);

                        if (c[description.Index] > 0)
                        {
                            plant = rnd.NextFloat(0, 1) < c[description.Index];
                        }
                    }
                    else
                    {
                        plant = true;
                    }

                    if (plant)
                    {
                        var size = new Vector2(
                            rnd.NextFloat(description.MinSize.X, description.MaxSize.X),
                            rnd.NextFloat(description.MinSize.Y, description.MaxSize.Y));

                        var planted = Plant(scene, pos, size, out var res);
                        if (planted)
                        {
                            result = res;

                            break;
                        }
                    }
                }

                return result;
            }
            /// <summary>
            /// Plants one item
            /// </summary>
            /// <param name="scene">Scene</param>
            /// <param name="pos">Position</param>
            /// <param name="size">Size</param>
            /// <param name="res">Resulting item</param>
            /// <returns>Returns true if an item has been planted</returns>
            private static bool Plant(Scene scene, Vector3 pos, Vector2 size, out VertexBillboard res)
            {
                var ray = scene.GetTopDownRay(pos, RayPickingParams.FacingOnly | RayPickingParams.Objects);

                bool found = scene.PickFirst<Triangle>(ray, SceneObjectUsages.Ground, out var r);
                if (found && r.PickingResult.Primitive.Normal.Y > 0.5f)
                {
                    res = new VertexBillboard()
                    {
                        Position = r.PickingResult.Position,
                        Size = size,
                    };

                    return true;
                }

                res = new VertexBillboard();

                return false;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            public FoliagePatch()
            {
                Planted = false;
                Planting = false;

                CurrentNode = null;
                Channel = -1;
            }
            /// <summary>
            /// Destructor
            /// </summary>
            ~FoliagePatch()
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
            /// Resource disposal
            /// </summary>
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    foliageData = null;
                }
            }

            /// <summary>
            /// Launchs foliage population asynchronous task
            /// </summary>
            /// <param name="scene">Scene</param>
            /// <param name="node">Foliage Node</param>
            /// <param name="map">Foliage map</param>
            /// <param name="description">Terrain vegetation description</param>
            /// <param name="gbbox">Relative bounding box to plant</param>
            public async Task PlantAsync(Scene scene, QuadTreeNode node, FoliageMap map, FoliageMapChannel description, BoundingBox gbbox)
            {
                if (Planting)
                {
                    return;
                }

                //Start planting task
                Planting = true;

                try
                {
                    CurrentNode = node;
                    Channel = description.Index;

                    foliageData = await Task.Run(() => PlantNode(scene, node, map, description, gbbox));

                    Planted = true;
                }
                finally
                {
                    Planting = false;
                }
            }
            /// <summary>
            /// Get foliage data
            /// </summary>
            /// <param name="eyePosition">Eye position</param>
            /// <param name="transparent">Use transparency</param>
            /// <returns>Returns the foliage data ordered by distance to eye position. Far first if transparency specified, near first otherwise</returns>
            public IEnumerable<VertexBillboard> GetData(Vector3 eyePosition, bool transparent)
            {
                if (!foliageData.Any())
                {
                    return Enumerable.Empty<VertexBillboard>();
                }

                //Sort data
                foliageData = foliageData
                    .OrderBy(obj => (transparent ? -1 : 1) * Vector3.DistanceSquared(obj.Position, eyePosition));

                return foliageData;
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
            /// Destructor
            /// </summary>
            ~FoliageMapChannel()
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
            /// Resource disposal
            /// </summary>
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (Textures != null)
                    {
                        Textures.Dispose();
                        Textures = null;
                    }
                    if (NormalMaps != null)
                    {
                        NormalMaps.Dispose();
                        NormalMaps = null;
                    }
                }
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
            /// Gets the next instance Id
            /// </summary>
            /// <returns>Returns the next Instance Id</returns>
            private static int GetID()
            {
                return ++ID;
            }

            /// <summary>
            /// Vertex count
            /// </summary>
            private int vertexDrawCount = 0;

            /// <summary>
            /// Game
            /// </summary>
            protected readonly Game Game = null;
            /// <summary>
            /// Buffer manager
            /// </summary>
            protected BufferManager BufferManager = null;

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
                Game = game;
                BufferManager = bufferManager;
                Id = GetID();
                Attached = false;
                CurrentPatch = null;
                VertexBuffer = bufferManager.AddVertexData(string.Format("{1}.{0}", Id, name), true, new VertexBillboard[FoliagePatch.MAX]);
            }
            /// <summary>
            /// Destructor
            /// </summary>
            ~FoliageBuffer()
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
            /// Resource disposal
            /// </summary>
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    //Remove data from buffer manager
                    BufferManager?.RemoveVertexData(VertexBuffer);
                }
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
                vertexDrawCount = 0;
                Attached = false;
                CurrentPatch = null;

                //Get the data
                var data = patch.GetData(eyePosition, transparent);
                if (!data.Any())
                {
                    return;
                }

                //Attach data to buffer
                if (!bufferManager.WriteVertexBuffer(VertexBuffer, data))
                {
                    return;
                }

                vertexDrawCount = data.Count();
                Attached = true;
                CurrentPatch = patch;
            }
            /// <summary>
            /// Draw foliage shadows
            /// </summary>
            /// <param name="technique">Technique</param>
            public void DrawFoliageShadows(EngineEffectTechnique technique)
            {
                if (vertexDrawCount > 0)
                {
                    var graphics = Game.Graphics;

                    for (int p = 0; p < technique.PassCount; p++)
                    {
                        graphics.EffectPassApply(technique, p, 0);

                        graphics.Draw(vertexDrawCount, VertexBuffer.BufferOffset);
                    }
                }
            }
            /// <summary>
            /// Draws the foliage data
            /// </summary>
            /// <param name="technique">Technique</param>
            public void DrawFoliage(EngineEffectTechnique technique)
            {
                if (vertexDrawCount > 0)
                {
                    var graphics = Game.Graphics;

                    Counters.InstancesPerFrame++;
                    Counters.PrimitivesPerFrame += vertexDrawCount / 3;

                    for (int p = 0; p < technique.PassCount; p++)
                    {
                        graphics.EffectPassApply(technique, p, 0);

                        graphics.Draw(vertexDrawCount, VertexBuffer.BufferOffset);
                    }
                }
            }

            /// <summary>
            /// Gets the text representation of the buffer
            /// </summary>
            /// <returns>Returns the text representation of the buffer</returns>
            public override string ToString()
            {
                return string.Format("{0} => Attached: {1}; HasPatch: {2}", Id, Attached, CurrentPatch != null);
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
        private readonly Dictionary<QuadTreeNode, List<FoliagePatch>> foliagePatches = new Dictionary<QuadTreeNode, List<FoliagePatch>>();
        /// <summary>
        /// Foliage buffer list
        /// </summary>
        private readonly List<FoliageBuffer> foliageBuffers = new List<FoliageBuffer>();
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
        private readonly List<FoliageMapChannel> foliageMapChannels = new List<FoliageMapChannel>();
        /// <summary>
        /// Material
        /// </summary>
        private IMeshMaterial foliageMaterial;
        /// <summary>
        /// Foliage visible sphere
        /// </summary>
        private BoundingSphere foliageSphere;
        /// <summary>
        /// Foliage quadtree
        /// </summary>
        private QuadTree foliageQuadtree;
        /// <summary>
        /// Last visible node collection
        /// </summary>
        private IEnumerable<QuadTreeNode> visibleNodes = new QuadTreeNode[] { };
        /// <summary>
        /// Counter of the elapsed seconds between the last node sorting
        /// </summary>
        private float lastSortingElapsedSeconds = 0;
        /// <summary>
        /// Buffer data to write
        /// </summary>
        private readonly List<FoliagePatch> toAssign = new List<FoliagePatch>();
        /// <summary>
        /// Initialized flag
        /// </summary>
        private bool initialized = false;

        /// <summary>
        /// Wind direction
        /// </summary>
        public Vector3 WindDirection { get; set; }
        /// <summary>
        /// Wind strength
        /// </summary>
        public float WindStrength { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public GroundGardener(Scene scene, string id, string name) :
            base(scene, id, name)
        {

        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~GroundGardener()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int i = 0; i < foliageBuffers.Count; i++)
                {
                    foliageBuffers[i]?.Dispose();
                    foliageBuffers[i] = null;
                }
                foliageBuffers.Clear();

                foreach (var item in foliagePatches.Values)
                {
                    foreach (var value in item)
                    {
                        value?.Dispose();
                    }
                }
                foliagePatches.Clear();

                foliageMap?.Dispose();
                foliageMap = null;

                for (int i = 0; i < foliageMapChannels.Count; i++)
                {
                    foliageMapChannels[i]?.Dispose();
                    foliageMapChannels[i] = null;
                }
                foliageMapChannels.Clear();

                textureRandom?.Dispose();
                textureRandom = null;
            }
        }

        /// <inheritdoc/>
        public override async Task InitializeAssets(GroundGardenerDescription description)
        {
            await base.InitializeAssets(description);

            if (Description == null)
            {
                throw new EngineException("A gardener description should be specified.");
            }

            textureRandom = await Game.ResourceManager.RequestResource(Guid.NewGuid(), 1024, -1, 1, 24);

            foliageSphere = new BoundingSphere(Vector3.Zero, Description.VisibleRadius);

            //Material
            foliageMaterial = MeshMaterial.DefaultPhong;

            //Read foliage textures
            string contentPath = Description.ContentPath;

            if (!string.IsNullOrEmpty(Description.VegetationMap))
            {
                var foliageMapData = ContentManager.FindContent(contentPath, Description.VegetationMap).FirstOrDefault();
                foliageMap = FoliageMap.FromStream(foliageMapData);
            }

            for (int i = 0; i < Description.Channels.Length; i++)
            {
                var channelDesc = Description.Channels[i];
                if (channelDesc?.Enabled == true)
                {
                    var newChannel = await CreateChannel(channelDesc, i, contentPath);

                    foliageMapChannels.Add(newChannel);
                }
            }

            for (int i = 0; i < MaxFoliageBuffers; i++)
            {
                foliageBuffers.Add(new FoliageBuffer(Game, BufferManager, Name));
            }

            initialized = true;
        }

        /// <summary>
        /// Creates a map channel from the specified description
        /// </summary>
        /// <param name="channel">Channel description</param>
        /// <param name="index">Channel index</param>
        /// <param name="contentPath">Resources content path</param>
        /// <returns>Returns the new map channel</returns>
        private async Task<FoliageMapChannel> CreateChannel(GroundGardenerDescription.Channel channel, int index, string contentPath)
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
                var image = new FileArrayImageContent(contentPath, channel.VegetationTextures);
                foliageTextures = await Game.ResourceManager.RequestResource(image);
            }

            if (normalMapCount > 0)
            {
                var image = new FileArrayImageContent(contentPath, channel.VegetationNormalMaps);
                foliageNormalMaps = await Game.ResourceManager.RequestResource(image);
            }

            return new FoliageMapChannel()
            {
                Index = index,
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
            };
        }

        bool planting = false;

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!initialized)
            {
                return;
            }

            windTime += context.GameTime.ElapsedSeconds * WindStrength;

            UpdatePatchesAsync(context);
        }

        /// <inheritdoc/>
        public override void DrawShadows(DrawContextShadows context)
        {
            if (!initialized)
            {
                return;
            }

            if (!Visible)
            {
                return;
            }

            if (!visibleNodes.Any())
            {
                return;
            }

            foreach (var item in visibleNodes)
            {
                DrawShadowsNode(context, item);
            }
        }
        /// <summary>
        /// Draws the node shadows
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="item">Node</param>
        private void DrawShadowsNode(DrawContextShadows context, QuadTreeNode item)
        {
            var buffers = foliageBuffers.Where(b => b.CurrentPatch?.CurrentNode == item);
            if (!buffers.Any())
            {
                return;
            }

            foreach (var buffer in buffers)
            {
                var vegetationTechnique = SetTechniqueVegetationShadowMap(context, buffer.CurrentPatch.Channel);
                if (vegetationTechnique != null)
                {
                    BufferManager.SetInputAssembler(
                        vegetationTechnique,
                        buffer.VertexBuffer,
                        Topology.PointList);

                    buffer.DrawFoliageShadows(vegetationTechnique);
                }
            }
        }

        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            if (!initialized)
            {
                return;
            }

            if (!Visible)
            {
                return;
            }

            if (!visibleNodes.Any())
            {
                return;
            }

            bool draw = context.ValidateDraw(BlendMode);
            if (!draw)
            {
                return;
            }

            WritePatches(context.EyePosition);

            foreach (var item in visibleNodes)
            {
                DrawNode(context, item);
            }
        }
        /// <summary>
        /// Draws the node
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="item">Node</param>
        private void DrawNode(DrawContext context, QuadTreeNode item)
        {
            var buffers = foliageBuffers.Where(b => b.CurrentPatch?.CurrentNode == item);
            if (!buffers.Any())
            {
                return;
            }

            foreach (var buffer in buffers)
            {
                var vegetationTechnique = SetTechniqueVegetationDefault(context, buffer.CurrentPatch.Channel);
                if (vegetationTechnique != null)
                {
                    BufferManager.SetInputAssembler(
                        vegetationTechnique,
                        buffer.VertexBuffer,
                        Topology.PointList);

                    buffer.DrawFoliage(vegetationTechnique);
                }
            }
        }

        /// <inheritdoc/>
        public override bool Cull(IIntersectionVolume volume, out float distance)
        {
            distance = float.MaxValue;

            if (foliageQuadtree == null)
            {
                return false;
            }

            bool cull = volume.Contains(foliageQuadtree.BoundingBox) == ContainmentType.Disjoint;

            if (!cull)
            {
                distance = 0;
            }

            return cull;
        }

        /// <summary>
        /// Sets thecnique for vegetation drawing with forward renderer
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="channel">Channel</param>
        /// <returns>Returns the selected technique</returns>
        private EngineEffectTechnique SetTechniqueVegetationDefault(DrawContext context, int channel)
        {
            var channelData = foliageMapChannels[channel];

            var effect = DrawerPool.EffectDefaultFoliage;

            effect.UpdatePerFrame(
                context,
                new EffectFoliageState
                {
                    WindDirection = WindDirection,
                    WindStrength = WindStrength * channelData.WindEffect,
                    TotalTime = windTime * channelData.WindEffect,
                    Delta = channelData.Delta,
                    StartRadius = channelData.StartRadius,
                    EndRadius = channelData.EndRadius,
                    RandomTexture = textureRandom,
                    MaterialIndex = foliageMaterial.ResourceIndex,
                    TextureCount = channelData.TextureCount,
                    Texture = channelData.Textures,
                    NormalMapCount = channelData.NormalMapCount,
                    NormalMaps = channelData.NormalMaps,
                });

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
            var channelData = foliageMapChannels[channel];

            var effect = DrawerPool.EffectShadowFoliage;

            effect.UpdatePerFrame(
                context,
                new EffectShadowFoliageState
                {
                    WindDirection = WindDirection,
                    WindStrength = WindStrength * channelData.WindEffect,
                    TotalTime = windTime * channelData.WindEffect,
                    Delta = channelData.Delta,
                    StartRadius = channelData.StartRadius,
                    EndRadius = channelData.EndRadius,
                    RandomTexture = textureRandom,
                    TextureCount = channelData.TextureCount,
                    Texture = channelData.Textures,
                });

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
            WindDirection = direction;
            WindStrength = strength;
        }

        #region Handle patch data in CPU

        /// <summary>
        /// Updates patches state
        /// </summary>
        /// <param name="context">Updating context</param>
        private void UpdatePatchesAsync(UpdateContext context)
        {
            if (planting)
            {
                return;
            }

            planting = true;

            Task.Run(async () =>
            {
                try
                {
                    await UpdatePatches(context);
                }
                finally
                {
                    planting = false;
                }
            });
        }
        /// <summary>
        /// Updates patches state
        /// </summary>
        /// <param name="context">Updating context</param>
        private async Task UpdatePatches(UpdateContext context)
        {
            var bbox = Description.PlantingArea ?? Scene.GetGroundBoundingBox();
            if (!bbox.HasValue)
            {
                return;
            }

            BuildQuadtree(bbox.Value, Description.NodeSize);

            foliageSphere.Center = context.EyePosition;

            visibleNodes = GetFoliageNodes(context.CameraVolume, foliageSphere);
            if (!visibleNodes.Any())
            {
                return;
            }

            bool transparent = BlendMode.HasFlag(BlendModes.Alpha) || BlendMode.HasFlag(BlendModes.Transparent);

            //Sort nodes by distance from camera position
            SortVisibleNodes(context.GameTime, context.EyePosition, transparent);

            //Find patches to assign data
            toAssign.Clear();
            toAssign.AddRange(await DoPlantAsync(bbox.Value));
        }
        /// <summary>
        /// Builds the quadtree from the specified bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="nodeSize">Maximum quadtree node size</param>
        private void BuildQuadtree(BoundingBox bbox, float nodeSize)
        {
            if (foliageQuadtree == null || foliageQuadtree.BoundingBox != bbox)
            {
                //Creates the quadtree if not exists, or if the reference bounding box has changed
                float sizeParts = Math.Max(bbox.Width, bbox.Depth) / nodeSize;

                int levels = Math.Max(1, (int)Math.Log(sizeParts, 2));

                foliageQuadtree = new QuadTree(bbox, levels);
            }
        }
        /// <summary>
        /// Gets the node list suitable for foliage planting
        /// </summary>
        /// <param name="volume">Culling volume</param>
        /// <param name="sph">Foliagle bounding sphere</param>
        /// <returns>Returns a node list</returns>
        private IEnumerable<QuadTreeNode> GetFoliageNodes(IIntersectionVolume volume, BoundingSphere sph)
        {
            var nodes = foliageQuadtree.GetNodesInVolume(ref sph);
            if (nodes?.Any() != true)
            {
                return Enumerable.Empty<QuadTreeNode>();
            }

            return nodes.Where(n => volume.Contains(n.BoundingBox) != ContainmentType.Disjoint).ToArray();
        }
        /// <summary>
        /// Sorts the visible nodes
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="transparent">Specifies wether the billboards are transparent or not</param>
        /// <remarks>Sorts nodes every second</remarks>
        private void SortVisibleNodes(GameTime gameTime, Vector3 eyePosition, bool transparent)
        {
            lastSortingElapsedSeconds += gameTime.ElapsedSeconds;

            if (lastSortingElapsedSeconds < 1f)
            {
                return;
            }

            lastSortingElapsedSeconds = 0f;

            visibleNodes = visibleNodes
                .OrderBy(obj => (transparent ? -1 : 1) * Vector3.DistanceSquared(obj.Center, eyePosition));
        }
        /// <summary>
        /// Updates the patch list state and finds a list of patches with assigned data
        /// </summary>
        /// <param name="bbox">Relative bounding box to plant</param>
        /// <returns>Returns a list of patches</returns>
        /// <remarks>
        /// Foreach high lod visible node, look for planted foliagePatches.
        /// - If planted, see if they were assigned to a foliageBuffer
        ///   - If assigned, do nothing
        ///   - If not, add to toAssign list
        /// - If not planted, launch planting task y do nothing more. The node will be processed next time
        /// </remarks>
        private async Task<IEnumerable<FoliagePatch>> DoPlantAsync(BoundingBox bbox)
        {
            List<FoliagePatch> toAssignList = new List<FoliagePatch>();

            List<Task> plantTaskList = new List<Task>();

            int channelCount = foliageMapChannels.Count;

            foreach (var node in visibleNodes)
            {
                var fPatchList = GetNodePatches(node, channelCount);

                for (int i = 0; i < fPatchList.Count(); i++)
                {
                    var fPatch = fPatchList.ElementAt(i);
                    if (!fPatch.Planted)
                    {
                        plantTaskList.Add(fPatch.PlantAsync(Scene, node, foliageMap, foliageMapChannels[i], bbox));
                    }
                    else if (fPatch.HasData && !foliageBuffers.Any(b => b.CurrentPatch == fPatch))
                    {
                        toAssignList.Add(fPatch);
                    }
                }
            }

            await ExecutePlantTasksAsync(plantTaskList);

            return toAssignList;
        }
        /// <summary>
        /// Gets node patches
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="channelCount">Channel count</param>
        private IEnumerable<FoliagePatch> GetNodePatches(QuadTreeNode node, int channelCount)
        {
            if (foliagePatches.ContainsKey(node))
            {
                return foliagePatches[node];
            }

            foliagePatches.Add(node, new List<FoliagePatch>());

            for (int i = 0; i < channelCount; i++)
            {
                foliagePatches[node].Add(new FoliagePatch());
            }

            return foliagePatches[node];
        }
        /// <summary>
        /// Executes a task list
        /// </summary>
        /// <param name="list">Task list</param>
        private async Task ExecutePlantTasksAsync(IEnumerable<Task> list)
        {
            if (!list.Any())
            {
                return;
            }

            Logger.WriteTrace(this, $"{Name}. => Executing a planting task over {list.Count()} patches.");

            var taskList = list.ToList();

            while (taskList.Any())
            {
                var t = await Task.WhenAny(taskList);

                taskList.Remove(t);

                bool completedOk = t.Status == TaskStatus.RanToCompletion;
                if (!completedOk)
                {
                    Logger.WriteError(this, $"{Name}. => A planting task ends with errors: {t.Exception.Message}", t.Exception);
                }
            }

            await Task.CompletedTask;
        }

        #endregion

        #region Handle patch data in GPU

        /// <summary>
        /// Writes patch data into graphic buffers
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        private void WritePatches(Vector3 eyePosition)
        {
            //Mark patches to delete
            AttachFreePatches(eyePosition);

            //Free unused patches
            FreeUnusedPatches(eyePosition);
        }
        /// <summary>
        /// Attaches patches data into graphic buffers
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        /// <remarks>
        /// For each node to assign
        /// - Look for a free buffer. It's free if unassigned or assigned to not visible node
        ///   - If free buffer found, assign
        ///   - If not, look for a buffer to free, farthests from camera first
        /// </remarks>
        private void AttachFreePatches(Vector3 eyePosition)
        {
            if (!toAssign.Any())
            {
                return;
            }

            //Sort nearest first
            toAssign.Sort((f1, f2) =>
            {
                float d1 = Vector3.DistanceSquared(f1.CurrentNode.Center, eyePosition);
                float d2 = Vector3.DistanceSquared(f2.CurrentNode.Center, eyePosition);

                return d1.CompareTo(d2);
            });

            var freeBuffers = foliageBuffers.FindAll(b =>
                (b.CurrentPatch == null) ||
                (b.CurrentPatch != null && !visibleNodes.Any(n => n == b.CurrentPatch.CurrentNode)));

            if (!freeBuffers.Any())
            {
                Logger.WriteWarning(this, $"{Name}. => No free buffers found for {toAssign.Count} patches to attach.");

                return;
            }

            //Sort free first and farthest first
            freeBuffers.Sort((f1, f2) =>
            {
                float d1 = f1.CurrentPatch == null ? -1 : Vector3.DistanceSquared(f1.CurrentPatch.CurrentNode.Center, eyePosition);
                float d2 = f2.CurrentPatch == null ? -1 : Vector3.DistanceSquared(f2.CurrentPatch.CurrentNode.Center, eyePosition);

                return -d1.CompareTo(d2);
            });

            bool transparent = BlendMode.HasFlag(BlendModes.Alpha) || BlendMode.HasFlag(BlendModes.Transparent);

            Logger.WriteTrace(this, $"{Name}. => Attaching {toAssign.Count} patches into {freeBuffers.Count} buffers.");

            while (toAssign.Count > 0 && freeBuffers.Count > 0)
            {
                freeBuffers[0].AttachFoliage(eyePosition, transparent, toAssign[0], BufferManager);

                toAssign.RemoveAt(0);
                freeBuffers.RemoveAt(0);
            }

            if (toAssign.Any())
            {
                Logger.WriteWarning(this, $"{Name}. => No free buffers found for {toAssign.Count} patches to attach.");
            }
        }
        /// <summary>
        /// Frees the unused patch data
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        private void FreeUnusedPatches(Vector3 eyePosition)
        {
            if (foliagePatches.Keys.Count <= MaxFoliagePatches)
            {
                return;
            }

            var nodes = foliagePatches.Keys.ToArray();
            var notVisible = Array.FindAll(nodes, n => !visibleNodes.Any(v => v == n));

            if (notVisible.Length <= 0)
            {
                return;
            }

            Array.Sort(notVisible, (n1, n2) =>
            {
                float d1 = Vector3.DistanceSquared(n1.Center, eyePosition);
                float d2 = Vector3.DistanceSquared(n2.Center, eyePosition);

                return d2.CompareTo(d1);
            });

            int toDelete = foliagePatches.Keys.Count - MaxFoliagePatches;
            for (int i = 0; i < Math.Min(notVisible.Length, toDelete); i++)
            {
                foliagePatches.Remove(notVisible[i]);
            }
        }

        #endregion

        /// <inheritdoc/>
        public IEnumerable<IMeshMaterial> GetMaterials()
        {
            return foliageMaterial != null ? new[] { foliageMaterial } : Enumerable.Empty<IMeshMaterial>();
        }
        /// <inheritdoc/>
        public IMeshMaterial GetMaterial(string meshMaterialName)
        {
            return foliageMaterial;
        }
        /// <inheritdoc/>
        public void ReplaceMaterial(string meshMaterialName, IMeshMaterial material)
        {
            if (foliageMaterial == material)
            {
                return;
            }

            foliageMaterial = material;

            Scene.UpdateMaterialPalette();
        }
    }
}
