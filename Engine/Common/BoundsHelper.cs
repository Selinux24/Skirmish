using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Collider helper class
    /// </summary>
    class BoundsHelper
    {
        /// <summary>
        /// Bounds
        /// </summary>
        private readonly BoundsHelperInitialState bounds;
        /// <summary>
        /// Bounds salt value
        /// </summary>
        private int boundsSalt = 0;

        /// <summary>
        /// Transformed bounding sphere
        /// </summary>
        private BoundingSphere boundingSphere;
        /// <summary>
        /// Update bounding sphere flag
        /// </summary>
        private bool updateBoundingSphere = true;

        /// <summary>
        /// Transformed bounding box
        /// </summary>
        private BoundingBox boundingBox;
        /// <summary>
        /// Update bounding box flag
        /// </summary>
        private bool updateBoundingBox = true;

        /// <summary>
        /// Transformed bounding box
        /// </summary>
        private OrientedBoundingBox orientedBox;
        /// <summary>
        /// Update bounding box flag
        /// </summary>
        private bool updateOrientedBox = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bounds">Initial bounds state</param>
        public BoundsHelper(BoundsHelperInitialState bounds)
        {
            this.bounds = bounds;

            boundsSalt = bounds.Salt;
        }

        /// <summary>
        /// Invalidates the internal state
        /// </summary>
        public void Invalidate()
        {
            updateBoundingBox = true;
            updateBoundingSphere = true;
            updateOrientedBox = true;
        }
        /// <summary>
        /// Checks de initial bounds state value
        /// </summary>
        private void CheckBoundsState()
        {
            if (boundsSalt == bounds.Salt)
            {
                return;
            }

            //State changed, update internal state
            boundsSalt = bounds.Salt;

            Invalidate();
        }

        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        /// <param name="refresh">Sets if the cache must be refreshed or not</param>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public BoundingSphere GetBoundingSphere(ITransform manipulator, bool refresh = false)
        {
            CheckBoundsState();

            if (updateBoundingSphere || refresh)
            {
                boundingSphere = bounds.BoundingSphere.SetTransform(manipulator?.GlobalTransform ?? Matrix.Identity);

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
        public BoundingBox GetBoundingBox(ITransform manipulator, bool refresh = false)
        {
            CheckBoundsState();

            if (updateBoundingBox || refresh)
            {
                boundingBox = GetOrientedBoundingBox(manipulator, refresh).GetBoundingBox();

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
        public OrientedBoundingBox GetOrientedBoundingBox(ITransform manipulator, bool refresh = false)
        {
            CheckBoundsState();

            if (updateOrientedBox || refresh)
            {
                orientedBox = new OrientedBoundingBox(bounds.BoundingBox);
                orientedBox.Transform(manipulator?.GlobalTransform ?? Matrix.Identity);

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
        public bool Cull(ITransform manipulator, CullingVolumeTypes volumeType, ICullingVolume volume, out float distance)
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
        public bool CullBoundingSphere(ITransform manipulator, ICullingVolume volume, out float distance)
        {
            distance = float.MaxValue;

            var sphere = GetBoundingSphere(manipulator);
            var contains = volume.Contains(sphere);

            bool cull = contains == ContainmentType.Disjoint;
            if (!cull)
            {
                var eyePosition = volume.Position;

                distance = Vector3.DistanceSquared(manipulator?.Position ?? Vector3.Zero, eyePosition);
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
        public bool CullBoundingBox(ITransform manipulator, ICullingVolume volume, out float distance)
        {
            distance = float.MaxValue;

            var box = GetBoundingBox(manipulator);
            var contains = volume.Contains(box);

            bool cull = contains == ContainmentType.Disjoint;
            if (!cull)
            {
                var eyePosition = volume.Position;

                distance = Vector3.DistanceSquared(manipulator?.Position ?? Vector3.Zero, eyePosition);
            }

            return cull;
        }
    }
}
