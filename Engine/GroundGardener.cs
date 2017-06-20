using SharpDX;
using SharpDX.Direct3D;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

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
            public QuadTreeNode CurrentNode { get; protected set; }
            /// <summary>
            /// Foliage map channel
            /// </summary>
            public int Channel { get; protected set; }
            /// <summary>
            /// Foliage populating flag
            /// </summary>
            public bool Planting { get; private set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public FoliagePatch()
            {
                this.CurrentNode = null;
                this.Channel = -1;
            }
            /// <summary>
            /// Dispose
            /// </summary>
            public void Dispose()
            {
                this.FoliageData = null;
            }

            /// <summary>
            /// Launchs foliage population asynchronous task
            /// </summary>
            /// <param name="scene">Scene</param>
            /// <param name="node">Foliage Node</param>
            /// <param name="map">Foliage map</param>
            /// <param name="description">Terrain vegetation description</param>
            public void Plant(Scene scene, QuadTreeNode node, FoliageMap map, FoliageMapChannel description)
            {
                if (!this.Planting)
                {
                    //Start planting task
                    this.CurrentNode = node;
                    this.Channel = description.Index;
                    this.Planting = true;

                    var t = Task.Factory.StartNew<VertexBillboard[]>(() => PlantTask(scene, node, map, description), TaskCreationOptions.PreferFairness);

                    t.ContinueWith(task =>
                    {
                        PlantThreadCompleted(task.Result);
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

                    //Number of points
                    while (count > 0)
                    {
                        try
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
                                    }
                                }
                            }
                        }
                        finally
                        {
                            count--;
                        }
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
            /// Foliage buffer id static counter
            /// </summary>
            private static int ID = 0;

            /// <summary>
            /// Vertex buffer descriptor
            /// </summary>
            public BufferDescriptor VertexBuffer = null;
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
            public int Id = 0;
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
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="name">Name</param>
            public FoliageBuffer(Game game, BufferManager bufferManager, string name)
            {
                this.Game = game;
                this.Id = ++ID;

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
            /// <param name="patch">Patch</param>
            /// <param name="bufferManager">Buffer manager</param>
            public void AttachFoliage(Vector3 eyePosition, FoliagePatch patch, BufferManager bufferManager)
            {
                this.vertexDrawCount = 0;
                this.Attached = false;
                this.CurrentPatch = null;

                if (patch.FoliageData != null && patch.FoliageData.Length > 0)
                {
                    //Sort data
                    Array.Sort(patch.FoliageData, (f1, f2) =>
                    {
                        float d1 = Vector3.DistanceSquared(f1.Position, eyePosition);
                        float d2 = Vector3.DistanceSquared(f2.Position, eyePosition);

                        return -d1.CompareTo(d2);
                    });

                    //Attach data
                    bufferManager.WriteBuffer(
                        this.VertexBuffer.Slot,
                        this.VertexBuffer.Offset,
                        patch.FoliageData);

                    this.vertexDrawCount = patch.FoliageData.Length;
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
                if (this.vertexDrawCount > 0)
                {
                    if (context.DrawerMode != DrawerModesEnum.ShadowMap)
                    {
                        Counters.InstancesPerFrame++;
                        Counters.PrimitivesPerFrame += this.vertexDrawCount / 3;
                    }

                    for (int p = 0; p < technique.Description.PassCount; p++)
                    {
                        technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                        this.Game.Graphics.DeviceContext.Draw(this.vertexDrawCount, this.VertexBuffer.Offset);

                        Counters.DrawCallsPerFrame++;
                    }
                }
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
        /// Game
        /// </summary>
        private Game game = null;

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
        private ShaderResourceView textureRandom = null;
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
        /// Parent ground
        /// </summary>
        public Scene ParentScene { get; set; }
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
        public GroundGardener(Game game, BufferManager bufferManager, GroundGardenerDescription description) :
            base(game, bufferManager)
        {
            this.game = game;

            this.textureRandom = game.ResourceManager.CreateResource(Guid.NewGuid(), 1024, -1, 1, 24);

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
                    ImageContent foliageMapImage = new ImageContent()
                    {
                        Streams = ContentManager.FindContent(contentPath, description.VegetationMap),
                    };
                    this.foliageMap = FoliageMap.FromStream(foliageMapImage.Stream);
                }

                for (int i = 0; i < description.Channels.Length; i++)
                {
                    var channel = description.Channels[i];
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
                                StartRadius = channel.StartRadius,
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
                    this.foliageBuffers.Add(new FoliageBuffer(game, this.BufferManager, description.Name));
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

            if (this.ParentScene != null)
            {
                if (this.foliageQuadtree == null)
                {
                    BoundingBox bbox = this.ParentScene.GetBoundingBox();

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
                    //Sort fartest first
                    Array.Sort(this.visibleNodes, (f1, f2) =>
                    {
                        float d1 = Vector3.DistanceSquared(f1.Center, context.EyePosition);
                        float d2 = Vector3.DistanceSquared(f2.Center, context.EyePosition);

                        return -d1.CompareTo(d2);
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
                                fPatch.Plant(this.ParentScene, node, this.foliageMap, this.foliageMapChannels[i]);
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
                                freeBuffers[0].AttachFoliage(context.EyePosition, toAssign[0], this.BufferManager);

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
            }

            this.PlantingTasks = 0;

            foreach (var q in this.foliagePatches)
            {
                this.PlantingTasks += q.Value.FindAll(f => f.Planting == true).Count;
            }
        }
        /// <summary>
        /// Draws the gardener
        /// </summary>
        /// <param name="context">Drawing context</param>
        public override void Draw(DrawContext context)
        {
            if (this.ParentScene != null)
            {
                if (this.visibleNodes != null && this.visibleNodes.Length > 0)
                {
                    foreach (var item in this.visibleNodes)
                    {
                        var buffers = this.foliageBuffers.FindAll(b => b.CurrentPatch != null && b.CurrentPatch.CurrentNode == item);
                        if (buffers.Count > 0)
                        {
                            foreach (var buffer in buffers)
                            {
                                var vegetationTechnique = this.SetTechniqueVegetation(context, buffer.CurrentPatch.Channel);
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
            var effect = DrawerPool.EffectDefaultBillboard;

            this.game.Graphics.SetBlendTransparent();

            #region Per frame update

            effect.UpdatePerFrame(
                context.World,
                context.ViewProjection,
                context.EyePosition,
                context.Lights,
                context.ShadowMaps,
                context.ShadowMapLow,
                context.ShadowMapHigh,
                context.FromLightViewProjectionLow,
                context.FromLightViewProjectionHigh,
                this.WindDirection,
                this.WindStrength * this.foliageMapChannels[channel].WindEffect,
                this.windTime * this.foliageMapChannels[channel].WindEffect,
                this.textureRandom,
                this.foliageMapChannels[channel].StartRadius,
                this.foliageMapChannels[channel].EndRadius,
                (uint)this.material.ResourceIndex,
                this.foliageMapChannels[channel].TextureCount,
                this.foliageMapChannels[channel].ToggleUV,
                this.foliageMapChannels[channel].Textures);

            #endregion

            return effect.GetTechnique(VertexTypes.Billboard, false, DrawingStages.Drawing, context.DrawerMode);
        }
        /// <summary>
        /// Sets thecnique for vegetation drawing in shadow mapping
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="channel">Channel</param>
        /// <returns>Returns the selected technique</returns>
        private EffectTechnique SetTechniqueVegetationShadowMap(DrawContext context, int channel)
        {
            var effect = DrawerPool.EffectShadowBillboard;

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
                this.foliageMapChannels[channel].StartRadius,
                this.foliageMapChannels[channel].EndRadius,
                this.foliageMapChannels[channel].TextureCount,
                this.foliageMapChannels[channel].ToggleUV,
                this.foliageMapChannels[channel].Textures);

            #endregion

            return effect.GetTechnique(VertexTypes.Billboard, false, DrawingStages.Drawing, context.DrawerMode);
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
