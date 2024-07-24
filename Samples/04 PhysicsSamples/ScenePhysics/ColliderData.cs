using Engine;
using Engine.BuiltIn.Components.Models;
using Engine.BuiltIn.Components.Primitives;
using Engine.Physics;
using SharpDX;
using System.Collections.Generic;

namespace PhysicsSamples.ScenePhysics
{
    class ColliderData(RigidBodyState rbState, Model model)
    {
        private readonly RigidBodyState rbState = rbState;
        private float time;

        public IPhysicsObject PhysicsObject { get; private set; } = new PhysicsObject(model, new RigidBody(rbState));
        public Model Model { get; private set; } = model;
        public IEnumerable<Line3D> Lines { get; set; }
        public ISceneLightPoint Light { get; private set; }

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

        public void SetLines(GeometryColorDrawer<Line3D> lineDrawer)
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
