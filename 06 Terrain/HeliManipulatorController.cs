using Engine;
using SharpDX;

namespace Terrain
{
    /// <summary>
    /// Helicopter controller
    /// </summary>
    public class HeliManipulatorController : SteerManipulatorController
    {
        /// <summary>
        /// Internal manipulator
        /// </summary>
        private Manipulator3D internalManipulator = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="manipulator">Parent manipulator</param>
        public HeliManipulatorController(Manipulator3D manipulator) : base()
        {
            this.internalManipulator = new Manipulator3D();
            this.internalManipulator.Parent = manipulator;
        }

        /// <summary>
        /// Updates the manipulator's view and position
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="manipulator">Manipulator</param>
        public override void UpdateManipulator(GameTime gameTime, Manipulator3D manipulator)
        {
            base.UpdateManipulator(gameTime, manipulator);

            if (this.HasPath)
            {
                this.internalManipulator.SetRotation(0, 0, MathUtil.PiOverFour, true);
            }
        }
    }
}
