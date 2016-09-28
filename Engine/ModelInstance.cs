using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Animation;
    using Engine.Common;

    /// <summary>
    /// Model instance
    /// </summary>
    public class ModelInstance : IPickable
    {
        /// <summary>
        /// Global id counter
        /// </summary>
        private static int InstanceId = 0;

        /// <summary>
        /// Model
        /// </summary>
        private ModelBase model = null;
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
                return this.positionCache != null && this.positionCache.Length > 0;
            }
        }
        /// <summary>
        /// Level of detail
        /// </summary>
        private LevelOfDetailEnum levelOfDetail = LevelOfDetailEnum.None;

        /// <summary>
        /// Instance id
        /// </summary>
        public readonly int Id;
        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; private set; }
        /// <summary>
        /// Manipulator has changed last frame
        /// </summary>
        public bool ManipulatorChanged { get; private set; }
        /// <summary>
        /// Texture index
        /// </summary>
        public int TextureIndex = 0;
        /// <summary>
        /// Active
        /// </summary>
        public bool Active = true;
        /// <summary>
        /// Visible
        /// </summary>
        public bool Visible = true;
        /// <summary>
        /// Culling test flag
        /// </summary>
        public bool Cull { get; private set; }
        /// <summary>
        /// Instance level of detail
        /// </summary>
        public LevelOfDetailEnum LevelOfDetail
        {
            get
            {
                return this.levelOfDetail;
            }
            set
            {
                this.levelOfDetail = this.model.GetLODNearest(value);
            }
        }
        /// <summary>
        /// Animation controller
        /// </summary>
        public AnimationController AnimationController = new AnimationController();
        /// <summary>
        /// Do model animations using manipulator changes only
        /// </summary>
        public bool AnimateWithManipulator { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">Model</param>
        public ModelInstance(ModelBase model)
        {
            this.Id = ++InstanceId;
            this.model = model;
            this.Manipulator = new Manipulator3D();
            this.Manipulator.Updated += new EventHandler(ManipulatorUpdated);
            this.Cull = false;
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="context">Context</param>
        public virtual void Update(UpdateContext context)
        {
            this.ManipulatorChanged = false;

            this.Manipulator.Update(context.GameTime);
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

            this.ManipulatorChanged = true;
        }

        /// <summary>
        /// Invalidates the internal caché
        /// </summary>
        public void InvalidateCache()
        {
            this.updatePoints = true;
            this.updateTriangles = true;

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
                var drawingData = this.model.GetDrawingData(this.model.GetLODMinimum());
                if (drawingData.SkinningData != null)
                {
                    this.positionCache = drawingData.GetPoints(this.Manipulator.LocalTransform, drawingData.SkinningData.GetFinalTransforms());
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
        /// <returns>Returns null or triangle list</returns>
        public Triangle[] GetTriangles()
        {
            if (this.updateTriangles)
            {
                var drawingData = this.model.GetDrawingData(this.model.GetLODMinimum());
                if (drawingData.SkinningData != null)
                {
                    this.triangleCache = drawingData.GetTriangles(this.Manipulator.LocalTransform, drawingData.SkinningData.GetFinalTransforms());
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
                if (triangles != null && triangles.Length > 0)
                {
                    Vector3 p;
                    Triangle t;
                    float d;
                    if (Triangle.IntersectNearest(ref ray, triangles, facingOnly, out p, out t, out d))
                    {
                        position = p;
                        triangle = t;
                        distance = d;

                        return true;
                    }
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
                if (triangles != null && triangles.Length > 0)
                {
                    Vector3 p;
                    Triangle t;
                    float d;
                    if (Triangle.IntersectFirst(ref ray, triangles, facingOnly, out p, out t, out d))
                    {
                        position = p;
                        triangle = t;
                        distance = d;

                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Get all picking positions of giving ray
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
                Triangle[] ts = this.GetTriangles();
                if (ts != null && ts.Length > 0)
                {
                    Vector3[] p;
                    Triangle[] t;
                    float[] d;
                    if (Triangle.IntersectAll(ref ray, ts, facingOnly, out p, out t, out d))
                    {
                        positions = p;
                        triangles = t;
                        distances = d;

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="frustum">Frustum</param>
        public virtual void Culling(BoundingFrustum frustum)
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
        /// Performs culling test
        /// </summary>
        /// <param name="sphere">Sphere</param>
        public virtual void Culling(BoundingSphere sphere)
        {
            if (this.hasVolumes)
            {
                this.Cull = this.GetBoundingSphere().Contains(ref sphere) == ContainmentType.Disjoint;
            }
            else
            {
                this.Cull = false;
            }

            if (!this.Cull)
            {
                var dist = Vector3.DistanceSquared(this.Manipulator.Position, sphere.Center);
                if (dist < 100f) { this.LevelOfDetail = LevelOfDetailEnum.High; }
                else if (dist < 400f) { this.LevelOfDetail = LevelOfDetailEnum.Medium; }
                else if (dist < 1600f) { this.LevelOfDetail = LevelOfDetailEnum.Low; }
                else { this.LevelOfDetail = LevelOfDetailEnum.Minimum; }
            }
        }
        /// <summary>
        /// Sets cull value
        /// </summary>
        /// <param name="value">New value</param>
        public virtual void SetCulling(bool value)
        {
            this.Cull = value;
        }
    }
}
