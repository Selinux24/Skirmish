using SharpDX;
using System;
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
        private BoundingBox initialBox;
        /// <summary>
        /// Transformed bounding box
        /// </summary>
        private BoundingBox boundingBox;
        /// <summary>
        /// Update bounding box flag
        /// </summary>
        private bool updateBoundingBox;
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
                //Initialize the identity sphere
                initialSphere = BoundingSphere.FromPoints(points.ToArray());

                //Initialize the identity box
                initialBox = BoundingBox.FromPoints(points.ToArray());
            }
            else
            {
                initialSphere = new BoundingSphere();

                initialBox = new BoundingBox();
            }

            boundingSphere = initialSphere;
            boundingBox = initialBox;
            orientedBox = new OrientedBoundingBox(initialBox);

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
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public BoundingSphere GetBoundingSphere(Manipulator3D manipulator, bool refresh = false)
        {
            if (updateBoundingSphere || refresh)
            {
                float maxScale = Math.Max(manipulator.Scaling.X, manipulator.Scaling.Y);
                maxScale = Math.Max(maxScale, manipulator.Scaling.Z);

                boundingSphere = new BoundingSphere(initialSphere.Center + manipulator.Position, initialSphere.Radius * maxScale);

                updateBoundingSphere = false;
            }

            return boundingSphere;
        }
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        public BoundingBox GetBoundingBox(Manipulator3D manipulator, bool refresh = false)
        {
            if (updateBoundingBox || refresh)
            {
                var obb = new OrientedBoundingBox(initialBox);
                obb.Transform(manipulator.FinalTransform);
                boundingBox = obb.GetBoundingBox();

                updateBoundingBox = false;
            }

            return boundingBox;
        }
        /// <summary>
        /// Gets oriented bounding box
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns oriented bounding box. Empty if the vertex type hasn't position channel</returns>
        public OrientedBoundingBox GetOrientedBoundingBox(Manipulator3D manipulator, bool refresh = false)
        {
            if (updateOrientedBox || refresh)
            {
                orientedBox = new OrientedBoundingBox(initialBox);
                orientedBox.Transform(manipulator.FinalTransform);

                updateOrientedBox = false;
            }

            return orientedBox;
        }
    }
}
