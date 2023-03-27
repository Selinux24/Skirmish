using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Collider helper class
    /// </summary>
    class BoundsHelper
    {
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
        /// Initializes internal volumes
        /// </summary>
        /// <param name="points">Point list</param>
        public void Initialize(IEnumerable<Vector3> points)
        {
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

            boundingSphere = initialSphere;
            boundingBox = initialAabb;
            orientedBox = initialObb;

            updateBoundingSphere = false;
            updateBoundingBox = false;
            updateOrientedBox = false;
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
        /// Gets bounding sphere
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        /// <param name="refresh">Sets if the cache must be refreshed or not</param>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public BoundingSphere GetBoundingSphere(Manipulator3D manipulator, bool refresh = false)
        {
            if (updateBoundingSphere || refresh)
            {
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
        public OrientedBoundingBox GetOrientedBoundingBox(Manipulator3D manipulator, bool refresh = false)
        {
            if (updateOrientedBox || refresh)
            {
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

            if (volumeType == CullingVolumeTypes.None)
            {
                return false;
            }

            if (volumeType == CullingVolumeTypes.SphericVolume)
            {
                return CullBoundingSphere(manipulator, volume, out distance);
            }

            if (volumeType == CullingVolumeTypes.BoxVolume)
            {
                return CullBoundingBox(manipulator, volume, out distance);
            }

            return false;
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
