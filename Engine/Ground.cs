using Engine.Collections.Generic;
using Engine.Common;
using Engine.Mathematics;
using System.Linq;

namespace Engine
{
    /// <summary>
    /// Ground class
    /// </summary>
    /// <remarks>Used for picking tests and navigation over surfaces</remarks>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public abstract class Ground<T>(Scene scene, string id, string name) : Drawable<T>(scene, id, name), IGround, IRayPickable<Triangle>, IIntersectable where T : GroundDescription
    {
        /// <inheritdoc/>
        public PickingHullTypes PathFindingHull { get; set; } = PickingHullTypes.Hull;
        /// <inheritdoc/>
        public PickingHullTypes PickingHull { get; set; } = PickingHullTypes.Hull;

        /// <summary>
        /// Quadtree for base ground picking
        /// </summary>
        protected PickingQuadTree<Triangle> GroundPickingQuadtree = null;

        /// <inheritdoc/>
        public override bool Cull(int cullIndex, ICullingVolume volume, out float distance)
        {
            distance = float.MaxValue;

            if (GroundPickingQuadtree == null)
            {
                return false;
            }

            if (volume == null)
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
        public bool PickNearest(ref PickingRay ray, out PickingResult<Triangle> result)
        {
            return GroundPickingQuadtree?.PickNearest(ref ray, out result) ?? RayPickingHelper.PickNearest(this, ref ray, out result);
        }
        /// <inheritdoc/>
        public bool PickFirst(ref PickingRay ray, out PickingResult<Triangle> result)
        {
            return GroundPickingQuadtree?.PickFirst(ref ray, out result) ?? RayPickingHelper.PickFirst(this, ref ray, out result);
        }
        /// <inheritdoc/>
        public bool PickAll(ref PickingRay ray, out PickingResult<Triangle>[] results)
        {
            return GroundPickingQuadtree?.PickAll(ref ray, out results) ?? RayPickingHelper.PickAll(this, ref ray, out results);
        }

        /// <inheritdoc/>
        public virtual BoundingSphere GetBoundingSphere(bool refresh = false)
        {
            return BoundingSphere.FromBox(GetBoundingBox(refresh));
        }
        /// <inheritdoc/>
        public virtual BoundingBox GetBoundingBox(bool refresh = false)
        {
            return GroundPickingQuadtree?.BoundingBox ?? new BoundingBox();
        }
        /// <inheritdoc/>
        public virtual OrientedBoundingBox GetOrientedBoundingBox(bool refresh = false)
        {
            return new OrientedBoundingBox(GetBoundingBox(refresh));
        }
        /// <inheritdoc/>
        public virtual Triangle[] GetGeometry(PickingMode geometryType, bool refresh = false)
        {
            var hull = geometryType switch
            {
                PickingMode.Picking => PickingHull,
                PickingMode.PathFinding => PathFindingHull,
                _ => PickingHullTypes.None,
            };

            if (hull.HasFlag(PickingHullTypes.Hull))
            {
                return GetGeometry(refresh);
            }

            if (hull.HasFlag(PickingHullTypes.Geometry))
            {
                return GetGeometry(refresh);
            }

            return [];
        }
        /// <inheritdoc/>
        public virtual Triangle[] GetGeometry(bool refresh = false)
        {
            return GroundPickingQuadtree?.GetLeafNodes().SelectMany(n => n.Items).ToArray() ?? [];
        }
        /// <inheritdoc/>
        public virtual Vector3[] GetPoints(bool refresh = false)
        {
            return [.. GetGeometry(refresh).SelectMany(t => t.GetVertices())];
        }

        /// <inheritdoc/>
        public bool Intersects(IntersectionVolumeSphere sphere, out PickingResult<Triangle> result)
        {
            if (GroundPickingQuadtree == null)
            {
                // Brute force
                var mesh = GetGeometry(PickingMode.Picking);

                return Collision.SphereIntersectsMesh(ref sphere.Sphere, mesh, out result);
            }

            result = PickingResult<Triangle>.Empty;

            // Use quadtree
            var nodes = GroundPickingQuadtree.FindNodesInVolume(sphere);
            if (nodes.Length == 0)
            {
                return false;
            }

            bool intersects = false;
            float minDistance = float.MaxValue;
            foreach (var node in nodes)
            {
                if (Collision.SphereIntersectsMesh(ref sphere.Sphere, node.Items, out var res))
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
            return Collision.Intersects(this, detectionModeThis, other, detectionModeOther);
        }
        /// <inheritdoc/>
        public bool Intersects(IntersectDetectionMode detectionModeThis, ICullingVolume volume)
        {
            return Collision.Intersects(this, detectionModeThis, volume);
        }
        /// <inheritdoc/>
        public ICullingVolume GetIntersectionVolume(IntersectDetectionMode detectionMode)
        {
            if (detectionMode == IntersectDetectionMode.Box)
            {
                return (IntersectionVolumeAxisAlignedBox)GetBoundingBox();
            }

            if (detectionMode == IntersectDetectionMode.Sphere)
            {
                return (IntersectionVolumeSphere)GetBoundingSphere();
            }

            return new IntersectionVolumeMesh([.. GetGeometry(PickingMode.Picking)]);
        }

        /// <inheritdoc/>
        public virtual ICullingVolume GetCullingVolume()
        {
            return null;
        }
        /// <inheritdoc/>
        public BoundingBox[] GetBoundingBoxes(int level = 0)
        {
            return GroundPickingQuadtree?.GetBoundingBoxes(level) ?? [];
        }
    }
}
