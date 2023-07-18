﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Foliage;
    using Engine.Collections;
    using Engine.Common;
    using Engine.Content;
    using Engine.PathFinding;

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
            private IEnumerable<VertexBillboard> foliageData = Array.Empty<VertexBillboard>();

            /// <summary>
            /// Foliage populating flag
            /// </summary>
            public bool Planting { get; protected set; }
            /// <summary>
            /// Foliage populated flag
            /// </summary>
            public bool Planted { get; protected set; }
            /// <summary>
            /// Gets the node to which this patch is currently assigned
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
            private static IEnumerable<VertexBillboard> PlantNode(WalkableScene scene, QuadTreeNode node, FoliageMap map, FoliageMapChannel description, BoundingBox gbbox)
            {
                if (node == null)
                {
                    return Enumerable.Empty<VertexBillboard>();
                }

                List<VertexBillboard> vertexData = new(MAX);

                Random rnd = new(description.Seed);
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
            private static VertexBillboard? CalculatePoint(WalkableScene scene, FoliageMap map, FoliageMapChannel description, BoundingBox gbbox, BoundingBox bbox, Random rnd)
            {
                VertexBillboard? result = null;

                Vector2 min = new(gbbox.Minimum.X, gbbox.Minimum.Z);
                Vector2 max = new(gbbox.Maximum.X, gbbox.Maximum.Z);

                //Attempts
                for (int i = 0; i < 3; i++)
                {
                    Vector3 pos = new(
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
                        Vector2 size = new(
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
            private static bool Plant(WalkableScene scene, Vector3 pos, Vector2 size, out VertexBillboard res)
            {
                var ray = scene.GetTopDownRay(pos, PickingHullTypes.FacingOnly | PickingHullTypes.Geometry);

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
            /// Launches foliage population asynchronous task
            /// </summary>
            /// <param name="scene">Scene</param>
            /// <param name="node">Foliage Node</param>
            /// <param name="map">Foliage map</param>
            /// <param name="description">Terrain vegetation description</param>
            /// <param name="gbbox">Relative bounding box to plant</param>
            public async Task PlantAsync(WalkableScene scene, QuadTreeNode node, FoliageMap map, FoliageMapChannel description, BoundingBox gbbox)
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
            /// Geometry output count
            /// </summary>
            public BuiltInFoliageInstances Count;

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
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="name">Name</param>
            public FoliageBuffer(BufferManager bufferManager, string name)
            {
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
            /// Attaches the specified patch to buffer
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
            /// Draws the foliage data
            /// </summary>
            /// <param name="context">Device context</param>
            /// <param name="drawer">Drawer</param>
            public bool DrawFoliage(EngineDeviceContext context, BuiltInDrawer drawer)
            {
                if (vertexDrawCount <= 0)
                {
                    return false;
                }

                return drawer.Draw(context, BufferManager, new DrawOptions
                {
                    VertexBuffer = VertexBuffer,
                    VertexDrawCount = vertexDrawCount,
                    Topology = Topology.PointList,
                });
            }

            /// <summary>
            /// Gets the text representation of the buffer
            /// </summary>
            /// <returns>Returns the text representation of the buffer</returns>
            public override string ToString()
            {
                return $"{Id} => Attached: {Attached}; HasPatch: {CurrentPatch != null}";
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
        private readonly Dictionary<QuadTreeNode, List<FoliagePatch>> foliagePatches = new();
        /// <summary>
        /// Foliage buffer list
        /// </summary>
        private readonly List<FoliageBuffer> foliageBuffers = new();
        /// <summary>
        /// Random texture
        /// </summary>
        private EngineShaderResourceView textureRandom = null;
        /// <summary>
        /// Foliage map for vegetation planting task
        /// </summary>
        private FoliageMap foliageMap = null;
        /// <summary>
        /// Foliage map channels for vegetation planting task
        /// </summary>
        private readonly List<FoliageMapChannel> foliageMapChannels = new();
        /// <summary>
        /// Material
        /// </summary>
        private IMeshMaterial foliageMaterial;
        /// <summary>
        /// Foliage visible sphere
        /// </summary>
        private BoundingSphere foliageSphere;
        /// <summary>
        /// Foliage quad-tree
        /// </summary>
        private QuadTree foliageQuadtree;
        /// <summary>
        /// Last visible node collection
        /// </summary>
        private IEnumerable<QuadTreeNode> visibleNodes = Enumerable.Empty<QuadTreeNode>();
        /// <summary>
        /// Counter of the elapsed seconds between the last node sorting
        /// </summary>
        private float lastSortingElapsedSeconds = 0;
        /// <summary>
        /// Buffer data to write
        /// </summary>
        private readonly List<FoliagePatch> toAssign = new();
        /// <summary>
        /// Initialized flag
        /// </summary>
        private bool initialized = false;
        /// <summary>
        /// Foliage drawer
        /// </summary>
        private BuiltInFoliage foliageDrawer = null;

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
                foliageBuffers.Add(new FoliageBuffer(BufferManager, Name));
            }

            foliageDrawer = BuiltInShaders.GetDrawer<BuiltInFoliage>();

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
                Count = (BuiltInFoliageInstances)channel.Instances,
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

            UpdatePatchesAsync(context);
        }

        /// <inheritdoc/>
        public override bool Draw(DrawContext context)
        {
            if (!initialized)
            {
                return false;
            }

            if (!Visible)
            {
                return false;
            }

            if (!visibleNodes.Any())
            {
                return false;
            }

            bool draw = context.ValidateDraw(BlendMode);
            if (!draw)
            {
                return false;
            }

            WritePatches(context.EyePosition);

            bool drawn = false;
            foreach (var item in visibleNodes)
            {
                drawn = DrawNode(context.DeviceContext, item) || drawn;
            }

            return drawn;
        }
        /// <summary>
        /// Draws the node
        /// </summary>
        /// <param name="context">Device context</param>
        /// <param name="item">Node</param>
        private bool DrawNode(EngineDeviceContext context, QuadTreeNode item)
        {
            var buffers = foliageBuffers.Where(b => b.CurrentPatch?.CurrentNode == item);
            if (!buffers.Any())
            {
                return false;
            }

            bool drawn = false;
            foreach (var buffer in buffers)
            {
                var channelData = foliageMapChannels[buffer.CurrentPatch.Channel];

                var state = new BuiltInFoliageState
                {
                    StartRadius = channelData.StartRadius,
                    EndRadius = channelData.EndRadius,
                    TintColor = Color4.White,
                    MaterialIndex = foliageMaterial.ResourceIndex,
                    TextureCount = channelData.TextureCount,
                    NormalMapCount = channelData.NormalMapCount,
                    RandomTexture = textureRandom,
                    Texture = channelData.Textures,
                    NormalMaps = channelData.NormalMaps,
                    WindDirection = WindDirection,
                    WindStrength = WindStrength * channelData.WindEffect,
                    Delta = channelData.Delta,
                    WindEffect = channelData.WindEffect,
                    Instances = channelData.Count,
                };

                foliageDrawer.UpdateFoliage(state);

                if (buffer.DrawFoliage(context, foliageDrawer))
                {
                    drawn = true;
                }
            }

            return drawn;
        }

        /// <inheritdoc/>
        public override bool Cull(ICullingVolume volume, out float distance)
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
            var bbox = GetPlantingArea();
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
        /// Gets the planting area bounding box
        /// </summary>
        private BoundingBox? GetPlantingArea()
        {
            if (Description.PlantingArea != null)
            {
                return Description.PlantingArea;
            }

            if (Scene is WalkableScene walkableScene)
            {
                return walkableScene.GetBoundingBox(SceneObjectUsages.Ground);
            }

            return null;
        }
        /// <summary>
        /// Builds the quad-tree from the specified bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="nodeSize">Maximum quad-tree node size</param>
        private void BuildQuadtree(BoundingBox bbox, float nodeSize)
        {
            if (foliageQuadtree == null || foliageQuadtree.BoundingBox != bbox)
            {
                //Creates the quad-tree if not exists, or if the reference bounding box has changed
                float sizeParts = Math.Max(bbox.Width, bbox.Depth) / nodeSize;

                int levels = Math.Max(1, (int)Math.Log(sizeParts, 2));

                foliageQuadtree = new QuadTree(bbox, levels);
            }
        }
        /// <summary>
        /// Gets the node list suitable for foliage planting
        /// </summary>
        /// <param name="volume">Culling volume</param>
        /// <param name="sph">Foliage bounding sphere</param>
        /// <returns>Returns a node list</returns>
        private IEnumerable<QuadTreeNode> GetFoliageNodes(ICullingVolume volume, BoundingSphere sph)
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
        /// <param name="transparent">Specifies whether the billboards are transparent or not</param>
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
        /// For each high LOD visible node, look for planted foliagePatches.
        /// - If planted, see if they were assigned to a foliageBuffer
        ///   - If assigned, do nothing
        ///   - If not, add to toAssign list
        /// - If not planted, launch planting task y do nothing more. The node will be processed next time
        /// </remarks>
        private async Task<IEnumerable<FoliagePatch>> DoPlantAsync(BoundingBox bbox)
        {
            if (Scene is not WalkableScene walkableScene)
            {
                return Enumerable.Empty<FoliagePatch>();
            }

            List<FoliagePatch> toAssignList = new();

            List<Task> plantTaskList = new();

            int channelCount = foliageMapChannels.Count;

            foreach (var node in visibleNodes)
            {
                var fPatchList = GetNodePatches(node, channelCount);

                for (int i = 0; i < fPatchList.Count(); i++)
                {
                    var fPatch = fPatchList.ElementAt(i);
                    if (!fPatch.Planted)
                    {
                        plantTaskList.Add(fPatch.PlantAsync(walkableScene, node, foliageMap, foliageMapChannels[i], bbox));
                    }
                    else if (fPatch.HasData && !foliageBuffers.Exists(b => b.CurrentPatch == fPatch))
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
            if (foliagePatches.TryGetValue(node, out var value))
            {
                return value;
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
        ///   - If not, look for a buffer to free, farthest from camera first
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
