using SharpDX;
using System;
using System.Collections.Generic;
using PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology;

namespace Engine
{
    using Engine.Collections;
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;
    using Engine.PathFinding;

    /// <summary>
    /// Terrain model
    /// </summary>
    public class Scenery : Ground, UseMaterials
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
            const int MAX = 1024 * 2;

            /// <summary>
            /// Creates a new patch
            /// </summary>
            /// <param name="game">Game</param>
            /// <param name="content">Content</param>
            /// <param name="node">Quadtree node</param>
            /// <returns>Returns the new generated patch</returns>
            public static SceneryPatch CreatePatch(Game game, BufferManager bufferManager, ModelContent content, PickingQuadTreeNode node)
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
            public PickingQuadTreeNode Current { get; private set; }

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
            /// Releases created resources
            /// </summary>
            public void Dispose()
            {
                Helper.Dispose(this.DrawingData);
            }

            /// <summary>
            /// Updates the scenery patch
            /// </summary>
            /// <param name="context">Context</param>
            /// <param name="node">Node</param>
            public void Update(UpdateContext context, PickingQuadTreeNode node)
            {
                this.Current = node;
            }
            /// <summary>
            /// Draws the scenery patch
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <param name="sceneryEffect">Scenery effect</param>
            public void DrawScenery(DrawContext context, Drawer sceneryEffect, BufferManager bufferManager)
            {
                int count = 0;

                foreach (string meshName in this.DrawingData.Meshes.Keys)
                {
                    var dictionary = this.DrawingData.Meshes[meshName];

                    foreach (string material in dictionary.Keys)
                    {
                        #region Per object update

                        var mat = this.DrawingData.Materials[material];

                        if (context.DrawerMode == DrawerModesEnum.Forward)
                        {
                            ((EffectDefaultBasic)sceneryEffect).UpdatePerObject(
                                mat.DiffuseTexture,
                                mat.NormalMap,
                                mat.SpecularTexture,
                                mat.ResourceIndex,
                                0,
                                0);
                        }
                        else if (context.DrawerMode == DrawerModesEnum.Deferred)
                        {
                            ((EffectDeferredBasic)sceneryEffect).UpdatePerObject(
                                mat.DiffuseTexture,
                                mat.NormalMap,
                                mat.SpecularTexture,
                                mat.ResourceIndex,
                                0,
                                0);
                        }
                        else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                        {
                            ((EffectShadowBasic)sceneryEffect).UpdatePerObject(
                                0,
                                0);
                        }

                        #endregion

                        var mesh = dictionary[material];
                        bufferManager.SetIndexBuffer(this.Game.Graphics, mesh.IndexBufferSlot);

                        var technique = sceneryEffect.GetTechnique(mesh.VertextType, mesh.Instanced, DrawingStages.Drawing, context.DrawerMode);
                        bufferManager.SetInputAssembler(this.Game.Graphics, technique, mesh.VertextType, false, mesh.Topology);

                        count += mesh.IndexCount > 0 ? mesh.IndexCount : mesh.VertexCount;

                        for (int p = 0; p < technique.Description.PassCount; p++)
                        {
                            technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                            mesh.Draw(this.Game.Graphics);

                            Counters.DrawCallsPerFrame++;
                        }
                    }
                }

                if (context.DrawerMode != DrawerModesEnum.ShadowMap)
                {
                    Counters.InstancesPerFrame++;
                    Counters.PrimitivesPerFrame += count / 3;
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
        private PickingQuadTreeNode[] visibleNodes;
        /// <summary>
        /// Buffer manager
        /// </summary>
        private BufferManager bufferManager = new BufferManager();

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
        /// Maximum number of instances
        /// </summary>
        public override int MaxInstances
        {
            get
            {
                return 1;
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
                var patch = SceneryPatch.CreatePatch(game, this.bufferManager, content, nodes[i]);
                this.patchDictionary.Add(nodes[i].Id, patch);
            }

            this.bufferManager.CreateBuffers(game.Graphics, this.Name);

            #endregion

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
            Helper.Dispose(this.bufferManager);
        }
        /// <summary>
        /// Objects updating
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
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

                    this.patchDictionary[current.Id].Update(context, current);
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
                this.bufferManager.SetVertexBuffers(this.Game.Graphics);

                Drawer sceneryEffect = null;

                if (context.DrawerMode == DrawerModesEnum.Forward)
                {
                    sceneryEffect = DrawerPool.EffectDefaultBasic;

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

                    #endregion
                }
                else if (context.DrawerMode == DrawerModesEnum.Deferred)
                {
                    sceneryEffect = DrawerPool.EffectDeferredBasic;

                    #region Per frame update

                    ((EffectDeferredBasic)sceneryEffect).UpdatePerFrame(
                        context.World,
                        context.ViewProjection);

                    #endregion
                }
                else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                {
                    sceneryEffect = DrawerPool.EffectShadowBasic;

                    #region Per frame update

                    ((EffectShadowBasic)sceneryEffect).UpdatePerFrame(
                        context.World,
                        context.ViewProjection);

                    #endregion
                }

                this.Game.Graphics.SetBlendDefault();

                for (int i = 0; i < nodes.Length; i++)
                {
                    this.patchDictionary[nodes[i].Id].DrawScenery(context, sceneryEffect, this.bufferManager);
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

                    model.Manipulator.UpdateInternals(true);

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
                            m.Manipulator.UpdateInternals(true);

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
                            m.Manipulator.UpdateInternals(true);

                            //Use full mesh
                            tris.AddRange(m.GetTriangles());
                        });
                    }
                }
            }

            return tris.ToArray();
        }

        /// <summary>
        /// Gets the visible nodes collection
        /// </summary>
        /// <returns>Returns a list of visible nodes</returns>
        public override PickingQuadTreeNode[] GetVisibleNodes()
        {
            return this.visibleNodes;
        }
    }
}
