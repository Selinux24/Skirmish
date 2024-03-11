using Engine;
using SharpDX;
using System;

namespace TerrainSamples.SceneRts.Controllers
{
    /// <summary>
    /// Tank controller
    /// </summary>
    public class TankManipulatorController : SteerManipulatorController
    {
        /// <inheritdoc/>
        public override void UpdateManipulator(IGameTime gameTime, IManipulator3D manipulator)
        {
            if (!HasPath)
            {
                return;
            }

            var target = path.GetNextControlPoint(path.Length);
            var position = manipulator.Position;
            float dToTarget = (target - position).Length();

            if (dToTarget > ArrivingThreshold)
            {
                MoveToTarget(gameTime, manipulator, dToTarget);
            }
            else
            {
                Clear();
            }
        }
        /// <summary>
        /// Move to target
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="manipulator">Manipulator</param>
        /// <param name="distanceToTarget">Distance to target</param>
        private void MoveToTarget(IGameTime gameTime, IManipulator3D manipulator, float distanceToTarget)
        {
            var position = manipulator.Position;
            var rotation = manipulator.Rotation;

            float maxSpeed = MaximumSpeed * gameTime.ElapsedSeconds;
            float maxForce = MaximumForce * gameTime.ElapsedSeconds;

            var next = path.GetNextControlPoint(pathTime + maxSpeed);

            // A vector pointing from the location to the target
            var desired = (next - position);
            float dToNext = desired.Length();
            if (MathUtil.IsZero(dToNext))
            {
                return;
            }

            if (distanceToTarget < ArrivingRadius)
            {
                var m = Map(distanceToTarget, 0, ArrivingRadius, 0, maxSpeed);
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
            var newVelocity = Velocity + steer;

            // Limit speed
            newVelocity = newVelocity.Limit(maxSpeed);

            //Calculates 2 seconds in future
            var futureTime = pathTime + 2;
            var futurePosition = path.GetPosition(futureTime);
            var futureNormal = path.GetNormal(futureTime);
            var futureTarget = futurePosition + (futurePosition - position);

            //Calculates a delta using the future angle
            var futureRotation = Helper.LookAt(position, futureTarget, futureNormal, Axis.Y);
            float futureAngle = Helper.Angle(rotation, futureRotation);
            float maxRot = MathUtil.PiOverTwo;
            futureAngle = Math.Min(futureAngle, maxRot);
            float velDelta = 1.0f - (futureAngle / maxRot);

            //Apply delta to velocity
            newVelocity *= velDelta;
            Velocity = newVelocity;

            if (MathUtil.IsZero(velDelta) && !MathUtil.IsZero(futureAngle))
            {
                //Rotates only
                manipulator.RotateTo(futureTarget, futureNormal, Axis.Y, 0.01f);
            }
            else
            {
                //Gets new time
                var newTime = pathTime + newVelocity.Length();
                var newPosition = path.GetPosition(newTime);
                var newNormal = path.GetNormal(newTime);
                var newTarget = newPosition + (newPosition - position);

                //Rotate and move
                manipulator.SetPosition(newPosition);
                manipulator.RotateTo(newTarget, newNormal, Axis.Y, 0.01f);
                manipulator.SetNormal(newNormal);

                //Updates new time in curve
                pathTime = newTime;
            }
        }
    }
}
