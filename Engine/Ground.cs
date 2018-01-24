using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Collections.Generic;

    /// <summary>
    /// Ground class
    /// </summary>
    /// <remarks>Used for picking tests and navigation over surfaces</remarks>
    public abstract class Ground : Drawable, IRayPickable<Triangle>
    {
        /// <summary>
        /// Quadtree for base ground picking
        /// </summary>
        protected PickingQuadTree<Triangle> groundPickingQuadtree = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Ground description</param>
        public Ground(Scene scene, GroundDescription description)
            : base(scene, description)
        {

        }

        /// <summary>
        /// Pick ground nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only triangles facing to ray origin</param>
        /// <param name="position">Picked position if exists</param>
        /// <param name="triangle">Picked triangle if exists</param>
        /// <param name="distance">Distance to position</param>
        /// <returns>Returns true if picked position found</returns>
        public virtual bool PickNearest(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            bool res = false;

            position = Vector3.Zero;
            triangle = new Triangle();
            distance = float.MaxValue;

            if (this.groundPickingQuadtree != null)
            {
                Vector3 gP;
                Triangle gT;
                float gD;
                if (this.groundPickingQuadtree.PickNearest(ref ray, facingOnly, out gP, out gT, out gD))
                {
                    if (distance > gD)
                    {
                        position = gP;
                        triangle = gT;
                        distance = gD;
                    }

                    res = true;
                }
            }

            return res;
        }
        /// <summary>
        /// Pick ground first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only triangles facing to ray origin</param>
        /// <param name="position">Picked position if exists</param>
        /// <param name="triangle">Picked triangle if exists</param>
        /// <param name="distance">Distance to position</param>
        /// <returns>Returns true if picked position found</returns>
        public virtual bool PickFirst(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            bool res = false;

            position = Vector3.Zero;
            triangle = new Triangle();
            distance = float.MaxValue;

            if (this.groundPickingQuadtree != null)
            {
                Vector3 gP;
                Triangle gT;
                float gD;
                if (this.groundPickingQuadtree.PickFirst(ref ray, facingOnly, out gP, out gT, out gD))
                {
                    if (distance > gD)
                    {
                        position = gP;
                        triangle = gT;
                        distance = gD;
                    }

                    res = true;
                }
            }

            return res;
        }
        /// <summary>
        /// Pick ground positions
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only triangles facing to ray origin</param>
        /// <param name="positions">Picked positions if exists</param>
        /// <param name="triangles">Picked triangles if exists</param>
        /// <param name="distances">Distances to positions</param>
        /// <returns>Returns true if picked position found</returns>
        public virtual bool PickAll(ref Ray ray, bool facingOnly, out Vector3[] positions, out Triangle[] triangles, out float[] distances)
        {
            bool res = false;

            positions = null;
            triangles = null;
            distances = null;

            if (this.groundPickingQuadtree != null)
            {
                Vector3[] gP;
                Triangle[] gT;
                float[] gD;
                if (this.groundPickingQuadtree.PickAll(ref ray, facingOnly, out gP, out gT, out gD))
                {
                    positions = gP;
                    triangles = gT;
                    distances = gD;

                    res = true;
                }
            }

            return res;
        }

        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public virtual BoundingSphere GetBoundingSphere()
        {
            return this.groundPickingQuadtree != null ?
                BoundingSphere.FromBox(this.groundPickingQuadtree.BoundingBox) :
                new BoundingSphere();
        }
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        public virtual BoundingBox GetBoundingBox()
        {
            return this.groundPickingQuadtree != null ?
                this.groundPickingQuadtree.BoundingBox :
                new BoundingBox();
        }

        /// <summary>
        /// Gets the ground volume
        /// </summary>
        /// <param name="full"></param>
        /// <returns>Returns all the triangles of the ground</returns>
        public virtual Triangle[] GetVolume(bool full)
        {
            List<Triangle> res = new List<Triangle>();

            var leafNodes = this.groundPickingQuadtree.GetLeafNodes();

            for (int i = 0; i < leafNodes.Length; i++)
            {
                res.AddRange(leafNodes[i].Items);
            }

            return res.ToArray();
        }

        /// <summary>
        /// Gets the culling volume for scene culling tests
        /// </summary>
        /// <returns>Return the culling volume</returns>
        public virtual ICullingVolume GetCullingVolume()
        {
            return null;
        }
    }
}
