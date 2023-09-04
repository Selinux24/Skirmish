using Engine;
using Engine.Physics;
using SharpDX;
using System.Collections.Generic;

namespace Physics
{
    class ColliderData
    {
        private readonly RigidBodyState rbState;
        private float time;

        public IPhysicsObject PhysicsObject { get; private set; }
        public Model Model { get; private set; }
        public IEnumerable<Line3D> Lines { get; set; }
        public ISceneLightPoint Light { get; private set; }

        public ColliderData(RigidBodyState rbState, Model model)
        {
            this.rbState = rbState;

            PhysicsObject = new PhysicsObject(model, new RigidBody(rbState));
            Model = model;
        }

        public void Initialize()
        {
            Model.Manipulator.SetTransform(rbState.InitialTransform);

            float radius = Model.GetBoundingSphere().Radius * 3f;
            Light = new SceneLightPoint(Model.Name, true, Model.TintColor.RGB(), Color.Yellow.RGB(), true, SceneLightPointDescription.Create(Vector3.Zero, radius, 2f));
        }

        public void UpdateBodyState(float elapsed, float bodyTime, float bodyDistance)
        {
            time += elapsed;

            if (time > bodyTime || Model.Manipulator.Position.LengthSquared() > bodyDistance)
            {
                Reset();
            }

            Model.Manipulator.UpdateInternals(true);
            Light.Position = PhysicsObject.RigidBody.Position;
            Light.Enabled = PhysicsObject.RigidBody.IsAwake;
        }

        public void SetLines(PrimitiveListDrawer<Line3D> lineDrawer)
        {
            lineDrawer.SetPrimitives(Color4.AdjustContrast(Model.TintColor, 0.1f), Line3D.Transform(Lines, Model.Manipulator.GlobalTransform));
        }

        public void Reset()
        {
            PhysicsObject.Reset(rbState.InitialTransform);
            time = 0;
        }
    }
}
