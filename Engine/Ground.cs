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
        /// <inheritdoc/>
        public PickingHullTypes PathFindingHull { get; set; } = PickingHullTypes.Hull;
        /// <inheritdoc/>
        public PickingHullTypes PickingHull { get; set; } = PickingHullTypes.Hull;

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
        public override bool Cull(int cullIndex, ICullingVolume volume, out float distance)
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
            return GroundPickingQuadtree?.PickNearest(ray, out result) ?? RayPickingHelper.PickNearest(this, ray, out result);
        }
        /// <inheritdoc/>
        public bool PickFirst(PickingRay ray, out PickingResult<Triangle> result)
        {
            return GroundPickingQuadtree?.PickFirst(ray, out result) ?? RayPickingHelper.PickFirst(this, ray, out result);
        }
        /// <inheritdoc/>
        public bool PickAll(PickingRay ray, out IEnumerable<PickingResult<Triangle>> results)
        {
            return GroundPickingQuadtree?.PickAll(ray, out results) ?? RayPickingHelper.PickAll(this, ray, out results);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<Vector3> GetPoints(bool refresh = false)
        {
            return GetTriangles(refresh)
                .SelectMany(t => t.GetVertices())
                .AsEnumerable();
        }
        /// <inheritdoc/>
        public virtual IEnumerable<Triangle> GetTriangles(bool refresh = false)
        {
            return GroundPickingQuadtree?.GetLeafNodes().SelectMany(n => n.Items).AsEnumerable() ?? Enumerable.Empty<Triangle>();
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
        public virtual IEnumerable<Triangle> GetGeometry(GeometryTypes geometryType)
        {
            var hull = geometryType switch
            {
                GeometryTypes.Picking => PickingHull,
                GeometryTypes.PathFinding => PathFindingHull,
                _ => PickingHullTypes.None,
            };

            if (hull.HasFlag(PickingHullTypes.Hull))
            {
                return GetTriangles();
            }

            if (hull.HasFlag(PickingHullTypes.Geometry))
            {
                return GetTriangles();
            }

            return Enumerable.Empty<Triangle>();
        }

        /// <inheritdoc/>
        public bool Intersects(IntersectionVolumeSphere sphere, out PickingResult<Triangle> result)
        {
            if (GroundPickingQuadtree == null)
            {
                // Brute force
                var mesh = GetGeometry(GeometryTypes.Picking);

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
        public bool Intersects(IntersectDetectionMode detectionModeThis, ICullingVolume volume)
        {
            return IntersectionHelper.Intersects(this, detectionModeThis, volume);
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

            return (IntersectionVolumeMesh)GetGeometry(GeometryTypes.Picking).ToArray();
        }

        /// <inheritdoc/>
        public virtual ICullingVolume GetCullingVolume()
        {
            return null;
        }
        /// <inheritdoc/>
        public IEnumerable<BoundingBox> GetBoundingBoxes(int level = 0)
        {
            return GroundPickingQuadtree?.GetBoundingBoxes(level) ?? Enumerable.Empty<BoundingBox>();
        }
    }
}
