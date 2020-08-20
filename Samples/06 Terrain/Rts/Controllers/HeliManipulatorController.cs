using Engine;

namespace Terrain.Rts.Controllers
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
        /// <param name="manipulator">Parent manipulator</param>
        public HeliManipulatorController(Manipulator3D manipulator) : base()
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

            float maxSpeed = this.MaximumSpeed * gameTime.ElapsedSeconds;

            var lookPos = pos + dir;
            lookPos.Y = pos.Y - (curSpeed / (maxSpeed == 0 ? 1 : maxSpeed));

            if (lookPos != pos)
            {
                manipulator.LookAt(lookPos, Axis.None, 0.1f, true);
            }
        }
    }
}
