using System;
using SharpDX;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Basic Model
    /// </summary>
    public class Model : ModelBase
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
        /// Enables transparent blending
        /// </summary>
        public bool EnableAlphaBlending { get; set; }
        /// <summary>
        /// Model manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; set; }
        /// <summary>
        /// Texture index
        /// </summary>
        public int TextureIndex { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="content">Content</param>
        public Model(Game game, ModelContent content)
            : base(game, content, false, 0, true, true)
        {
            this.Manipulator = new Manipulator3D();
            this.Manipulator.Updated += new EventHandler(ManipulatorUpdated);
        }
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        public override void Update(GameTime gameTime, Context context)
        {
            base.Update(gameTime, context);

            this.Manipulator.Update(gameTime);
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        public override void Draw(GameTime gameTime, Context context)
        {
            if (this.Meshes != null)
            {
                Drawer effect = null;
                if (context.DrawerMode == DrawerModesEnum.Forward) effect = DrawerPool.EffectBasic;
                else if (context.DrawerMode == DrawerModesEnum.Deferred) effect = DrawerPool.EffectGBuffer;
                else if (context.DrawerMode == DrawerModesEnum.ShadowMap) effect = DrawerPool.EffectShadow;

                if (effect != null)
                {
                    if (this.EnableAlphaBlending)
                    {
                        this.Game.Graphics.SetBlendTransparent();
                    }
                    else
                    {
                        this.Game.Graphics.SetBlendAlphaToCoverage();
                    }

                    #region Per frame update

                    if (context.DrawerMode == DrawerModesEnum.Forward)
                    {
                        Matrix local = this.Manipulator.LocalTransform;
                        Matrix world = context.World * local;
                        Matrix worldInverse = Matrix.Invert(world);
                        Matrix worldViewProjection = world * context.ViewProjection;

                        ((EffectBasic)effect).FrameBuffer.World = world;
                        ((EffectBasic)effect).FrameBuffer.WorldInverse = worldInverse;
                        ((EffectBasic)effect).FrameBuffer.WorldViewProjection = worldViewProjection;
                        ((EffectBasic)effect).FrameBuffer.ShadowTransform = context.ShadowTransform;
                        ((EffectBasic)effect).FrameBuffer.Lights = new BufferLights(context.EyePosition, context.Lights);
                        ((EffectBasic)effect).UpdatePerFrame(context.ShadowMap);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        Matrix local = this.Manipulator.LocalTransform;
                        Matrix world = context.World * local;
                        Matrix worldInverse = Matrix.Invert(world);
                        Matrix worldViewProjection = world * context.ViewProjection;
                        Matrix shadowTransform = context.ShadowTransform;

                        ((EffectBasicGBuffer)effect).FrameBuffer.World = world;
                        ((EffectBasicGBuffer)effect).FrameBuffer.WorldInverse = worldInverse;
                        ((EffectBasicGBuffer)effect).FrameBuffer.WorldViewProjection = worldViewProjection;
                        ((EffectBasicGBuffer)effect).FrameBuffer.ShadowTransform = context.ShadowTransform;
                        ((EffectBasicGBuffer)effect).UpdatePerFrame(context.ShadowMap);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        Matrix local = this.Manipulator.LocalTransform;
                        Matrix world = context.World * local;
                        Matrix worldInverse = Matrix.Invert(world);
                        Matrix worldViewProjection = world * context.ViewProjection;

                        ((EffectBasicShadow)effect).FrameBuffer.WorldViewProjection = worldViewProjection;
                        ((EffectBasicShadow)effect).UpdatePerFrame();
                    }

                    #endregion

                    foreach (string meshName in this.Meshes.Keys)
                    {
                        #region Per skinning update

                        if (this.SkinningData != null)
                        {
                            if (context.DrawerMode == DrawerModesEnum.Forward)
                            {
                                ((EffectBasic)effect).SkinningBuffer.FinalTransforms = this.SkinningData.GetFinalTransforms(meshName);
                                ((EffectBasic)effect).UpdatePerSkinning();
                            }
                            else if (context.DrawerMode == DrawerModesEnum.Deferred)
                            {
                                ((EffectBasicGBuffer)effect).SkinningBuffer.FinalTransforms = this.SkinningData.GetFinalTransforms(meshName);
                                ((EffectBasicGBuffer)effect).UpdatePerSkinning();
                            }
                            else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                            {
                                ((EffectBasicShadow)effect).SkinningBuffer.FinalTransforms = this.SkinningData.GetFinalTransforms(meshName);
                                ((EffectBasicShadow)effect).UpdatePerSkinning();
                            }
                        }

                        #endregion

                        MeshMaterialsDictionary dictionary = this.Meshes[meshName];

                        foreach (string material in dictionary.Keys)
                        {
                            Mesh mesh = dictionary[material];
                            MeshMaterial mat = this.Materials[material];

                            #region Per object update

                            var matdata = mat != null ? mat.Material : Material.Default;
                            var texture = mat != null ? mat.DiffuseTexture : null;
                            var normalMap = mat != null ? mat.NormalMap : null;

                            if (context.DrawerMode == DrawerModesEnum.Forward)
                            {
                                ((EffectBasic)effect).ObjectBuffer.Material.SetMaterial(matdata);
                                ((EffectBasic)effect).UpdatePerObject(texture, normalMap);
                            }
                            else if (context.DrawerMode == DrawerModesEnum.Deferred)
                            {
                                ((EffectBasicGBuffer)effect).ObjectBuffer.Material.SetMaterial(mat.Material);
                                ((EffectBasicGBuffer)effect).UpdatePerObject(texture, normalMap);
                            }

                            #endregion

                            #region Per instance update

                            if (context.DrawerMode == DrawerModesEnum.Forward)
                            {
                                ((EffectBasic)effect).InstanceBuffer.TextureIndex = this.TextureIndex;
                                ((EffectBasic)effect).UpdatePerInstance();
                            }
                            else if (context.DrawerMode == DrawerModesEnum.Deferred)
                            {
                                ((EffectBasicGBuffer)effect).InstanceBuffer.TextureIndex = this.TextureIndex;
                                ((EffectBasicGBuffer)effect).UpdatePerInstance();
                            }

                            #endregion

                            EffectTechnique technique = effect.GetTechnique(mesh.VertextType, DrawingStages.Drawing);

                            mesh.SetInputAssembler(this.DeviceContext, effect.GetInputLayout(technique));

                            for (int p = 0; p < technique.Description.PassCount; p++)
                            {
                                technique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                                mesh.Draw(gameTime, this.DeviceContext);
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
                this.positionCache = base.GetPoints(this.Manipulator.LocalTransform);

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
                this.triangleCache = base.GetTriangles(this.Manipulator.LocalTransform);

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
        /// Gets nearest picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <returns>Returns true if ground position found</returns>
        /// <remarks>By default, result is constrained to front faces only</remarks>
        public virtual bool PickNearest(ref Ray ray, out Vector3 position, out Triangle triangle)
        {
            return this.PickNearest(ref ray, true, out position, out triangle);
        }
        /// <summary>
        /// Gets nearest picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <returns>Returns true if ground position found</returns>
        public virtual bool PickNearest(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle)
        {
            position = new Vector3();
            triangle = new Triangle();

            BoundingSphere bsph = this.GetBoundingSphere();
            if (bsph.Intersects(ref ray))
            {
                Triangle[] triangles = this.GetTriangles();

                Vector3 pos;
                Triangle tri;
                if (Triangle.IntersectNearest(ref ray, triangles, facingOnly, out pos, out tri))
                {
                    position = pos;
                    triangle = tri;

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets first picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <returns>Returns true if ground position found</returns>
        /// <remarks>By default, result is constrained to front faces only</remarks>
        public virtual bool PickFirst(ref Ray ray, out Vector3 position, out Triangle triangle)
        {
            return this.PickFirst(ref ray, true, out position, out triangle);
        }
        /// <summary>
        /// Gets first picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <returns>Returns true if ground position found</returns>
        public virtual bool PickFirst(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle)
        {
            position = new Vector3();
            triangle = new Triangle();

            BoundingSphere bsph = this.GetBoundingSphere();
            if (bsph.Intersects(ref ray))
            {
                Triangle[] triangles = this.GetTriangles();

                Vector3 pos;
                Triangle tri;
                if (Triangle.IntersectFirst(ref ray, triangles, facingOnly, out pos, out tri))
                {
                    position = pos;
                    triangle = tri;

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets all picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="positions">Ground positions if exists</param>
        /// <param name="triangles">Triangles found</param>
        /// <returns>Returns true if ground position found</returns>
        /// <remarks>By default, result is constrained to front faces only</remarks>
        public virtual bool PickAll(ref Ray ray, out Vector3[] positions, out Triangle[] triangles)
        {
            return this.PickAll(ref ray, true, out positions, out triangles);
        }
        /// <summary>
        /// Gets all picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="positions">Ground positions if exists</param>
        /// <param name="triangles">Triangles found</param>
        /// <returns>Returns true if ground position found</returns>
        public virtual bool PickAll(ref Ray ray, bool facingOnly, out Vector3[] positions, out Triangle[] triangles)
        {
            positions = null;
            triangles = null;

            BoundingSphere bsph = this.GetBoundingSphere();
            if (bsph.Intersects(ref ray))
            {
                Triangle[] tris = this.GetTriangles();

                Vector3[] pos;
                Triangle[] tri;
                if (Triangle.IntersectAll(ref ray, tris, facingOnly, out pos, out tri))
                {
                    positions = pos;
                    triangles = tri;

                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Terrain description
    /// </summary>
    public class ModelDescription
    {
        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath = "Resources";
        /// <summary>
        /// Model file name
        /// </summary>
        public string ModelFileName = null;
        /// <summary>
        /// Texture index
        /// </summary>
        public int TextureIndex = 0;
        /// <summary>
        /// Drops shadow
        /// </summary>
        public bool Opaque = false;
    }
}
