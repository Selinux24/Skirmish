using Engine;
using Engine.Physics;
using Engine.Physics.Colliders;
using SharpDX;
using System;

namespace Physics
{
    public class PhysicsObject : IPhysicsObject
    {
        public IRigidBody Body { get; private set; }
        public ICollider Collider { get; private set; }
        public Model Model { get; private set; }

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

        public PhysicsObject(IRigidBody body, Model model)
        {
            Body = body ?? throw new ArgumentNullException(nameof(body), $"Physics object must have a rigid body.");
            Model = model ?? throw new ArgumentNullException(nameof(model), $"Physics object must have a model.");

            Collider = model.CullingVolumeType switch
            {
                CullingVolumeTypes.None => CollisionTriangleSoupFromModel(model),
                CullingVolumeTypes.BoxVolume => CollisionBoxFromModel(model),
                CullingVolumeTypes.CylinderVolume => CollisionCylinderFromModel(model),
                CullingVolumeTypes.CapsuleVolume => CollisionCapsuleFromModel(model),
                _ => CollisionSphereFromModel(model),
            };
            Collider.Attach(body);
        }

        public void Update()
        {
            if (Body == null)
            {
                return;
            }

            if (Model == null)
            {
                return;
            }

            Model.Manipulator.SetRotation(Body.Rotation);
            Model.Manipulator.SetPosition(Body.Position);
        }

        public void Reset(Matrix transform)
        {
            if (Body == null)
            {
                return;
            }

            Body.SetInitialState(transform);
        }

        public void Reset(Vector3 position, Quaternion rotation)
        {
            if (Body == null)
            {
                return;
            }

            Body.SetInitialState(position, rotation);
        }
    }
}
