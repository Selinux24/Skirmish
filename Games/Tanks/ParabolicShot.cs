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

        /// <inheritdoc/>
        public void Configure(IGameTime gameTime, Vector3 shotDirection, float shotForce, Vector2 windDirection, float windForce)
        {
            initialTime = TimeSpan.FromMilliseconds(gameTime.TotalMilliseconds);
            var initialVelocity = shotDirection * shotForce;
            horizontalVelocity = initialVelocity.XZ();
            verticalVelocity = initialVelocity.Y;
            wind = new Vector3(windDirection.X, 0, windDirection.Y) * windForce;
        }

        /// <inheritdoc/>
        public Vector2 GetHorizontalDistance(float time)
        {
            return horizontalVelocity * time;
        }
        /// <inheritdoc/>
        public float GetVerticalDistance(float time, Vector3 shooterPosition, Vector3 targetPosition)
        {
            float h = shooterPosition.Y - targetPosition.Y;

            return h + (verticalVelocity * time) - (g * time * time / 2f);
        }

        /// <inheritdoc/>
        public Vector2 GetHorizontalVelocity()
        {
            return horizontalVelocity;
        }
        /// <inheritdoc/>
        public float GetVerticalVelocity(float time)
        {
            return verticalVelocity - g * time;
        }

        /// <inheritdoc/>
        public float GetHorizontalAcceleration()
        {
            return 0f;
        }
        /// <inheritdoc/>
        public float GetVerticalAcceleration()
        {
            return -g;
        }

        /// <inheritdoc/>
        public float GetTimeOfFlight(Vector3 shooterPosition, Vector3 targetPosition)
        {
            float h = shooterPosition.Y - targetPosition.Y;
            if (MathUtil.IsZero(h))
            {
                return 2f * verticalVelocity / g;
            }
            else
            {
                return (verticalVelocity + (float)Math.Sqrt((verticalVelocity * verticalVelocity) + 2f * g * h)) / g;
            }
        }

        /// <inheritdoc/>
        public Curve3D ComputeCurve(Vector3 shooterPosition, Vector3 targetPosition)
        {
            var curve = new Curve3D();

            float flightTime = GetTimeOfFlight(shooterPosition, targetPosition);
            float sampleTime = 0.1f;
            for (float time = 0; time < flightTime; time += sampleTime)
            {
                var horizontalDist = GetHorizontalDistance(time);
                float verticalDist = GetVerticalDistance(time, shooterPosition, targetPosition);

                var position = new Vector3(horizontalDist.X, verticalDist, horizontalDist.Y) + (wind * time);
                curve.AddPosition(time, position);
            }

            return curve;
        }

        /// <inheritdoc/>
        public Vector3 Integrate(IGameTime gameTime, Vector3 shooter, Vector3 target)
        {
            float time = (float)(gameTime.TotalSeconds - initialTime.TotalSeconds);

            Vector2 horizontalDist = GetHorizontalDistance(time);
            float verticalDist = GetVerticalDistance(time, shooter, target);

            return new Vector3(horizontalDist.X, verticalDist, horizontalDist.Y) + (wind * time);
        }
    }
}
