﻿using SharpDX;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Collections.Generic;
    using Engine.Common;

    /// <summary>
    /// Ground class
    /// </summary>
    /// <remarks>Used for picking tests and navigation over surfaces</remarks>
    public abstract class Ground<T> : Drawable<T>, IGround, IRayPickable<Triangle>, IIntersectable where T : GroundDescription
    {
        /// <summary>
        /// Quadtree for base ground picking
        /// </summary>
        protected PickingQuadTree<Triangle> GroundPickingQuadtree = null;
        /// <summary>
        /// Collision detection mode
        /// </summary>
        protected CollisionDetectionMode CollisionDetection;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        protected Ground(Scene scene, string id, string name)
            : base(scene, id, name)
        {

        }

        /// <inheritdoc/>
        public override async Task InitializeAssets(T description)
        {
            await base.InitializeAssets(description);

            CollisionDetection = description.CollisionDetection;
        }

        /// <inheritdoc/>
        public override bool Cull(IIntersectionVolume volume, out float distance)
        {
            distance = float.MaxValue;

            if (GroundPickingQuadtree == null)
            {
                return false;
            }

            bool cull = volume.Contains(GroundPickingQuadtree.BoundingBox) == ContainmentType.Disjoint;
            if (!cull)
            {
                distance = 0;
            }

            return cull;
        }

        /// <inheritdoc/>
        public bool PickNearest(PickingRay ray, out PickingResult<Triangle> result)
        {
            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            if (GroundPickingQuadtree != null)
            {
                // Use quadtree
                if (!GroundPickingQuadtree.PickNearest(ray, out var gResult))
                {
                    // Without contacts
                    return false;
                }

                // Store result
                result.Position = gResult.Position;
                result.Primitive = gResult.Primitive;
                result.Distance = gResult.Distance;

                return true;
            }
            else if (CollisionDetection == CollisionDetectionMode.BruteForce)
            {
                // Brute force
                var mesh = GetVolume(true);
                if (!mesh.Any())
                {
                    // Empty mesh
                    return false;
                }

                if (!Intersection.IntersectNearest(ray, mesh, out var pos, out var tri, out var d))
                {
                    // There are no intersected primitives
                    return false;
                }

                // Store result
                result.Position = pos;
                result.Primitive = tri;
                result.Distance = d;

                return true;
            }

            return false;
        }
        /// <inheritdoc/>
        public bool PickFirst(PickingRay ray, out PickingResult<Triangle> result)
        {
            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            if (GroundPickingQuadtree != null)
            {
                // Use quadtree
                if (!GroundPickingQuadtree.PickFirst(ray, out var gResult))
                {
                    return false;
                }

                // Store result
                result.Position = gResult.Position;
                result.Primitive = gResult.Primitive;
                result.Distance = gResult.Distance;

                return true;
            }
            else if (CollisionDetection == CollisionDetectionMode.BruteForce)
            {
                // Brute force
                var mesh = GetVolume(true);
                if (!mesh.Any())
                {
                    // Empty mesh
                    return false;
                }

                if (!Intersection.IntersectFirst(ray, mesh, out var pos, out var tri, out var d))
                {
                    // There are no intersected primitives
                    return false;
                }

                // Store result
                result.Position = pos;
                result.Primitive = tri;
                result.Distance = d;

                return true;
            }

            return false;
        }
        /// <inheritdoc/>
        public bool PickAll(PickingRay ray, out IEnumerable<PickingResult<Triangle>> results)
        {
            results = new PickingResult<Triangle>[] { };

            if (GroundPickingQuadtree != null)
            {
                // Use quadtree
                if (!GroundPickingQuadtree.PickAll(ray, out var gResults))
                {
                    // Without contacts
                    return false;
                }

                results = gResults;

                return true;
            }
            else if (CollisionDetection == CollisionDetectionMode.BruteForce)
            {
                // Brute force
                var mesh = GetVolume(true);
                if (!mesh.Any())
                {
                    // Empty mesh
                    return false;
                }

                if (!Intersection.IntersectAll(ray, mesh, out var pos, out var tris, out var ds))
                {
                    // There are no intersected primitives
                    return false;
                }

                // Store results
                List<PickingResult<Triangle>> picks = new List<PickingResult<Triangle>>(pos.Count());
                for (int i = 0; i < pos.Count(); i++)
                {
                    picks.Add(new PickingResult<Triangle>
                    {
                        Position = pos.ElementAt(i),
                        Primitive = tris.ElementAt(i),
                        Distance = ds.ElementAt(i),
                    });
                }

                results = picks;

                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public virtual BoundingSphere GetBoundingSphere(bool refresh = false)
        {
            return GroundPickingQuadtree != null ? BoundingSphere.FromBox(GroundPickingQuadtree.BoundingBox) : new BoundingSphere();
        }
        /// <inheritdoc/>
        public virtual BoundingBox GetBoundingBox(bool refresh = false)
        {
            return GroundPickingQuadtree != null ? GroundPickingQuadtree.BoundingBox : new BoundingBox();
        }
        /// <inheritdoc/>
        public virtual OrientedBoundingBox GetOrientedBoundingBox(bool refresh = false)
        {
            return new OrientedBoundingBox(GetBoundingBox(refresh));
        }

        /// <inheritdoc/>
        public IEnumerable<BoundingBox> GetBoundingBoxes(int level = 0)
        {
            return GroundPickingQuadtree.GetBoundingBoxes(level);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<Triangle> GetVolume(bool full)
        {
            List<Triangle> res = new List<Triangle>();

            var leafNodes = GroundPickingQuadtree.GetLeafNodes();

            foreach (var node in leafNodes)
            {
                res.AddRange(node.Items);
            }

            return res.ToArray();
        }

        /// <inheritdoc/>
        public virtual IIntersectionVolume GetCullingVolume()
        {
            return null;
        }

        /// <inheritdoc/>
        public bool Intersects(IntersectionVolumeSphere sphere, out PickingResult<Triangle> result)
        {
            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            if (GroundPickingQuadtree != null)
            {
                // Use quadtree
                var nodes = GroundPickingQuadtree.GetNodesInVolume(sphere);
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
                            result.Primitive = tri;
                        }
                    }
                }

                return intersects;
            }
            else if (CollisionDetection == CollisionDetectionMode.BruteForce)
            {
                // Brute force
                var mesh = GetVolume(true);
                if (Intersection.SphereIntersectsMesh(sphere, mesh, out Triangle tri, out Vector3 position, out float distance))
                {
                    result.Distance = distance;
                    result.Position = position;
                    result.Primitive = tri;

                    return true;
                }
            }

            return false;
        }
        /// <inheritdoc/>
        public bool Intersects(IntersectDetectionMode detectionModeThis, IIntersectable other, IntersectDetectionMode detectionModeOther)
        {
            return IntersectionHelper.Intersects(this, detectionModeThis, other, detectionModeOther);
        }
        /// <inheritdoc/>
        public bool Intersects(IntersectDetectionMode detectionModeThis, IIntersectionVolume volume)
        {
            return IntersectionHelper.Intersects(this, detectionModeThis, volume);
        }

        /// <inheritdoc/>
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
