using SharpDX;
using System;
using System.Collections.Generic;
using PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology;

namespace Engine
{
    using Engine.Animation;
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Basic Model
    /// </summary>
    public class Model : ModelBase, ITransformable3D, IRayPickable<Triangle>, ICullable
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
        /// Coarse bounding sphere
        /// </summary>
        private BoundingSphere coarseBoundingSphere;
        /// <summary>
        /// Bounding sphere
        /// </summary>
        private BoundingSphere boundingSphere;
        /// <summary>
        /// Bounding box
        /// </summary>
        private BoundingBox boundingBox;
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
        /// Model parts collection
        /// </summary>
        protected List<ModelPart> ModelParts = new List<ModelPart>();

        /// <summary>
        /// Model manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; private set; }
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
        /// Gets the model part by name
        /// </summary>
        /// <param name="name">Part name</param>
        /// <returns>Returns the model part name</returns>
        public ModelPart this[string name]
        {
            get
            {
                return this.ModelParts.Find(p => p.Name == name);
            }
        }
        /// <summary>
        /// Gets the model part count
        /// </summary>
        public int ModelPartCount
        {
            get
            {
                return this.ModelParts.Count;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public Model(Scene scene, ModelDescription description)
            : base(scene, description)
        {
            this.TextureIndex = description.TextureIndex;

            if (description.TransformDependences != null && description.TransformDependences.Length > 0)
            {
                for (int i = 0; i < description.TransformNames.Length; i++)
                {
                    this.ModelParts.Add(new ModelPart(description.TransformNames[i]));
                }

                for (int i = 0; i < description.TransformNames.Length; i++)
                {
                    var thisName = description.TransformNames[i];
                    var thisMan = this[thisName].Manipulator;
                    thisMan.Updated += new EventHandler(ManipulatorUpdated);

                    var parentIndex = description.TransformDependences[i];
                    if (parentIndex >= 0)
                    {
                        var parentName = description.TransformNames[parentIndex];

                        thisMan.Parent = this[parentName].Manipulator;
                    }
                    else
                    {
                        this.Manipulator = thisMan;
                    }
                }
            }
            else
            {
                this.Manipulator = new Manipulator3D();
                this.Manipulator.Updated += new EventHandler(ManipulatorUpdated);
            }

            var drawData = this.GetDrawingData(LevelOfDetailEnum.High);
            if (drawData != null)
            {
                this.coarseBoundingSphere = BoundingSphere.FromPoints(drawData.GetPoints(true));

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

            if (this.ModelParts.Count > 0)
            {
                this.ModelParts.ForEach(p => p.Manipulator.Update(context.GameTime));
            }
            else
            {
                this.Manipulator.Update(context.GameTime);
            }

            if (this.Lights != null && this.Lights.Length > 0)
            {
                for (int i = 0; i < this.Lights.Length; i++)
                {
                    this.Lights[i].ParentTransform = this.Manipulator.LocalTransform;
                }
            }
        }
        /// <summary>
        /// Draw shadows
        /// </summary>
        /// <param name="context"></param>
        public override void DrawShadows(DrawContextShadows context)
        {
            if (this.DrawingData != null)
            {
                int count = 0;
                int instanceCount = 0;

                instanceCount++;

                IShadowMapDrawer effect = null;

                if (context.ShadowMap is ShadowMap)
                {
                    effect = DrawerPool.EffectShadowBasic;
                }
                else if (context.ShadowMap is CubicShadowMap)
                {
                    effect = DrawerPool.EffectShadowPoint;
                }

                if (effect != null)
                {
                    var graphics = this.Game.Graphics;

                    foreach (string meshName in this.DrawingData.Meshes.Keys)
                    {
                        var dictionary = this.DrawingData.Meshes[meshName];

                        var localTransform = this.GetTransformByName(meshName);

                        effect.UpdatePerFrame(localTransform, context);

                        foreach (string material in dictionary.Keys)
                        {
                            var mesh = dictionary[material];
                            bool transparent = mesh.Transparent && this.Description.AlphaEnabled;

                            var mat = this.DrawingData.Materials[material];

                            effect.UpdatePerObject(this.AnimationIndex, mat, this.TextureIndex);

                            this.BufferManager.SetIndexBuffer(mesh.IndexBuffer.Slot);

                            var technique = effect.GetTechnique(mesh.VertextType, mesh.Instanced, mesh.Transparent);
                            this.BufferManager.SetInputAssembler(technique, mesh.VertexBuffer.Slot, mesh.Topology);

                            count += mesh.IndexBuffer.Count > 0 ? mesh.IndexBuffer.Count / 3 : mesh.VertexBuffer.Count / 3;

                            for (int p = 0; p < technique.PassCount; p++)
                            {
                                graphics.EffectPassApply(technique, p, 0);

                                mesh.Draw(graphics);
                            }
                        }
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
            if (this.DrawingData != null)
            {
                int count = 0;
                int instanceCount = 0;

                instanceCount++;

                var mode = context.DrawerMode;

                IGeometryDrawer effect = null;

                if (mode.HasFlag(DrawerModesEnum.Forward))
                {
                    effect = DrawerPool.EffectDefaultBasic;
                }
                else if (mode.HasFlag(DrawerModesEnum.Deferred))
                {
                    effect = DrawerPool.EffectDeferredBasic;
                }

                if (effect != null)
                {
                    var graphics = this.Game.Graphics;

                    foreach (string meshName in this.DrawingData.Meshes.Keys)
                    {
                        var dictionary = this.DrawingData.Meshes[meshName];

                        var localTransform = this.GetTransformByName(meshName);

                        effect.UpdatePerFrameFull(localTransform, context);

                        foreach (string material in dictionary.Keys)
                        {
                            var mesh = dictionary[material];
                            bool transparent = mesh.Transparent && this.Description.AlphaEnabled;

                            if (mode.HasFlag(DrawerModesEnum.OpaqueOnly) && transparent)
                            {
                                continue;
                            }
                            if (mode.HasFlag(DrawerModesEnum.TransparentOnly) && !transparent)
                            {
                                continue;
                            }

                            effect.UpdatePerObject(
                                this.AnimationIndex,
                                this.DrawingData.Materials[material],
                                this.TextureIndex,
                                this.UseAnisotropicFiltering);

                            this.BufferManager.SetIndexBuffer(mesh.IndexBuffer.Slot);

                            var technique = effect.GetTechnique(mesh.VertextType, mesh.Instanced);
                            this.BufferManager.SetInputAssembler(technique, mesh.VertexBuffer.Slot, mesh.Topology);

                            count += mesh.IndexBuffer.Count > 0 ? mesh.IndexBuffer.Count / 3 : mesh.VertexBuffer.Count / 3;

                            for (int p = 0; p < technique.PassCount; p++)
                            {
                                graphics.EffectPassApply(technique, p, 0);

                                mesh.Draw(graphics);
                            }
                        }
                    }
                }

                Counters.InstancesPerFrame += instanceCount;
                Counters.PrimitivesPerFrame += count;
            }
        }

        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the object is outside of the frustum</returns>
        public override bool Cull(BoundingFrustum frustum, out float? distance)
        {
            bool cull = false;
            distance = null;

            if (this.hasVolumes)
            {
                if (this.SphericVolume)
                {
                    cull = frustum.Contains(this.GetBoundingSphere()) == ContainmentType.Disjoint;
                }
                else
                {
                    cull = frustum.Contains(this.GetBoundingBox()) == ContainmentType.Disjoint;
                }
            }
            else
            {
                cull = false;
            }

            if (!cull)
            {
                var eyePosition = frustum.GetCameraParams().Position;

                distance = Vector3.DistanceSquared(this.Manipulator.Position, eyePosition);

                this.SetLOD(eyePosition);
            }

            return cull;
        }
        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the object is outside of the box</returns>
        public override bool Cull(BoundingBox box, out float? distance)
        {
            bool cull = false;
            distance = null;

            if (this.hasVolumes)
            {
                if (this.SphericVolume)
                {
                    cull = this.GetBoundingBox().Contains(ref box) == ContainmentType.Disjoint;
                }
                else
                {
                    cull = this.GetBoundingBox().Contains(ref box) == ContainmentType.Disjoint;
                }
            }
            else
            {
                cull = false;
            }

            if (!cull)
            {
                var eyePosition = box.GetCenter();

                distance = Vector3.DistanceSquared(this.Manipulator.Position, eyePosition);

                this.SetLOD(eyePosition);
            }

            return cull;
        }
        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the object is outside of the sphere</returns>
        public override bool Cull(BoundingSphere sphere, out float? distance)
        {
            bool cull = false;
            distance = null;

            if (this.hasVolumes)
            {
                if (this.SphericVolume)
                {
                    cull = this.GetBoundingSphere().Contains(ref sphere) == ContainmentType.Disjoint;
                }
                else
                {
                    cull = this.GetBoundingBox().Contains(ref sphere) == ContainmentType.Disjoint;
                }
            }
            else
            {
                cull = false;
            }

            if (!cull)
            {
                distance = Vector3.DistanceSquared(this.Manipulator.Position, sphere.Center);

                this.SetLOD(sphere.Center);
            }

            return cull;
        }
        /// <summary>
        /// Set level of detail values
        /// </summary>
        /// <param name="origin">Origin point</param>
        private void SetLOD(Vector3 origin)
        {
            var position = Vector3.TransformCoordinate(this.coarseBoundingSphere.Center, this.Manipulator.LocalTransform);
            var radius = this.coarseBoundingSphere.Radius * this.Manipulator.AveragingScale;
            var bsph = new BoundingSphere(position, radius);

            var dist = Vector3.Distance(position, origin) - radius;
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
                this.levelOfDetail = LevelOfDetailEnum.None;
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
        }

        /// <summary>
        /// Gets the transform by transform name
        /// </summary>
        /// <param name="name">Transform name</param>
        /// <returns>Retusn the transform of the specified transform name</returns>
        public Matrix GetTransformByName(string name)
        {
            Manipulator3D man = null;

            var part = this.ModelParts.Find(p => p.Name == name);
            if (part != null)
            {
                man = part.Manipulator;
            }
            else
            {
                man = this.Manipulator;
            }

            return man.FinalTransform;
        }

        /// <summary>
        /// Invalidates the internal caché
        /// </summary>
        private void InvalidateCache()
        {
            this.updatePoints = true;
            this.updateTriangles = true;

            this.boundingSphere = new BoundingSphere();
            this.boundingBox = new BoundingBox();
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
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public BoundingSphere GetBoundingSphere()
        {
            return this.GetBoundingSphere(false);
        }
        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public BoundingSphere GetBoundingSphere(bool refresh)
        {
            if (refresh || this.boundingSphere == new BoundingSphere())
            {
                var points = this.GetPoints(refresh);
                if (points != null && points.Length > 0)
                {
                    this.boundingSphere = BoundingSphere.FromPoints(points);
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
            return this.GetBoundingBox(false);
        }
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        public BoundingBox GetBoundingBox(bool refresh)
        {
            if (refresh || this.boundingBox == new BoundingBox())
            {
                var points = this.GetPoints(refresh);
                if (points != null && points.Length > 0)
                {
                    this.boundingBox = BoundingBox.FromPoints(points);
                }
            }

            return this.boundingBox;
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

        /// <summary>
        /// Gets internal volume
        /// </summary>
        /// <param name="full"></param>
        /// <returns>Returns internal volume</returns>
        public Triangle[] GetVolume(bool full)
        {
            if (full)
            {
                return this.GetTriangles(true);
            }
            else
            {
                if (this.DrawingData.VolumeMesh != null)
                {
                    return Triangle.Transform(this.DrawingData.VolumeMesh, this.Manipulator.LocalTransform);
                }
                else
                {
                    //Generate cylinder
                    var cylinder = BoundingCylinder.FromPoints(this.GetPoints());
                    return Triangle.ComputeTriangleList(PrimitiveTopology.TriangleList, cylinder, 8);
                }
            }
        }
    }
}
