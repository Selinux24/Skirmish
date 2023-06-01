﻿using SharpDX;
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
        private bool updateBoundingSphere = false;

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
        private bool updateBoundingBox = false;

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
        private bool updateOrientedBox = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="points">Point list</param>
        public BoundsHelper(IEnumerable<Vector3> points)
        {
            if (points?.Any() != true)
            {
                boundingSphere = initialSphere = new BoundingSphere();

                boundingBox = initialAabb = new BoundingBox();

                orientedBox = initialObb = new OrientedBoundingBox();
            }
            else
            {
                var distinctPoints = points.ToArray();

                //Initialize the identity sphere
                boundingSphere = initialSphere = BoundingSphere.FromPoints(distinctPoints);

                //Initialize the identity box
                boundingBox = initialAabb = BoundingBox.FromPoints(distinctPoints);

                //Initialize the identity obb
                orientedBox = initialObb = new OrientedBoundingBox(initialAabb);
            }
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
