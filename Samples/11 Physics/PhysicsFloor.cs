using Engine;
using Engine.Physics;
using Engine.Physics.Colliders;
using SharpDX;
using System;

namespace Physics
{
    public class PhysicsFloor : IPhysicsObject
    {
        public IRigidBody RigidBody { get; private set; }
        public ICollider Collider { get; private set; }
        public Model Model { get; private set; }

        public PhysicsFloor(IRigidBody body, Model model)
        {
            RigidBody = body ?? throw new ArgumentNullException(nameof(body), $"Physics object must have a rigid body.");
            Model = model ?? throw new ArgumentNullException(nameof(model), $"Physics object must have a model.");

            var tris = model.GetTriangles(true);
            tris = Triangle.Transform(tris, Matrix.Invert(model.Manipulator.FinalTransform));
            Collider = new MeshCollider(tris);
            Collider.Attach(body);
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
