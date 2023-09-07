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
        /// Maps n into start and stop pairs
        /// </summary>
        /// <param name="n">Value to map</param>
        /// <param name="start1">Start reference 1</param>
        /// <param name="stop1">Stop reference 1</param>
        /// <param name="start2">Start reference 2</param>
        /// <param name="stop2">Stop reference 2</param>
        /// <returns>Returns mapped value of n</returns>
        public static float Map(float n, float start1, float stop1, float start2, float stop2)
        {
            return (n - start1) / (stop1 - start1) * (stop2 - start2) + start2;
        }

        /// <summary>
        /// Arriving radius
        /// </summary>
        public float ArrivingRadius { get; set; } = 10f;
        /// <summary>
        /// Arriving threshold
        /// </summary>
        public float ArrivingThreshold { get; set; } = 0.01f;

        /// <summary>
        /// Updates the manipulator's view and position
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="manipulator">Manipulator</param>
        public override void UpdateManipulator(GameTime gameTime, Manipulator3D manipulator)
        {
            if (!HasPath)
            {
                return;
            }

            var target = path.GetNextControlPoint(path.Length);
            var position = manipulator.Position;
            float dToTarget = (target - position).Length();

            if (dToTarget <= ArrivingThreshold)
            {
                Clear();

                return;
            }

            float maxSpeed = MaximumSpeed * gameTime.ElapsedSeconds;
            float maxForce = MaximumForce * gameTime.ElapsedSeconds;

            var next = path.GetNextControlPoint(pathTime + maxSpeed);

            // A vector pointing from the location to the target
            var desired = next - position;
            float dToNext = desired.Length();
            if (dToNext == 0)
            {
                return;
            }

            if (dToTarget < ArrivingRadius)
            {
                var m = Map(dToTarget, 0, ArrivingRadius, 0, maxSpeed);
                desired = Vector3.Normalize(desired) * m;
            }
            else
            {
                desired = Vector3.Normalize(desired) * maxSpeed;
            }

            // Steering = Desired minus Velocity
            var steer = desired - Velocity;

            // Limit to maximum steering force
            steer = steer.Limit(maxForce);

            // Update velocity
            Velocity += steer;

            // Limit speed
            Velocity = Velocity.Limit(maxSpeed);

            pathTime += Velocity.Length();
            var newPosition = path.GetPosition(pathTime);
            var newNormal = path.GetNormal(pathTime);

            manipulator.SetPosition(newPosition, true);
            manipulator.LookAt(newPosition + (newPosition - position), newNormal, Axis.Y, 0.1f);
        }

        /// <inheritdoc/>
        public override IGameState GetState()
        {
            return new SteerManipulatorControllerState
            {
                Path = path,
                PathTime = pathTime,
                Velocity = Velocity,
                MaximumSpeed = MaximumSpeed,
                MaximumForce = MaximumForce,
                ArrivingThreshold = ArrivingThreshold,
                ArrivingRadius = ArrivingRadius,
            };
        }
        /// <inheritdoc/>
        public override void SetState(IGameState state)
        {
            if (state is not SteerManipulatorControllerState steerManipulator)
            {
                return;
            }

            path = steerManipulator.Path;
            pathTime = steerManipulator.PathTime;
            Velocity = steerManipulator.Velocity;
            MaximumForce = steerManipulator.MaximumForce;
            MaximumSpeed = steerManipulator.MaximumSpeed;
            ArrivingThreshold = steerManipulator.ArrivingThreshold;
            ArrivingRadius = steerManipulator.ArrivingRadius;
        }
    }
}
