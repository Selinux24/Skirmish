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
    public abstract class Ground<T> : Drawable<T>, IGround, IRayPickable<Triangle>, IIntersectable where T : GroundDescription
    {
        /// <summary>
        /// Quadtree for base ground picking
        /// </summary>
        protected PickingQuadTree<Triangle> GroundPickingQuadtree = null;

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
            if (GroundPickingQuadtree != null)
            {
                // Use quadtree
                return GroundPickingQuadtree.PickNearest(ray, out result);
            }

            return RayPickingHelper.PickNearest(this, ray, out result);
        }
        /// <inheritdoc/>
        public bool PickFirst(PickingRay ray, out PickingResult<Triangle> result)
        {
            if (GroundPickingQuadtree != null)
            {
                // Use quadtree
                return GroundPickingQuadtree.PickFirst(ray, out result);
            }

            return RayPickingHelper.PickFirst(this, ray, out result);
        }
        /// <inheritdoc/>
        public bool PickAll(PickingRay ray, out IEnumerable<PickingResult<Triangle>> results)
        {
            if (GroundPickingQuadtree != null)
            {
                // Use quadtree
                return GroundPickingQuadtree.PickAll(ray, out results);
            }

            return RayPickingHelper.PickAll(this, ray, out results);
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
        public virtual IEnumerable<Triangle> GetGeometry(GeometryTypes geometryType)
        {
            if (GroundPickingQuadtree == null)
            {
                return Enumerable.Empty<Triangle>();
            }

            List<Triangle> res = new List<Triangle>();

            var leafNodes = GroundPickingQuadtree.GetLeafNodes();

            foreach (var node in leafNodes)
            {
                res.AddRange(node.Items);
            }

            return res.ToArray();
        }

        /// <inheritdoc/>
        public bool Intersects(IntersectionVolumeSphere sphere, out PickingResult<Triangle> result)
        {
            if (GroundPickingQuadtree == null)
            {
                // Brute force
                var mesh = GetGeometry(GeometryTypes.Hull);

                return Intersection.SphereIntersectsMesh(sphere, mesh, out result);
            }

            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

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
                if (Intersection.SphereIntersectsMesh(sphere, node.Items, out var res))
                {
                    intersects = true;

                    if (res.Distance < minDistance)
                    {
                        minDistance = res.Distance;

                        result = res;
                    }
                }
            }

            return intersects;
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
                return (IntersectionVolumeMesh)GetGeometry(GeometryTypes.Hull).ToArray();
            }
        }

        /// <inheritdoc/>
        public virtual IIntersectionVolume GetCullingVolume()
        {
            return null;
        }
        /// <inheritdoc/>
        public IEnumerable<BoundingBox> GetBoundingBoxes(int level = 0)
        {
            if (GroundPickingQuadtree == null)
            {
                return Enumerable.Empty<BoundingBox>();
            }

            return GroundPickingQuadtree.GetBoundingBoxes(level);
        }
    }
}
