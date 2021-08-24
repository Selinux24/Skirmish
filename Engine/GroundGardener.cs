﻿using SharpDX;
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
    public class GroundGardener : Drawable, IUseMaterials
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
            /// Counter of the elapsed seconds between the last node sorting
            /// </summary>
            private float lastSortingElapsedSeconds = 0;

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
            /// <param name="delay">Task delay</param>
            public void Plant(Scene scene, QuadTreeNode node, FoliageMap map, FoliageMapChannel description, BoundingBox gbbox, int delay)
            {
                if (!Planting)
                {
                    //Start planting task
                    CurrentNode = node;
                    Channel = description.Index;
                    Planting = true;

                    Task
                        .Run(async () =>
                        {
                            await Task.Delay(delay);

                            return PlantTask(scene, node, map, description, gbbox);
                        })
                        .ContinueWith((t) =>
                        {
                            Planting = false;
                            Planted = true;
                            foliageData = t.Result;
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
            /// <param name="gbbox">Relative bounding box to plant</param>
            /// <returns>Returns generated vertex data</returns>
            private static IEnumerable<VertexBillboard> PlantTask(Scene scene, QuadTreeNode node, FoliageMap map, FoliageMapChannel description, BoundingBox gbbox)
            {
                if (node == null)
                {
                    return new VertexBillboard[] { };
                }

                List<VertexBillboard> vertexData = new List<VertexBillboard>(MAX);

                Vector2 min = new Vector2(gbbox.Minimum.X, gbbox.Minimum.Z);
                Vector2 max = new Vector2(gbbox.Maximum.X, gbbox.Maximum.Z);

                Random rnd = new Random(description.Seed);
                var bbox = node.BoundingBox;
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
                        var c = map.GetRelative(pos, min, max);

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
                        var size = new Vector2(
                            rnd.NextFloat(description.MinSize.X, description.MaxSize.X),
                            rnd.NextFloat(description.MinSize.Y, description.MaxSize.Y));

                        var planted = Plant(scene, pos, size, out var res);
                        if (planted)
                        {
                            vertexData.Add(res);

                            count--;
                        }
                    }
                    else
                    {
                        count--;
                    }
                }

                return vertexData.ToArray();
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
                var ray = scene.GetTopDownRay(pos);

                bool found = scene.PickFirst<Triangle>(
                    ray,
                    RayPickingParams.FacingOnly | RayPickingParams.Geometry,
                    SceneObjectUsages.Ground,
                    out var r);

                if (found && r.Item.Normal.Y > 0.5f)
                {
                    res = new VertexBillboard()
                    {
                        Position = r.Position,
                        Size = size,
                    };

                    return true;
                }

                res = new VertexBillboard();

                return false;
            }
            /// <summary>
            /// Get foliage data
            /// </summary>
            /// <param name="gameTime">Game time</param>
            /// <param name="eyePosition">Eye position</param>
            /// <param name="transparent">Use transparency</param>
            /// <returns>Returns the foliage data ordered by distance to eye position. Far first if transparency specified, near first otherwise</returns>
            /// <remarks>Returns the foliage data</remarks>
            public IEnumerable<VertexBillboard> GetData(GameTime gameTime, Vector3 eyePosition, bool transparent)
            {
                lastSortingElapsedSeconds += gameTime.ElapsedSeconds;

                if (lastSortingElapsedSeconds < 1f)
                {
                    return foliageData;
                }

                lastSortingElapsedSeconds = 0f;

                //Sort data
                return foliageData
                    .OrderBy(obj => (transparent ? -1 : 1) * Vector3.DistanceSquared(obj.Position, eyePosition));
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
            /// <param name="gameTime">Game time</param>
            /// <param name="eyePosition">Eye position</param>
            /// <param name="transparent">The billboards were transparent</param>
            /// <param name="patch">Patch</param>
            /// <param name="bufferManager">Buffer manager</param>
            public void AttachFoliage(GameTime gameTime, Vector3 eyePosition, bool transparent, FoliagePatch patch, BufferManager bufferManager)
            {
                vertexDrawCount = 0;
                Attached = false;
                CurrentPatch = null;

                if (patch.HasData)
                {
                    var data = patch.GetData(gameTime, eyePosition, transparent);

                    //Attach data
                    if (!bufferManager.WriteVertexBuffer(VertexBuffer, data.ToArray()))
                    {
                        return;
                    }

                    vertexDrawCount = data.Count();
                    Attached = true;
                    CurrentPatch = patch;
                }
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
        /// Gets the ground gardener descrition
        /// </summary>
        protected new GroundGardenerDescription Description
        {
            get
            {
                return base.Description as GroundGardenerDescription;
            }
        }

        /// <summary>
        /// Current active planting tasks
        /// </summary>
        public int PlantingTasks { get; private set; }

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
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public GroundGardener(string id, string name, Scene scene, GroundGardenerDescription description) :
            base(id, name, scene, description)
        {
            if (description == null)
            {
                throw new EngineException("A gardener description should be specified.");
            }

            textureRandom = Game.ResourceManager.RequestResource(Guid.NewGuid(), 1024, -1, 1, 24);

            foliageSphere = new BoundingSphere(Vector3.Zero, description.VisibleRadius);

            //Material
            foliageMaterial = MeshMaterial.DefaultPhong;

            //Read foliage textures
            string contentPath = description.ContentPath;

            if (!string.IsNullOrEmpty(description.VegetationMap))
            {
                var foliageMapData = ContentManager.FindContent(contentPath, description.VegetationMap).FirstOrDefault();
                foliageMap = FoliageMap.FromStream(foliageMapData);
            }

            for (int i = 0; i < description.Channels.Length; i++)
            {
                var channelDesc = description.Channels[i];
                if (channelDesc?.Enabled == true)
                {
                    var newChannel = CreateChannel(channelDesc, i, contentPath);

                    foliageMapChannels.Add(newChannel);
                }
            }

            for (int i = 0; i < MaxFoliageBuffers; i++)
            {
                foliageBuffers.Add(new FoliageBuffer(Game, BufferManager, name));
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~GroundGardener()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Resource disposal
        /// </summary>
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

        /// <summary>
        /// Creates a map channel from the specified description
        /// </summary>
        /// <param name="channel">Channel description</param>
        /// <param name="index">Channel index</param>
        /// <param name="contentPath">Resources content path</param>
        /// <returns>Returns the new map channel</returns>
        private FoliageMapChannel CreateChannel(GroundGardenerDescription.Channel channel, int index, string contentPath)
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
                var image = ImageContent.Array(contentPath, channel.VegetationTextures);
                foliageTextures = Game.ResourceManager.RequestResource(image);
            }

            if (normalMapCount > 0)
            {
                var image = ImageContent.Array(contentPath, channel.VegetationNormalMaps);
                foliageNormalMaps = Game.ResourceManager.RequestResource(image);
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

        /// <summary>
        /// Updates the gardener
        /// </summary>
        /// <param name="context">Update context</param>
        public override void Update(UpdateContext context)
        {
            windTime += context.GameTime.ElapsedSeconds * WindStrength;

            var bbox = Description.PlantingArea ?? Scene.GetGroundBoundingBox();
            if (!bbox.HasValue)
            {
                return;
            }

            BuildQuadtree(bbox.Value, Description.NodeSize);

            foliageSphere.Center = context.EyePosition;

            visibleNodes = GetFoliageNodes(context.CameraVolume, foliageSphere);
            if (visibleNodes.Any())
            {
                bool transparent = BlendMode.HasFlag(BlendModes.Alpha) || BlendMode.HasFlag(BlendModes.Transparent);

                //Sort nodes by distance from camera position
                SortVisibleNodes(context.GameTime, context.EyePosition, transparent);

                //Assign foliage patches
                AssignPatches(context.GameTime, context.EyePosition, bbox.Value);
            }

            PlantingTasks = 0;

            foreach (var q in foliagePatches)
            {
                PlantingTasks += q.Value.FindAll(f => f.Planting).Count;
            }
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
        /// Assign patches
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="bbox">Relative bounding box to plant</param>
        private void AssignPatches(GameTime gameTime, Vector3 eyePosition, BoundingBox bbox)
        {
            //Find patches to assign data
            var toAssign = FindAssigned(bbox);
            if (toAssign.Count > 0)
            {
                //Mark patches to delete
                MarkFreePatches(gameTime, eyePosition, toAssign);
            }

            //Free unused patches
            FreeUnusedPatches(eyePosition);
        }
        /// <summary>
        /// Finds a list of patches with assigned data
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
        private List<FoliagePatch> FindAssigned(BoundingBox bbox)
        {
            List<FoliagePatch> toAssign = new List<FoliagePatch>();

            int count = foliageMapChannels.Count;

            foreach (var node in visibleNodes)
            {
                if (!foliagePatches.ContainsKey(node))
                {
                    foliagePatches.Add(node, new List<FoliagePatch>());

                    for (int i = 0; i < count; i++)
                    {
                        foliagePatches[node].Add(new FoliagePatch());
                    }
                }

                var fPatchList = foliagePatches[node];

                for (int i = 0; i < fPatchList.Count; i++)
                {
                    var fPatch = fPatchList[i];

                    if (!fPatch.Planted)
                    {
                        fPatch.Plant(Scene, node, foliageMap, foliageMapChannels[i], bbox, i * 100);
                    }
                    else if (fPatch.HasData && !foliageBuffers.Exists(b => b.CurrentPatch == fPatch))
                    {
                        toAssign.Add(fPatch);
                    }
                }
            }

            return toAssign;
        }
        /// <summary>
        /// Mark patches for deletion
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="toAssign">To assign patches</param>
        /// <remarks>
        /// For each node to assign
        /// - Look for a free buffer. It's free if unassigned or assigned to not visible node
        ///   - If free buffer found, assign
        ///   - If not, look for a buffer to free, farthests from camera first
        /// </remarks>
        private void MarkFreePatches(GameTime gameTime, Vector3 eyePosition, List<FoliagePatch> toAssign)
        {
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

            if (freeBuffers.Count > 0)
            {
                //Sort free first and farthest first
                freeBuffers.Sort((f1, f2) =>
                {
                    float d1 = f1.CurrentPatch == null ? -1 : Vector3.DistanceSquared(f1.CurrentPatch.CurrentNode.Center, eyePosition);
                    float d2 = f2.CurrentPatch == null ? -1 : Vector3.DistanceSquared(f2.CurrentPatch.CurrentNode.Center, eyePosition);

                    return -d1.CompareTo(d2);
                });

                bool transparent = BlendMode.HasFlag(BlendModes.Alpha) || BlendMode.HasFlag(BlendModes.Transparent);

                while (toAssign.Count > 0 && freeBuffers.Count > 0)
                {
                    freeBuffers[0].AttachFoliage(gameTime, eyePosition, transparent, toAssign[0], BufferManager);

                    toAssign.RemoveAt(0);
                    freeBuffers.RemoveAt(0);
                }
            }
        }
        /// <summary>
        /// Frees the unused patch data
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        private void FreeUnusedPatches(Vector3 eyePosition)
        {
            if (foliagePatches.Keys.Count > MaxFoliagePatches)
            {
                var nodes = foliagePatches.Keys.ToArray();
                var notVisible = Array.FindAll(nodes, n => !visibleNodes.Any(v => v == n));
                if (notVisible.Length > 0)
                {
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
            }
        }
        /// <summary>
        /// Draws the gardener shadows
        /// </summary>
        /// <param name="context">Context</param>
        public override void DrawShadows(DrawContextShadows context)
        {
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
            var buffers = foliageBuffers.FindAll(b => b.CurrentPatch?.CurrentNode == item);
            if (buffers.Count > 0)
            {
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
        }
        /// <summary>
        /// Draws the gardener
        /// </summary>
        /// <param name="context">Drawing context</param>
        public override void Draw(DrawContext context)
        {
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
            var buffers = foliageBuffers.FindAll(b => b.CurrentPatch?.CurrentNode == item);
            if (buffers.Count > 0)
            {
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
        }

        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="volume">Culling volume</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the object is outside of the frustum</returns>
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

            #region Per frame update

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
            var channelData = foliageMapChannels[channel];

            var effect = DrawerPool.EffectShadowFoliage;

            #region Per frame update

            effect.UpdatePerFrame(
                context.ViewProjection,
                context.EyePosition,
                WindDirection,
                WindStrength * channelData.WindEffect,
                windTime * channelData.WindEffect,
                channelData.Delta,
                textureRandom);

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
            WindDirection = direction;
            WindStrength = strength;
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
            if (nodes?.Any() == true)
            {
                return nodes.Where(n => volume.Contains(n.BoundingBox) != ContainmentType.Disjoint);
            }

            return new QuadTreeNode[] { };
        }

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

    /// <summary>
    /// Gardener extensions
    /// </summary>
    public static class GroundGardenerExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="name">Name</param>
        /// <param name="id">Id</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<GroundGardener> AddComponentGroundGardener(this Scene scene, string id, string name, GroundGardenerDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int layer = Scene.LayerDefault)
        {
            GroundGardener component = null;

            await Task.Run(() =>
            {
                component = new GroundGardener(id, name, scene, description);

                scene.AddComponent(component, usage, layer);
            });

            return component;
        }
    }
}
