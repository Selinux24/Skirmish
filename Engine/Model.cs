using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Basic Model
    /// </summary>
    public class Model : ModelBase, IPickable
    {
        /// <summary>
        /// Update point cache flag
        /// </summary>
        private bool updatePoints = true;
        /// <summary>
        /// Update triangle cache flag
        /// </summary>
        private bool updateTriangles = true;
        /// <summary>
        /// Points caché
        /// </summary>
        private Vector3[] positionCache = null;
        /// <summary>
        /// Triangle list cache
        /// </summary>
        private Triangle[] triangleCache = null;
        /// <summary>
        /// Bounding sphere cache
        /// </summary>
        private BoundingSphere boundingSphere = new BoundingSphere();
        /// <summary>
        /// Bounding box cache
        /// </summary>
        private BoundingBox boundingBox = new BoundingBox();
        /// <summary>
        /// Oriented bounding box cache
        /// </summary>
        private OrientedBoundingBox orientedBoundingBox = new OrientedBoundingBox();
        /// <summary>
        /// Gets if model has volumes
        /// </summary>
        private bool hasVolumes
        {
            get
            {
                Vector3[] positions = this.GetPoints();

                return positions != null && positions.Length > 0;
            }
        }
        /// <summary>
        /// Local transform
        /// </summary>
        private Matrix local = Matrix.Identity;
        /// <summary>
        /// Level of detail
        /// </summary>
        private LevelOfDetailEnum levelOfDetail = LevelOfDetailEnum.None;
        /// <summary>
        /// Datos renderización
        /// </summary>
        protected DrawingData DrawingData { get; private set; }
        /// <summary>
        /// Enables z-buffer writting
        /// </summary>
        public bool EnableDepthStencil { get; set; }
        /// <summary>
        /// Enables transparent blending
        /// </summary>
        public bool EnableAlphaBlending { get; set; }
        /// <summary>
        /// Model manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; private set; }
        /// <summary>
        /// Texture index
        /// </summary>
        public int TextureIndex { get; set; }
        /// <summary>
        /// Level of detail
        /// </summary>
        public override LevelOfDetailEnum LevelOfDetail
        {
            get
            {
                return this.levelOfDetail;
            }
            set
            {
                this.levelOfDetail = this.GetLODNearest(value);
                this.DrawingData = this.ChangeDrawingData(this.DrawingData, this.levelOfDetail);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="content">Content</param>
        /// <param name="dynamic">Sets whether the buffers must be created inmutables or not</param>
        public Model(Game game, ModelContent content, bool dynamic = false)
            : base(game, content, false, 0, true, true, dynamic)
        {
            this.Manipulator = new Manipulator3D();
            this.Manipulator.Updated += new EventHandler(ManipulatorUpdated);

            this.EnableDepthStencil = true;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="content">Content</param>
        /// <param name="dynamic">Sets whether the buffers must be created inmutables or not</param>
        public Model(Game game, LODModelContent content, bool dynamic = false)
            : base(game, content, false, 0, true, true, dynamic)
        {
            this.Manipulator = new Manipulator3D();
            this.Manipulator.Updated += new EventHandler(ManipulatorUpdated);

            this.EnableDepthStencil = true;
        }
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            if (this.DrawingData != null && this.DrawingData.SkinningData != null)
            {
                this.DrawingData.SkinningData.Update(context.GameTime);
            }

            this.Manipulator.Update(context.GameTime);

            this.local = context.World * this.Manipulator.LocalTransform;
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (this.DrawingData != null)
            {
                Drawer effect = null;
                if (context.DrawerMode == DrawerModesEnum.Forward) effect = DrawerPool.EffectBasic;
                else if (context.DrawerMode == DrawerModesEnum.Deferred) effect = DrawerPool.EffectGBuffer;
                else if (context.DrawerMode == DrawerModesEnum.ShadowMap) effect = DrawerPool.EffectShadow;

                if (effect != null)
                {
                    #region Per frame update

                    if (context.DrawerMode == DrawerModesEnum.Forward)
                    {
                        ((EffectBasic)effect).UpdatePerFrame(
                            this.local,
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
                            this.local,
                            context.ViewProjection);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        ((EffectBasicShadow)effect).UpdatePerFrame(
                            this.local,
                            context.ViewProjection);
                    }

                    #endregion

                    if (this.EnableDepthStencil)
                    {
                        this.Game.Graphics.SetDepthStencilZEnabled();
                    }
                    else
                    {
                        this.Game.Graphics.SetDepthStencilZDisabled();
                    }

                    if (this.EnableAlphaBlending)
                    {
                        this.Game.Graphics.SetBlendTransparent();
                    }

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
                                ((EffectBasic)effect).UpdatePerObject(matdata, texture, normalMap, this.TextureIndex);
                            }
                            else if (context.DrawerMode == DrawerModesEnum.Deferred)
                            {
                                ((EffectBasicGBuffer)effect).UpdatePerObject(mat.Material, texture, normalMap, this.TextureIndex);
                            }

                            #endregion

                            var technique = effect.GetTechnique(mesh.VertextType, mesh.Instanced, DrawingStages.Drawing, context.DrawerMode);

                            mesh.SetInputAssembler(this.DeviceContext, effect.GetInputLayout(technique));

                            for (int p = 0; p < technique.Description.PassCount; p++)
                            {
                                technique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                                mesh.Draw(this.DeviceContext);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Frustum culling
        /// </summary>
        /// <param name="frustum">Frustum</param>
        public override void FrustumCulling(BoundingFrustum frustum)
        {
            if (this.hasVolumes)
            {
                this.Cull = frustum.Contains(this.GetBoundingSphere()) == ContainmentType.Disjoint;
            }
            else
            {
                this.Cull = false;
            }

            if (!this.Cull)
            {
                var pars = frustum.GetCameraParams();
                var dist = Vector3.DistanceSquared(this.Manipulator.Position, pars.Position);
                if (dist < 100f) { this.LevelOfDetail = LevelOfDetailEnum.High; }
                else if (dist < 400f) { this.LevelOfDetail = LevelOfDetailEnum.Medium; }
                else if (dist < 1600f) { this.LevelOfDetail = LevelOfDetailEnum.Low; }
                else { this.LevelOfDetail = LevelOfDetailEnum.Minimum; }
            }
        }

        /// <summary>
        /// Sets a new manipulator to this instance
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        public void SetManipulator(Manipulator3D manipulator)
        {
            this.Manipulator.Updated -= ManipulatorUpdated;
            this.Manipulator = null;

            this.Manipulator = manipulator;
            this.Manipulator.Updated += ManipulatorUpdated;
        }
        /// <summary>
        /// Occurs when manipulator transform updated
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void ManipulatorUpdated(object sender, EventArgs e)
        {
            this.updatePoints = true;

            this.updateTriangles = true;

            this.boundingSphere = new BoundingSphere();
            this.boundingBox = new BoundingBox();
            this.orientedBoundingBox = new OrientedBoundingBox();
        }

        /// <summary>
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <returns>Returns null or position list</returns>
        public Vector3[] GetPoints()
        {
            if (this.updatePoints)
            {
                var drawingData = this.GetDrawingData(this.GetLODMinimum());

                this.positionCache = drawingData.GetPoints(this.Manipulator.LocalTransform);

                this.updatePoints = false;
            }

            return this.positionCache;
        }
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <returns>Returns null or triangle list</returns>
        public Triangle[] GetTriangles()
        {
            if (this.updateTriangles)
            {
                var drawingData = this.GetDrawingData(this.GetLODMinimum());

                this.triangleCache = drawingData.GetTriangles(this.Manipulator.LocalTransform);

                this.updateTriangles = false;
            }

            return this.triangleCache;
        }
        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public BoundingSphere GetBoundingSphere()
        {
            if (this.boundingSphere == new BoundingSphere())
            {
                Vector3[] positions = this.GetPoints();
                if (positions != null && positions.Length > 0)
                {
                    this.boundingSphere = BoundingSphere.FromPoints(positions);
                }
            }

            return this.boundingSphere;
        }
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        public BoundingBox GetBoundingBox()
        {
            if (this.boundingBox == new BoundingBox())
            {
                Vector3[] positions = this.GetPoints();
                if (positions != null && positions.Length > 0)
                {
                    this.boundingBox = BoundingBox.FromPoints(positions);
                }
            }

            return this.boundingBox;
        }
        /// <summary>
        /// Gets oriented bounding box
        /// </summary>
        /// <returns>Returns oriented bounding box with identity transformation. Empty if the vertex type hasn't position channel</returns>
        public OrientedBoundingBox GetOrientedBoundingBox()
        {
            if (this.orientedBoundingBox == new OrientedBoundingBox())
            {
                Vector3[] positions = this.GetPoints();
                if (positions != null && positions.Length > 0)
                {
                    this.orientedBoundingBox = new OrientedBoundingBox(positions);
                    this.orientedBoundingBox.Transform(Matrix.Identity);
                }
            }

            return this.orientedBoundingBox;
        }

        /// <summary>
        /// Gets model pose at time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="transform">Transform to apply</param>
        /// <returns>Returns the positioned triangle array of the model</returns>
        public Triangle[] GetPoseAtTime(float time, Matrix transform)
        {
            List<Triangle> res = new List<Triangle>();

            var globalTransform = this.Manipulator.LocalTransform * transform;

            foreach (var meshName in this.ModelContent.Geometry.Keys)
            {
                var cr = this.ModelContent.Controllers.GetControllerForMesh(meshName);
                var vw = cr.Weights;

                var smat = this.ModelContent.Geometry[meshName];
                foreach (var sm in smat.Values)
                {
                    string[] boneNames;
                    Matrix[] boneTransforms;
                    this.GetFinalTransforms(time, meshName, out boneNames, out boneTransforms);

                    var verts = sm.Vertices;
                    Triangle[] tris = new Triangle[verts.Length / 3];
                    int index = 0;

                    for (int i = 0; i < verts.Length; i += 3)
                    {
                        var p0 = VertexData.ApplyWeight(verts[i + 0], vw, boneNames, boneTransforms, globalTransform);
                        var p1 = VertexData.ApplyWeight(verts[i + 1], vw, boneNames, boneTransforms, globalTransform);
                        var p2 = VertexData.ApplyWeight(verts[i + 2], vw, boneNames, boneTransforms, globalTransform);

                        tris[index++] = new Triangle(
                            p0.Position.Value,
                            p1.Position.Value,
                            p2.Position.Value);
                    }

                    res.AddRange(tris);
                }
            }

            return res.ToArray();
        }
        /// <summary>
        /// Gets the final bone transforms for the specified mesh at specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="meshName">Mesh name</param>
        /// <param name="boneNames">Returns the bone names</param>
        /// <param name="boneTransforms">Returns the bone transforms</param>
        private void GetFinalTransforms(float time, string meshName, out string[] boneNames, out Matrix[] boneTransforms)
        {
            boneNames = null;
            boneTransforms = null;

            if (this.DrawingData != null && this.DrawingData.SkinningData != null)
            {
                this.DrawingData.SkinningData.Resolve(time, meshName, out boneNames, out boneTransforms);
            }
        }

        /// <summary>
        /// Gets nearest picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <param name="distance">Distance to position</param>
        /// <returns>Returns true if ground position found</returns>
        public virtual bool PickNearest(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            position = new Vector3();
            triangle = new Triangle();
            distance = float.MaxValue;

            BoundingSphere bsph = this.GetBoundingSphere();
            if (bsph.Intersects(ref ray))
            {
                Triangle[] triangles = this.GetTriangles();

                Vector3 pos;
                Triangle tri;
                float d;
                if (Triangle.IntersectNearest(ref ray, triangles, facingOnly, out pos, out tri, out d))
                {
                    position = pos;
                    triangle = tri;
                    distance = d;

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets first picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <param name="distance">Distance to position</param>
        /// <returns>Returns true if ground position found</returns>
        public virtual bool PickFirst(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            position = new Vector3();
            triangle = new Triangle();
            distance = float.MaxValue;

            BoundingSphere bsph = this.GetBoundingSphere();
            if (bsph.Intersects(ref ray))
            {
                Triangle[] triangles = this.GetTriangles();

                Vector3 pos;
                Triangle tri;
                float d;
                if (Triangle.IntersectFirst(ref ray, triangles, facingOnly, out pos, out tri, out d))
                {
                    position = pos;
                    triangle = tri;
                    distance = d;

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets all picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="positions">Ground positions if exists</param>
        /// <param name="triangles">Triangles found</param>
        /// <param name="distances">Distances to positions</param>
        /// <returns>Returns true if ground position found</returns>
        public virtual bool PickAll(ref Ray ray, bool facingOnly, out Vector3[] positions, out Triangle[] triangles, out float[] distances)
        {
            positions = null;
            triangles = null;
            distances = null;

            BoundingSphere bsph = this.GetBoundingSphere();
            if (bsph.Intersects(ref ray))
            {
                Triangle[] tris = this.GetTriangles();

                Vector3[] pos;
                Triangle[] tri;
                float[] ds;
                if (Triangle.IntersectAll(ref ray, tris, facingOnly, out pos, out tri, out ds))
                {
                    positions = pos;
                    triangles = tri;
                    distances = ds;

                    return true;
                }
            }

            return false;
        }
    }
}
