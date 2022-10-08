using Engine;
using Engine.Common;
using SharpDX;

namespace Terrain.Rts.Emitters
{
    /// <summary>
    /// Moving emitter
    /// </summary>
    public class MovingEmitter : ParticleEmitter
    {
        /// <summary>
        /// Manipulator to follow
        /// </summary>
        private readonly Manipulator3D manipulator;
        /// <summary>
        /// Relative delta position from manipulator position
        /// </summary>
        private readonly Vector3 delta;

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
            Vector3 rDelta = Vector3.Transform(delta, manipulator.Rotation);

            Position = manipulator.Position + rDelta;

            base.Update(context);
        }
    }
}
