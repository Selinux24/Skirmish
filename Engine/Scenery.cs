using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            /// <param name="name">Owner name</param>
            /// <param name="content">Content</param>
            /// <param name="node">Quadtree node</param>
            /// <returns>Returns the new generated patch</returns>
            public static async Task<SceneryPatch> CreatePatch(Game game, string name, ModelContent content, PickingQuadTreeNode<Triangle> node)
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

                var drawingData = await DrawingData.Build(game, name, content, desc);

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
            /// Cosntructor
            /// </summary>
            /// <param name="game">Game</param>
            /// <param name="drawingData">Drawing data</param>
            public SceneryPatch(Game game, DrawingData drawingData)
            {
                Game = game;
                DrawingData = drawingData;
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
                    DrawingData?.Dispose();
                    DrawingData = null;
                }
            }
            /// <summary>
            /// Draws the scenery patch shadows
            /// </summary>
            /// <param name="sceneryEffect">Scenery effect</param>
            /// <param name="bufferManager">Buffer manager</param>
            public void DrawSceneryShadows(IShadowMapDrawer sceneryEffect, BufferManager bufferManager)
            {
                var graphics = Game.Graphics;

                foreach (string meshName in DrawingData.Meshes.Keys)
                {
                    var meshDict = DrawingData.Meshes[meshName];

                    foreach (string materialName in meshDict.Keys)
                    {
                        var mesh = meshDict[materialName];
                        if (!mesh.Ready)
                        {
                            continue;
                        }

                        var material = DrawingData.Materials[materialName];

                        var technique = sceneryEffect.GetTechnique(mesh.VertextType, false, material.Material.IsTransparent);

                        sceneryEffect.UpdatePerObject(0, material, 0);

                        bufferManager.SetIndexBuffer(mesh.IndexBuffer);
                        bufferManager.SetInputAssembler(technique, mesh.VertexBuffer, mesh.Topology);

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
            /// <param name="context">Context</param>
            /// <param name="sceneryEffect">Scenery effect</param>
            /// <param name="bufferManager">Buffer manager</param>
            public void DrawScenery(DrawContext context, IGeometryDrawer sceneryEffect, BufferManager bufferManager)
            {
                var graphics = Game.Graphics;
                int count = 0;

                foreach (string meshName in DrawingData.Meshes.Keys)
                {
                    var meshDict = DrawingData.Meshes[meshName];

                    foreach (string materialName in meshDict.Keys)
                    {
                        var mesh = meshDict[materialName];
                        if (!mesh.Ready)
                        {
                            continue;
                        }

                        var material = DrawingData.Materials[materialName];

                        bool draw = context.ValidateDraw(BlendModes.Default, material.Material.IsTransparent);
                        if (!draw)
                        {
                            continue;
                        }

                        var technique = sceneryEffect.GetTechnique(mesh.VertextType, false);

                        sceneryEffect.UpdatePerObject(0, material, 0, true);

                        bufferManager.SetIndexBuffer(mesh.IndexBuffer);
                        bufferManager.SetInputAssembler(technique, mesh.VertexBuffer, mesh.Topology);

                        count += mesh.Count;

                        for (int p = 0; p < technique.PassCount; p++)
                        {
                            graphics.EffectPassApply(technique, p, 0);

                            mesh.Draw(graphics);
                        }
                    }
                }

                Counters.InstancesPerFrame++;
                Counters.PrimitivesPerFrame += count;
            }

            /// <summary>
            /// Gets all the used materials
            /// </summary>
            /// <returns>Returns the used materials array</returns>
            public IEnumerable<MeshMaterial> GetMaterials()
            {
                List<MeshMaterial> matList = new List<MeshMaterial>();

                foreach (string meshName in DrawingData.Meshes.Keys)
                {
                    var dictionary = DrawingData.Meshes[meshName];

                    foreach (string material in dictionary.Keys)
                    {
                        matList.Add(DrawingData.Materials[material]);
                    }
                }

                return matList;
            }
        }
        /// <summary>
        /// Path load task helper
        /// </summary>
        struct SceneryPatchTask
        {
            /// <summary>
            /// Node id
            /// </summary>
            public int Id { get; set; }
            /// <summary>
            /// Created patch
            /// </summary>
            public SceneryPatch Patch { get; set; }
        }

        #endregion

        /// <summary>
        /// Model content
        /// </summary>
        private readonly ModelContent content;
        /// <summary>
        /// Scenery patch list
        /// </summary>
        private SceneryPatchDictionary patchDictionary = new SceneryPatchDictionary();
        /// <summary>
        /// Visible Nodes
        /// </summary>
        private PickingQuadTreeNode<Triangle>[] visibleNodes = new PickingQuadTreeNode<Triangle>[] { };

        /// <summary>
        /// Gets the used material list
        /// </summary>
        public virtual IEnumerable<MeshMaterial> Materials
        {
            get
            {
                var matList = patchDictionary.Values.SelectMany(v => v?.GetMaterials() ?? new MeshMaterial[] { }).ToArray();

                return matList;
            }
        }
        /// <summary>
        /// Gets the visible node count
        /// </summary>
        public int VisiblePatchesCount
        {
            get
            {
                return visibleNodes?.Length ?? 0;
            }
        }
        /// <summary>
        /// Gets the current model lights collection
        /// </summary>
        public SceneLight[] Lights { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Terrain description</param>
        public Scenery(string name, Scene scene, GroundDescription description)
            : base(name, scene, description)
        {
            // Generate model content
            content = description.ReadModelContent();

            // Generate quadtree
            groundPickingQuadtree = description.ReadQuadTree(content.GetTriangles());

            // Generate initial patches
            var nodes = groundPickingQuadtree.GetLeafNodes();
            foreach (var node in nodes)
            {
                var patch = SceneryPatch.CreatePatch(Game, name, content, node).GetAwaiter().GetResult();

                patchDictionary.Add(node.Id, patch);
            }

            // Retrieve lights from content
            Lights = content.GetLights().ToArray();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~Scenery()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override bool Cull(IIntersectionVolume volume, out float distance)
        {
            distance = float.MaxValue;

            if (base.Cull(volume, out distance))
            {
                return true;
            }

            visibleNodes = groundPickingQuadtree.GetNodesInVolume(volume).ToArray();
            if (!visibleNodes.Any())
            {
                return true;
            }

            if (visibleNodes.Length > 1)
            {
                //Sort nodes by center distance to the culling volume position - nearest nodes first
                Array.Sort(visibleNodes, (n1, n2) =>
                {
                    float d1 = (n1.Center - volume.Position).LengthSquared();
                    float d2 = (n2.Center - volume.Position).LengthSquared();

                    return d1.CompareTo(d2);
                });
            }

            distance = Vector3.DistanceSquared(volume.Position, visibleNodes[0].Center);

            return false;
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (visibleNodes?.Any() != true)
            {
                return;
            }

            // Detect nodes without assigned patch
            List<Task<SceneryPatchTask>> taskList = new List<Task<SceneryPatchTask>>();

            foreach (var node in visibleNodes)
            {
                if (!patchDictionary.ContainsKey(node.Id))
                {
                    Logger.WriteTrace(this, $"Loading node {node.Id} patch.");

                    // Reserve position
                    patchDictionary.Add(node.Id, null);

                    // Add creation task
                    taskList.Add(LoadPatch(node));
                }
            }

            // Launch creation tasks
            LoadPatches(taskList);
        }

        /// <inheritdoc/>
        public override void DrawShadows(DrawContextShadows context)
        {
            if (visibleNodes?.Any() != true)
            {
                return;
            }

            var sceneryEffect = context.ShadowMap.GetEffect();
            if (sceneryEffect == null)
            {
                return;
            }

            sceneryEffect.UpdatePerFrame(Matrix.Identity, context);

            foreach (var node in visibleNodes)
            {
                if (patchDictionary.ContainsKey(node.Id))
                {
                    Logger.WriteTrace(this, $"Scenery DrawShadows {context.ShadowMap} {node.Id} patch.");

                    patchDictionary[node.Id]?.DrawSceneryShadows(sceneryEffect, BufferManager);
                }
                else
                {
                    Logger.WriteWarning(this, $"Scenery DrawShadows {context.ShadowMap} {node.Id} without assigned patch. No draw method called");
                }
            }
        }
        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            if (visibleNodes?.Any() != true)
            {
                return;
            }

            var sceneryEffect = GetEffect(context.DrawerMode);
            if (sceneryEffect == null)
            {
                return;
            }

            sceneryEffect.UpdatePerFrameFull(Matrix.Identity, context);

            foreach (var node in visibleNodes)
            {
                if (patchDictionary.ContainsKey(node.Id))
                {
                    Logger.WriteTrace(this, $"Scenery Draw {node.Id} patch.");

                    patchDictionary[node.Id]?.DrawScenery(context, sceneryEffect, BufferManager);
                }
                else
                {
                    Logger.WriteWarning(this, $"Scenery Draw {node.Id} without assigned patch. No draw method called");
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
        /// Launch an async resource load with the task list
        /// </summary>
        /// <param name="taskList">Task list to launch</param>
        private void LoadPatches(IEnumerable<Task<SceneryPatchTask>> taskList)
        {
            if (!taskList.Any())
            {
                return;
            }

            // Fire and forget
            Task.Run(async () =>
            {
                Logger.WriteTrace(this, $"LoadPatches Init: {taskList.Count()} tasks.");

                await Scene.LoadResourcesAsync(
                    taskList,
                    (result) =>
                    {
                        foreach (var res in result.Results)
                        {
                            // Assign patch to dictionary
                            if (res.Completed)
                            {
                                patchDictionary[res.Result.Id] = res.Result.Patch;
                            }
                            else
                            {
                                patchDictionary.Remove(res.Result.Id);
                                Logger.WriteError(this, $"Error creating patch {res.Result.Id}: {res.Exception.Message}", res.Exception);
                            }
                        }
                    });

                Logger.WriteTrace(this, "LoadPatches End.");
            });
        }
        /// <summary>
        /// Loads a new patch
        /// </summary>
        /// <param name="node">Node to load in the patch</param>
        private async Task<SceneryPatchTask> LoadPatch(PickingQuadTreeNode<Triangle> node)
        {
            // Create patch
            var patch = await SceneryPatch.CreatePatch(Game, Name, content, node);

            SceneryPatchTask res = new SceneryPatchTask
            {
                Id = node.Id,
                Patch = patch,
            };

            return res;
        }
    }

    /// <summary>
    /// Scenery extensions
    /// </summary>
    public static class SceneryExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<Scenery> AddComponentScenery(this Scene scene, string name, GroundDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int order = 0)
        {
            Scenery component = null;

            await Task.Run(() =>
            {
                component = new Scenery(name, scene, description);

                scene.AddComponent(component, usage, order);
            });

            return component;
        }
    }
}
