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
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if picked position found</returns>
        public virtual bool PickNearest(ref Ray ray, bool facingOnly, out PickingResult<Triangle> result)
        {
            bool res = false;

            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            if (this.groundPickingQuadtree != null)
            {
                if (this.groundPickingQuadtree.PickNearest(ref ray, facingOnly, out PickingResult<Triangle> gResult))
                {
                    if (result.Distance > gResult.Distance)
                    {
                        result = gResult;
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
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if picked position found</returns>
        public virtual bool PickFirst(ref Ray ray, bool facingOnly, out PickingResult<Triangle> result)
        {
            bool res = false;

            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            if (this.groundPickingQuadtree != null)
            {
                if (this.groundPickingQuadtree.PickFirst(ref ray, facingOnly, out PickingResult<Triangle> gResult))
                {
                    if (result.Distance > gResult.Distance)
                    {
                        result = gResult;
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
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if picked position found</returns>
        public virtual bool PickAll(ref Ray ray, bool facingOnly, out PickingResult<Triangle>[] results)
        {
            bool res = false;

            results = null;

            if (this.groundPickingQuadtree != null)
            {
                if (this.groundPickingQuadtree.PickAll(ref ray, facingOnly, out PickingResult<Triangle>[]  gResults))
                {
                    results = gResults;

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
