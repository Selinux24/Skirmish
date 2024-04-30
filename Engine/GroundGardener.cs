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
        /// <summary>
        /// Maximum number of active buffer for foliage drawing
        /// </summary>
        public const int MaxFoliageBuffers = 32;
        /// <summary>
        /// Maximum number of cached patches for foliage data
        /// </summary>
        public const int MaxFoliagePatches = MaxFoliageBuffers * 2;

        /// <summary>
        /// Initialized flag
        /// </summary>
        private bool initialized = false;
        /// <summary>
        /// Updating nodes flag
        /// </summary>
        private bool updatingNodes = false;
        /// <summary>
        /// Update pending flag
        /// </summary>
        private bool updatePending = false;

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
        private readonly Dictionary<QuadTreeNode, FoliageNode[]> foliagePatches = [];

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
        /// Last eye position for node sorting
        /// </summary>
        private Vector3 lastSortingEyePosition = Vector3.One * float.MaxValue;
        /// <summary>
        /// Foliage drawer
        /// </summary>
        private BuiltInFoliage foliageDrawer = null;

        /// <summary>
        /// Time between node sorting
        /// </summary>
        public float NodeSortingSeconds { get; set; } = 5f;
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
                foliagePatches.Clear();

                for (int i = 0; i < foliageBuffers.Count; i++)
                {
                    foliageBuffers[i]?.Dispose();
                    foliageBuffers[i] = null;
                }
                foliageBuffers.Clear();

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

        /// <summary>
        /// Gets whether the selected blend mode has transparency
        /// </summary>
        private bool IsTransparent() => BlendMode.HasFlag(BlendModes.Alpha) || BlendMode.HasFlag(BlendModes.Transparent);

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
                foliageBuffers.Add(new(BufferManager, Name));
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
                Density = channel.Density,
                MinSize = channel.MinSize,
                MaxSize = channel.MaxSize,
                Delta = channel.Delta,
                StartRadius = channel.StartRadius,
                EndRadius = channel.EndRadius,
                TextureCount = (uint)textureCount,
                NormalMapCount = (uint)normalMapCount,
                Textures = foliageTextures,
                NormalMaps = foliageNormalMaps,
                TintColor = channel.TintColor,
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

            if (updatingNodes)
            {
                //Node update task runing
                return;
            }

            updatingNodes = true;

            //Copy variables for the async task
            float time = context.GameTime.ElapsedSeconds;
            var eyePosition = Scene.Camera.Position;
            foliageSphere.Center = eyePosition;
            var frustum = Scene.Camera.Frustum;
            var sph = foliageSphere;

            Task.Run(() =>
            {
                try
                {
                    //Sort nodes
                    SortVisibleNodes(time, eyePosition);

                    //Update node visibility
                    if (!UpdateNodeVisibility(frustum, sph))
                    {
                        return;
                    }

                    //Update visible patches data
                    UpdatePatches(eyePosition);
                }
                finally
                {
                    updatingNodes = false;

                    //Enque an update
                    updatePending = true;
                }
            }).ConfigureAwait(false);

            if (updatePending)
            {
                //Writes data into graphics device
                WritePatches();

                updatePending = false;
            }
        }

        /// <summary>
        /// Sorts the visible nodes
        /// </summary>
        /// <param name="time">Elapsed time in seconds</param>
        /// <param name="eyePosition">Eye position</param>
        /// <remarks>Sorts nodes every second</remarks>
        private void SortVisibleNodes(float time, Vector3 eyePosition)
        {
            lastSortingElapsedSeconds += time;

            if (lastSortingElapsedSeconds < NodeSortingSeconds)
            {
                //Perform sorting every NodeSortingSeconds
                return;
            }

            if (Vector3.DistanceSquared(eyePosition, lastSortingEyePosition) < foliageSphere.Radius * foliageSphere.Radius)
            {
                //Perform sorting if eye position moves away of the foliage sphere radius
                return;
            }

            lastSortingElapsedSeconds = 0f;
            lastSortingEyePosition = eyePosition;

            int f = IsTransparent() ? -1 : 1;

            Array.Sort(visibleNodes, (n1, n2) =>
            {
                float d1 = f * Vector3.DistanceSquared(n1.Center, eyePosition);
                float d2 = f * Vector3.DistanceSquared(n2.Center, eyePosition);
                return d1.CompareTo(d2);
            });
        }

        /// <summary>
        /// Updates the visible nodes, and frees the buffers of the not visible nodes
        /// </summary>
        /// <param name="frustum">Camera frustum</param>
        /// <param name="sph">Sphere</param>
        /// <returns>Returns whether the visibility has changed or not</returns>
        private bool UpdateNodeVisibility(BoundingFrustum frustum, BoundingSphere sph)
        {
            var newVisible = GetFoliageNodes(frustum, sph);

            bool allNewNodesIn = EnumerableContains(visibleNodes, newVisible, out var remNodes);
            bool nodesOut = remNodes.Any();

            visibleNodes = newVisible;

            if (nodesOut)
            {
                FreeBuffers(remNodes);
            }

            if (allNewNodesIn)
            {
                //No new nodes
                return false;
            }

            return true;
        }
        /// <summary>
        /// Gets the visible node list from the quad-tree
        /// </summary>
        /// <param name="frustum">Camera frustum</param>
        /// <param name="sph">Sphere</param>
        /// <returns>Returns a node list</returns>
        private QuadTreeNode[] GetFoliageNodes(BoundingFrustum frustum, BoundingSphere sph)
        {
            var nodes = foliageQuadtree.GetNodesInVolume(ref sph);
            if (!nodes.Any())
            {
                return [];
            }

            var volume = (IntersectionVolumeFrustum)frustum;

            return nodes.Where(n => volume.Contains(n.BoundingBox) != ContainmentType.Disjoint).ToArray();
        }
        /// <summary>
        /// Gets whether the first enumerable contains all the elements of the second enumerable
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="enum1">First enumerable list</param>
        /// <param name="enum2">Second enumerable list</param>
        /// <param name="outNodes">Returns the nodes from enum1, not in enum 2</param>
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
        /// Frees the buffers associated to the specified node list
        /// </summary>
        /// <param name="nodes">Node list</param>
        private void FreeBuffers(IEnumerable<QuadTreeNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (!foliagePatches.TryGetValue(node, out var nodeData))
                {
                    continue;
                }

                foreach (var d in nodeData)
                {
                    //Frees the buffer, but maintain the patch data
                    d.FreeBuffer(false);
                }
            }
        }

        /// <summary>
        /// Updates patches state
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        private void UpdatePatches(Vector3 eyePosition)
        {
            List<FoliageNode> toAssign = [];

            var gbbox = foliageQuadtree.BoundingBox;
            int channelCount = foliageMapChannels.Count;

            //Iterate over the visible nodes, and get the current patch configuration of each
            foreach (var node in visibleNodes)
            {
                var nbbox = node.BoundingBox;

                //Get the node data
                bool initialized = GetNodePatches(node, channelCount, out var nodeData);
                if (!initialized)
                {
                    //Initialice the node data
                    DoPlant(nodeData, gbbox, nbbox);
                }

                //Collect the node data to assign a buffer
                toAssign.AddRange(nodeData.Where(n => n.IsReadyToAssign()));
            }

            //Iterate over the node data
            foreach (var n in toAssign)
            {
                if (n.Buffer != null)
                {
                    //Already has a buffer
                    continue;
                }

                //Assign a new free buffer to the node data
                n.SetBuffer(GetNextFreeBuffer(eyePosition));
            }
        }
        /// <summary>
        /// Gets node patches
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="channelCount">Channel count</param>
        /// <param name="nodeData">Node data</param>
        /// <returns>Returns true if the node is initialized</returns>
        private bool GetNodePatches(QuadTreeNode node, int channelCount, out FoliageNode[] nodeData)
        {
            if (foliagePatches.TryGetValue(node, out nodeData))
            {
                return true;
            }

            nodeData = Helper.CreateArray<FoliageNode>(channelCount, () => new());

            foliagePatches.Add(node, nodeData);

            return false;
        }
        /// <summary>
        /// Updates the patch list state and finds a list of patches with assigned data
        /// </summary>
        /// <param name="nodeData">Node data</param>
        /// <param name="gbbox">Global bounding box</param>
        /// <param name="nbbox">Node bounding box</param>
        /// <remarks>
        /// For each high LOD visible node, look for planted foliagePatches.
        /// - If planted, see if they were assigned to a foliageBuffer
        ///   - If assigned, do nothing
        ///   - If not, add to toAssign list
        /// - If not planted, launch planting task y do nothing more. The node will be processed next time
        /// </remarks>
        private void DoPlant(FoliageNode[] nodeData, BoundingBox gbbox, BoundingBox nbbox)
        {
            //Select all patches without calculated data
            var toPlant = nodeData
                .Where(n => !n.Patch.Planted)
                .Select(n => n.Patch)
                .ToArray();

            var res = Parallel.For(0, toPlant.Length, (i) =>
            {
                toPlant[i].Plant(Scene, foliageMap, foliageMapChannels[i], gbbox, nbbox);
            });
        }
        /// <summary>
        /// Gets the next free buffer
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        private FoliageBuffer GetNextFreeBuffer(Vector3 eyePosition)
        {
            //Try to find unused first
            foreach (var buffer in foliageBuffers)
            {
                if (foliagePatches.Values.Any(n => n.Any(pb => pb.Buffer == buffer)))
                {
                    continue;
                }

                return buffer;
            }

            //All buffer already used. Try to find not visible & fartest from position
            var notVisibleNodes = foliagePatches.Keys.Except(visibleNodes).OrderByDescending(n => Vector3.DistanceSquared(n.Center, eyePosition));
            foreach (var node in notVisibleNodes)
            {
                foreach (var nd in foliagePatches[node])
                {
                    var buffer = nd.Buffer;

                    if (buffer == null || buffer.Attached)
                    {
                        continue;
                    }

                    return buffer;
                }
            }

            //No buffer
            return null;
        }

        /// <summary>
        /// Writes patches data into graphic buffers
        /// </summary>
        private void WritePatches()
        {
            var dc = Scene.Game.Graphics.ImmediateContext;
            var eyePosition = Scene.Camera.Position;
            bool transparent = IsTransparent();

            foreach (var node in visibleNodes)
            {
                if (!foliagePatches.TryGetValue(node, out var nodeData))
                {
                    continue;
                }

                foreach (var d in nodeData)
                {
                    d.WriteData(dc, BufferManager, eyePosition, transparent);
                }
            }
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

            return DrawPatches(dc);
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
                if (!foliagePatches.TryGetValue(node, out var nodeData))
                {
                    continue;
                }

                foreach (var d in nodeData)
                {
                    if (!d.IsReadyToDraw())
                    {
                        continue;
                    }

                    var channelData = foliageMapChannels[d.Patch.Channel];

                    var state = new BuiltInFoliageState
                    {
                        TintColor = Color4.White,

                        StartRadius = channelData.StartRadius,
                        EndRadius = channelData.EndRadius,
                        TextureCount = channelData.TextureCount,
                        NormalMapCount = channelData.NormalMapCount,
                        Texture = channelData.Textures,
                        NormalMaps = channelData.NormalMaps,
                        Delta = channelData.Delta,
                        WindEffect = channelData.WindEffect,
                        Instances = channelData.Count,

                        MaterialIndex = foliageMaterial.ResourceIndex,
                        RandomTexture = textureRandom,

                        WindDirection = WindDirection,
                        WindStrength = WindStrength * channelData.WindEffect,
                    };

                    foliageDrawer.UpdateFoliage(dc, state);

                    drawn = d.Buffer.DrawFoliage(dc, foliageDrawer) || drawn;
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
