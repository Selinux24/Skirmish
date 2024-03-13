using Engine;
using SharpDX;

namespace TerrainSamples.SceneRts.Emitters
{
    /// <summary>
    /// Moving emitter
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="manipulator">Manipulator</param>
    /// <param name="delta">Relative delta position from manipulator position</param>
    public class MovingEmitter(IManipulator3D manipulator, Vector3 delta) : ParticleEmitter()
    {
        /// <summary>
        /// Manipulator to follow
        /// </summary>
        private readonly IManipulator3D manipulator = manipulator;
        /// <summary>
        /// Relative delta position from manipulator position
        /// </summary>
        private readonly Vector3 delta = delta;

        /// <inheritdoc/>
        public override void Update(IGameTime gameTime, Vector3 pointOfView)
        {
            Vector3 rDelta = Vector3.Transform(delta, manipulator.Rotation);

            Position = manipulator.Position + rDelta;

            base.Update(gameTime, pointOfView);
        }
    }
}
