using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.Collections.Generic;
    using Engine.Common;

    /// <summary>
    /// Ground class
    /// </summary>
    /// <remarks>Used for picking tests and navigation over surfaces</remarks>
    public abstract class Ground : Drawable, IRayPickable<Triangle>
    {
        /// <summary>
        /// Ground description
        /// </summary>
        protected new GroundDescription Description
        {
            get
            {
                return base.Description as GroundDescription;
            }
        }
        /// <summary>
        /// Quadtree for base ground picking
        /// </summary>
        protected PickingQuadTree<Triangle> groundPickingQuadtree = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Ground description</param>
        protected Ground(Scene scene, GroundDescription description)
            : base(scene, description)
        {

        }

        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="volume">Culling volume</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the object is outside of the frustum</returns>
        public override bool Cull(ICullingVolume volume, out float distance)
        {
            distance = float.MaxValue;

            if (groundPickingQuadtree == null)
            {
                return false;
            }

            bool cull = volume.Contains(groundPickingQuadtree.BoundingBox) == ContainmentType.Disjoint;
            if (!cull)
            {
                distance = 0;
            }

            return cull;
        }

        /// <summary>
        /// Gets nearest picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickNearest(Ray ray, out PickingResult<Triangle> result)
        {
            return PickNearest(ray, RayPickingParams.Default, out result);
        }
        /// <summary>
        /// Pick ground nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="rayPickingParams">Ray picking params</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickNearest(Ray ray, RayPickingParams rayPickingParams, out PickingResult<Triangle> result)
        {
            bool res = false;

            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            bool facingOnly = rayPickingParams.HasFlag(RayPickingParams.FacingOnly);

            if (this.groundPickingQuadtree != null)
            {
                // Use quadtree
                if (this.groundPickingQuadtree.PickNearest(ray, facingOnly, out PickingResult<Triangle> gResult))
                {
                    if (result.Distance > gResult.Distance)
                    {
                        result = gResult;
                    }

                    res = true;
                }
            }
            else
            {
                // Brute force
                var mesh = GetVolume(true);
                var inItem = Intersection.IntersectNearest(ray, mesh, facingOnly, out var pos, out var tri, out var d);
                if (inItem)
                {
                    result.Position = pos;
                    result.Item = tri;
                    result.Distance = d;

                    res = true;
                }
            }

            return res;
        }
        /// <summary>
        /// Gets first picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickFirst(Ray ray, out PickingResult<Triangle> result)
        {
            return PickFirst(ray, RayPickingParams.Default, out result);
        }
        /// <summary>
        /// Pick ground first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="rayPickingParams">Ray picking params</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickFirst(Ray ray, RayPickingParams rayPickingParams, out PickingResult<Triangle> result)
        {
            bool res = false;

            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            bool facingOnly = rayPickingParams.HasFlag(RayPickingParams.FacingOnly);

            if (this.groundPickingQuadtree != null)
            {
                // Use quadtree
                if (this.groundPickingQuadtree.PickFirst(ray, facingOnly, out PickingResult<Triangle> gResult))
                {
                    if (result.Distance > gResult.Distance)
                    {
                        result = gResult;
                    }

                    res = true;
                }
            }
            else
            {
                // Brute force
                var mesh = GetVolume(true);
                var inItem = Intersection.IntersectFirst(ray, mesh, facingOnly, out var pos, out var tri, out var d);
                if (inItem)
                {
                    result.Position = pos;
                    result.Item = tri;
                    result.Distance = d;

                    res = true;
                }
            }

            return res;
        }
        /// <summary>
        /// Get all picking positions of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickAll(Ray ray, out PickingResult<Triangle>[] results)
        {
            return PickAll(ray, RayPickingParams.Default, out results);
        }
        /// <summary>
        /// Pick ground positions
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="rayPickingParams">Ray picking params</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickAll(Ray ray, RayPickingParams rayPickingParams, out PickingResult<Triangle>[] results)
        {
            bool res = false;

            results = null;

            bool facingOnly = rayPickingParams.HasFlag(RayPickingParams.FacingOnly);

            if (this.groundPickingQuadtree != null)
            {
                // Use quadtree
                if (this.groundPickingQuadtree.PickAll(ray, facingOnly, out PickingResult<Triangle>[] gResults))
                {
                    results = gResults;

                    res = true;
                }
            }
            else
            {
                // Brute force
                var mesh = GetVolume(true);
                var inItem = Intersection.IntersectAll(ray, mesh, facingOnly, out var pos, out var tris, out var ds);
                if (inItem)
                {
                    results = new PickingResult<Triangle>[pos.Length];
                    for (int i = 0; i < pos.Length; i++)
                    {
                        results[i] = new PickingResult<Triangle>
                        {
                            Position = pos[i],
                            Item = tris[i],
                            Distance = ds[i],
                        };
                    }

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
        /// Gets bounding boxes at specified level
        /// </summary>
        /// <param name="level">Level</param>
        /// <returns>Returns a bounding boxes array</returns>
        public IEnumerable<BoundingBox> GetBoundingBoxes(int level = 0)
        {
            return this.groundPickingQuadtree.GetBoundingBoxes(level);
        }

        /// <summary>
        /// Gets the ground volume
        /// </summary>
        /// <param name="full"></param>
        /// <returns>Returns all the triangles of the ground</returns>
        public virtual IEnumerable<Triangle> GetVolume(bool full)
        {
            List<Triangle> res = new List<Triangle>();

            var leafNodes = this.groundPickingQuadtree.GetLeafNodes();

            foreach (var node in leafNodes)
            {
                res.AddRange(node.Items);
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

        /// <summary>
        /// Gets whether the sphere intersects with the ground
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="result">Picking results</param>
        /// <returns>Returns true if intersects</returns>
        public bool Intersects(BoundingSphere sphere, out PickingResult<Triangle> result)
        {
            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            if (groundPickingQuadtree != null)
            {
                // Use quadtree
                var nodes = groundPickingQuadtree.GetNodesInVolume(new CullingVolumeSphere(sphere));
                if (!nodes.Any())
                {
                    return false;
                }

                bool intersects = false;
                float minDistance = float.MaxValue;
                foreach (var node in nodes)
                {
                    if (Intersection.SphereIntersectsMesh(sphere, node.Items, out Triangle tri, out Vector3 position, out float distance))
                    {
                        intersects = true;

                        if (distance < minDistance)
                        {
                            minDistance = distance;

                            result.Distance = distance;
                            result.Position = position;
                            result.Item = tri;
                        }
                    }
                }

                return intersects;
            }
            else
            {
                // Brute force
                var mesh = GetVolume(true);
                if (Intersection.SphereIntersectsMesh(sphere, mesh, out Triangle tri, out Vector3 position, out float distance))
                {
                    result.Distance = distance;
                    result.Position = position;
                    result.Item = tri;

                    return true;
                }
            }

            return false;
        }
    }
}
