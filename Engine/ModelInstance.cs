using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.Animation;
    using Engine.Common;

    /// <summary>
    /// Model instance
    /// </summary>
    public class ModelInstance : ITransformable3D, IRayPickable<Triangle>, ICullable
    {
        /// <summary>
        /// Global id counter
        /// </summary>
        private static int InstanceId = 0;
        /// <summary>
        /// Gets the next instance Id
        /// </summary>
        /// <returns>Returns the next instance Id</returns>
        private static int GetNextInstanceId()
        {
            return ++InstanceId;
        }

        /// <summary>
        /// Model
        /// </summary>
        private readonly BaseModel model = null;
        /// <summary>
        /// Update point cache flag
        /// </summary>
        private bool updatePoints = true;
        /// <summary>
        /// Update triangle cache flag
        /// </summary>
        private bool updateTriangles = true;
        /// <summary>
        /// Points cache
        /// </summary>
        private Vector3[] positionCache = null;
        /// <summary>
        /// Triangle list cache
        /// </summary>
        private Triangle[] triangleCache = null;
        /// <summary>
        /// Coarse bounding sphere
        /// </summary>
        private BoundingSphere? coarseBoundingSphere = null;
        /// <summary>
        /// Bounding sphere
        /// </summary>
        private BoundingSphere? boundingSphere = null;
        /// <summary>
        /// Bounding box
        /// </summary>
        private BoundingBox? boundingBox = null;
        /// <summary>
        /// Level of detail
        /// </summary>
        private LevelOfDetail levelOfDetail = LevelOfDetail.High;

        /// <summary>
        /// Gets if model has volumes
        /// </summary>
        protected bool HasVolumes
        {
            get
            {
                return this.positionCache?.Any() == true;
            }
        }

        /// <summary>
        /// Instance id
        /// </summary>
        public readonly int Id;
        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; private set; }
        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex { get; set; } = 0;
        /// <summary>
        /// Active
        /// </summary>
        public bool Active { get; set; } = true;
        /// <summary>
        /// Visible
        /// </summary>
        public bool Visible { get; set; } = true;
        /// <summary>
        /// Instance level of detail
        /// </summary>
        public LevelOfDetail LevelOfDetail
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
        public AnimationController AnimationController { get; set; } = new AnimationController();
        /// <summary>
        /// Gets the current instance lights collection
        /// </summary>
        public IEnumerable<ISceneLight> Lights { get; protected set; } = new ISceneLight[] { };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">Model</param>
        public ModelInstance(BaseModel model)
        {
            this.Id = GetNextInstanceId();
            this.model = model;

            this.Manipulator = new Manipulator3D();
            this.Manipulator.Updated += new EventHandler(ManipulatorUpdated);

            var drawData = model.GetDrawingData(LevelOfDetail.High);
            this.Lights = drawData?.Lights.Select(l => l.Clone()).ToArray() ?? new ISceneLight[] { };
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="context">Context</param>
        public virtual void Update(UpdateContext context)
        {
            this.Manipulator.Update(context.GameTime);

            if (this.Lights.Any())
            {
                foreach (var light in this.Lights)
                {
                    light.ParentTransform = this.Manipulator.LocalTransform;
                }
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

            this.coarseBoundingSphere = this.GetBoundingSphere();
        }

        /// <summary>
        /// Invalidates the internal cache
        /// </summary>
        public void InvalidateCache()
        {
            this.updatePoints = true;
            this.updateTriangles = true;

            this.boundingSphere = null;
            this.boundingBox = null;
        }
        /// <summary>
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or position list</returns>
        public IEnumerable<Vector3> GetPoints(bool refresh = false)
        {
            if (refresh || this.updatePoints)
            {
                var drawingData = this.model.GetDrawingData(this.model.GetLODMinimum());
                if (drawingData == null)
                {
                    return new Vector3[] { };
                }

                IEnumerable<Vector3> cache;

                if (drawingData.SkinningData != null)
                {
                    cache = drawingData.GetPoints(
                        this.Manipulator.LocalTransform,
                        this.AnimationController.GetCurrentPose(drawingData.SkinningData),
                        refresh);
                }
                else
                {
                    cache = drawingData.GetPoints(
                        this.Manipulator.LocalTransform,
                        refresh);
                }

                this.positionCache = cache.ToArray();

                this.updatePoints = false;
            }

            return this.positionCache.ToArray() ?? new Vector3[] { };
        }
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or triangle list</returns>
        public IEnumerable<Triangle> GetTriangles(bool refresh = false)
        {
            if (refresh || this.updateTriangles)
            {
                var drawingData = this.model.GetDrawingData(this.model.GetLODMinimum());
                if (drawingData == null)
                {
                    return new Triangle[] { };
                }

                IEnumerable<Triangle> cache;

                if (drawingData.SkinningData != null)
                {
                    cache = drawingData.GetTriangles(
                        this.Manipulator.LocalTransform,
                        this.AnimationController.GetCurrentPose(drawingData.SkinningData),
                        refresh);
                }
                else
                {
                    cache = drawingData.GetTriangles(
                        this.Manipulator.LocalTransform,
                        refresh);
                }

                this.triangleCache = cache.ToArray();

                this.updateTriangles = false;
            }

            return this.triangleCache.ToArray() ?? new Triangle[] { };
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
            if (refresh || this.boundingSphere == null)
            {
                var points = this.GetPoints(refresh);
                if (points.Any())
                {
                    this.boundingSphere = BoundingSphere.FromPoints(points.ToArray());
                }
            }

            return this.boundingSphere ?? new BoundingSphere(this.Manipulator.Position, 0);
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
            if (refresh || this.boundingBox == null)
            {
                var points = this.GetPoints(refresh);
                if (points.Any())
                {
                    this.boundingBox = BoundingBox.FromPoints(points.ToArray());
                }
            }

            return this.boundingBox ?? new BoundingBox(this.Manipulator.Position, this.Manipulator.Position);
        }

        /// <summary>
        /// Gets nearest picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public virtual bool PickNearest(Ray ray, out PickingResult<Triangle> result)
        {
            return PickNearest(ray, RayPickingParams.Default, out result);
        }
        /// <summary>
        /// Gets nearest picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking params</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public virtual bool PickNearest(Ray ray, RayPickingParams rayPickingParams, out PickingResult<Triangle> result)
        {
            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            var bsph = this.GetBoundingSphere();
            if (bsph.Intersects(ref ray))
            {
                bool facingOnly = rayPickingParams.HasFlag(RayPickingParams.FacingOnly);
                var triangles = this.GetVolume(rayPickingParams.HasFlag(RayPickingParams.Geometry));

                if (triangles.Any() && Intersection.IntersectNearest(ray, triangles, facingOnly, out Vector3 p, out Triangle t, out float d))
                {
                    result.Position = p;
                    result.Item = t;
                    result.Distance = d;

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets first picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public virtual bool PickFirst(Ray ray, out PickingResult<Triangle> result)
        {
            return PickFirst(ray, RayPickingParams.Default, out result);
        }
        /// <summary>
        /// Gets first picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking params</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public virtual bool PickFirst(Ray ray, RayPickingParams rayPickingParams, out PickingResult<Triangle> result)
        {
            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            var bsph = this.GetBoundingSphere();
            if (bsph.Intersects(ref ray))
            {
                bool facingOnly = rayPickingParams.HasFlag(RayPickingParams.FacingOnly);
                var triangles = this.GetVolume(rayPickingParams.HasFlag(RayPickingParams.Geometry));

                if (triangles.Any() && Intersection.IntersectFirst(ray, triangles, facingOnly, out Vector3 p, out Triangle t, out float d))
                {
                    result.Position = p;
                    result.Item = t;
                    result.Distance = d;

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Get all picking positions of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if ground position found</returns>
        public virtual bool PickAll(Ray ray, out PickingResult<Triangle>[] results)
        {
            return PickAll(ray, RayPickingParams.Default, out results);
        }
        /// <summary>
        /// Get all picking positions of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking params</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if ground position found</returns>
        public virtual bool PickAll(Ray ray, RayPickingParams rayPickingParams, out PickingResult<Triangle>[] results)
        {
            results = null;

            var bsph = this.GetBoundingSphere();
            if (bsph.Intersects(ref ray))
            {
                bool facingOnly = rayPickingParams.HasFlag(RayPickingParams.FacingOnly);
                var triangles = this.GetVolume(rayPickingParams.HasFlag(RayPickingParams.Geometry));

                if (triangles.Any() && Intersection.IntersectAll(ray, triangles, facingOnly, out Vector3[] p, out Triangle[] t, out float[] d))
                {
                    results = new PickingResult<Triangle>[p.Length];
                    for (int i = 0; i < results.Length; i++)
                    {
                        results[i] = new PickingResult<Triangle>()
                        {
                            Position = p[i],
                            Item = t[i],
                            Distance = d[i],
                        };
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="volume">Culling volume</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the object is outside of the frustum</returns>
        public virtual bool Cull(ICullingVolume volume, out float distance)
        {
            bool cull;
            distance = float.MaxValue;

            if (this.HasVolumes)
            {
                if (this.coarseBoundingSphere.HasValue)
                {
                    cull = volume.Contains(this.coarseBoundingSphere.Value) == ContainmentType.Disjoint;
                }
                else if (this.model.SphericVolume)
                {
                    cull = volume.Contains(this.GetBoundingSphere(false)) == ContainmentType.Disjoint;
                }
                else
                {
                    cull = volume.Contains(this.GetBoundingBox(false)) == ContainmentType.Disjoint;
                }
            }
            else
            {
                cull = false;
            }

            if (!cull)
            {
                var eyePosition = volume.Position;

                distance = Vector3.DistanceSquared(this.Manipulator.Position, eyePosition);
            }

            return cull;
        }
        /// <summary>
        /// Set level of detail values
        /// </summary>
        /// <param name="origin">Origin point</param>
        public void SetLOD(Vector3 origin)
        {
            this.LevelOfDetail = GameEnvironment.GetLOD(
                origin,
                this.coarseBoundingSphere,
                this.Manipulator.LocalTransform);
        }

        /// <summary>
        /// Gets internal volume
        /// </summary>
        /// <param name="full"></param>
        /// <returns>Returns internal volume</returns>
        public IEnumerable<Triangle> GetVolume(bool full)
        {
            var drawingData = this.model.GetDrawingData(this.model.GetLODMinimum());
            if (!full && drawingData?.VolumeMesh?.Any() == true)
            {
                //Transforms the volume mesh
                return Triangle.Transform(drawingData.VolumeMesh, this.Manipulator.LocalTransform);
            }

            //Returns the actual triangles (yet transformed)
            return this.GetTriangles(true);
        }

        /// <summary>
        /// Gets the text representation of the current instance
        /// </summary>
        /// <returns>Returns the text representation of the current instance</returns>
        public override string ToString()
        {
            return string.Format(
                "Id: {0}; LOD: {1}; Active: {2}; Visible: {3}",
                this.Id,
                this.LevelOfDetail,
                this.Active,
                this.Visible);
        }
    }
}
