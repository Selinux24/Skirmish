using Engine;
using Engine.Physics;
using SharpDX;
using System;

namespace Physics
{
    public class PhysicsFloor : IPhysicsObject
    {
        public IRigidBody Body { get; private set; }
        public ICollisionPrimitive Collider { get; private set; }
        public Model Model { get; private set; }

        public PhysicsFloor(IRigidBody body, Model model)
        {
            Body = body ?? throw new ArgumentNullException(nameof(body), $"Physics object must have a rigid body.");
            Model = model ?? throw new ArgumentNullException(nameof(model), $"Physics object must have a model.");

            Collider = new CollisionPlane(new Plane(model.Manipulator.Up, 0));
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
