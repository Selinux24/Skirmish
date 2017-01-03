using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Buffer = SharpDX.Direct3D11.Buffer;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Collections;
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;
    using Engine.Helpers;
    using Engine.PathFinding;

    /// <summary>
    /// Terrain model
    /// </summary>
    public class Scenery : Ground, UseMaterials
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
            /// Toggles UV horizontal coordinate by primitive ID
            /// </summary>
            public bool FoliageToggleUV;
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
            /// <param name="parent">Parent</param>
            /// <param name="content">Content</param>
            /// <param name="node">Quadtree node</param>
            /// <returns>Returns the new generated patch</returns>
            public static SceneryPatch CreatePatch(Game game, Scenery parent, ModelContent content, PickingQuadTreeNode node)
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

                SceneryPatch patch = new SceneryPatch(game, parent, drawingData);

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
            /// Parent scenery instance
            /// </summary>
            protected Scenery Parent = null;
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
            /// <param name="parent">Parent scenery instance</param>
            /// <param name="drawingData">Drawing data</param>
            public SceneryPatch(Game game, Scenery parent, DrawingData drawingData)
            {
                this.Game = game;
                this.Parent = parent;
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
                    if (!this.foliageAttached)
                    {
                        this.Plant(context.FoliageDescription);
                    }
                }
            }
            /// <summary>
            /// Draws the scenery patch
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <param name="sceneryEffect">Scenery effect</param>
            public void DrawScenery(SceneryDrawContext context, Drawer sceneryEffect)
            {
                foreach (string meshName in this.DrawingData.Meshes.Keys)
                {
                    var dictionary = this.DrawingData.Meshes[meshName];

                    foreach (string material in dictionary.Keys)
                    {
                        #region Per object update

                        var mat = this.DrawingData.Materials[material];

                        if (context.BaseContext.DrawerMode == DrawerModesEnum.Forward)
                        {
                            ((EffectDefaultBasic)sceneryEffect).UpdatePerObject(
                                mat.DiffuseTexture,
                                mat.NormalMap,
                                mat.SpecularTexture,
                                mat.ResourceIndex,
                                0,
                                0);
                        }
                        else if (context.BaseContext.DrawerMode == DrawerModesEnum.Deferred)
                        {
                            ((EffectDeferredBasic)sceneryEffect).UpdatePerObject(
                                mat.DiffuseTexture,
                                mat.NormalMap,
                                mat.SpecularTexture,
                                mat.ResourceIndex,
                                0,
                                0);
                        }
                        else if (context.BaseContext.DrawerMode == DrawerModesEnum.ShadowMap)
                        {
                            ((EffectShadowBasic)sceneryEffect).UpdatePerObject(
                                0,
                                0);
                        }

                        #endregion

                        var mesh = dictionary[material];
                        var technique = sceneryEffect.GetTechnique(mesh.VertextType, mesh.Instanced, DrawingStages.Drawing, context.BaseContext.DrawerMode);
                        mesh.SetInputAssembler(this.Game.Graphics.DeviceContext, sceneryEffect.GetInputLayout(technique));

                        for (int p = 0; p < technique.Description.PassCount; p++)
                        {
                            technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                            mesh.Draw(this.Game.Graphics.DeviceContext);
                        }
                    }
                }
            }
            /// <summary>
            /// Draws the foliage patch
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <param name="foliageTechnique">Foliage technique</param>
            public void DrawFoliage(SceneryDrawContext context, EffectTechnique foliageTechnique)
            {
                if (this.foliageCount > 0)
                {
                    this.AttachFoliage();

                    this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, this.foliageBufferBinding);
                    Counters.IAVertexBuffersSets++;
                    this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(null, SharpDX.DXGI.Format.R32_UInt, 0);
                    Counters.IAIndexBufferSets++;

                    for (int p = 0; p < foliageTechnique.Description.PassCount; p++)
                    {
                        foliageTechnique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                        this.Game.Graphics.DeviceContext.Draw(this.foliageCount, 0);

                        Counters.DrawCallsPerFrame++;
                        Counters.InstancesPerFrame++;
                        Counters.PrimitivesPerFrame += this.foliageCount;
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
                    if (!this.foliagePlanted && !this.foliagePlanting)
                    {
                        //Start planting task
                        this.foliagePlanting = true;

                        Task<VertexData[]> t = Task.Factory.StartNew<VertexData[]>(() => PlantTask(this.Current, description));

                        t.ContinueWith(task => PlantThreadCompleted(task.Result));
                    }
                }
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
                    if (count > MAX) count = MAX;

                    //Number of points
                    while (count > 0)
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
                        this.Game.Graphics.DeviceContext.WriteVertexBuffer(this.foliageBuffer, this.foliageData);

                        this.foliageAttached = true;
                    }
                }
            }

            /// <summary>
            /// Gets all the used materials
            /// </summary>
            /// <returns>Returns the used materials array</returns>
            public MeshMaterial[] GetMaterials()
            {
                List<MeshMaterial> matList = new List<MeshMaterial>();

                foreach (string meshName in this.DrawingData.Meshes.Keys)
                {
                    var dictionary = this.DrawingData.Meshes[meshName];

                    foreach (string material in dictionary.Keys)
                    {
                        matList.Add(this.DrawingData.Materials[material]);
                    }
                }

                return matList.ToArray();
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
        /// Gets the used material list
        /// </summary>
        public virtual MeshMaterial[] Materials
        {
            get
            {
                List<MeshMaterial> matList = new List<MeshMaterial>();

                var nodes = this.pickingQuadtree.GetTailNodes();

                for (int i = 0; i < nodes.Length; i++)
                {
                    var mats = this.patchDictionary[nodes[i].Id].GetMaterials();
                    if (mats != null)
                    {
                        matList.AddRange(mats);
                    }
                }

                return matList.ToArray();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="content">Geometry content</param>
        /// <param name="description">Terrain description</param>
        public Scenery(Game game, ModelContent content, GroundDescription description)
            : base(game, description)
        {
            #region Patches

            this.triangleCache = content.GetTriangles();
            this.pickingQuadtree = new PickingQuadTree(this.triangleCache, description.Quadtree.MaximumDepth);
            var nodes = this.pickingQuadtree.GetTailNodes();
            for (int i = 0; i < nodes.Length; i++)
            {
                var patch = SceneryPatch.CreatePatch(game, this, content, nodes[i]);
                this.patchDictionary.Add(nodes[i].Id, patch);
            }

            #endregion

            #region Read foliage textures

            if (this.Description != null && this.Description.Vegetation != null)
            {
                //Read foliage textures
                string contentPath = this.Description.Vegetation.ContentPath;

                ImageContent foliageTextures = new ImageContent()
                {
                    Streams = ContentManager.FindContent(contentPath, this.Description.Vegetation.VegetarionTextures),
                };

                this.foliageTextures = game.ResourceManager.CreateResource(foliageTextures);
                this.foliageTextureCount = (uint)foliageTextures.Count;
            }

            #endregion

            #region Random texture generation

            this.textureRandom = game.ResourceManager.CreateResource(Guid.NewGuid(), 1024, -1, 1, 24);

            #endregion

            //Initialize update context
            this.updateContext = new SceneryUpdateContext()
            {
                FoliageDescription = this.Description.Vegetation,
            };

            //Initialize draw context
            this.drawContext = new SceneryDrawContext()
            {
                FoliageToggleUV = true,
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
        }
        /// <summary>
        /// Objects updating
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            this.updateContext.BaseContext = context;

            this.visibleNodes = this.pickingQuadtree.GetNodesInVolume(ref context.Frustum);
            if (this.visibleNodes != null && this.visibleNodes.Length > 0)
            {
                //Sort nodes - draw far nodes first
                Array.Sort(this.visibleNodes, (n1, n2) =>
                {
                    var d1 = (n1.Center - context.EyePosition).LengthSquared();
                    var d2 = (n2.Center - context.EyePosition).LengthSquared();

                    return -d1.CompareTo(d2);
                });

                for (int i = 0; i < this.visibleNodes.Length; i++)
                {
                    var current = this.visibleNodes[i];

                    this.patchDictionary[current.Id].Update(this.updateContext, current);
                }
            }
        }
        /// <summary>
        /// Objects drawing
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            PickingQuadTreeNode[] nodes = this.Cull ? this.visibleNodes : this.pickingQuadtree.GetTailNodes();

            if (nodes != null && nodes.Length > 0)
            {
                this.windTime += context.GameTime.ElapsedSeconds * this.drawContext.WindStrength;

                this.drawContext.BaseContext = context;
                this.drawContext.Time = this.windTime;

                Drawer sceneryEffect = null;
                Drawer foliageEffect = null;
                if (context.DrawerMode == DrawerModesEnum.Forward)
                {
                    sceneryEffect = DrawerPool.EffectDefaultBasic;
                    foliageEffect = DrawerPool.EffectDefaultBillboard;

                    #region Per frame update

                    ((EffectDefaultBasic)sceneryEffect).UpdatePerFrame(
                        context.World,
                        context.ViewProjection,
                        context.EyePosition,
                        context.Lights,
                        context.ShadowMaps,
                        context.ShadowMapStatic,
                        context.ShadowMapDynamic,
                        context.FromLightViewProjection);

                    ((EffectDefaultBillboard)foliageEffect).UpdatePerFrame(
                        context.World,
                        context.ViewProjection,
                        context.EyePosition,
                        context.Lights,
                        context.ShadowMaps,
                        context.ShadowMapStatic,
                        context.ShadowMapDynamic,
                        context.FromLightViewProjection,
                        this.drawContext.WindDirection,
                        this.drawContext.WindStrength,
                        this.drawContext.Time,
                        this.drawContext.RandomTexture,
                        0,
                        this.drawContext.FoliageEndRadius,
                        this.drawContext.FoliageTextureCount,
                        0,
                        this.drawContext.FoliageToggleUV,
                        this.drawContext.FoliageTextures);

                    #endregion
                }
                else if (context.DrawerMode == DrawerModesEnum.Deferred)
                {
                    sceneryEffect = DrawerPool.EffectDeferredBasic;
                    foliageEffect = DrawerPool.EffectDefaultBillboard; //TODO: build a proper deferred billboard

                    #region Per frame update

                    ((EffectDeferredBasic)sceneryEffect).UpdatePerFrame(
                        context.World,
                        context.ViewProjection);

                    ((EffectDefaultBillboard)foliageEffect).UpdatePerFrame(
                        context.World,
                        context.ViewProjection,
                        context.EyePosition,
                        context.Lights,
                        context.ShadowMaps,
                        context.ShadowMapStatic,
                        context.ShadowMapDynamic,
                        context.FromLightViewProjection,
                        this.drawContext.WindDirection,
                        this.drawContext.WindStrength,
                        this.drawContext.Time,
                        this.drawContext.RandomTexture,
                        0,
                        this.drawContext.FoliageEndRadius,
                        this.drawContext.FoliageTextureCount,
                        0,
                        this.drawContext.FoliageToggleUV,
                        this.drawContext.FoliageTextures);

                    #endregion
                }
                else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                {
                    sceneryEffect = DrawerPool.EffectShadowBasic;
                    foliageEffect = DrawerPool.EffectShadowBillboard;

                    #region Per frame update

                    ((EffectShadowBasic)sceneryEffect).UpdatePerFrame(
                        context.World,
                        context.ViewProjection);

                    ((EffectShadowBillboard)foliageEffect).UpdatePerFrame(
                        context.World,
                        context.ViewProjection,
                        context.EyePosition,
                        this.drawContext.WindDirection,
                        this.drawContext.WindStrength,
                        this.drawContext.Time,
                        this.drawContext.RandomTexture);

                    #endregion

                    #region Per object update

                    ((EffectShadowBillboard)foliageEffect).UpdatePerObject(
                        0,
                        this.drawContext.FoliageEndRadius,
                        this.drawContext.FoliageTextureCount,
                        this.drawContext.FoliageToggleUV,
                        this.drawContext.FoliageTextures);

                    #endregion
                }

                this.Game.Graphics.SetBlendDefault();

                for (int i = 0; i < nodes.Length; i++)
                {
                    this.patchDictionary[nodes[i].Id].DrawScenery(this.drawContext, sceneryEffect);
                }

                {
                    var foliageTechnique = foliageEffect.GetTechnique(VertexTypes.Billboard, false, DrawingStages.Drawing, context.DrawerMode);

                    this.Game.Graphics.DeviceContext.InputAssembler.InputLayout = foliageEffect.GetInputLayout(foliageTechnique);
                    Counters.IAInputLayoutSets++;
                    this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
                    Counters.IAPrimitiveTopologySets++;

                    this.Game.Graphics.SetBlendTransparent();

                    for (int i = 0; i < nodes.Length; i++)
                    {
                        this.patchDictionary[nodes[i].Id].DrawFoliage(this.drawContext, foliageTechnique);
                    }
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
    }
}
