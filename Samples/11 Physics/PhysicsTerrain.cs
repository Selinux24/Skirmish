using Engine;
using Engine.Collections.Generic;
using Engine.Physics;
using Engine.Physics.Colliders;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Physics
{
    /// <summary>
    /// Terrain
    /// </summary>
    public class PhysicsTerrain : IPhysicsObject
    {
        /// <summary>
        /// OcTree
        /// </summary>
        private readonly OcTree<ICollider> octree;

        /// <inheritdoc/>
        public IRigidBody RigidBody { get; private set; }
        /// <inheritdoc/>
        public IEnumerable<ICollider> Colliders { get; private set; }
        /// <summary>
        /// Terrain
        /// </summary>
        public Scenery Terrain { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PhysicsTerrain(IRigidBody body, Scenery model)
        {
            RigidBody = body ?? throw new ArgumentNullException(nameof(body), $"Physics object must have a rigid body.");
            Terrain = model ?? throw new ArgumentNullException(nameof(model), $"Physics object must have a model.");

            var bbox = model.GetBoundingBox();
            octree = new OcTree<ICollider>(bbox, 50);

            var tris = model.GetTriangles(true);
            var colliders = new ICollider[tris.Count()];
            for (int i = 0; i < tris.Count(); i++)
            {
                var tri = tris.ElementAt(i);

                colliders[i] = new TriangleCollider(tri);
                colliders[i].Attach(body);

                var colliderBox = colliders[i].BoundingBox;

                octree.Insert((IntersectionVolumeAxisAlignedBox)colliderBox, colliders[i]);
            }

            Colliders = colliders;
        }

        /// <inheritdoc/>
        public void Update()
        {
            
        }
        /// <inheritdoc/>
        public bool BroadPhaseTest(IPhysicsObject obj)
        {
            if (obj == null)
            {
                return false;
            }

            var intersects = Terrain.Intersects(IntersectDetectionMode.Box, obj.GetBroadPhaseBounds());

            return intersects;
        }
        /// <inheritdoc/>
        public IEnumerable<ICollider> GetBroadPhaseColliders(IPhysicsObject obj)
        {
            var cullingVolume = obj.GetBroadPhaseBounds();

            var colliders = octree.Query(cullingVolume);

            return colliders;
        }
        /// <inheritdoc/>
        public ICullingVolume GetBroadPhaseBounds()
        {
            return Terrain.GetIntersectionVolume(IntersectDetectionMode.Box);
        }
        /// <inheritdoc/>
        public void Reset(Matrix transform)
        {
            RigidBody?.SetInitialState(transform);
        }
        /// <inheritdoc/>
        public void Reset(Vector3 position, Quaternion rotation)
        {
            RigidBody?.SetInitialState(position, rotation);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Terrain}";
        }
    }
}
