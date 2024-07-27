using Engine.BuiltIn.Drawers;
using Engine.BuiltIn.Drawers.Foliage;
using Engine.Collections;
using Engine.Common;
using Engine.Content;
using SharpDX;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.BuiltIn.Components.Foliage
{
    /// <summary>
    /// Foliage class helper
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class Foliage(Scene scene, string id, string name) : Drawable<FoliageDescription>(scene, id, name), IUseMaterials
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
        private readonly List<FoliagePatch> foliagePatches = [];
        /// <summary>
        /// To assign buffer foliage patch queue
        /// </summary>
        private readonly ConcurrentQueue<FoliagePatch> toAssignQueue = [];

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
        /// Point of view
        /// </summary>
        public Vector3 PointOfView { get; set; }
        /// <summary>
        /// Point of view frustum
        /// </summary>
        public BoundingFrustum PointOfViewFrustum { get; set; }
        /// <summary>
        /// Uses the camera as point of view
        /// </summary>
        public bool UseCameraAsPointOfView { get; set; } = true;
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
        ~Foliage()
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
        public override async Task ReadAssets(FoliageDescription description)
        {
            await base.ReadAssets(description);

            if (Description == null)
            {
                throw new EngineException("A gardener description should be specified.");
            }

            textureRandom = Game.ResourceManager.RequestResource(Guid.NewGuid(), 1024, -1, 1, 24);

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
                    var newChannel = CreateChannel(channelDesc, i, contentPath);

                    foliageMapChannels.Add(newChannel);
                }
            }

            for (int i = 0; i < MaxFoliageBuffers; i++)
            {
                foliageBuffers.Add(new(Game, Name));
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
        private FoliageMapChannel CreateChannel(FoliageDescription.Channel channel, int index, string contentPath)
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
                foliageTextures = Game.ResourceManager.RequestResource(image);
            }

            if (normalMapCount > 0)
            {
                var image = new FileArrayImageContent(contentPath, channel.VegetationNormalMaps);
                foliageNormalMaps = Game.ResourceManager.RequestResource(image);
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

            //Creates the foliage patch structure
            int channelCount = foliageMapChannels.Count;
            foreach (var node in foliageQuadtree.GetLeafNodes())
            {
                for (int i = 0; i < channelCount; i++)
                {
                    foliagePatches.Add(new(node));
                }
            }
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!initialized)
            {
                return;
            }

            if (UseCameraAsPointOfView)
            {
                PointOfView = Scene.Camera.Position;
                PointOfViewFrustum = Scene.Camera.Frustum;
            }

            if (updatingNodes)
            {
                //Node update task runing
                return;
            }

            updatingNodes = true;

            //Copy variables for the async task
            float time = context.GameTime.ElapsedSeconds;
            var frustum = PointOfViewFrustum;
            var eyePosition = PointOfView;
            foliageSphere.Center = eyePosition;
            var sph = foliageSphere;
            var ic = Scene.Game.Graphics.ImmediateContext;
            bool transparent = IsTransparent();

            //Update node visibility
            UpdateNodeVisibility(ic, eyePosition, transparent, frustum, sph);

            //Writes data into graphics device
            WritePatches(ic, eyePosition, transparent);

            Task.Run(() =>
            {
                try
                {
                    //Sort nodes
                    SortVisibleNodes(time, eyePosition);

                    //Update visible patches data
                    UpdatePatches();
                }
                finally
                {
                    updatingNodes = false;
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates the node visibility
        /// </summary>
        /// <param name="ic">Device context</param>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="transparent">Transparent</param>
        /// <param name="frustum">Bounding frustum</param>
        /// <param name="sph">Bounding sphere</param>
        private void UpdateNodeVisibility(IEngineDeviceContext ic, Vector3 eyePosition, bool transparent, BoundingFrustum frustum, BoundingSphere sph)
        {
            //Update visible nodes
            visibleNodes = GetVisibleNodes(frustum, sph);

            foreach (var node in visibleNodes)
            {
                //Get the node filled patch list, without buffer
                var patchList = foliagePatches
                    .Where(p => p.Node == node && !p.ReadyForDrawing && p.Planted && p.HasData)
                    .ToArray();

                foreach (var patch in patchList)
                {
                    //Assign a buffer
                    var freeBuffer = FindBuffer(eyePosition);

                    //Assign a new free buffer to the node data
                    patch.SetBuffer(freeBuffer);
                    patch.SortData(eyePosition, transparent);
                    patch.WriteData(ic);
                }
            }
        }
        /// <summary>
        /// Gets the visible node list from the quad-tree
        /// </summary>
        /// <param name="frustum">Camera frustum</param>
        /// <param name="sph">Sphere</param>
        /// <returns>Returns a node list</returns>
        private QuadTreeNode[] GetVisibleNodes(BoundingFrustum frustum, BoundingSphere sph)
        {
            var nodes = foliageQuadtree.FindNodesInVolume((IntersectionVolumeSphere)sph);
            if (!nodes.Any())
            {
                return [];
            }

            var volume = (IntersectionVolumeFrustum)frustum;

            return nodes.Where(n => volume.Contains(n.BoundingBox) != ContainmentType.Disjoint).ToArray();
        }

        /// <summary>
        /// Finds a buffer
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        private FoliageBuffer FindBuffer(Vector3 eyePosition)
        {
            // Find a free buffer
            var freeBuffer = GetNextFreeBuffer();
            if (freeBuffer == null)
            {
                // Take the buffer from another patch
                var farestPatch = GetFreeFarestPatch(eyePosition);
                freeBuffer = farestPatch?.Buffer;

                // Remove the buffer from the farest patch
                farestPatch?.SetBuffer(null);
            }

            return freeBuffer;
        }
        /// <summary>
        /// Gets the next free buffer
        /// </summary>
        private FoliageBuffer GetNextFreeBuffer()
        {
            //Try to find unused first
            var freeBuffer = foliageBuffers.Find(b => !foliagePatches.Exists(p => p.Buffer == b));
            if (freeBuffer != null)
            {
                return freeBuffer;
            }

            //All buffers already used.
            return null;
        }
        /// <summary>
        /// Gets the farest not visible patch
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        private FoliagePatch GetFreeFarestPatch(Vector3 eyePosition)
        {
            //Try to find not visible & fartest from position
            var notVisible = foliagePatches
                .FindAll(p => p.Buffer != null && !Array.Exists(visibleNodes, n => n == p.Node))
                .OrderByDescending(p => Vector3.DistanceSquared(p.Node.Center, eyePosition));

            return notVisible.FirstOrDefault();
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
        /// Updates patches state
        /// </summary>
        private void UpdatePatches()
        {
            var gbbox = foliageQuadtree.BoundingBox;

            //Iterate over the visible nodes, and get the current patch configuration of each
            foreach (var node in visibleNodes)
            {
                var nbbox = node.BoundingBox;

                //Get the node patch list
                var patchList = foliagePatches.Where(p => p.Node == node).ToArray();

                //Initialize the patch list, and enqueue the resulting action in the assign buffer queue
                DoPlant(patchList, gbbox, nbbox, toAssignQueue.Enqueue);
            }
        }
        /// <summary>
        /// Updates the patch list state and finds a list of patches with assigned data
        /// </summary>
        /// <param name="patches">Patch list</param>
        /// <param name="gbbox">Global bounding box</param>
        /// <param name="nbbox">Node bounding box</param>
        /// <param name="callback">Planted patch callback</param>
        /// <remarks>
        /// For each high LOD visible node, look for planted foliagePatches.
        /// - If planted, see if they were assigned to a foliageBuffer
        ///   - If assigned, do nothing
        ///   - If not, add to toAssign list
        /// - If not planted, launch planting task y do nothing more. The node will be processed next time
        /// </remarks>
        private void DoPlant(FoliagePatch[] patches, BoundingBox gbbox, BoundingBox nbbox, Action<FoliagePatch> callback)
        {
            //Select all patches without calculated data
            var toPlant = patches
                .Where(p => !p.ReadyForDrawing && !p.Planting && !p.Planted)
                .ToArray();

            Parallel.For(0, toPlant.Length, (i) =>
            {
                var p = toPlant[i];

                p.Plant(Scene, foliageMap, foliageMapChannels[i], gbbox, nbbox, () =>
                {
                    //Enqueue
                    callback?.Invoke(p);
                });
            });
        }

        /// <summary>
        /// Writes patches data into graphic buffers
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        private void WritePatches(IEngineDeviceContext ic, Vector3 eyePosition, bool transparent)
        {
            while (toAssignQueue.TryDequeue(out var patch))
            {
                if (patch.ReadyForDrawing)
                {
                    //Already has a buffer
                    return;
                }

                if (patch.Planted && !patch.HasData)
                {
                    //No data
                    patch.SetBuffer(null);
                    return;
                }

                // Find a free buffer
                var freeBuffer = FindBuffer(eyePosition);

                //Assign a new free buffer to the node data
                patch.SetBuffer(freeBuffer);
                patch.SortData(eyePosition, transparent);
                patch.WriteData(ic);
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

            var visiblePatches = foliagePatches
                .FindAll(p => Array.Exists(visibleNodes, n => n == p.Node));

            var toDrawPatches = visiblePatches
                .FindAll(p => p.ReadyForDrawing);

            foreach (var patch in toDrawPatches)
            {
                var channelData = foliageMapChannels[patch.Channel];

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

                    PointOfView = PointOfView,

                    WindDirection = WindDirection,
                    WindStrength = WindStrength * channelData.WindEffect,
                };

                foliageDrawer.UpdateFoliage(dc, state);

                drawn = patch.Buffer.DrawFoliage(dc, foliageDrawer) || drawn;
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
        /// Gets the bounds of the foliage area
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
        /// <summary>
        /// Gets the quadtree node list
        /// </summary>
        public QuadTreeNode[] GetAllNodes()
        {
            var allNodes = foliageQuadtree.GetLeafNodes();

            return [.. allNodes];
        }
        /// <summary>
        /// Gets the patch list state
        /// </summary>
        public (QuadTreeNode Node, bool WithData, bool Ready)[] GetPatches()
        {
            return foliagePatches
                .Select(patch => (patch.Node, patch.Planted && patch.HasData, patch.ReadyForDrawing))
                .ToArray();
        }
    }
}
