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
                foliagePatches.Clear();
                assignedPatches.Clear();

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

            //Sort nodes by distance from camera position
            SortVisibleNodes(context.GameTime, Scene.Camera.Position);

            if (!UpdateVisibleNodes())
            {
                return;
            }

            UpdatePatches();
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

            Array.Sort(visibleNodes, (n1, n2) =>
            {
                int f = transparent ? -1 : 1;
                float d1 = f * Vector3.DistanceSquared(n1.Center, eyePosition);
                float d2 = f * Vector3.DistanceSquared(n2.Center, eyePosition);
                return d1.CompareTo(d2);
            });
        }
        /// <summary>
        /// Update visible nodes
        /// </summary>
        /// <returns>Returns whether the visibility has changed or not</returns>
        private bool UpdateVisibleNodes()
        {
            foliageSphere.Center = Scene.Camera.Position;

            var newVisible = GetFoliageNodes((IntersectionVolumeFrustum)Scene.Camera.Frustum, foliageSphere);

            bool allNewNodesIn = EnumerableContains(visibleNodes, newVisible, out var remNodes);
            bool nodesOut = remNodes.Any();

            visibleNodes = newVisible;

            if (nodesOut)
            {
                FreeBuffers(remNodes);
            }

            if (allNewNodesIn && !nodesOut)
            {
                //No changes
                return false;
            }

            return true;
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
        /// Updates patches state
        /// </summary>
        private void UpdatePatches()
        {
            List<FoliagePatch> toAssign = [];

            var gbbox = foliageQuadtree.BoundingBox;

            int channelCount = foliageMapChannels.Count;
            for (int i = 0; i < visibleNodes.Length; i++)
            {
                var node = visibleNodes[i];
                var nbbox = node.BoundingBox;

                bool init = GetNodePatches(node, channelCount, out var fPatchList);
                if (!init)
                {
                    toAssign.AddRange(DoPlant(fPatchList, gbbox, nbbox));
                }
            }

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
        /// <param name="patches">Patch list</param>
        /// <param name="gbbox">Global bounding box</param>
        /// <param name="nbbox">Node bounding box</param>
        /// <returns>Returns a list of patches</returns>
        /// <remarks>
        /// For each high LOD visible node, look for planted foliagePatches.
        /// - If planted, see if they were assigned to a foliageBuffer
        ///   - If assigned, do nothing
        ///   - If not, add to toAssign list
        /// - If not planted, launch planting task y do nothing more. The node will be processed next time
        /// </remarks>
        private IEnumerable<FoliagePatch> DoPlant(List<FoliagePatch> patches, BoundingBox gbbox, BoundingBox nbbox)
        {
            for (int i = 0; i < patches.Count; i++)
            {
                var patch = patches[i];
                if (!patch.Planted)
                {
                    //Do the planting task
                    Task.Run(async () => await patch.PlantAsync(Scene, foliageMap, foliageMapChannels[i], gbbox, nbbox))
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                }
                else if (!patch.HasData || assignedPatches.ContainsKey(patch))
                {
                    continue;
                }

                yield return patch;
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

            var free = assignedPatches.Select(pb => pb.Value).FirstOrDefault(b => b != null && !b.Attached);
            if (free != null)
            {
                return free;
            }

            return null;
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
            foreach (var (patch, buffer) in assignedPatches)
            {
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
