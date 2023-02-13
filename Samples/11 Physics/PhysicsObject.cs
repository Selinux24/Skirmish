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

        private static CollisionTriangleSoup CollisionTriangleSoupFromModel(Model model)
        {
            var tris = model.GetTriangles(true);
            tris = Triangle.Transform(tris, Matrix.Invert(model.Manipulator.FinalTransform));

            return new CollisionTriangleSoup(tris);
        }

        private static CollisionBox CollisionBoxFromModel(Model model)
        {
            return new CollisionBox(model.GetBoundingBox(true).GetExtents());
        }

        private static CollisionSphere CollisionSphereFromModel(Model model)
        {
            return new CollisionSphere(model.GetBoundingSphere(true).Radius);
        }

        public PhysicsObject(IRigidBody body, Model model)
        {
            Body = body ?? throw new ArgumentNullException(nameof(body), $"Physics object must have a rigid body.");
            Model = model ?? throw new ArgumentNullException(nameof(model), $"Physics object must have a model.");

            Collider = model.CullingVolumeType switch
            {
                CullingVolumeTypes.None => CollisionTriangleSoupFromModel(model),
                CullingVolumeTypes.BoxVolume => CollisionBoxFromModel(model),
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
