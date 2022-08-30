﻿using SharpDX;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Default;
    using Engine.Collections.Generic;
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Terrain model
    /// </summary>
    public sealed class Scenery : Ground<GroundDescription>, IUseMaterials
    {
        #region Helper Classes

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
            public static async Task<SceneryPatchTask> CreatePatch(Game game, string name, ContentData content, PickingQuadTreeNode<Triangle> node)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

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

                watch.Stop();

                return new SceneryPatchTask
                {
                    Id = node.Id,
                    Duration = watch.Elapsed,
                    Patch = new SceneryPatch(game, drawingData),
                };
            }

            /// <summary>
            /// Game
            /// </summary>
            protected Game Game = null;
            /// <summary>
            /// Drawing data
            /// </summary>
            public DrawingData DrawingData = null;
            public int CreationNodeId = -1;
            public TimeSpan CreationDuration = TimeSpan.Zero;

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

                        var materialInfo = new MaterialShadowDrawInfo
                        {
                            Material = material
                        };

                        sceneryEffect.UpdatePerObject(AnimationShadowDrawInfo.Empty, materialInfo, 0);

                        var technique = sceneryEffect.GetTechnique(mesh.VertextType, false, material.Material.IsTransparent);

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
            /// <param name="bufferManager">Buffer manager</param>
            public void DrawScenery(DrawContext context, BufferManager bufferManager)
            {
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

                        var sceneryDrawer = GetDrawer(context.DrawerMode, mesh.VertextType);
                        if (sceneryDrawer != null)
                        {
                            DrawWithDrawer(bufferManager, sceneryDrawer, mesh, material);

                            continue;
                        }

                        var sceneryEffect = GetEffect(context.DrawerMode);
                        if (sceneryEffect != null)
                        {
                            DrawWithEffect(context, bufferManager, sceneryEffect, mesh, material);

                            continue;
                        }

                        count += mesh.Count;
                    }
                }

                Counters.InstancesPerFrame++;
                Counters.PrimitivesPerFrame += count;
            }
            /// <summary>
            /// Draws the patch using the effects framework
            /// </summary>
            /// <param name="context">Draw context</param>
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="sceneryEffect">Effect</param>
            /// <param name="mesh">Mesh</param>
            /// <param name="material">Material</param>
            private void DrawWithEffect(DrawContext context, BufferManager bufferManager, IGeometryDrawer sceneryEffect, Mesh mesh, IMeshMaterial material)
            {
                var graphics = Game.Graphics;

                sceneryEffect.UpdatePerFrameFull(Matrix.Identity, context);

                var technique = sceneryEffect.GetTechnique(mesh.VertextType, false);

                var materialInfo = new MaterialDrawInfo
                {
                    Material = material,
                    UseAnisotropic = true,
                };

                sceneryEffect.UpdatePerObject(materialInfo, Color4.White, 0, AnimationDrawInfo.Empty);

                bufferManager.SetIndexBuffer(mesh.IndexBuffer);
                bufferManager.SetInputAssembler(technique, mesh.VertexBuffer, mesh.Topology);

                for (int p = 0; p < technique.PassCount; p++)
                {
                    graphics.EffectPassApply(technique, p, 0);

                    mesh.Draw(graphics);
                }
            }
            /// <summary>
            /// Draws the patch using shaders
            /// </summary>
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="sceneryDrawer">Drawer</param>
            /// <param name="mesh">Mesh</param>
            /// <param name="material">Material</param>
            private void DrawWithDrawer(BufferManager bufferManager, IBuiltInDrawer sceneryDrawer, Mesh mesh, IMeshMaterial material)
            {
                var materialInfo = new MaterialDrawInfo
                {
                    Material = material,
                    UseAnisotropic = true,
                };

                var meshState = new BuiltInDrawerMeshState
                {
                    Local = Matrix.Identity,
                    Animation = AnimationDrawInfo.Empty,
                };
                sceneryDrawer.UpdateMesh(meshState);

                var materialState = new BuiltInDrawerMaterialState
                {
                    Material = materialInfo,
                    TintColor = Color4.White,
                    TextureIndex = 0,
                };
                sceneryDrawer.UpdateMaterial(materialState);

                bufferManager.SetIndexBuffer(mesh.IndexBuffer);

                sceneryDrawer.Draw(bufferManager, new[] { mesh });
            }

            /// <summary>
            /// Gets effect for rendering based on drawing mode
            /// </summary>
            /// <param name="mode">Drawing mode</param>
            /// <returns>Returns the effect for rendering</returns>
            private IGeometryDrawer GetEffect(DrawerModes mode)
            {
                if (mode.HasFlag(DrawerModes.Deferred))
                {
                    return DrawerPool.GetEffect<EffectDeferredBasic>();
                }

                return null;
            }
            /// <summary>
            /// Gets the drawing effect for the current instance
            /// </summary>
            /// <param name="mode">Drawing mode</param>
            /// <param name="vertexType">Vertex type</param>
            /// <returns>Returns the drawing effect</returns>
            private IBuiltInDrawer GetDrawer(DrawerModes mode, VertexTypes vertexType)
            {
                if (!mode.HasFlag(DrawerModes.Forward))
                {
                    return null;
                }

                switch (vertexType)
                {
                    case VertexTypes.PositionColor:
                        return BuiltInShaders.GetDrawer<BuiltInPositionColor>();
                    case VertexTypes.PositionTexture:
                        return BuiltInShaders.GetDrawer<BuiltInPositionTexture>();
                    case VertexTypes.PositionNormalColor:
                        return BuiltInShaders.GetDrawer<BuiltInPositionNormalColor>();
                    case VertexTypes.PositionNormalTexture:
                        return BuiltInShaders.GetDrawer<BuiltInPositionNormalTexture>();
                    case VertexTypes.PositionNormalTextureTangent:
                        return BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureTangent>();
                    case VertexTypes.PositionColorSkinned:
                        return BuiltInShaders.GetDrawer<BuiltInPositionColorSkinned>();
                    case VertexTypes.PositionTextureSkinned:
                        return BuiltInShaders.GetDrawer<BuiltInPositionTextureSkinned>();
                    case VertexTypes.PositionNormalColorSkinned:
                        return BuiltInShaders.GetDrawer<BuiltInPositionNormalColorSkinned>();
                    case VertexTypes.PositionNormalTextureSkinned:
                        return BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureSkinned>();
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureTangentSkinned>();
                    default:
                        return null;
                }
            }

            /// <summary>
            /// Gets all the used materials
            /// </summary>
            /// <returns>Returns the used materials array</returns>
            public IEnumerable<IMeshMaterial> GetMaterials()
            {
                return DrawingData.Materials.Values.ToArray();
            }
            /// <summary>
            /// Gets a material by mesh material name
            /// </summary>
            /// <param name="meshMaterialName">Name of the material</param>
            /// <returns>Returns a material by mesh material name</returns>
            public IMeshMaterial GetMaterial(string meshMaterialName)
            {
                if (!DrawingData.Materials.Any())
                {
                    return null;
                }

                var meshMaterial = DrawingData.Materials.Keys.FirstOrDefault(m => string.Equals(m, meshMaterialName, StringComparison.OrdinalIgnoreCase));
                if (meshMaterial == null)
                {
                    return null;
                }

                return DrawingData.Materials[meshMaterial];
            }
            /// <summary>
            /// Replaces the material
            /// </summary>
            /// <param name="meshMaterialName">Name of the material to replace</param>
            /// <param name="material">Material</param>
            /// <returns>Returns true if any material were replaced</returns>
            public bool ReplaceMaterial(string meshMaterialName, IMeshMaterial material)
            {
                if (!DrawingData.Materials.Any())
                {
                    return false;
                }

                var meshMaterial = DrawingData.Materials.Keys.FirstOrDefault(m => string.Equals(m, meshMaterialName, StringComparison.OrdinalIgnoreCase));
                if (meshMaterial == null)
                {
                    return false;
                }

                DrawingData.Materials[meshMaterial] = material;

                return true;
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return $"{CreationNodeId} - {CreationDuration}";
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
            /// Task duration
            /// </summary>
            public TimeSpan Duration { get; set; }
            /// <summary>
            /// Created patch
            /// </summary>
            public SceneryPatch Patch { get; set; }
        }

        #endregion

        /// <summary>
        /// Model content
        /// </summary>
        private ContentData content;
        /// <summary>
        /// Scenery patch list
        /// </summary>
        private ConcurrentDictionary<int, SceneryPatch> patchDictionary = new ConcurrentDictionary<int, SceneryPatch>();
        /// <summary>
        /// Visible Nodes
        /// </summary>
        private PickingQuadTreeNode<Triangle>[] visibleNodes = new PickingQuadTreeNode<Triangle>[] { };

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
        public IEnumerable<SceneLight> Lights { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public Scenery(Scene scene, string id, string name)
            : base(scene, id, name)
        {

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
        public override async Task InitializeAssets(GroundDescription description)
        {
            await base.InitializeAssets(description);

            // Generate model content
            content = await Description.ReadModelContent();

            // Generate quadtree
            GroundPickingQuadtree = Description.ReadQuadTree(content.GetTriangles());

            // Retrieve lights from content
            Lights = content.GetLights().ToArray();

            await IntializePatches();
        }
        /// <summary>
        /// Initializes internal patch collection
        /// </summary>
        /// <returns></returns>
        internal async Task IntializePatches()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            // Generate initial patches
            var nodes = GroundPickingQuadtree.GetLeafNodes();
            if (!nodes.Any())
            {
                return;
            }

            var tasks = nodes.Select(async node => await SceneryPatch.CreatePatch(Game, Name, content, node));
            var taskResults = await Task.WhenAll(tasks);
            if (!taskResults.Any())
            {
                return;
            }

            foreach (var taskResult in taskResults)
            {
                if (!Helper.Retry(() => patchDictionary.TryAdd(taskResult.Id, taskResult.Patch), 10))
                {
                    Logger.WriteWarning(nameof(Scenery), $"The node {taskResult.Id} has no created patch.");
                }
            }

            watch.Stop();

            var taskDuration = TimeSpan.FromMilliseconds(taskResults.Sum(p => p.Duration.TotalMilliseconds));
            var maxTaskDuration = TimeSpan.FromMilliseconds(taskResults.Max(p => p.Duration.TotalMilliseconds));
            Logger.WriteDebug(nameof(Scenery), $"Created {nodes.Count()} nodes in {watch.Elapsed}. Task sum = {taskDuration}. Task max = {maxTaskDuration}");
        }

        /// <inheritdoc/>
        public override bool Cull(IIntersectionVolume volume, out float distance)
        {
            distance = float.MaxValue;

            if (base.Cull(volume, out distance))
            {
                return true;
            }

            visibleNodes = GroundPickingQuadtree.GetNodesInVolume(volume).ToArray();
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
                if (patchDictionary.ContainsKey(node.Id))
                {
                    continue;
                }

                Logger.WriteTrace(this, $"Loading node {node.Id} patch.");

                // Reserve position
                patchDictionary.TryAdd(node.Id, null);

                // Add creation task
                taskList.Add(SceneryPatch.CreatePatch(Game, Name, content, node));
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

            var nodeIds = visibleNodes.Select(n => n.Id).ToArray();
            foreach (var nodeId in nodeIds)
            {
                if (patchDictionary.ContainsKey(nodeId))
                {
                    Logger.WriteTrace(this, $"Scenery DrawShadows {context.ShadowMap} {nodeId} patch.");

                    patchDictionary[nodeId]?.DrawSceneryShadows(sceneryEffect, BufferManager);
                }
                else
                {
                    Logger.WriteWarning(this, $"Scenery DrawShadows {context.ShadowMap} {nodeId} without assigned patch. No draw method called");
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

            var nodeIds = visibleNodes.Select(n => n.Id).ToArray();
            foreach (var nodeId in nodeIds)
            {
                if (!patchDictionary.ContainsKey(nodeId))
                {
                    Logger.WriteWarning(this, $"Scenery Draw {nodeId} without assigned patch. No draw method called");

                    continue;
                }

                Logger.WriteTrace(this, $"Scenery Draw {nodeId} patch.");

                patchDictionary[nodeId]?.DrawScenery(context, BufferManager);
            }
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
            Logger.WriteTrace(this, $"LoadPatches Init: {taskList.Count()} tasks.");

            Scene.LoadResourcesAsync(taskList, LoadPatchesCompleted);

            Logger.WriteTrace(this, "LoadPatches End.");
        }
        /// <summary>
        /// Load patches callback
        /// </summary>
        /// <param name="result">Process result</param>
        private void LoadPatchesCompleted(LoadResourcesResult<SceneryPatchTask> result)
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
                    while (!patchDictionary.TryRemove(res.Result.Id, out _))
                    {
                        //None
                    }

                    Logger.WriteError(this, $"Error creating patch {res.Result.Id}: {res.Exception.Message}", res.Exception);
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IMeshMaterial> GetMaterials()
        {
            return patchDictionary.Values.SelectMany(v => v?.GetMaterials() ?? Enumerable.Empty<IMeshMaterial>()).ToArray();
        }
        /// <inheritdoc/>
        public IMeshMaterial GetMaterial(string meshMaterialName)
        {
            foreach (var v in patchDictionary.Values)
            {
                var m = v?.GetMaterial(meshMaterialName);
                if (m != null)
                {
                    return m;
                }
            }

            return null;
        }
        /// <inheritdoc/>
        public void ReplaceMaterial(string meshMaterialName, IMeshMaterial material)
        {
            bool updated = false;

            foreach (var v in patchDictionary.Values)
            {
                if (v?.ReplaceMaterial(meshMaterialName, material) == true)
                {
                    updated = true;
                }
            }

            if (updated)
            {
                Scene.UpdateMaterialPalette();
            }
        }
    }
}
