using Engine;
using Engine.Physics;
using SharpDX;
using System.Collections.Generic;

namespace Physics
{
    class ColliderData
    {
        private readonly float mass;
        private readonly Matrix transform;
        private float time;

        public Model Model { get; set; }
        public IEnumerable<Line3D> Lines { get; set; }
        public IPhysicsObject PhysicsObject { get; private set; }
        public ISceneLightPoint Light { get; private set; }

        public ColliderData(float mass, Matrix transform)
        {
            this.mass = mass;
            this.transform = transform;
        }

        public void Initialize()
        {
            Model.Manipulator.SetTransform(transform);

            var rbState = new RigidBodyState
            {
                Mass = mass,
                InitialTransform = Model.Manipulator.FinalTransform,
            };

            PhysicsObject = new PhysicsObject(new RigidBody(rbState), Model);

            float radius = Model.GetBoundingSphere().Radius * 2f;
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
            Light.Position = PhysicsObject.Body.Position;
        }

        public void SetLines(PrimitiveListDrawer<Line3D> lineDrawer)
        {
            lineDrawer.SetPrimitives(Color4.AdjustContrast(Model.TintColor, 0.1f), Line3D.Transform(Lines, Model.Manipulator.FinalTransform));
        }

        public void Reset()
        {
            PhysicsObject.Reset(transform);
            time = 0;
        }
    }
}
