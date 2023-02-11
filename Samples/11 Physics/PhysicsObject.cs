using Engine;
using Engine.Physics;
using SharpDX;
using System;

namespace Physics
{
    public class PhysicsObject : IPhysicsObject
    {
        public IRigidBody Body { get; private set; }
        public ICollisionPrimitive Collider { get; private set; }
        public Model Model { get; private set; }

        public PhysicsObject(IRigidBody body, Model model)
        {
            Body = body ?? throw new ArgumentNullException(nameof(body), $"Physics object must have a rigid body.");
            Model = model ?? throw new ArgumentNullException(nameof(model), $"Physics object must have a model.");

            Collider = model.CullingVolumeType switch
            {
                CullingVolumeTypes.None => new CollisionTriangleSoup(model.GetTriangles(true)),
                CullingVolumeTypes.BoxVolume => new CollisionBox(model.GetBoundingBox(true).GetExtents()),
                _ => new CollisionSphere(model.GetBoundingSphere(true).Radius),
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
