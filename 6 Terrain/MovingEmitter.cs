using Engine;
using Engine.Common;
using SharpDX;

namespace TerrainTest
{
    /// <summary>
    /// Moving emitter
    /// </summary>
    public class MovingEmitter : ParticleEmitter
    {
        /// <summary>
        /// Manipulator to follow
        /// </summary>
        private Manipulator3D manipulator;
        /// <summary>
        /// Relative delta position from manipulator position
        /// </summary>
        private Vector3 delta;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        /// <param name="delta">Relative delta position from manipulator position</param>
        public MovingEmitter(Manipulator3D manipulator, Vector3 delta) : base()
        {
            this.manipulator = manipulator;
            this.delta = delta;
        }

        /// <summary>
        /// Updates the emitter state
        /// </summary>
        /// <param name="context">Updating context</param>
        public override void Update(UpdateContext context)
        {
            Vector3 rDelta = Vector3.Transform(this.delta, this.manipulator.Rotation);

            this.Position = this.manipulator.Position + rDelta;

            base.Update(context);
        }
    }

    public class LinealEmitter : ParticleEmitter
    {
        private Vector3 from;
        private Vector3 to;
        private float speed;
        private Vector3 direction;

        public LinealEmitter(Vector3 from, Vector3 to, float speed) : base()
        {
            this.Position = from;
            this.from = from;
            this.to = to;
            this.speed = speed;
            this.direction = (to - from);
            float distance = direction.Length();
            this.direction.Normalize();

            this.Duration = distance / speed;
        }

        public override void Update(UpdateContext context)
        {
            this.Position += (direction * speed * context.GameTime.ElapsedSeconds);

            base.Update(context);
        }
    }
}
