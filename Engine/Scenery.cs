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
    public class Scenery : Ground
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

                return new SceneryPatch(game, drawingData);
            }

            /// <summary>
            /// Game
            /// </summary>
            protected Game Game = null;
            /// <summary>
            /// Drawing data
            /// </summary>
            protected DrawingData DrawingData = null;

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
            }

            /// <summary>
            /// Draw the patch terrain
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <param name="technique">Technique</param>
            public void Draw(DrawContext context, Drawer effect)
            {
                foreach (string meshName in this.DrawingData.Meshes.Keys)
                {
                    #region Per skinning update

                    if (this.DrawingData.SkinningData != null)
                    {
                        if (context.DrawerMode == DrawerModesEnum.Forward)
                        {
                            ((EffectBasic)effect).UpdatePerSkinning(this.DrawingData.SkinningData.GetFinalTransforms(meshName));
                        }
                        else if (context.DrawerMode == DrawerModesEnum.Deferred)
                        {
                            ((EffectBasicGBuffer)effect).UpdatePerSkinning(this.DrawingData.SkinningData.GetFinalTransforms(meshName));
                        }
                        else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                        {
                            ((EffectBasicShadow)effect).UpdatePerSkinning(this.DrawingData.SkinningData.GetFinalTransforms(meshName));
                        }
                    }
                    else
                    {
                        if (context.DrawerMode == DrawerModesEnum.Forward)
                        {
                            ((EffectBasic)effect).UpdatePerSkinning(null);
                        }
                        else if (context.DrawerMode == DrawerModesEnum.Deferred)
                        {
                            ((EffectBasicGBuffer)effect).UpdatePerSkinning(null);
                        }
                        else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                        {
                            ((EffectBasicShadow)effect).UpdatePerSkinning(null);
                        }
                    }

                    #endregion

                    var dictionary = this.DrawingData.Meshes[meshName];

                    foreach (string material in dictionary.Keys)
                    {
                        var mesh = dictionary[material];
                        var mat = this.DrawingData.Materials[material];

                        #region Per object update

                        var matdata = mat != null ? mat.Material : Material.Default;
                        var texture = mat != null ? mat.DiffuseTexture : null;
                        var normalMap = mat != null ? mat.NormalMap : null;

                        if (context.DrawerMode == DrawerModesEnum.Forward)
                        {
                            ((EffectBasic)effect).UpdatePerObject(matdata, texture, normalMap, 0);
                        }
                        else if (context.DrawerMode == DrawerModesEnum.Deferred)
                        {
                            ((EffectBasicGBuffer)effect).UpdatePerObject(mat.Material, texture, normalMap, 0);
                        }

                        #endregion

                        var technique = effect.GetTechnique(mesh.VertextType, DrawingStages.Drawing, context.DrawerMode);

                        mesh.SetInputAssembler(this.Game.Graphics.DeviceContext, effect.GetInputLayout(technique));

                        for (int p = 0; p < technique.Description.PassCount; p++)
                        {
                            technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                            mesh.Draw(this.Game.Graphics.DeviceContext);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Scenery patches dictionary
        /// </summary>
        class SceneryPatchDictionary : Dictionary<int, SceneryPatch>
        {

        }

        #endregion

        /// <summary>
        /// Visible nodes
        /// </summary>
        private PickingQuadTreeNode[] visibleNodes = null;
        /// <summary>
        /// Scenery patch list
        /// </summary>
        private SceneryPatchDictionary patchDictionary = new SceneryPatchDictionary();
        /// <summary>
        /// Cached triangle list
        /// </summary>
        private Triangle[] triangleCache = null;

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
            this.DeferredEnabled = this.Description.DeferredEnabled;

            this.triangleCache = content.GetTriangles();
            this.pickingQuadtree = PickingQuadTree.Build(this.triangleCache, description);
            var nodes = this.pickingQuadtree.GetTailNodes();
            for (int i = 0; i < nodes.Length; i++)
            {
                var patch = SceneryPatch.CreatePatch(game, content, nodes[i]);
                this.patchDictionary.Add(nodes[i].Id, patch);
            }

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
            this.visibleNodes = this.pickingQuadtree.GetNodesInVolume(ref context.Frustum);
        }
        /// <summary>
        /// Objects drawing
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (visibleNodes != null && visibleNodes.Length > 0)
            {
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

                for (int i = 0; i < visibleNodes.Length; i++)
                {
                    this.patchDictionary[visibleNodes[i].Id].Draw(context, effect);
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

                this.pickingQuadtree = PickingQuadTree.Build(triangles, this.Description);
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
                    if (usage == UsageEnum.Picking && curr.Use.HasFlag(AttachedModelUsesEnum.CoarsePicking) ||
                        usage == UsageEnum.PathFinding && curr.Use.HasFlag(AttachedModelUsesEnum.CoarsePathFinding))
                    {
                        var cylinder = BoundingCylinder.FromPoints(((Model)curr.Model).GetPoints());
                        tris.AddRange(Triangle.ComputeTriangleList(PrimitiveTopology.TriangleList, cylinder, 8));
                    }
                    else if (
                        usage == UsageEnum.Picking && curr.Use.HasFlag(AttachedModelUsesEnum.FullPicking) ||
                        usage == UsageEnum.PathFinding && curr.Use.HasFlag(AttachedModelUsesEnum.FullPathFinding))
                    {
                        tris.AddRange(((Model)curr.Model).GetTriangles());
                    }
                }
                else if (curr.Model is ModelInstanced)
                {
                    if (usage == UsageEnum.Picking && curr.Use.HasFlag(AttachedModelUsesEnum.CoarsePicking) ||
                        usage == UsageEnum.PathFinding && curr.Use.HasFlag(AttachedModelUsesEnum.CoarsePathFinding))
                    {
                        Array.ForEach(((ModelInstanced)curr.Model).Instances, (m) =>
                        {
                            var cylinder = BoundingCylinder.FromPoints(m.GetPoints());
                            tris.AddRange(Triangle.ComputeTriangleList(PrimitiveTopology.TriangleList, cylinder, 8));
                        });
                    }
                    else if (
                        usage == UsageEnum.Picking && curr.Use.HasFlag(AttachedModelUsesEnum.FullPicking) ||
                        usage == UsageEnum.PathFinding && curr.Use.HasFlag(AttachedModelUsesEnum.FullPathFinding))
                    {
                        Array.ForEach(((ModelInstanced)curr.Model).Instances, (m) =>
                        {
                            tris.AddRange(m.GetTriangles());
                        });
                    }
                }
            }

            return tris.ToArray();
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
