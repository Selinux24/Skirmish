using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;

namespace Engine
{
    using Engine.Collections.Generic;
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Terrain model
    /// </summary>
    public class Scenery : Ground, IUseMaterials
    {
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
            /// <summary>
            /// Creates a new patch
            /// </summary>
            /// <param name="game">Game</param>
            /// <param name="content">Content</param>
            /// <param name="node">Quadtree node</param>
            /// <returns>Returns the new generated patch</returns>
            public static SceneryPatch CreatePatch(Game game, BufferManager bufferManager, ModelContent content, PickingQuadTreeNode<Triangle> node)
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

                var drawingData = DrawingData.Build(game, bufferManager, content, desc);

                return new SceneryPatch(game, drawingData);
            }

            /// <summary>
            /// Game
            /// </summary>
            protected Game Game = null;
            /// <summary>
            /// Drawing data
            /// </summary>
            public DrawingData DrawingData = null;

            /// <summary>
            /// Current quadtree node
            /// </summary>
            public PickingQuadTreeNode<Triangle> Current { get; set; }

            /// <summary>
            /// Cosntructor
            /// </summary>
            /// <param name="game">Game</param>
            /// <param name="drawingData">Drawing data</param>
            public SceneryPatch(Game game, DrawingData drawingData)
            {
                this.Game = game;
                this.DrawingData = drawingData;
            }
            /// <summary>
            /// Destructor
            /// </summary>
            ~SceneryPatch()
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
            /// Dispose resources
            /// </summary>
            /// <param name="disposing">Free managed resources</param>
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.DrawingData?.Dispose();
                    this.DrawingData = null;
                }
            }
            /// <summary>
            /// Draws the scenery patch shadows
            /// </summary>
            /// <param name="sceneryEffect">Scenery effect</param>
            /// <param name="bufferManager">Buffer manager</param>
            public void DrawSceneryShadows(IShadowMapDrawer sceneryEffect, BufferManager bufferManager)
            {
                var graphics = this.Game.Graphics;

                foreach (string meshName in this.DrawingData.Meshes.Keys)
                {
                    var dictionary = this.DrawingData.Meshes[meshName];

                    foreach (string material in dictionary.Keys)
                    {
                        var mesh = dictionary[material];

                        var technique = sceneryEffect.GetTechnique(mesh.VertextType, mesh.Instanced, mesh.Transparent);

                        var mat = this.DrawingData.Materials[material];

                        sceneryEffect.UpdatePerObject(0, mat, 0);

                        bufferManager.SetIndexBuffer(mesh.IndexBuffer.Slot);
                        bufferManager.SetInputAssembler(technique, mesh.VertexBuffer.Slot, mesh.Topology);

                        for (int p = 0; p < technique.PassCount; p++)
                        {
                            graphics.EffectPassApply(technique, p, 0);

                            mesh.Draw(graphics);
                        }
                    }
                }
            }
            /// <summary>
            /// Draws the scenery patch
            /// </summary>
            /// <param name="sceneryEffect">Scenery effect</param>
            /// <param name="techniqueFn">Function for technique</param>
            /// <param name="bufferManager">Buffer manager</param>
            public void DrawScenery(IGeometryDrawer sceneryEffect, BufferManager bufferManager)
            {
                var graphics = this.Game.Graphics;
                int count = 0;

                foreach (string meshName in this.DrawingData.Meshes.Keys)
                {
                    var dictionary = this.DrawingData.Meshes[meshName];

                    foreach (string material in dictionary.Keys)
                    {
                        var mesh = dictionary[material];

                        var technique = sceneryEffect.GetTechnique(mesh.VertextType, mesh.Instanced);

                        sceneryEffect.UpdatePerObject(
                            0,
                            this.DrawingData.Materials[material],
                            0,
                            true);

                        bufferManager.SetIndexBuffer(mesh.IndexBuffer.Slot);
                        bufferManager.SetInputAssembler(technique, mesh.VertexBuffer.Slot, mesh.Topology);

                        count += mesh.IndexBuffer.Count > 0 ? mesh.IndexBuffer.Count : mesh.VertexBuffer.Count;

                        for (int p = 0; p < technique.PassCount; p++)
                        {
                            graphics.EffectPassApply(technique, p, 0);

                            mesh.Draw(graphics);
                        }
                    }
                }

                Counters.InstancesPerFrame++;
                Counters.PrimitivesPerFrame += count / 3;
            }

            /// <summary>
            /// Gets all the used materials
            /// </summary>
            /// <returns>Returns the used materials array</returns>
            public IEnumerable<MeshMaterial> GetMaterials()
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

                return matList;
            }
        }

        #endregion

        /// <summary>
        /// Scenery patch list
        /// </summary>
        private SceneryPatchDictionary patchDictionary = new SceneryPatchDictionary();
        /// <summary>
        /// Visible Nodes
        /// </summary>
        private PickingQuadTreeNode<Triangle>[] visibleNodes = null;

        /// <summary>
        /// Gets the used material list
        /// </summary>
        public virtual MeshMaterial[] Materials
        {
            get
            {
                List<MeshMaterial> matList = new List<MeshMaterial>();

                var nodes = this.groundPickingQuadtree.GetLeafNodes();

                foreach (var node in nodes)
                {
                    var mats = this.patchDictionary[node.Id].GetMaterials();
                    if (mats != null)
                    {
                        matList.AddRange(mats);
                    }
                }

                return matList.ToArray();
            }
        }
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
        /// Gets the current model lights collection
        /// </summary>
        public SceneLight[] Lights { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Terrain description</param>
        public Scenery(Scene scene, GroundDescription description)
            : base(scene, description)
        {
            ModelContent content = null;

            if (!string.IsNullOrEmpty(description.Content.ModelContentFilename))
            {
                var contentDesc = Helper.DeserializeFromFile<ModelContentDescription>(Path.Combine(description.Content.ContentFolder, description.Content.ModelContentFilename));
                var loader = contentDesc.GetLoader();
                var t = loader.Load(description.Content.ContentFolder, contentDesc);
                content = t[0];
            }
            else if (description.Content.ModelContentDescription != null)
            {
                var loader = description.Content.ModelContentDescription.GetLoader();
                var t = loader.Load(description.Content.ContentFolder, description.Content.ModelContentDescription);
                content = t[0];
            }
            else if (description.Content.HeightmapDescription != null)
            {
                content = ModelContent.FromHeightmap(
                    description.Content.HeightmapDescription.ContentPath,
                    description.Content.HeightmapDescription.HeightmapFileName,
                    description.Content.HeightmapDescription.Textures.TexturesLR,
                    description.Content.HeightmapDescription.CellSize,
                    description.Content.HeightmapDescription.MaximumHeight);
            }
            else if (description.Content.ModelContent != null)
            {
                content = description.Content.ModelContent;
            }
            else
            {
                throw new EngineException("No geometry found in description.");
            }

            #region Patches

            this.groundPickingQuadtree = new PickingQuadTree<Triangle>(content.GetTriangles(), description.Quadtree.MaximumDepth);

            var nodes = this.groundPickingQuadtree.GetLeafNodes();

            foreach (var node in nodes)
            {
                var patch = SceneryPatch.CreatePatch(this.Game, this.BufferManager, content, node);

                this.patchDictionary.Add(node.Id, patch);
            }

            #endregion

            #region Lights

            List<SceneLight> lights = new List<SceneLight>();

            foreach (var key in content.Lights.Keys)
            {
                var l = content.Lights[key];

                if (l.LightType == LightContentTypes.Point)
                {
                    lights.Add(l.CreatePointLight());
                }
                else if (l.LightType == LightContentTypes.Spot)
                {
                    lights.Add(l.CreateSpotLight());
                }
            }

            this.Lights = lights.ToArray();

            #endregion
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~Scenery()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose of created resources
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var item in patchDictionary?.Values)
                {
                    item?.Dispose();
                }

                patchDictionary?.Clear();
                patchDictionary = null;
            }
        }

        /// <summary>
        /// Draw shadows
        /// </summary>
        /// <param name="context">Context</param>
        public override void DrawShadows(DrawContextShadows context)
        {
            var nodes = this.visibleNodes.Length > 0 ? this.visibleNodes : this.groundPickingQuadtree.GetLeafNodes();
            if (nodes != null && nodes.Length > 0)
            {
                var graphics = this.Game.Graphics;

                var sceneryEffect = context.ShadowMap.GetEffect();
                if (sceneryEffect != null)
                {
                    sceneryEffect.UpdatePerFrame(Matrix.Identity, context);

                    graphics.SetBlendDefault();

                    foreach (var node in nodes)
                    {
                        this.patchDictionary[node.Id].DrawSceneryShadows(sceneryEffect, this.BufferManager);
                    }
                }
            }
        }
        /// <summary>
        /// Objects drawing
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            var mode = context.DrawerMode;
            var graphics = this.Game.Graphics;

            if (mode.HasFlag(DrawerModes.OpaqueOnly))
            {
                var nodes = this.visibleNodes.Length > 0 ? this.visibleNodes : this.groundPickingQuadtree.GetLeafNodes();
                if (nodes?.Length > 0)
                {
                    var sceneryEffect = GetEffect(mode);
                    if (sceneryEffect == null)
                    {
                        return;
                    }

                    sceneryEffect.UpdatePerFrameFull(Matrix.Identity, context);

                    graphics.SetBlendDefault();

                    foreach (var node in nodes)
                    {
                        this.patchDictionary[node.Id].DrawScenery(sceneryEffect, this.BufferManager);
                    }
                }
            }
        }
        /// <summary>
        /// Gets effect for rendering based on drawing mode
        /// </summary>
        /// <param name="mode">Drawing mode</param>
        /// <returns>Returns the effect for rendering</returns>
        private IGeometryDrawer GetEffect(DrawerModes mode)
        {
            if (mode.HasFlag(DrawerModes.Forward))
            {
                return DrawerPool.EffectDefaultBasic;
            }
            else if (mode.HasFlag(DrawerModes.Deferred))
            {
                return DrawerPool.EffectDeferredBasic;
            }

            return null;
        }

        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="volume">Culling volume</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the object is outside of the frustum</returns>
        public override bool Cull(ICullingVolume volume, out float distance)
        {
            this.visibleNodes = this.groundPickingQuadtree.GetNodesInVolume(volume);

            return this.CullNodes(volume.Position, out distance);
        }
        /// <summary>
        /// Node culling
        /// </summary>
        /// <param name="pov">Point of view</param>
        /// <param name="distance">Returns the distance to the nearest visible node</param>
        /// <returns>Returns true if the object is culled</returns>
        private bool CullNodes(Vector3 pov, out float distance)
        {
            distance = float.MaxValue;

            if (this.visibleNodes != null && this.visibleNodes.Length > 0)
            {
                //Sort nodes - draw nearest nodes first
                Array.Sort(this.visibleNodes, (n1, n2) =>
                {
                    float d1 = (n1.Center - pov).LengthSquared();
                    float d2 = (n2.Center - pov).LengthSquared();

                    return d1.CompareTo(d2);
                });

                foreach (var node in this.visibleNodes)
                {
                    this.patchDictionary[node.Id].Current = node;
                }

                distance = Vector3.DistanceSquared(pov, this.visibleNodes[0].Center);

                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Gets terrain bounding boxes at specified level
        /// </summary>
        /// <param name="level">Level</param>
        /// <returns>Returns terrain bounding boxes</returns>
        public BoundingBox[] GetBoundingBoxes(int level = 0)
        {
            return this.groundPickingQuadtree.GetBoundingBoxes(level);
        }
    }
}
