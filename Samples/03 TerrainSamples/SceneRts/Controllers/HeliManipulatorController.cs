using Engine;

namespace TerrainSamples.SceneRts.Controllers
{
    /// <summary>
    /// Helicopter controller
    /// </summary>
    public class HeliManipulatorController : SteerManipulatorController
    {
        /// <summary>
        /// Previous speed
        /// </summary>
        private float prevSpeed = 0;

        /// <summary>
        /// Current acceleration
        /// </summary>
        public float Acceleration { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public HeliManipulatorController() : base()
        {

        }

        /// <summary>
        /// Updates the manipulator's view and position
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="manipulator">Manipulator</param>
        public override void UpdateManipulator(GameTime gameTime, Manipulator3D manipulator)
        {
            var prevPos = manipulator.Position;

            base.UpdateManipulator(gameTime, manipulator);

            manipulator.UpdateInternals(true);

            var pos = manipulator.Position;
            var dir = manipulator.Forward;

            var vel = pos - prevPos;
            vel.Y = 0;

            float curSpeed = vel.Length();

            var tDelta = gameTime.ElapsedSeconds;
            if (tDelta > 0)
            {
                var vDelta = curSpeed - prevSpeed;
                Acceleration = vDelta / tDelta;
            }
            prevSpeed = curSpeed;

            float maxSpeed = MaximumSpeed * gameTime.ElapsedSeconds;

            var lookPos = pos - dir;
            lookPos.Y = pos.Y - (curSpeed / (maxSpeed == 0 ? 1 : maxSpeed));

            if (lookPos != pos)
            {
                manipulator.RotateTo(lookPos, Axis.None, 0.1f);
            }
        }
    }
}
