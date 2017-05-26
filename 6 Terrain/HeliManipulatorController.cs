using Engine;

namespace TerrainTest
{
    /// <summary>
    /// Helicopter controller
    /// </summary>
    public class HeliManipulatorController : SteerManipulatorController
    {
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
                //var nfw = Vector3.Normalize(position - this.velocity);
                //var pitch = MathUtil.Pi / 6f;
                //pitch *= (Vector3.Cross(nfw, this.velocity).Length() / maxspeed);

                //var prot = Quaternion.Slerp(manipulator.Rotation, Quaternion.RotationYawPitchRoll(0, -pitch, 0), 0.2f);
                //manipulator.SetRotation(prot, true);
            }
        }
    }
}
