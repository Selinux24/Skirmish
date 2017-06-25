using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Common
{
    /// <summary>
    /// Volume manager
    /// </summary>
    public class VolumeManager
    {
        private BoundingSphere boundingSphere;
        private BoundingBox boundingBox;
        private BoundingCylinder boundingCylinder;
        private OrientedBoundingBox orientedBoundingBox;

        /// <summary>
        /// Feeder function
        /// </summary>
        public Func<bool, Vector3[]> Feeder { get; set; }

        /// <summary>
        /// Sets points
        /// </summary>
        /// <param name="points"></param>
        public void Invalidate()
        {
            this.boundingSphere = new BoundingSphere();
            this.boundingBox = new BoundingBox();
            this.orientedBoundingBox = new OrientedBoundingBox();
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
                var points = this.Feeder(refresh);
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
                var points = this.Feeder(refresh);
                if (points != null && points.Length > 0)
                {
                    this.boundingBox = BoundingBox.FromPoints(points);
                }
            }

            return this.boundingBox;
        }
        /// <summary>
        /// Gets oriented bounding box
        /// </summary>
        /// <returns>Returns oriented bounding box with identity transformation. Empty if the vertex type hasn't position channel</returns>
        public BoundingCylinder GetBoundingCylinder()
        {
            return this.GetBoundingCylinder(false);
        }
        /// <summary>
        /// Gets oriented bounding box
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns oriented bounding box with identity transformation. Empty if the vertex type hasn't position channel</returns>
        public BoundingCylinder GetBoundingCylinder(bool refresh)
        {
            if (refresh || this.boundingCylinder == new BoundingCylinder())
            {
                var points = this.Feeder(refresh);
                if (points != null && points.Length > 0)
                {
                    this.boundingCylinder = BoundingCylinder.FromPoints(points);
                }
            }

            return this.boundingCylinder;
        }
        /// <summary>
        /// Gets oriented bounding box
        /// </summary>
        /// <returns>Returns oriented bounding box with identity transformation. Empty if the vertex type hasn't position channel</returns>
        public OrientedBoundingBox GetOrientedBoundingBox()
        {
            return this.GetOrientedBoundingBox(false);
        }
        /// <summary>
        /// Gets oriented bounding box
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns oriented bounding box with identity transformation. Empty if the vertex type hasn't position channel</returns>
        public OrientedBoundingBox GetOrientedBoundingBox(bool refresh)
        {
            if (refresh || this.orientedBoundingBox == new OrientedBoundingBox())
            {
                var points = this.Feeder(refresh);
                if (points != null && points.Length > 0)
                {
                    this.orientedBoundingBox = new OrientedBoundingBox(points);
                    this.orientedBoundingBox.Transform(Matrix.Identity);
                }
            }

            return this.orientedBoundingBox;
        }
    }
}
