using Engine;
using Engine.Physics;
using Engine.Physics.Colliders;
using SharpDX;
using System;

namespace Physics
{
    public class PhysicsObject : IPhysicsObject
    {
        public IRigidBody RigidBody { get; private set; }
        public Model Model { get; private set; }
        public ICollider Collider { get; private set; }

        private static MeshCollider CollisionTriangleSoupFromModel(Model model)
        {
            var tris = model.GetTriangles(true);
            tris = Triangle.Transform(tris, Matrix.Invert(model.Manipulator.FinalTransform));

            return new MeshCollider(tris);
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

        public PhysicsObject(IRigidBody rigidBody, Model model)
        {
            RigidBody = rigidBody ?? throw new ArgumentNullException(nameof(rigidBody), $"Physics object must have a rigid body.");
            Model = model ?? throw new ArgumentNullException(nameof(model), $"Physics object must have a model.");

            Collider = model.ColliderType switch
            {
                ColliderTypes.Spheric => CollisionSphereFromModel(model),
                ColliderTypes.Box => CollisionBoxFromModel(model),
                ColliderTypes.Cylinder => CollisionCylinderFromModel(model),
                ColliderTypes.Capsule => CollisionCapsuleFromModel(model),
                ColliderTypes.Mesh => CollisionTriangleSoupFromModel(model),
                _ => null,
            };
            Collider?.Attach(rigidBody);
        }

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

        public void Reset(Matrix transform)
        {
            if (RigidBody == null)
            {
                return;
            }

            RigidBody.SetInitialState(transform);
        }

        public void Reset(Vector3 position, Quaternion rotation)
        {
            if (RigidBody == null)
            {
                return;
            }

            RigidBody.SetInitialState(position, rotation);
        }
    }
}
