using Engine;
using SharpDX;

namespace TerrainSamples.SceneRts.Emitters
{
    /// <summary>
    /// Moving emitter
    /// </summary>
    public class MovingEmitter : ParticleEmitter
    {
        /// <summary>
        /// Manipulator to follow
        /// </summary>
        private readonly IManipulator3D manipulator;
        /// <summary>
        /// Relative delta position from manipulator position
        /// </summary>
        private readonly Vector3 delta;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        /// <param name="delta">Relative delta position from manipulator position</param>
        public MovingEmitter(IManipulator3D manipulator, Vector3 delta) : base()
        {
            this.manipulator = manipulator;
            this.delta = delta;
        }

        /// <inheritdoc/>
        public override void Update(IGameTime gameTime, Vector3 pointOfView)
        {
            Vector3 rDelta = Vector3.Transform(delta, manipulator.Rotation);

            Position = manipulator.Position + rDelta;

            base.Update(gameTime, pointOfView);
        }
    }
}
