﻿using SharpDX;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.BuiltIn.Components.Ground
{
    using Engine.BuiltIn.Drawers;
    using Engine.BuiltIn.Drawers.Deferred;
    using Engine.BuiltIn.Drawers.Forward;
    using Engine.Collections.Generic;
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Terrain model
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class Scenery(Scene scene, string id, string name) : Ground<SceneryDescription>(scene, id, name), IUseMaterials
    {
        #region Helper Classes

        /// <summary>
        /// Terrain patch
        /// </summary>
        /// <remarks>
        /// Holds the necessary information to render a portion of terrain using an arbitrary level of detail
        /// </remarks>
        /// <remarks>
        /// Cosntructor
        /// </remarks>
        /// <param name="drawingData">Drawing data</param>
        class SceneryPatch(DrawingData drawingData) : IDisposable
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
                Stopwatch watch = new();
                watch.Start();

                DrawingDataDescription desc = new()
                {
                    Instanced = false,
                    Instances = 0,
                    LoadAnimation = true,
                    LoadNormalMaps = true,
                    TextureCount = 0,
                    DynamicBuffers = false,
                    Constraint = node.BoundingBox,
                };

                var dData = await DrawingData.Read(game, content, desc);

                await dData.Initialize(name);

                watch.Stop();

                return new SceneryPatchTask
                {
                    Id = node.Id,
                    Duration = watch.Elapsed,
                    Patch = new SceneryPatch(dData),
                };
            }

            /// <summary>
            /// Drawing data
            /// </summary>
            public DrawingData DrawingData = drawingData;
            /// <summary>
            /// Next node id
            /// </summary>
            public int CreationNodeId = -1;
            /// <summary>
            /// Node creation time
            /// </summary>
            public TimeSpan CreationDuration = TimeSpan.Zero;

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
            /// <param name="context">Draw context</param>
            public bool DrawSceneryShadows(DrawContextShadows context)
            {
                if (context.ShadowMap == null)
                {
                    return false;
                }

                int count = 0;

                foreach (var matData in DrawingData.IterateMaterials())
                {
                    var meshMaterial = matData.MeshMaterial;
                    var mesh = matData.Mesh;

                    var sceneryDrawer = context.ShadowMap.GetDrawer(mesh.VertextType, false, meshMaterial.Material.IsTransparent);
                    if (sceneryDrawer == null)
                    {
                        continue;
                    }

                    if (DrawWithDrawer(context.DeviceContext, sceneryDrawer, mesh, meshMaterial))
                    {
                        count += mesh.Count;
                    }
                }

                return count > 0;
            }
            /// <summary>
            /// Draws the scenery patch
            /// </summary>
            /// <param name="context">Context</param>
            /// <param name="blendMode">Blend mode</param>
            public bool DrawScenery(DrawContext context, BlendModes blendMode)
            {
                int count = 0;

                foreach (var matData in DrawingData.IterateMaterials())
                {
                    var meshMaterial = matData.MeshMaterial;
                    var mesh = matData.Mesh;

                    bool draw = context.ValidateDraw(blendMode, meshMaterial.Material.IsTransparent);
                    if (!draw)
                    {
                        continue;
                    }

                    var sceneryDrawer = GetDrawer(context.DrawerMode, mesh.VertextType);
                    if (sceneryDrawer == null)
                    {
                        continue;
                    }

                    if (DrawWithDrawer(context.DeviceContext, sceneryDrawer, mesh, meshMaterial))
                    {
                        count += mesh.Count;
                    }
                }

                return count > 0;
            }
            /// <summary>
            /// Draws the patch using shaders
            /// </summary>
            /// <param name="dc">Device context</param>
            /// <param name="sceneryDrawer">Drawer</param>
            /// <param name="mesh">Mesh</param>
            /// <param name="material">Material</param>
            private static bool DrawWithDrawer(IEngineDeviceContext dc, IBuiltInDrawer sceneryDrawer, Mesh mesh, IMeshMaterial material)
            {
                sceneryDrawer.UpdateMesh(dc, BuiltInDrawerMeshState.Default());

                var materialState = new BuiltInDrawerMaterialState
                {
                    TintColor = Color4.White,
                    Material = material,
                    TextureIndex = 0,
                    UseAnisotropic = true,
                };
                sceneryDrawer.UpdateMaterial(dc, materialState);

                return sceneryDrawer.Draw(dc, [mesh]);
            }

            /// <summary>
            /// Gets the drawing effect for the current instance
            /// </summary>
            /// <param name="mode">Drawing mode</param>
            /// <param name="vertexType">Vertex type</param>
            /// <returns>Returns the drawing effect</returns>
            private static IBuiltInDrawer GetDrawer(DrawerModes mode, VertexTypes vertexType)
            {
                if (mode.HasFlag(DrawerModes.Forward))
                {
                    return ForwardDrawerManager.GetDrawer(vertexType, false);
                }

                if (mode.HasFlag(DrawerModes.Deferred))
                {
                    return DeferredDrawerManager.GetDrawer(vertexType, false);
                }

                return null;
            }

            /// <summary>
            /// Gets all the used materials
            /// </summary>
            /// <returns>Returns the used materials array</returns>
            public IEnumerable<IMeshMaterial> GetMaterials()
            {
                return DrawingData.GetMaterials();
            }
            /// <summary>
            /// Gets a material by mesh material name
            /// </summary>
            /// <param name="meshMaterialName">Name of the material</param>
            /// <returns>Returns a material by mesh material name</returns>
            public IMeshMaterial GetMaterial(string meshMaterialName)
            {
                return DrawingData.GetFirstMaterial(meshMaterialName);
            }
            /// <summary>
            /// Replaces the material
            /// </summary>
            /// <param name="meshMaterialName">Name of the material to replace</param>
            /// <param name="material">Material</param>
            /// <returns>Returns true if any material were replaced</returns>
            public bool ReplaceMaterial(string meshMaterialName, IMeshMaterial material)
            {
                return DrawingData.ReplaceMaterial(meshMaterialName, material);
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
        private readonly ConcurrentDictionary<int, SceneryPatch> patchDictionary = new();
        /// <summary>
        /// Visible Nodes
        /// </summary>
        private PickingQuadTreeNode<Triangle>[] visibleNodes = [];

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
            if (!disposing)
            {
                return;
            }

            foreach (var item in patchDictionary.Values)
            {
                item?.Dispose();
            }

            patchDictionary.Clear();
        }

        /// <inheritdoc/>
        public override async Task ReadAssets(SceneryDescription description)
        {
            await base.ReadAssets(description);

            // Generate model content
            var contentList = await Description.ReadContentData();
            if (!contentList.Any())
            {
                throw new ArgumentNullException(nameof(description));
            }

            content = contentList.First();

            // Generate quadtree
            GroundPickingQuadtree = Description.ReadQuadTree(content.GetTriangles());

            // Retrieve lights from content
            Lights = content.CreateLights().ToArray();

            await IntializePatches();
        }
        /// <summary>
        /// Initializes internal patch collection
        /// </summary>
        private async Task IntializePatches()
        {
            Stopwatch watch = new();
            watch.Start();

            // Generate initial patches
            var nodes = GroundPickingQuadtree.GetLeafNodes();
            if (!nodes.Any())
            {
                return;
            }

            var tasks = nodes.Select(async node => await SceneryPatch.CreatePatch(Game, Name, content, node));
            var taskResults = await Task.WhenAll(tasks);
            if (taskResults.Length == 0)
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
        public override bool Cull(int cullIndex, ICullingVolume volume, out float distance)
        {
            distance = float.MaxValue;

            if (base.Cull(cullIndex, volume, out distance))
            {
                return true;
            }

            var nodes = GroundPickingQuadtree.FindNodesInVolume(volume).ToArray();
            if (nodes.Length == 0)
            {
                return true;
            }

            if (nodes.Length > 1)
            {
                var pointOfView = volume.Position;

                //Sort nodes by center distance to the culling volume position - nearest nodes first
                Array.Sort(nodes, (n1, n2) =>
                {
                    float d1 = (n1.Center - pointOfView).LengthSquared();
                    float d2 = (n2.Center - pointOfView).LengthSquared();

                    return d1.CompareTo(d2);
                });
            }

            distance = Vector3.DistanceSquared(volume.Position, nodes[0].Center);

            return false;
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            visibleNodes = GroundPickingQuadtree.FindNodesInVolume((IntersectionVolumeFrustum)Scene.Camera.Frustum).ToArray();
            if (visibleNodes.Length == 0)
            {
                return;
            }

            // Detect nodes without assigned patch
            List<Func<Task<SceneryPatchTask>>> taskList = [];

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
                taskList.Add(() => SceneryPatch.CreatePatch(Game, Name, content, node));
            }

            // Launch creation tasks
            LoadPatches(taskList);
        }

        /// <inheritdoc/>
        public override bool DrawShadows(DrawContextShadows context)
        {
            if (visibleNodes.Length == 0)
            {
                return false;
            }

            bool drawn = false;

            var nodeIds = visibleNodes.Select(n => n.Id).ToArray();
            foreach (var nodeId in nodeIds)
            {
                if (!patchDictionary.ContainsKey(nodeId))
                {
                    Logger.WriteWarning(this, $"Scenery DrawShadows {context.ShadowMap} {nodeId} without assigned patch. No draw method called");
                }

                Logger.WriteTrace(this, $"Scenery DrawShadows {context.ShadowMap} {nodeId} patch.");

                drawn = (patchDictionary[nodeId]?.DrawSceneryShadows(context) ?? false) || drawn;
            }

            return drawn;
        }
        /// <inheritdoc/>
        public override bool Draw(DrawContext context)
        {
            if (visibleNodes.Length == 0)
            {
                return false;
            }

            bool drawn = false;

            var nodeIds = visibleNodes.Select(n => n.Id).ToArray();
            foreach (var nodeId in nodeIds)
            {
                if (!patchDictionary.ContainsKey(nodeId))
                {
                    Logger.WriteWarning(this, $"Scenery Draw {nodeId} without assigned patch. No draw method called");

                    continue;
                }

                Logger.WriteTrace(this, $"Scenery Draw {nodeId} patch.");

                drawn = (patchDictionary[nodeId]?.DrawScenery(context, BlendMode) ?? false) || drawn;
            }

            return drawn;
        }

        /// <summary>
        /// Launch an async resource load with the task list
        /// </summary>
        /// <param name="taskList">Task list to launch</param>
        private void LoadPatches(IEnumerable<Func<Task<SceneryPatchTask>>> taskList)
        {
            if (!taskList.Any())
            {
                return;
            }

            const string loadTaskName = nameof(LoadPatches);

            // Fire and forget
            Logger.WriteTrace(this, $"{loadTaskName} Init: {taskList.Count()} tasks.");

            var loadGroup = LoadResourceGroup<SceneryPatchTask>.FromTasks(taskList, LoadPatchesCompleted, null, $"{loadTaskName}");

            Scene.LoadResources(loadGroup);

            Logger.WriteTrace(this, $"{loadTaskName} End.");
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
            return patchDictionary.Values.SelectMany(v => v?.GetMaterials() ?? []).ToArray();
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
        public bool ReplaceMaterial(string meshMaterialName, IMeshMaterial material)
        {
            bool updated = false;

            foreach (var v in patchDictionary.Values)
            {
                if (v?.ReplaceMaterial(meshMaterialName, material) == true)
                {
                    updated = true;
                }
            }

            return updated;
        }
    }
}
