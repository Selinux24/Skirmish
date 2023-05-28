using SharpDX;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Collider helper class
    /// </summary>
    class BoundsHelper<T> where T : IRayIntersectable
    {
        private readonly IRayPickable<T> model;

        /// <summary>
        /// Complete caché rebuild flag
        /// </summary>
        private bool rebuild;
        /// <summary>
        /// Initial bounding sphere
        /// </summary>
        private BoundingSphere initialSphere;
        /// <summary>
        /// Transformed bounding sphere
        /// </summary>
        private BoundingSphere boundingSphere;
        /// <summary>
        /// Update bounding sphere flag
        /// </summary>
        private bool updateBoundingSphere;
        /// <summary>
        /// Initial bounding box
        /// </summary>
        private BoundingBox initialAabb;
        /// <summary>
        /// Transformed bounding box
        /// </summary>
        private BoundingBox boundingBox;
        /// <summary>
        /// Update bounding box flag
        /// </summary>
        private bool updateBoundingBox;
        /// <summary>
        /// Initial oriented bounding box
        /// </summary>
        private OrientedBoundingBox initialObb;
        /// <summary>
        /// Transformed bounding box
        /// </summary>
        private OrientedBoundingBox orientedBox;
        /// <summary>
        /// Update bounding box flag
        /// </summary>
        private bool updateOrientedBox;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">Model</param>
        public BoundsHelper(IRayPickable<T> model)
        {
            this.model = model;
        }

        /// <summary>
        /// Initializes internal volumes
        /// </summary>
        /// <param name="points">Point list</param>
        public void Initialize()
        {
            UpdateInitialBoundingVolumes();

            boundingSphere = initialSphere;
            boundingBox = initialAabb;
            orientedBox = initialObb;

            updateBoundingSphere = false;
            updateBoundingBox = false;
            updateOrientedBox = false;
        }
        /// <summary>
        /// Updates initial bounding volume structures
        /// </summary>
        private void UpdateInitialBoundingVolumes()
        {
            rebuild = false;

            var points = model.GetPoints();

            if (points.Any())
            {
                var distinctPoints = points.Distinct().ToArray();

                //Initialize the identity sphere
                initialSphere = BoundingSphere.FromPoints(distinctPoints);

                //Initialize the identity box
                initialAabb = BoundingBox.FromPoints(distinctPoints);

                //Initialize the identity obb
                initialObb = new OrientedBoundingBox(initialAabb);
            }
            else
            {
                initialSphere = new BoundingSphere();

                initialAabb = new BoundingBox();

                initialObb = new OrientedBoundingBox();
            }
        }
        /// <summary>
        /// Invalidates the internal state
        /// </summary>
        public void Invalidate(bool rebuild)
        {
            this.rebuild = rebuild;

            updateBoundingBox = true;
            updateBoundingSphere = true;
            updateOrientedBox = true;
        }

        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        /// <param name="refresh">Sets if the cache must be refreshed or not</param>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public BoundingSphere GetBoundingSphere(Manipulator3D manipulator, bool refresh = false)
        {
            if (updateBoundingSphere || refresh)
            {
                if (rebuild)
                {
                    UpdateInitialBoundingVolumes();
                }

                boundingSphere = initialSphere.SetTransform(manipulator.FinalTransform);

                updateBoundingSphere = false;
            }

            return boundingSphere;
        }
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        /// <param name="refresh">Sets if the cache must be refreshed or not</param>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        public BoundingBox GetBoundingBox(Manipulator3D manipulator, bool refresh = false)
        {
            if (updateBoundingBox || refresh)
            {
                if (rebuild)
                {
                    UpdateInitialBoundingVolumes();
                }

                boundingBox = initialAabb.SetTransform(manipulator.FinalTransform);

                updateBoundingBox = false;
            }

            return boundingBox;
        }
        /// <summary>
        /// Gets oriented bounding box
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        /// <param name="refresh">Sets if the cache must be refreshed or not</param>
        /// <returns>Returns oriented bounding box. Empty if the vertex type hasn't position channel</returns>
        public OrientedBoundingBox GetOrientedBoundingBox(Manipulator3D manipulator, bool refresh = false)
        {
            if (updateOrientedBox || refresh)
            {
                if (rebuild)
                {
                    UpdateInitialBoundingVolumes();
                }

                orientedBox = initialObb.SetTransform(manipulator.FinalTransform);

                updateOrientedBox = false;
            }

            return orientedBox;
        }

        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        /// <param name="volumeType">Culling volume type</param>
        /// <param name="volume">Culling volume</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the object is outside of the frustum</returns>
        public bool Cull(Manipulator3D manipulator, CullingVolumeTypes volumeType, ICullingVolume volume, out float distance)
        {
            distance = float.MaxValue;

            return volumeType switch
            {
                CullingVolumeTypes.None => false,
                CullingVolumeTypes.SphericVolume => CullBoundingSphere(manipulator, volume, out distance),
                CullingVolumeTypes.BoxVolume => CullBoundingBox(manipulator, volume, out distance),
                _ => false,
            };
        }
        /// <summary>
        /// Performs culling test against the spheric volume
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        /// <param name="volume">Culling volume</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the object is outside of the frustum</returns>
        public bool CullBoundingSphere(Manipulator3D manipulator, ICullingVolume volume, out float distance)
        {
            distance = float.MaxValue;

            bool cull = volume.Contains(GetBoundingSphere(manipulator)) == ContainmentType.Disjoint;
            if (!cull)
            {
                var eyePosition = volume.Position;

                distance = Vector3.DistanceSquared(manipulator.Position, eyePosition);
            }

            return cull;
        }
        /// <summary>
        /// Performs culling test against the box volume
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        /// <param name="volume">Culling volume</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the object is outside of the frustum</returns>
        public bool CullBoundingBox(Manipulator3D manipulator, ICullingVolume volume, out float distance)
        {
            distance = float.MaxValue;

            bool cull = volume.Contains(GetBoundingBox(manipulator)) == ContainmentType.Disjoint;
            if (!cull)
            {
                var eyePosition = volume.Position;

                distance = Vector3.DistanceSquared(manipulator.Position, eyePosition);
            }

            return cull;
        }
    }
}
