using Engine;

namespace Terrain.Controllers
{
    /// <summary>
    /// Helicopter controller
    /// </summary>
    public class HeliManipulatorController : SteerManipulatorController
    {
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

            manipulator.Update(gameTime);

            var pos = manipulator.Position;
            var dir = manipulator.Forward;

            var vel = pos - prevPos;
            vel.Y = 0;

            float maxSpeed = this.MaximumSpeed * gameTime.ElapsedSeconds;
            float curSpeed = vel.Length();

            var lookPos = pos + dir;
            lookPos.Y = pos.Y - (1.5f * (curSpeed / (maxSpeed == 0 ? 1 : maxSpeed)));

            if (lookPos != pos)
            {
                manipulator.LookAt(lookPos, false, 0.05f, true);
            }
        }
    }
}
