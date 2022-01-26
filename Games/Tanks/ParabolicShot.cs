using Engine;
using SharpDX;
using System;

namespace Tanks
{
    /// <summary>
    /// Paralbolic shot helper
    /// </summary>
    public class ParabolicShot : IShot
    {
        /// <summary>
        /// Gravity acceleration
        /// </summary>
        private readonly float g = 50f;

        /// <summary>
        /// Initial shot time
        /// </summary>
        private TimeSpan initialTime;
        /// <summary>
        /// Horizontal velocity component
        /// </summary>
        private Vector2 horizontalVelocity;
        /// <summary>
        /// Vertical velocity component
        /// </summary>
        private float verticalVelocity;
        /// <summary>
        /// Wind force (direction plus magnitude)
        /// </summary>
        private Vector3 wind;

        /// <summary>
        /// Configures the parabolic shot
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="shotDirection">Shot direction</param>
        /// <param name="shotForce">Shot force</param>
        /// <param name="windDirection">Wind direction</param>
        /// <param name="windForce">Wind force</param>
        public void Configure(GameTime gameTime, Vector3 shotDirection, float shotForce, Vector2 windDirection, float windForce)
        {
            initialTime = TimeSpan.FromMilliseconds(gameTime.TotalMilliseconds);
            var initialVelocity = shotDirection * shotForce;
            horizontalVelocity = initialVelocity.XZ();
            verticalVelocity = initialVelocity.Y;
            wind = new Vector3(windDirection.X, 0, windDirection.Y) * windForce;
        }

        /// <summary>
        /// Gets the horizontal shot distance at the specified time
        /// </summary>
        /// <param name="time">Time</param>
        public Vector2 GetHorizontalDistance(float time)
        {
            return horizontalVelocity * time;
        }
        /// <summary>
        /// Gets the vertical shot distance at the specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="shooterPosition">Shooter position</param>
        /// <param name="targetPosition">Target position</param>
        /// <remarks>Shooter and target positions were used for height difference calculation</remarks>
        public float GetVerticalDistance(float time, Vector3 shooterPosition, Vector3 targetPosition)
        {
            float h = shooterPosition.Y - targetPosition.Y;

            return h + (verticalVelocity * time) - (g * time * time / 2f);
        }

        /// <summary>
        /// Gets the horizontal velocity
        /// </summary>
        public Vector2 GetHorizontalVelocity()
        {
            return horizontalVelocity;
        }
        /// <summary>
        /// Gets the vertical velocity at the specified time
        /// </summary>
        /// <param name="time">Time</param>
        public float GetVerticalVelocity(float time)
        {
            return verticalVelocity - g * time;
        }

        /// <summary>
        /// Gets the horizontal acceleration
        /// </summary>
        public float GetHorizontalAcceleration()
        {
            return 0f;
        }
        /// <summary>
        /// Gets the vertical acceleration
        /// </summary>
        public float GetVerticalAcceleration()
        {
            return -g;
        }

        /// <summary>
        /// Gets the total time of flight of the projectile, from shooter to target
        /// </summary>
        /// <param name="shooterPosition">Shooter position</param>
        /// <param name="targetPosition">Target position</param>
        /// <returns>Returns the total time of flight of the projectile</returns>
        public float GetTimeOfFlight(Vector3 shooterPosition, Vector3 targetPosition)
        {
            float h = shooterPosition.Y - targetPosition.Y;
            if (h == 0)
            {
                return 2f * verticalVelocity / g;
            }
            else
            {
                return (verticalVelocity + (float)Math.Sqrt((verticalVelocity * verticalVelocity) + 2f * g * h)) / g;
            }
        }

        /// <summary>
        /// Gets the trajectory curve of the shot
        /// </summary>
        /// <param name="shooterPosition">Shooter position</param>
        /// <param name="targetPosition">Target position</param>
        /// <returns>Returns the trajectory curve of the shot</returns>
        public Curve3D ComputeCurve(Vector3 shooterPosition, Vector3 targetPosition)
        {
            Curve3D curve = new Curve3D();

            float flightTime = GetTimeOfFlight(shooterPosition, targetPosition);
            float sampleTime = 0.1f;
            for (float time = 0; time < flightTime; time += sampleTime)
            {
                Vector2 horizontalDist = GetHorizontalDistance(time);
                float verticalDist = GetVerticalDistance(time, shooterPosition, targetPosition);

                Vector3 position = new Vector3(horizontalDist.X, verticalDist, horizontalDist.Y) + (wind * time);
                curve.AddPosition(time, position);
            }

            return curve;
        }

        /// <summary>
        /// Integrates the shot in time
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="shooter">Shooter position</param>
        /// <param name="target">Target position</param>
        /// <returns>Returns the current parabolic shot position (relative to shooter position)</returns>
        public Vector3 Integrate(GameTime gameTime, Vector3 shooter, Vector3 target)
        {
            float time = (float)(gameTime.TotalSeconds - initialTime.TotalSeconds);

            Vector2 horizontalDist = GetHorizontalDistance(time);
            float verticalDist = GetVerticalDistance(time, shooter, target);

            return new Vector3(horizontalDist.X, verticalDist, horizontalDist.Y) + (wind * time);
        }
    }
}
