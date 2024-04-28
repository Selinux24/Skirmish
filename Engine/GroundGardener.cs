using SharpDX;
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

    /// <summary>
    /// Ground garden planter
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class GroundGardener(Scene scene, string id, string name) : Drawable<GroundGardenerDescription>(scene, id, name), IUseMaterials
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
            private IEnumerable<VertexBillboard> foliageData = [];

            /// <summary>
            /// Foliage populating flag
            /// </summary>
            public bool Planting { get; protected set; }
            /// <summary>
            /// Foliage populated flag
            /// </summary>
            public bool Planted { get; protected set; }
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
            /// <param name="map">Foliage map</param>
            /// <param name="description">Vegetation task</param>
            /// <param name="gbbox">Global bounding box</param>
            /// <param name="nbbox">Node bounding box</param>
            /// <returns>Returns generated vertex data</returns>
            private static List<VertexBillboard> PlantNode(Scene scene, FoliageMap map, FoliageMapChannel description, BoundingBox gbbox, BoundingBox nbbox)
            {
                List<VertexBillboard> vertexData = new(MAX);

                Random rnd = new(description.Seed);
                int count = (int)MathF.Min(MAX, MAX * description.Saturation);

                Parallel.For(0, count, (index) =>
                {
                    var v = CalculatePoint(scene, map, description, gbbox, nbbox, rnd);
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
            /// <param name="nbbox">Node box</param>
            /// <param name="rnd">Randomizer</param>
            /// <returns>Returns the planting point</returns>
            private static VertexBillboard? CalculatePoint(Scene scene, FoliageMap map, FoliageMapChannel description, BoundingBox gbbox, BoundingBox nbbox, Random rnd)
            {
                VertexBillboard? result = null;

                Vector2 min = new(gbbox.Minimum.X, gbbox.Minimum.Z);
                Vector2 max = new(gbbox.Maximum.X, gbbox.Maximum.Z);

                //Attempts
                for (int i = 0; i < 3; i++)
                {
                    Vector3 pos = new(
                        rnd.NextFloat(nbbox.Minimum.X, nbbox.Maximum.X),
                        nbbox.Maximum.Y + 1f,
                        rnd.NextFloat(nbbox.Minimum.Z, nbbox.Maximum.Z));

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
            private static bool Plant(Scene scene, Vector3 pos, Vector2 size, out VertexBillboard res)
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
            /// <param name="map">Foliage map</param>
            /// <param name="description">Terrain vegetation description</param>
            /// <param name="gbbox">Global bounding box</param>
            /// <param name="nbbox">Node bounding box</param>
            public async Task PlantAsync(Scene scene, FoliageMap map, FoliageMapChannel description, BoundingBox gbbox, BoundingBox nbbox)
            {
                if (Planting)
                {
                    return;
                }

                //Start planting task
                Planting = true;

                try
                {
                    Channel = description.Index;

                    foliageData = await Task.Run(() => PlantNode(scene, map, description, gbbox, nbbox));

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
                    return [];
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
            /// <param name="dc">Device context</param>
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="data">Vertex data</param>
            public void WriteData(IEngineDeviceContext dc, BufferManager bufferManager, IEnumerable<VertexBillboard> data)
            {
                vertexDrawCount = 0;
                Attached = false;

                //Get the data
                if (!data.Any())
                {
                    return;
                }

                //Attach data to buffer
                if (!bufferManager.WriteVertexBuffer(dc, VertexBuffer, data))
                {
                    return;
                }

                vertexDrawCount = data.Count();
                Attached = true;
            }
            /// <summary>
            /// Frees the buffer
            /// </summary>
            public void Free()
            {
                vertexDrawCount = 0;
                Attached = false;
            }
            /// <summary>
            /// Draws the foliage data
            /// </summary>
            /// <param name="dc">Device context</param>
            /// <param name="drawer">Drawer</param>
            public bool DrawFoliage(IEngineDeviceContext dc, BuiltInDrawer drawer)
            {
                if (vertexDrawCount <= 0)
                {
                    return false;
                }

                return drawer.Draw(dc, BufferManager, new DrawOptions
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
                return $"{Id} => Attached: {Attached}";
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
        /// Last visible node collection
        /// </summary>
        private QuadTreeNode[] visibleNodes = [];
        /// <summary>
        /// Foliage buffer list
        /// </summary>
        private readonly List<FoliageBuffer> foliageBuffers = [];
        /// <summary>
        /// Foliage patches list
        /// </summary>
        private readonly Dictionary<QuadTreeNode, List<FoliagePatch>> foliagePatches = [];
        /// <summary>
        /// Assigned patches list
        /// </summary>
        private readonly Dictionary<FoliagePatch, FoliageBuffer> assignedPatches = [];

        private bool planting = false;

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
        private readonly List<FoliageMapChannel> foliageMapChannels = [];
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
        /// Counter of the elapsed seconds between the last node sorting
        /// </summary>
        private float lastSortingElapsedSeconds = 0;
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
        public override async Task ReadAssets(GroundGardenerDescription description)
        {
            await base.ReadAssets(description);

            if (Description == null)
            {
                throw new EngineException("A gardener description should be specified.");
            }

            textureRandom = await Game.ResourceManager.RequestResource(Guid.NewGuid(), 1024, -1, 1, 24);

            foliageSphere = new BoundingSphere(Vector3.Zero, Description.VisibleRadius);

            //Material
            foliageMaterial = MeshMaterial.FromMaterial(MaterialPhong.Default);

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

        /// <inheritdoc/>
        public override async Task InitializeAssets()
        {
            await base.InitializeAssets();

            var gbbox = Description.PlantingArea ?? Scene.GetBoundingBox(SceneObjectUsages.Ground);

            //Creates the quad-tree if not exists, or if the reference bounding box has changed
            float sizeParts = MathF.Max(gbbox.Width, gbbox.Depth) / Description.NodeSize;

            int levels = Math.Max(1, (int)MathF.Log(sizeParts, 2));

            foliageQuadtree = new(gbbox, levels);
        }

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
        /// <summary>
        /// Updates patches state
        /// </summary>
        /// <param name="context">Updating context</param>
        private void UpdatePatchesAsync(UpdateContext context)
        {
            UpdatePatches(context);
        }
        /// <summary>
        /// Updates patches state
        /// </summary>
        /// <param name="context">Updating context</param>
        private void UpdatePatches(UpdateContext context)
        {
            if (planting)
            {
                return;
            }

            foliageSphere.Center = Scene.Camera.Position;

            var newVisible = GetFoliageNodes((IntersectionVolumeFrustum)Scene.Camera.Frustum, foliageSphere);
            if (newVisible.Length == 0)
            {
                return;
            }

            bool allNewNodesIn = EnumerableContains(visibleNodes, newVisible, out var remNodes);

            if (remNodes.Any())
            {
                FreeBuffers(remNodes);
            }

            visibleNodes = newVisible;

            if (allNewNodesIn)
            {
                return;
            }

            //Sort nodes by distance from camera position
            SortVisibleNodes(context.GameTime, Scene.Camera.Position);

            planting = true;

            List<FoliagePatch> toAssign = [];

            Task.Run(async () =>
            {
                try
                {
                    int channelCount = foliageMapChannels.Count;
                    for (int i = 0; i < visibleNodes.Length; i++)
                    {
                        var node = visibleNodes[i];

                        bool init = GetNodePatches(node, channelCount, out var fPatchList);
                        if (!init)
                        {
                            toAssign.AddRange(await DoPlantAsync(node, fPatchList));
                        }
                    }
                }
                finally
                {
                    planting = false;
                }
            }).ConfigureAwait(false).GetAwaiter().GetResult();

            foreach (var patch in toAssign)
            {
                var freeBuffer = GetNextFreeBuffer();

                if (assignedPatches.TryAdd(patch, freeBuffer))
                {
                    continue;
                }

                assignedPatches[patch] = freeBuffer;
            }
        }
        /// <summary>
        /// Gets the node list suitable for foliage planting
        /// </summary>
        /// <param name="volume">Culling volume</param>
        /// <param name="sph">Foliage bounding sphere</param>
        /// <returns>Returns a node list</returns>
        private QuadTreeNode[] GetFoliageNodes(ICullingVolume volume, BoundingSphere sph)
        {
            var nodes = foliageQuadtree.GetNodesInVolume(ref sph);
            if (nodes?.Any() != true)
            {
                return [];
            }

            return nodes.Where(n => volume.Contains(n.BoundingBox) != ContainmentType.Disjoint).ToArray();
        }
        /// <summary>
        /// Gets whether the first enumerable contains all the elements of the second enumerable
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="enum1">First enumerable list</param>
        /// <param name="enum2">Second enumerable list</param>
        /// <returns>Returns true if the first enumerable contains all the elements of the second enumerable</returns>
        private static bool EnumerableContains(IEnumerable<QuadTreeNode> enum1, IEnumerable<QuadTreeNode> enum2, out IEnumerable<QuadTreeNode> outNodes)
        {
            if (!enum1.Any())
            {
                outNodes = [];

                return false;
            }

            var allInNodes = enum2.Except(enum1).ToList();

            outNodes = enum1.Except(enum2).ToList();

            return allInNodes.Count == 0;
        }
        /// <summary>
        /// Frees the buffers of the specified node list
        /// </summary>
        /// <param name="nodes">Node list</param>
        private void FreeBuffers(IEnumerable<QuadTreeNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (!foliagePatches.TryGetValue(node, out var patches))
                {
                    continue;
                }

                foreach (var patch in patches)
                {
                    if (!assignedPatches.TryGetValue(patch, out var buffer))
                    {
                        continue;
                    }

                    buffer.Free();

                    assignedPatches.Remove(patch);
                }
            }
        }
        /// <summary>
        /// Sorts the visible nodes
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="eyePosition">Eye position</param>
        /// <remarks>Sorts nodes every second</remarks>
        private void SortVisibleNodes(IGameTime gameTime, Vector3 eyePosition)
        {
            lastSortingElapsedSeconds += gameTime.ElapsedSeconds;

            if (lastSortingElapsedSeconds < 1f)
            {
                return;
            }

            lastSortingElapsedSeconds = 0f;

            bool transparent = BlendMode.HasFlag(BlendModes.Alpha) || BlendMode.HasFlag(BlendModes.Transparent);

            visibleNodes = [.. visibleNodes.OrderBy(obj => (transparent ? -1 : 1) * Vector3.DistanceSquared(obj.Center, eyePosition))];
        }
        /// <summary>
        /// Gets node patches
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="channelCount">Channel count</param>
        /// <param name="patches">Patch list</param>
        /// <returns>Returns true if the node is initialized</returns>
        private bool GetNodePatches(QuadTreeNode node, int channelCount, out List<FoliagePatch> patches)
        {
            if (foliagePatches.TryGetValue(node, out patches))
            {
                return true;
            }

            patches = [];
            for (int i = 0; i < channelCount; i++)
            {
                patches.Add(new());
            }
            foliagePatches.Add(node, patches);

            return false;
        }
        /// <summary>
        /// Updates the patch list state and finds a list of patches with assigned data
        /// </summary>
        /// <returns>Returns a list of patches</returns>
        /// <remarks>
        /// For each high LOD visible node, look for planted foliagePatches.
        /// - If planted, see if they were assigned to a foliageBuffer
        ///   - If assigned, do nothing
        ///   - If not, add to toAssign list
        /// - If not planted, launch planting task y do nothing more. The node will be processed next time
        /// </remarks>
        private async Task<IEnumerable<FoliagePatch>> DoPlantAsync(QuadTreeNode node, List<FoliagePatch> patches)
        {
            List<FoliagePatch> toAssign = [];

            var gbbox = foliageQuadtree.BoundingBox;
            var nbbox = node.BoundingBox;

            for (int i = 0; i < patches.Count; i++)
            {
                var fPatch = patches[i];
                if (!fPatch.Planted)
                {
                    //Do the planting task
                    await fPatch.PlantAsync(Scene, foliageMap, foliageMapChannels[i], gbbox, nbbox);
                }
                else if (!fPatch.HasData || assignedPatches.ContainsKey(fPatch))
                {
                    continue;
                }

                toAssign.Add(fPatch);
            }

            return toAssign;
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

            if (visibleNodes.Length == 0)
            {
                return false;
            }

            bool draw = context.ValidateDraw(BlendMode);
            if (!draw)
            {
                return false;
            }

            var dc = context.DeviceContext;
            var eyePosition = context.Camera.Position;
            bool transparent = BlendMode.HasFlag(BlendModes.Alpha) || BlendMode.HasFlag(BlendModes.Transparent);
            WritePatches(dc, eyePosition, transparent);

            return DrawPatches(dc);
        }
        /// <summary>
        /// Attaches patches data into graphic buffers
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="transparent">Transparent</param>
        /// <remarks>
        /// For each node to assign
        /// - Look for a free buffer. It's free if unassigned or assigned to not visible node
        ///   - If free buffer found, assign
        ///   - If not, look for a buffer to free, farthest from camera first
        /// </remarks>
        private void WritePatches(IEngineDeviceContext dc, Vector3 eyePosition, bool transparent)
        {
            foreach (var patch in assignedPatches.Keys)
            {
                var buffer = assignedPatches[patch];
                if (buffer == null)
                {
                    continue;
                }

                if (buffer.Attached)
                {
                    continue;
                }

                var data = patch.GetData(eyePosition, transparent);
                buffer.WriteData(dc, BufferManager, data);
            }
        }
        /// <summary>
        /// Gets the next free buffer
        /// </summary>
        private FoliageBuffer GetNextFreeBuffer()
        {
            foreach (var buffer in foliageBuffers)
            {
                if (assignedPatches.Any(pb => pb.Value == buffer))
                {
                    continue;
                }

                return buffer;
            }

            var free = assignedPatches.Select(pb => pb.Value).FirstOrDefault(b => b != null && b.Attached == false);
            if (free != null)
            {
                return free;
            }

            return null;
        }
        /// <summary>
        /// Draws the visible patch list
        /// </summary>
        /// <param name="dc">Device context</param>
        private bool DrawPatches(IEngineDeviceContext dc)
        {
            bool drawn = false;
            foreach (var node in visibleNodes)
            {
                var patches = foliagePatches[node];
                var patchBuffers = patches.Where(assignedPatches.ContainsKey).Select(p => new { Patch = p, Buffer = assignedPatches[p] });
                foreach (var pb in patchBuffers)
                {
                    var channelData = foliageMapChannels[pb.Patch.Channel];

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

                    foliageDrawer.UpdateFoliage(dc, state);

                    drawn = pb.Buffer.DrawFoliage(dc, foliageDrawer) || drawn;
                }
            }

            return drawn;
        }

        /// <inheritdoc/>
        public override bool Cull(int cullIndex, ICullingVolume volume, out float distance)
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

        /// <inheritdoc/>
        public IEnumerable<IMeshMaterial> GetMaterials()
        {
            return foliageMaterial != null ? [foliageMaterial] : [];
        }
        /// <inheritdoc/>
        public IMeshMaterial GetMaterial(string meshMaterialName)
        {
            return foliageMaterial;
        }
        /// <inheritdoc/>
        public bool ReplaceMaterial(string meshMaterialName, IMeshMaterial material)
        {
            if (foliageMaterial == material)
            {
                return false;
            }

            foliageMaterial = material;

            return true;
        }

        /// <summary>
        /// Gets the bounds of the ground gardener
        /// </summary>
        public BoundingBox GetPlantingBounds()
        {
            return foliageQuadtree.BoundingBox;
        }
        /// <summary>
        /// Gets the visible node list
        /// </summary>
        public QuadTreeNode[] GetVisibleNodes()
        {
            return [.. visibleNodes];
        }
    }
}
