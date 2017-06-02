using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Helicopter controller
    /// </summary>
    public class SteerManipulatorController : ManipulatorController
    {
        /// <summary>
        /// Current velocity
        /// </summary>
        protected Vector3 Velocity;
        /// <summary>
        /// Maximum force
        /// </summary>
        public float MaximumForce = 0.1f;
        /// <summary>
        /// Arriving radius
        /// </summary>
        public float ArrivingRadius = 10f;
        /// <summary>
        /// Arriving threshold
        /// </summary>
        public float ArrivingThreshold = 0.01f;

        /// <summary>
        /// Updates the manipulator's view and position
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="manipulator">Manipulator</param>
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
                    float maxForce = this.MaximumForce;

                    this.pathTime += maxSpeed;

                    var next = this.path.GetNextControlPoint(this.pathTime);

                    // A vector pointing from the location to the target
                    var desired = next - position;
                    float dToNext = desired.Length();
                    if (dToNext != 0)
                    {
                        if (dToTarget < this.ArrivingRadius)
                        {
                            var m = dToTarget.Map(0, this.ArrivingRadius, 0, maxSpeed);
                            desired = Vector3.Normalize(desired) * m;
                        }
                        else
                        {
                            desired = Vector3.Normalize(desired) * maxSpeed;
                        }

                        // Steering = Desired minus Velocity
                        var steer = desired - this.Velocity;

                        // Limit to maximum steering force
                        steer = steer.Limit(maxForce);

                        // Update velocity
                        this.Velocity += steer;

                        // Limit speed
                        this.Velocity = this.Velocity.Limit(maxSpeed);

                        position += this.Velocity;

                        manipulator.SetPosition(position);
                        manipulator.LookAt(position + this.Velocity, false);
                    }
                }
                else
                {
                    this.Clear();
                }
            }
        }
    }
}
