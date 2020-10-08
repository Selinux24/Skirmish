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
    public abstract class Ground : Drawable, IRayPickable<Triangle>, IIntersectable
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
        /// Collision detection mode
        /// </summary>
        protected CollisionDetectionMode collisionDetection;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Ground description</param>
        protected Ground(Scene scene, GroundDescription description)
            : base(scene, description)
        {
            collisionDetection = description.CollisionDetection;
        }

        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="volume">Culling volume</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the object is outside of the frustum</returns>
        public override bool Cull(IIntersectionVolume volume, out float distance)
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
            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            bool facingOnly = rayPickingParams.HasFlag(RayPickingParams.FacingOnly);

            if (groundPickingQuadtree != null)
            {
                // Use quadtree
                if (!groundPickingQuadtree.PickNearest(ray, facingOnly, out var gResult))
                {
                    // Without contacts
                    return false;
                }

                // Store result
                result.Position = gResult.Position;
                result.Item = gResult.Item;
                result.Distance = gResult.Distance;

                return true;
            }
            else if (collisionDetection == CollisionDetectionMode.BruteForce)
            {
                // Brute force
                var mesh = GetVolume(true);
                if (!mesh.Any())
                {
                    // Empty mesh
                    return false;
                }

                if (!Intersection.IntersectNearest(ray, mesh, facingOnly, out var pos, out var tri, out var d))
                {
                    // There are no intersected primitives
                    return false;
                }

                // Store result
                result.Position = pos;
                result.Item = tri;
                result.Distance = d;

                return true;
            }

            return false;
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
            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            bool facingOnly = rayPickingParams.HasFlag(RayPickingParams.FacingOnly);

            if (groundPickingQuadtree != null)
            {
                // Use quadtree
                if (!groundPickingQuadtree.PickFirst(ray, facingOnly, out var gResult))
                {
                    return false;
                }

                // Store result
                result.Position = gResult.Position;
                result.Item = gResult.Item;
                result.Distance = gResult.Distance;

                return true;
            }
            else if (collisionDetection == CollisionDetectionMode.BruteForce)
            {
                // Brute force
                var mesh = GetVolume(true);
                if (!mesh.Any())
                {
                    // Empty mesh
                    return false;
                }

                if (!Intersection.IntersectFirst(ray, mesh, facingOnly, out var pos, out var tri, out var d))
                {
                    // There are no intersected primitives
                    return false;
                }

                // Store result
                result.Position = pos;
                result.Item = tri;
                result.Distance = d;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Get all picking positions of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickAll(Ray ray, out IEnumerable<PickingResult<Triangle>> results)
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
        public bool PickAll(Ray ray, RayPickingParams rayPickingParams, out IEnumerable<PickingResult<Triangle>> results)
        {
            results = new PickingResult<Triangle>[] { };

            bool facingOnly = rayPickingParams.HasFlag(RayPickingParams.FacingOnly);

            if (groundPickingQuadtree != null)
            {
                // Use quadtree
                if (!groundPickingQuadtree.PickAll(ray, facingOnly, out var gResults))
                {
                    // Without contacts
                    return false;
                }

                results = gResults;

                return true;
            }
            else if (collisionDetection == CollisionDetectionMode.BruteForce)
            {
                // Brute force
                var mesh = GetVolume(true);
                if (!mesh.Any())
                {
                    // Empty mesh
                    return false;
                }

                if (!Intersection.IntersectAll(ray, mesh, facingOnly, out var pos, out var tris, out var ds))
                {
                    // There are no intersected primitives
                    return false;
                }

                // Store results
                List<PickingResult<Triangle>> picks = new List<PickingResult<Triangle>>(pos.Length);
                for (int i = 0; i < pos.Length; i++)
                {
                    picks.Add(new PickingResult<Triangle>
                    {
                        Position = pos[i],
                        Item = tris[i],
                        Distance = ds[i],
                    });
                }

                results = picks;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public virtual BoundingSphere GetBoundingSphere(bool refresh = false)
        {
            return groundPickingQuadtree != null ? BoundingSphere.FromBox(groundPickingQuadtree.BoundingBox) : new BoundingSphere();
        }
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        public virtual BoundingBox GetBoundingBox(bool refresh = false)
        {
            return groundPickingQuadtree != null ? groundPickingQuadtree.BoundingBox : new BoundingBox();
        }
        /// <summary>
        /// Gets oriented bounding box
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns oriented bounding box. Empty if the vertex type hasn't position channel</returns>
        public virtual OrientedBoundingBox GetOrientedBoundingBox(bool refresh = false)
        {
            return new OrientedBoundingBox(GetBoundingBox(refresh));
        }

        /// <summary>
        /// Gets bounding boxes at specified level
        /// </summary>
        /// <param name="level">Level</param>
        /// <returns>Returns a bounding boxes array</returns>
        public IEnumerable<BoundingBox> GetBoundingBoxes(int level = 0)
        {
            return groundPickingQuadtree.GetBoundingBoxes(level);
        }

        /// <summary>
        /// Gets the ground volume
        /// </summary>
        /// <param name="full"></param>
        /// <returns>Returns all the triangles of the ground</returns>
        public virtual IEnumerable<Triangle> GetVolume(bool full)
        {
            List<Triangle> res = new List<Triangle>();

            var leafNodes = groundPickingQuadtree.GetLeafNodes();

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
        public virtual IIntersectionVolume GetCullingVolume()
        {
            return null;
        }

        /// <summary>
        /// Gets whether the sphere intersects with the current object
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="result">Picking results</param>
        /// <returns>Returns true if intersects</returns>
        public bool Intersects(IntersectionVolumeSphere sphere, out PickingResult<Triangle> result)
        {
            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            if (groundPickingQuadtree != null)
            {
                // Use quadtree
                var nodes = groundPickingQuadtree.GetNodesInVolume(sphere);
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
            else if (collisionDetection == CollisionDetectionMode.BruteForce)
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

        /// <summary>
        /// Gets whether the actual object have intersection with the intersectable or not
        /// </summary>
        /// <param name="detectionModeThis">Detection mode for this object</param>
        /// <param name="other">Other intersectable</param>
        /// <param name="detectionModeOther">Detection mode for the other object</param>
        /// <returns>Returns true if have intersection</returns>
        public bool Intersects(IntersectDetectionMode detectionModeThis, IIntersectable other, IntersectDetectionMode detectionModeOther)
        {
            return IntersectionHelper.Intersects(this, detectionModeThis, other, detectionModeOther);
        }
        /// <summary>
        /// Gets whether the actual object have intersection with the volume or not
        /// </summary>
        /// <param name="detectionModeThis">Detection mode for this object</param>
        /// <param name="volume">Volume</param>
        /// <returns>Returns true if have intersection</returns>
        public bool Intersects(IntersectDetectionMode detectionModeThis, IIntersectionVolume volume)
        {
            return IntersectionHelper.Intersects(this, detectionModeThis, volume);
        }

        /// <summary>
        /// Gets the intersection volume based on the specified detection mode
        /// </summary>
        /// <param name="detectionMode">Detection mode</param>
        /// <returns>Returns an intersection volume</returns>
        public IIntersectionVolume GetIntersectionVolume(IntersectDetectionMode detectionMode)
        {
            if (detectionMode == IntersectDetectionMode.Box)
            {
                return (IntersectionVolumeAxisAlignedBox)GetBoundingBox();
            }
            else if (detectionMode == IntersectDetectionMode.Sphere)
            {
                return (IntersectionVolumeSphere)GetBoundingSphere();
            }
            else
            {
                return (IntersectionVolumeMesh)GetVolume(true).ToArray();
            }
        }
    }
}
