using Engine;
using Engine.Common;
using SharpDX;

namespace TerrainTest
{
    public class MovingEmitter : ParticleEmitter
    {
        private Manipulator3D manipulator;
        private Vector3 delta;

        public MovingEmitter(Manipulator3D manipulator, Vector3 delta) : base()
        {
            this.manipulator = manipulator;
            this.delta = delta;
        }

        public override void Update(UpdateContext context)
        {
            this.Position = this.manipulator.Position + this.delta;

            base.Update(context);
        }
    }
}
