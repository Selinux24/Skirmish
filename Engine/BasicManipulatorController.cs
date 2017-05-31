namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Manipulator controller interface
    /// </summary>
    public class BasicManipulatorController : ManipulatorController
    {
        /// <summary>
        /// Arriving threshold
        /// </summary>
        public float ArrivingThreshold = 0.01f;

        /// <summary>
        /// Computes current position and orientation in the curve
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="manipulator">Manipulator to update</param>
        public override void UpdateManipulator(GameTime gameTime, Manipulator3D manipulator)
        {
            if (this.HasPath)
            {
                var target = this.path.GetNextControlPoint(this.path.Length);
                var position = manipulator.Position;
                float dToTarget = (target - position).Length();

                if (dToTarget > this.ArrivingThreshold)
                {
                    float maxSpeed = manipulator.LinearVelocity * gameTime.ElapsedSeconds;

                    this.pathTime += maxSpeed;

                    var next = this.path.GetPosition(this.pathTime);
                    var velocity = next - position;

                    manipulator.SetPosition(next);
                    manipulator.LookAt(next + velocity, false);
                }
                else
                {
                    this.Clear();
                }
            }
        }
    }
}
