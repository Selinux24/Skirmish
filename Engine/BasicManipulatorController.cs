
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
        public float ArrivingThreshold { get; set; } = 0.01f;

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
                    float maxSpeed = this.MaximumSpeed * gameTime.ElapsedSeconds;

                    this.pathTime += maxSpeed;

                    var next = this.path.GetPosition(this.pathTime);
                    this.Velocity = next - position;

                    manipulator.SetPosition(next, true);
                    manipulator.LookAt(next + this.Velocity, Axis.None, 0, true);
                }
                else
                {
                    this.Clear();
                }
            }
        }

        /// <inheritdoc/>
        public override IGameState GetState()
        {
            return new BasicManipulatorControllerState
            {
                Path = path,
                PathTime = pathTime,
                Velocity = Velocity,
                MaximumSpeed = MaximumSpeed,
                MaximumForce = MaximumForce,
                ArrivingThreshold = ArrivingThreshold,
            };
        }
        /// <inheritdoc/>
        public override void SetState(IGameState state)
        {
            if (state is not BasicManipulatorControllerState basicManipulator)
            {
                return;
            }

            path = basicManipulator.Path;
            pathTime = basicManipulator.PathTime;
            Velocity = basicManipulator.Velocity;
            MaximumForce = basicManipulator.MaximumForce;
            MaximumSpeed = basicManipulator.MaximumSpeed;
            ArrivingThreshold = basicManipulator.ArrivingThreshold;
        }
    }
}
