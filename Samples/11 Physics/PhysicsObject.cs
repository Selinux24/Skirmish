using Engine;
using Engine.Common;
using Engine.Physics;
using Engine.Physics.Colliders;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Physics
{
    /// <summary>
    /// Physics object
    /// </summary>
    public class PhysicsObject : IPhysicsObject
    {
        /// <inheritdoc/>
        public IRigidBody RigidBody { get; private set; }
        /// <inheritdoc/>
        public IEnumerable<ICollider> Colliders { get; private set; }
        /// <summary>
        /// Model
        /// </summary>
        public Model Model { get; private set; }

        private static ConvexMeshCollider CollisionTriangleSoupFromModel(Model model)
        {
            var tris = model.GetTriangles(true);
            tris = Triangle.Transform(tris, Matrix.Invert(model.Manipulator.FinalTransform));

            return new ConvexMeshCollider(tris);
        }
        private static BoxCollider CollisionBoxFromModel(Model model)
        {
            return new BoxCollider(model.GetOrientedBoundingBox(true).Extents);
        }
        private static SphereCollider CollisionSphereFromModel(Model model)
        {
            return new SphereCollider(model.GetBoundingSphere(true).Radius);
        }
        private static CylinderCollider CollisionCylinderFromModel(Model model)
        {
            var extents = model.GetBoundingBox(true).GetExtents();

            return new CylinderCollider(extents.X, extents.Y * 2);
        }
        private static CapsuleCollider CollisionCapsuleFromModel(Model model)
        {
            var extents = model.GetBoundingBox(true).GetExtents();

            return new CapsuleCollider(extents.X, extents.Y * 2);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PhysicsObject(Model model, IRigidBody rigidBody)
        {
            RigidBody = rigidBody ?? throw new ArgumentNullException(nameof(rigidBody), $"Physics object must have a rigid body.");
            Model = model ?? throw new ArgumentNullException(nameof(model), $"Physics object must have a model.");

            ICollider collider = model.ColliderType switch
            {
                ColliderTypes.Spheric => CollisionSphereFromModel(model),
                ColliderTypes.Box => CollisionBoxFromModel(model),
                ColliderTypes.Cylinder => CollisionCylinderFromModel(model),
                ColliderTypes.Capsule => CollisionCapsuleFromModel(model),
                ColliderTypes.Mesh => CollisionTriangleSoupFromModel(model),
                _ => null,
            };
            collider.Attach(rigidBody);

            Colliders = new[] { collider };
        }

        /// <inheritdoc/>
        public void Update()
        {
            if (RigidBody == null)
            {
                return;
            }

            if (Model == null)
            {
                return;
            }

            Model.Manipulator.SetRotation(RigidBody.Rotation);
            Model.Manipulator.SetPosition(RigidBody.Position);
        }
        /// <inheritdoc/>
        public bool BroadPhaseTest(IPhysicsObject obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Model.Intersects(IntersectDetectionMode.Sphere, obj.GetBroadPhaseBounds());
        }
        /// <inheritdoc/>
        public IEnumerable<ICollider> GetBroadPhaseColliders(IPhysicsObject obj)
        {
            var cullingVolume = obj.GetBroadPhaseBounds();

            return Colliders.Where(c => IntersectionHelper.Intersects(cullingVolume, (IntersectionVolumeSphere)c.BoundingSphere));
        }
        /// <inheritdoc/>
        public ICullingVolume GetBroadPhaseBounds()
        {
            return Model.GetIntersectionVolume(IntersectDetectionMode.Sphere);
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
    }
}
