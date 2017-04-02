using SharpDX;
using System;

namespace Engine
{
    using Engine.Animation;
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Basic Model
    /// </summary>
    public class Model : ModelBase, IRayPickable<Triangle>
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
                var points = this.GetPoints();

                return points != null && points.Length > 0;
            }
        }
        /// <summary>
        /// Level of detail
        /// </summary>
        private LevelOfDetailEnum levelOfDetail = LevelOfDetailEnum.None;

        /// <summary>
        /// Current drawing data
        /// </summary>
        protected DrawingData DrawingData { get; private set; }
        /// <summary>
        /// Animation index
        /// </summary>
        protected uint AnimationIndex = 0;

        /// <summary>
        /// Model manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; private set; }
        /// <summary>
        /// Manipulator has changed last frame
        /// </summary>
        public bool ManipulatorChanged { get; private set; }
        /// <summary>
        /// Animation controller
        /// </summary>
        public AnimationController AnimationController = new AnimationController();
        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex { get; set; }
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
                this.DrawingData = this.GetDrawingData(this.levelOfDetail);
            }
        }
        /// <summary>
        /// Gets the current model lights collection
        /// </summary>
        public SceneLight[] Lights { get; protected set; }
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
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="content">Content</param>
        /// <param name="description">Description</param>
        /// <param name="dynamic">Sets whether the buffers must be created inmutables or not</param>
        public Model(Game game, BufferManager bufferManager, ModelContent content, ModelDescription description, bool dynamic = false)
            : base(game, bufferManager, content, description, false, 0, true, true, dynamic)
        {
            this.TextureIndex = description.TextureIndex;

            this.Manipulator = new Manipulator3D();
            this.Manipulator.Updated += new EventHandler(ManipulatorUpdated);

            var drawData = this.GetDrawingData(LevelOfDetailEnum.High);
            if (drawData != null)
            {
                this.Lights = drawData.Lights;
            }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="content">Content</param>
        /// <param name="description">Description</param>
        /// <param name="dynamic">Sets whether the buffers must be created inmutables or not</param>
        public Model(Game game, BufferManager bufferManager, LODModelContent content, ModelDescription description, bool dynamic = false)
            : base(game, bufferManager, content, description, false, 0, true, true, dynamic)
        {
            this.TextureIndex = description.TextureIndex;

            this.Manipulator = new Manipulator3D();
            this.Manipulator.Updated += new EventHandler(ManipulatorUpdated);

            var drawData = this.GetDrawingData(LevelOfDetailEnum.High);
            if (drawData != null)
            {
                this.Lights = drawData.Lights;
            }
        }
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            if (this.DrawingData != null && this.DrawingData.SkinningData != null)
            {
                this.AnimationController.Update(context.GameTime.ElapsedSeconds, this.DrawingData.SkinningData);

                this.AnimationIndex = this.AnimationController.GetAnimationOffset(this.DrawingData.SkinningData);
                this.InvalidateCache();
            }

            this.ManipulatorChanged = false;

            this.Manipulator.Update(context.GameTime);

            if (this.ManipulatorChanged)
            {
                if (this.Lights != null && this.Lights.Length > 0)
                {
                    for (int i = 0; i < this.Lights.Length; i++)
                    {
                        this.Lights[i].ParentTransform = this.Manipulator.LocalTransform;
                    }
                }
            }
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            int count = 0;
            int instanceCount = 0;

            if (this.DrawingData != null)
            {
                instanceCount++;

                Drawer effect = null;
                if (context.DrawerMode == DrawerModesEnum.Forward) effect = DrawerPool.EffectDefaultBasic;
                else if (context.DrawerMode == DrawerModesEnum.Deferred) effect = DrawerPool.EffectDeferredBasic;
                else if (context.DrawerMode == DrawerModesEnum.ShadowMap) effect = DrawerPool.EffectShadowBasic;

                if (effect != null)
                {
                    #region Per frame update

                    if (context.DrawerMode == DrawerModesEnum.Forward)
                    {
                        ((EffectDefaultBasic)effect).UpdatePerFrame(
                            this.Manipulator.LocalTransform * context.World,
                            context.ViewProjection,
                            context.EyePosition,
                            context.Lights,
                            context.ShadowMaps,
                            context.ShadowMapStatic,
                            context.ShadowMapDynamic,
                            context.FromLightViewProjection);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        ((EffectDeferredBasic)effect).UpdatePerFrame(
                            this.Manipulator.LocalTransform * context.World,
                            context.ViewProjection);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        ((EffectShadowBasic)effect).UpdatePerFrame(
                            this.Manipulator.LocalTransform * context.World,
                            context.ViewProjection);
                    }

                    #endregion

                    foreach (string meshName in this.DrawingData.Meshes.Keys)
                    {
                        var dictionary = this.DrawingData.Meshes[meshName];

                        foreach (string material in dictionary.Keys)
                        {
                            #region Per object update

                            var mat = this.DrawingData.Materials[material];

                            if (context.DrawerMode == DrawerModesEnum.Forward)
                            {
                                ((EffectDefaultBasic)effect).UpdatePerObject(
                                    mat.DiffuseTexture,
                                    mat.NormalMap,
                                    mat.SpecularTexture,
                                    mat.ResourceIndex,
                                    this.TextureIndex,
                                    this.AnimationIndex);
                            }
                            else if (context.DrawerMode == DrawerModesEnum.Deferred)
                            {
                                ((EffectDeferredBasic)effect).UpdatePerObject(
                                    mat.DiffuseTexture,
                                    mat.NormalMap,
                                    mat.SpecularTexture,
                                    mat.ResourceIndex,
                                    this.TextureIndex,
                                    this.AnimationIndex);
                            }
                            else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                            {
                                ((EffectShadowBasic)effect).UpdatePerObject(
                                    this.TextureIndex,
                                    this.AnimationIndex);
                            }

                            #endregion

                            var mesh = dictionary[material];
                            this.BufferManager.SetIndexBuffer(mesh.IndexBuffer.Slot);

                            var technique = effect.GetTechnique(mesh.VertextType, mesh.Instanced, DrawingStages.Drawing, context.DrawerMode);
                            this.BufferManager.SetInputAssembler(technique, mesh.VertexBuffer.Slot, mesh.Topology);

                            count += mesh.IndexBuffer.Count > 0 ? mesh.IndexBuffer.Count / 3 : mesh.VertexBuffer.Count / 3;

                            for (int p = 0; p < technique.Description.PassCount; p++)
                            {
                                technique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                                mesh.Draw(this.Game.Graphics);

                                Counters.DrawCallsPerFrame++;
                            }
                        }
                    }
                }
            }

            if (context.DrawerMode != DrawerModesEnum.ShadowMap)
            {
                Counters.InstancesPerFrame += instanceCount;
                Counters.PrimitivesPerFrame += count;
            }
        }
        /// <summary>
        /// Culling
        /// </summary>
        /// <param name="frustum">Frustum</param>
        public override void Culling(BoundingFrustum frustum)
        {
            if (this.hasVolumes)
            {
                if (this.SphericVolume)
                {
                    this.Cull = frustum.Contains(this.GetBoundingSphere()) == ContainmentType.Disjoint;
                }
                else
                {
                    this.Cull = frustum.Contains(this.GetBoundingBox()) == ContainmentType.Disjoint;
                }
            }
            else
            {
                this.Cull = false;
            }

            if (!this.Cull)
            {
                var pars = frustum.GetCameraParams();

                this.SetLOD(pars.Position);
            }
        }
        /// <summary>
        /// Culling
        /// </summary>
        /// <param name="sphere">Sphere</param>
        public override void Culling(BoundingSphere sphere)
        {
            if (this.hasVolumes)
            {
                if (this.SphericVolume)
                {
                    this.Cull = this.GetBoundingSphere().Contains(ref sphere) == ContainmentType.Disjoint;
                }
                else
                {
                    this.Cull = this.GetBoundingBox().Contains(ref sphere) == ContainmentType.Disjoint;
                }
            }
            else
            {
                this.Cull = false;
            }

            if (!this.Cull)
            {
                this.SetLOD(sphere.Center);
            }
        }
        /// <summary>
        /// Set level of detail values
        /// </summary>
        /// <param name="origin">Origin point</param>
        private void SetLOD(Vector3 origin)
        {
            var dist = Vector3.Distance(this.Manipulator.Position, origin) - this.GetBoundingSphere().Radius;
            if (dist < GameEnvironment.LODDistanceHigh)
            {
                this.LevelOfDetail = LevelOfDetailEnum.High;
            }
            else if (dist < GameEnvironment.LODDistanceMedium)
            {
                this.LevelOfDetail = LevelOfDetailEnum.Medium;
            }
            else if (dist < GameEnvironment.LODDistanceLow)
            {
                this.LevelOfDetail = LevelOfDetailEnum.Low;
            }
            else if (dist < GameEnvironment.LODDistanceMinimum)
            {
                this.LevelOfDetail = LevelOfDetailEnum.Minimum;
            }
            else
            {
                this.LevelOfDetail = LevelOfDetailEnum.None;
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
            this.InvalidateCache();

            this.boundingSphere = new BoundingSphere();
            this.boundingBox = new BoundingBox();

            if (this.Lights != null && this.Lights.Length > 0)
            {
                for (int i = 0; i < this.Lights.Length; i++)
                {
                    this.Lights[i].ParentTransform = this.Manipulator.LocalTransform;
                }
            }

            this.ManipulatorChanged = true;
        }

        /// <summary>
        /// Invalidates the internal caché
        /// </summary>
        private void InvalidateCache()
        {
            this.updatePoints = true;
            this.updateTriangles = true;

            this.orientedBoundingBox = new OrientedBoundingBox();
        }
        /// <summary>
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or position list</returns>
        public Vector3[] GetPoints(bool refresh = false)
        {
            if (refresh || this.updatePoints)
            {
                var drawingData = this.GetDrawingData(this.GetLODMinimum());
                if (drawingData.SkinningData != null && this.AnimationController.Playing)
                {
                    this.positionCache = drawingData.GetPoints(
                        this.Manipulator.LocalTransform,
                        this.AnimationController.GetCurrentPose(drawingData.SkinningData),
                        true);
                }
                else
                {
                    this.positionCache = drawingData.GetPoints(this.Manipulator.LocalTransform);
                }

                this.updatePoints = false;
            }

            return this.positionCache;
        }
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or triangle list</returns>
        public Triangle[] GetTriangles(bool refresh = false)
        {
            if (refresh || this.updateTriangles)
            {
                var drawingData = this.GetDrawingData(this.GetLODMinimum());
                if (drawingData.SkinningData != null && this.AnimationController.Playing)
                {
                    this.triangleCache = drawingData.GetTriangles(
                        this.Manipulator.LocalTransform,
                        this.AnimationController.GetCurrentPose(drawingData.SkinningData),
                        refresh);
                }
                else
                {
                    this.triangleCache = drawingData.GetTriangles(this.Manipulator.LocalTransform);
                }

                this.updateTriangles = false;
            }

            return this.triangleCache;
        }
        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public BoundingSphere GetBoundingSphere(bool refresh = false)
        {
            if (refresh || this.boundingSphere == new BoundingSphere())
            {
                var positions = this.GetPoints(refresh);
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
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        public BoundingBox GetBoundingBox(bool refresh = false)
        {
            if (refresh || this.boundingBox == new BoundingBox())
            {
                var positions = this.GetPoints(refresh);
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
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns oriented bounding box with identity transformation. Empty if the vertex type hasn't position channel</returns>
        public OrientedBoundingBox GetOrientedBoundingBox(bool refresh = false)
        {
            if (refresh || this.orientedBoundingBox == new OrientedBoundingBox())
            {
                var positions = this.GetPoints(refresh);
                if (positions != null && positions.Length > 0)
                {
                    this.orientedBoundingBox = new OrientedBoundingBox(positions);
                    this.orientedBoundingBox.Transform(Matrix.Identity);
                }
            }

            return this.orientedBoundingBox;
        }
        /// <summary>
        /// Gets internal volume
        /// </summary>
        /// <returns>Returns interna volume</returns>
        public Triangle[] GetVolume()
        {
            return this.DrawingData.VolumeMesh;
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
        public bool PickNearest(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            position = new Vector3();
            triangle = new Triangle();
            distance = float.MaxValue;

            BoundingSphere bsph = this.GetBoundingSphere();
            if (bsph.Intersects(ref ray))
            {
                var triangles = this.GetTriangles();

                Vector3 pos;
                Triangle tri;
                float d;
                if (Intersection.IntersectNearest(ref ray, triangles, facingOnly, out pos, out tri, out d))
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
        public bool PickFirst(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            position = new Vector3();
            triangle = new Triangle();
            distance = float.MaxValue;

            BoundingSphere bsph = this.GetBoundingSphere();
            if (bsph.Intersects(ref ray))
            {
                var triangles = this.GetTriangles();

                Vector3 pos;
                Triangle tri;
                float d;
                if (Intersection.IntersectFirst(ref ray, triangles, facingOnly, out pos, out tri, out d))
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
        public bool PickAll(ref Ray ray, bool facingOnly, out Vector3[] positions, out Triangle[] triangles, out float[] distances)
        {
            positions = null;
            triangles = null;
            distances = null;

            BoundingSphere bsph = this.GetBoundingSphere();
            if (bsph.Intersects(ref ray))
            {
                var tris = this.GetTriangles();

                Vector3[] pos;
                Triangle[] tri;
                float[] ds;
                if (Intersection.IntersectAll(ref ray, tris, facingOnly, out pos, out tri, out ds))
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
