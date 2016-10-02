using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Engine
{
    using Engine.Collections;
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;
    using Engine.Helpers;
    using Engine.PathFinding;
    using System.Threading.Tasks;

    /// <summary>
    /// Terrain model
    /// </summary>
    public class Scenery : Ground
    {
        #region Update and Draw context for Scenery

        /// <summary>
        /// Scenery updating context
        /// </summary>
        class SceneryUpdateContext
        {
            /// <summary>
            /// General update context
            /// </summary>
            public UpdateContext BaseContext;
            /// <summary>
            /// Foliage generation description
            /// </summary>
            public GroundDescription.VegetationDescription FoliageDescription;
        }
        /// <summary>
        /// Scenery drawing context
        /// </summary>
        class SceneryDrawContext
        {
            /// <summary>
            /// General draw context
            /// </summary>
            public DrawContext BaseContext;

            /// <summary>
            /// Foliage textures
            /// </summary>
            public ShaderResourceView FoliageTextures;
            /// <summary>
            /// Foliage texture count
            /// </summary>
            public uint FoliageTextureCount;
            /// <summary>
            /// Foliage end radius
            /// </summary>
            public float FoliageEndRadius;
            /// <summary>
            /// Wind direction
            /// </summary>
            public Vector3 WindDirection;
            /// <summary>
            /// Wind strength
            /// </summary>
            public float WindStrength;
            /// <summary>
            /// Time
            /// </summary>
            public float Time;
            /// <summary>
            /// Random texture
            /// </summary>
            public ShaderResourceView RandomTexture;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Scenery patches dictionary
        /// </summary>
        class SceneryPatchDictionary : Dictionary<int, SceneryPatch>
        {

        }
        /// <summary>
        /// Terrain patch
        /// </summary>
        /// <remarks>
        /// Holds the necessary information to render a portion of terrain using an arbitrary level of detail
        /// </remarks>
        class SceneryPatch : IDisposable
        {
            const int MAX = 1024 * 2;

            /// <summary>
            /// Creates a new patch
            /// </summary>
            /// <param name="game">Game</param>
            /// <param name="vertices">Vertex list</param>
            /// <param name="indices">Index list</param>
            /// <returns>Returns the new generated patch</returns>
            public static SceneryPatch CreatePatch(Game game, ModelContent content, PickingQuadTreeNode node)
            {
                var desc = new DrawingDataDescription()
                {
                    Instanced = false,
                    Instances = 0,
                    LoadAnimation = true,
                    LoadNormalMaps = true,
                    TextureCount = 0,
                    DynamicBuffers = false,
                    Constraint = node.BoundingBox,
                };

                var drawingData = DrawingData.Build(game, content, desc);

                SceneryPatch patch = new SceneryPatch(game, drawingData);

                //Foliage buffer
                {
                    VertexBillboard[] vertexData = new VertexBillboard[MAX];

                    patch.foliageBuffer = game.Graphics.Device.CreateVertexBufferWrite(vertexData);
                    patch.foliageBufferBinding = new[]
                    {
                        new VertexBufferBinding(patch.foliageBuffer, default(VertexBillboard).Stride, 0),
                    };
                }

                return patch;
            }

            /// <summary>
            /// Vertex buffer with foliage data
            /// </summary>
            private Buffer foliageBuffer = null;
            /// <summary>
            /// Foliage positions
            /// </summary>
            private int foliageCount = 0;
            /// <summary>
            /// Vertex buffer binding for foliage buffer
            /// </summary>
            private VertexBufferBinding[] foliageBufferBinding = null;
            /// <summary>
            /// Foliage populated flag
            /// </summary>
            private bool foliagePlanted = false;
            /// <summary>
            /// Foliage populating flag
            /// </summary>
            private bool foliagePlanting = false;
            /// <summary>
            /// Foliage attached to buffer flag
            /// </summary>
            private bool foliageAttached = false;
            /// <summary>
            /// Foliage generated data
            /// </summary>
            private IVertexData[] foliageData = null;

            /// <summary>
            /// Game
            /// </summary>
            protected Game Game = null;
            /// <summary>
            /// Drawing data
            /// </summary>
            protected DrawingData DrawingData = null;

            /// <summary>
            /// Current quadtree node
            /// </summary>
            public PickingQuadTreeNode Current { get; private set; }

            /// <summary>
            /// Cosntructor
            /// </summary>
            /// <param name="game">Game</param>
            public SceneryPatch(Game game, DrawingData drawingData)
            {
                this.Game = game;
                this.DrawingData = drawingData;
            }
            /// <summary>
            /// Releases created resources
            /// </summary>
            public void Dispose()
            {
                Helper.Dispose(this.DrawingData);
                Helper.Dispose(this.foliageBuffer);
            }

            /// <summary>
            /// Updates the scenery patch
            /// </summary>
            /// <param name="context"></param>
            public void Update(SceneryUpdateContext context, PickingQuadTreeNode node)
            {
                this.Current = node;

                if (context.FoliageDescription != null)
                {
                    this.Plant(context.FoliageDescription);
                }
            }
            /// <summary>
            /// Draws the scenery patch
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <param name="technique">Technique</param>
            /// <param name="sceneryEffect">Scenery effect</param>
            /// <param name="foliageDrawer">Foliage effect</param>
            public void Draw(SceneryDrawContext context, Drawer sceneryEffect, Drawer foliageDrawer)
            {
                foreach (string meshName in this.DrawingData.Meshes.Keys)
                {
                    var dictionary = this.DrawingData.Meshes[meshName];

                    foreach (string material in dictionary.Keys)
                    {
                        var mesh = dictionary[material];
                        var mat = this.DrawingData.Materials[material];

                        #region Per object update

                        var matdata = mat != null ? mat.Material : Material.Default;
                        var texture = mat != null ? mat.DiffuseTexture : null;
                        var normalMap = mat != null ? mat.NormalMap : null;

                        if (context.BaseContext.DrawerMode == DrawerModesEnum.Forward)
                        {
                            ((EffectBasic)sceneryEffect).UpdatePerObject(matdata, texture, normalMap, null, 0);
                        }
                        else if (context.BaseContext.DrawerMode == DrawerModesEnum.Deferred)
                        {
                            ((EffectBasicGBuffer)sceneryEffect).UpdatePerObject(mat.Material, texture, normalMap, null, 0);
                        }

                        #endregion

                        var technique = sceneryEffect.GetTechnique(mesh.VertextType, mesh.Instanced, DrawingStages.Drawing, context.BaseContext.DrawerMode);

                        mesh.SetInputAssembler(this.Game.Graphics.DeviceContext, sceneryEffect.GetInputLayout(technique));

                        for (int p = 0; p < technique.Description.PassCount; p++)
                        {
                            technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                            mesh.Draw(this.Game.Graphics.DeviceContext);
                        }
                    }
                }

                {
                    if (context.BaseContext.DrawerMode == DrawerModesEnum.Forward) this.Game.Graphics.SetBlendTransparent();
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.Deferred) this.Game.Graphics.SetBlendDeferredComposerTransparent();
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.ShadowMap) this.Game.Graphics.SetBlendTransparent();

                    #region Per frame update

                    if (context.BaseContext.DrawerMode == DrawerModesEnum.Forward)
                    {
                        ((EffectBillboard)foliageDrawer).UpdatePerFrame(
                            context.BaseContext.World,
                            context.BaseContext.ViewProjection,
                            context.BaseContext.EyePosition,
                            context.BaseContext.Frustum,
                            context.BaseContext.Lights,
                            context.BaseContext.ShadowMaps,
                            context.BaseContext.ShadowMapStatic,
                            context.BaseContext.ShadowMapDynamic,
                            context.BaseContext.FromLightViewProjection,
                            context.WindDirection,
                            context.WindStrength,
                            context.Time,
                            context.RandomTexture);
                    }
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        ((EffectBillboard)foliageDrawer).UpdatePerFrame(
                            context.BaseContext.World,
                            context.BaseContext.ViewProjection,
                            context.BaseContext.EyePosition,
                            context.BaseContext.Frustum,
                            context.BaseContext.Lights,
                            context.BaseContext.ShadowMaps,
                            context.BaseContext.ShadowMapStatic,
                            context.BaseContext.ShadowMapDynamic,
                            context.BaseContext.FromLightViewProjection,
                            context.WindDirection,
                            context.WindStrength,
                            context.Time,
                            context.RandomTexture);
                    }
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        ((EffectBillboard)foliageDrawer).UpdatePerFrame(
                            context.BaseContext.World,
                            context.BaseContext.ViewProjection,
                            context.BaseContext.EyePosition,
                            new BoundingFrustum(),
                            null,
                            0,
                            null,
                            null,
                            Matrix.Identity,
                            context.WindDirection,
                            context.WindStrength,
                            context.Time,
                            context.RandomTexture);
                    }

                    #endregion

                    #region Per object update

                    if (context.BaseContext.DrawerMode == DrawerModesEnum.Forward)
                    {
                        ((EffectBillboard)foliageDrawer).UpdatePerObject(Material.Default, 0, context.FoliageEndRadius, context.FoliageTextureCount, context.FoliageTextures);
                    }
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        ((EffectBillboard)foliageDrawer).UpdatePerObject(Material.Default, 0, context.FoliageEndRadius, context.FoliageTextureCount, context.FoliageTextures);
                    }
                    else if (context.BaseContext.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        ((EffectBillboard)foliageDrawer).UpdatePerObject(Material.Default, 0, context.FoliageEndRadius, context.FoliageTextureCount, context.FoliageTextures);
                    }

                    #endregion

                    var technique = foliageDrawer.GetTechnique(VertexTypes.Billboard, false, DrawingStages.Drawing, context.BaseContext.DrawerMode);

                    this.Game.Graphics.DeviceContext.InputAssembler.InputLayout = foliageDrawer.GetInputLayout(technique);
                    Counters.IAInputLayoutSets++;
                    this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
                    Counters.IAPrimitiveTopologySets++;

                    if (this.foliageCount > 0)
                    {
                        this.AttachFoliage();

                        //Sets vertex and index buffer
                        this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, this.foliageBufferBinding);
                        Counters.IAVertexBuffersSets++;
                        this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(null, SharpDX.DXGI.Format.R32_UInt, 0);
                        Counters.IAIndexBufferSets++;

                        for (int p = 0; p < technique.Description.PassCount; p++)
                        {
                            technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                            this.Game.Graphics.DeviceContext.Draw(this.foliageCount, 0);

                            Counters.DrawCallsPerFrame++;
                            Counters.InstancesPerFrame++;
                            Counters.TrianglesPerFrame += this.foliageCount / 3;
                        }
                    }
                }
            }

            /// <summary>
            /// Launchs foliage population asynchronous task
            /// </summary>
            /// <param name="description">Terrain vegetation description</param>
            public void Plant(GroundDescription.VegetationDescription description)
            {
                if (this.Current != null)
                {
                    this.foliageAttached = false;
                    this.foliagePlanted = false;

                    if (!this.foliagePlanting)
                    {
                        //Start planting task
                        this.foliagePlanting = true;

                        Task<VertexData[]> t = Task.Factory.StartNew<VertexData[]>(() => PlantTask(this.Current, description));

                        t.ContinueWith(task => PlantThreadCompleted(task.Result));
                    }
                }
            }
            /// <summary>
            /// Copies the foliage data from one patch to another
            /// </summary>
            /// <param name="patch">The other patch from copy data to</param>
            public void CopyFoliageData(SceneryPatch patch)
            {
                this.foliageCount = patch.foliageCount;
                this.foliageData = patch.foliageData;
                this.foliagePlanting = false;
                this.foliagePlanted = true;
                this.foliageAttached = false;
            }

            /// <summary>
            /// Asynchronous planting task
            /// </summary>
            /// <param name="node">Node to process</param>
            /// <param name="description">Vegetation task</param>
            /// <returns>Returns generated vertex data</returns>
            private static VertexData[] PlantTask(PickingQuadTreeNode node, GroundDescription.VegetationDescription description)
            {
                List<VertexData> vertexData = new List<VertexData>(MAX);

                if (node != null)
                {
                    Random rnd = new Random(description.Seed);
                    BoundingBox bbox = node.BoundingBox;
                    float area = bbox.GetX() * bbox.GetZ();
                    float density = description.Saturation;
                    int count = (int)(area * density);
                    if(count > MAX) count = MAX;

                    //Number of points
                    while(count > 0)
                    {
                        Vector3 pos = new Vector3(
                            rnd.NextFloat(bbox.Minimum.X, bbox.Maximum.X),
                            bbox.Maximum.Y + 1f,
                            rnd.NextFloat(bbox.Minimum.Z, bbox.Maximum.Z));

                        Ray ray = new Ray(pos, Vector3.Down);

                        Vector3 intersectionPoint;
                        Triangle t;
                        if (node.PickFirst(ref ray, out intersectionPoint, out t))
                        {
                            if (t.Normal.Y > 0.5f)
                            {
                                vertexData.Add(new VertexData()
                                {
                                    Position = intersectionPoint,
                                    Size = new Vector2(
                                        rnd.NextFloat(description.MinSize.X, description.MaxSize.X),
                                        rnd.NextFloat(description.MinSize.Y, description.MaxSize.Y)),
                                });
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
            private void PlantThreadCompleted(VertexData[] vData)
            {
                this.foliageCount = vData.Length;
                this.foliageData = VertexData.Convert(VertexTypes.Billboard, vData, null, null, Matrix.Identity);
                this.foliagePlanting = false;
                this.foliagePlanted = true;
                this.foliageAttached = false;
            }
            /// <summary>
            /// Attachs the foliage data to the vertex buffer
            /// </summary>
            private void AttachFoliage()
            {
                if (!this.foliageAttached)
                {
                    if (this.foliagePlanted && this.foliageData != null && this.foliageData.Length > 0)
                    {
                        //Attach data
                        VertexData.WriteVertexBuffer(
                            this.Game.Graphics.DeviceContext,
                            this.foliageBuffer,
                            this.foliageData);
                    }

                    this.foliageAttached = true;
                }
            }
        }

        #endregion

        /// <summary>
        /// Scenery patch list
        /// </summary>
        private SceneryPatchDictionary patchDictionary = new SceneryPatchDictionary();
        /// <summary>
        /// Cached triangle list
        /// </summary>
        private Triangle[] triangleCache = null;
        /// <summary>
        /// Visible Nodes
        /// </summary>
        public PickingQuadTreeNode[] visibleNodes;
        /// <summary>
        /// Gets the visible node count
        /// </summary>
        public int VisiblePatchesCount
        {
            get
            {
                return this.visibleNodes != null ? this.visibleNodes.Length : 0;
            }
        }
        /// <summary>
        /// Random texture
        /// </summary>
        private ShaderResourceView textureRandom = null;
        /// <summary>
        /// Foliage textures
        /// </summary>
        private ShaderResourceView foliageTextures = null;
        /// <summary>
        /// Foliage texture count
        /// </summary>
        private uint foliageTextureCount = 0;
        /// <summary>
        /// Wind total time
        /// </summary>
        private float windTime = 0;
        /// <summary>
        /// Scenery update context
        /// </summary>
        private SceneryUpdateContext updateContext = null;
        /// <summary>
        /// Scenery draw context
        /// </summary>
        private SceneryDrawContext drawContext = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="content">Geometry content</param>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="description">Terrain description</param>
        public Scenery(Game game, ModelContent content, string contentFolder, GroundDescription description)
            : base(game, description)
        {
            this.Name = this.Description.Name;
            this.DeferredEnabled = this.Description.DeferredEnabled;
            this.Opaque = this.Description.Opaque;
            this.Static = this.Description.Static;

            #region Patches

            this.triangleCache = content.GetTriangles();
            this.pickingQuadtree = new PickingQuadTree(this.triangleCache, description.Quadtree.MaximumDepth);
            var nodes = this.pickingQuadtree.GetTailNodes();
            for (int i = 0; i < nodes.Length; i++)
            {
                var patch = SceneryPatch.CreatePatch(game, content, nodes[i]);
                this.patchDictionary.Add(nodes[i].Id, patch);
            }

            #endregion

            #region Read foliage textures

            if (this.Description != null && this.Description.Vegetation != null)
            {
                //Read foliage textures
                string contentPath = Path.Combine(this.Description.ContentPath, this.Description.Vegetation.ContentPath);

                ImageContent foliageTextures = new ImageContent()
                {
                    Streams = ContentManager.FindContent(contentPath, this.Description.Vegetation.VegetarionTextures),
                };

                this.foliageTextures = game.Graphics.Device.LoadTextureArray(foliageTextures.Streams);
                this.foliageTextureCount = (uint)foliageTextures.Count;
            }

            #endregion

            #region Random texture generation

            this.textureRandom = game.Graphics.Device.CreateRandomTexture(1024, 24);

            #endregion

            //Initialize update context
            this.updateContext = new SceneryUpdateContext()
            {
                FoliageDescription = this.Description.Vegetation,
            };

            //Initialize draw context
            this.drawContext = new SceneryDrawContext()
            {
                FoliageTextureCount = this.foliageTextureCount,
                FoliageTextures = this.foliageTextures,
                FoliageEndRadius = this.Description.Vegetation != null ? this.Description.Vegetation.EndRadius : 0,
                RandomTexture = this.textureRandom,
            };

            if (!this.Description.DelayGeneration)
            {
                this.UpdateInternals();
            }
        }
        /// <summary>
        /// Dispose of created resources
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.patchDictionary);
            Helper.Dispose(this.textureRandom);
            Helper.Dispose(this.foliageTextures);
        }
        /// <summary>
        /// Objects updating
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            this.visibleNodes = this.pickingQuadtree.GetNodesInVolume(ref context.Frustum);

            this.updateContext.BaseContext = context;

            for (int i = 0; i < this.visibleNodes.Length; i++)
            {
                var current = this.visibleNodes[i];

                this.patchDictionary[current.Id].Update(this.updateContext, current);
            }
        }
        /// <summary>
        /// Objects drawing
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (this.visibleNodes != null && this.visibleNodes.Length > 0)
            {
                this.windTime += context.GameTime.ElapsedSeconds * this.drawContext.WindStrength;

                this.drawContext.BaseContext = context;
                this.drawContext.Time = this.windTime;

                Drawer effect = null;
                if (context.DrawerMode == DrawerModesEnum.Forward) effect = DrawerPool.EffectBasic;
                else if (context.DrawerMode == DrawerModesEnum.Deferred) effect = DrawerPool.EffectGBuffer;
                else if (context.DrawerMode == DrawerModesEnum.ShadowMap) effect = DrawerPool.EffectShadow;

                #region Per frame update

                if (context.DrawerMode == DrawerModesEnum.Forward)
                {
                    ((EffectBasic)effect).UpdatePerFrame(
                        Matrix.Identity,
                        context.ViewProjection,
                        context.EyePosition,
                        context.Frustum,
                        context.Lights,
                        context.ShadowMaps,
                        context.ShadowMapStatic,
                        context.ShadowMapDynamic,
                        context.FromLightViewProjection);
                }
                else if (context.DrawerMode == DrawerModesEnum.Deferred)
                {
                    ((EffectBasicGBuffer)effect).UpdatePerFrame(
                        Matrix.Identity,
                        context.ViewProjection);
                }
                else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                {
                    ((EffectBasicShadow)effect).UpdatePerFrame(
                        Matrix.Identity,
                        context.ViewProjection);
                }

                #endregion

                this.Game.Graphics.SetDepthStencilZEnabled();

                for (int i = 0; i < this.visibleNodes.Length; i++)
                {
                    this.patchDictionary[this.visibleNodes[i].Id].Draw(this.drawContext, effect, DrawerPool.EffectBillboard);
                }
            }
        }

        /// <summary>
        /// Updates internal objects
        /// </summary>
        public override void UpdateInternals()
        {
            if (this.Description != null && this.Description.Quadtree != null)
            {
                var triangles = this.GetTriangles(UsageEnum.Picking);

                this.pickingQuadtree = new PickingQuadTree(triangles, this.Description.Quadtree.MaximumDepth);
            }

            if (this.Description != null && this.Description.PathFinder != null)
            {
                var triangles = this.GetTriangles(UsageEnum.PathFinding);

                this.navigationGraph = PathFinder.Build(this.Description.PathFinder.Settings, triangles);
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

        /// <summary>
        /// Gets triangle list
        /// </summary>
        /// <returns>Returns triangle list. Empty if the vertex type hasn't position channel</returns>
        private Triangle[] GetTriangles(UsageEnum usage = UsageEnum.None)
        {
            List<Triangle> tris = new List<Triangle>();

            tris.AddRange(this.triangleCache);

            for (int i = 0; i < this.GroundObjects.Count; i++)
            {
                var curr = this.GroundObjects[i];

                if (curr.Model is Model)
                {
                    var model = (Model)curr.Model;

                    if (usage == UsageEnum.Picking && curr.Use.HasFlag(AttachedModelUsesEnum.CoarsePicking) ||
                        usage == UsageEnum.PathFinding && curr.Use.HasFlag(AttachedModelUsesEnum.CoarsePathFinding))
                    {
                        var vTris = model.GetVolume();
                        if (vTris != null && vTris.Length > 0)
                        {
                            //Use volume mesh
                            tris.AddRange(vTris);
                        }
                        else
                        {
                            //Generate cylinder
                            var cylinder = BoundingCylinder.FromPoints(model.GetPoints());
                            tris.AddRange(Triangle.ComputeTriangleList(PrimitiveTopology.TriangleList, cylinder, 8));
                        }
                    }
                    else if (
                        usage == UsageEnum.Picking && curr.Use.HasFlag(AttachedModelUsesEnum.FullPicking) ||
                        usage == UsageEnum.PathFinding && curr.Use.HasFlag(AttachedModelUsesEnum.FullPathFinding))
                    {
                        //Use full mesh
                        tris.AddRange(model.GetTriangles());
                    }
                }
                else if (curr.Model is ModelInstanced)
                {
                    var model = (ModelInstanced)curr.Model;

                    if (usage == UsageEnum.Picking && curr.Use.HasFlag(AttachedModelUsesEnum.CoarsePicking) ||
                        usage == UsageEnum.PathFinding && curr.Use.HasFlag(AttachedModelUsesEnum.CoarsePathFinding))
                    {
                        Array.ForEach(model.Instances, (m) =>
                        {
                            var vTris = m.GetVolume();
                            if (vTris != null && vTris.Length > 0)
                            {
                                //Use volume mesh
                                tris.AddRange(vTris);
                            }
                            else
                            {
                                //Generate cylinder
                                var cylinder = BoundingCylinder.FromPoints(m.GetPoints());
                                tris.AddRange(Triangle.ComputeTriangleList(PrimitiveTopology.TriangleList, cylinder, 8));
                            }
                        });
                    }
                    else if (
                        usage == UsageEnum.Picking && curr.Use.HasFlag(AttachedModelUsesEnum.FullPicking) ||
                        usage == UsageEnum.PathFinding && curr.Use.HasFlag(AttachedModelUsesEnum.FullPathFinding))
                    {
                        Array.ForEach(model.Instances, (m) =>
                        {
                            //Use full mesh
                            tris.AddRange(m.GetTriangles());
                        });
                    }
                }
            }

            return tris.ToArray();
        }

        /// <summary>
        /// Sets wind parameters
        /// </summary>
        /// <param name="direction">Direction</param>
        /// <param name="strength">Strength</param>
        public void SetWind(Vector3 direction, float strength)
        {
            if (this.drawContext != null)
            {
                this.drawContext.WindDirection = direction;
                this.drawContext.WindStrength = strength;
            }
        }

        /// <summary>
        /// Usage enumeration for internal's update
        /// </summary>
        public enum UsageEnum
        {
            /// <summary>
            /// None
            /// </summary>
            None,
            /// <summary>
            /// For picking test
            /// </summary>
            Picking,
            /// <summary>
            /// For path finding test
            /// </summary>
            PathFinding,
        }
    }
}
