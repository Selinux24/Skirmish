using System;
using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Model instance
    /// </summary>
    public class ModelInstance
    {
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
                Vector3[] positions = this.GetPoints();

                return positions != null && positions.Length > 0;
            }
        }

        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; private set; }
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
        public bool Cull = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">Model</param>
        public ModelInstance(ModelBase model)
        {
            this.model = model;
            this.Manipulator = new Manipulator3D();
            this.Manipulator.Updated += new System.EventHandler(ManipulatorUpdated);
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
                this.positionCache = this.model.GetPoints(this.Manipulator.LocalTransform);

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
                this.triangleCache = this.model.GetTriangles(this.Manipulator.LocalTransform);

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
        /// Gets picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <returns>Returns true if ground position found</returns>
        public virtual bool Pick(ref Ray ray, out Vector3 position, out Triangle triangle)
        {
            position = new Vector3();
            triangle = new Triangle();

            BoundingSphere bsph = this.GetBoundingSphere();

            if (bsph.Intersects(ref ray))
            {
                Triangle[] triangles = this.GetTriangles();
                if (triangles != null && triangles.Length > 0)
                {
                    for (int i = 0; i < triangles.Length; i++)
                    {
                        Triangle tri = triangles[i];

                        Vector3 pos;
                        if (tri.Intersects(ref ray, out pos))
                        {
                            position = pos;
                            triangle = tri;

                            return true;
                        }
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Performs frustum culling test
        /// </summary>
        /// <param name="frustum">Frustum</param>
        public virtual void FrustumCulling(BoundingFrustum frustum)
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
    }
}
