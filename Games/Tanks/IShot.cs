using Engine;
using SharpDX;

namespace Tanks
{
    /// <summary>
    /// Shot interface
    /// </summary>
    public interface IShot
    {
        /// <summary>
        /// Configures the parabolic shot
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="shotDirection">Shot direction</param>
        /// <param name="shotForce">Shot force</param>
        /// <param name="windDirection">Wind direction</param>
        /// <param name="windForce">Wind force</param>
        void Configure(GameTime gameTime, Vector3 shotDirection, float shotForce, Vector2 windDirection, float windForce);

        /// <summary>
        /// Gets the horizontal shot distance at the specified time
        /// </summary>
        /// <param name="time">Time</param>
        Vector2 GetHorizontalDistance(float time);
        /// <summary>
        /// Gets the vertical shot distance at the specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="shooterPosition">Shooter position</param>
        /// <param name="targetPosition">Target position</param>
        /// <remarks>Shooter and target positions were used for height difference calculation</remarks>
        float GetVerticalDistance(float time, Vector3 shooterPosition, Vector3 targetPosition);

        /// <summary>
        /// Gets the horizontal velocity
        /// </summary>
        Vector2 GetHorizontalVelocity();
        /// <summary>
        /// Gets the vertical velocity at the specified time
        /// </summary>
        /// <param name="time">Time</param>
        float GetVerticalVelocity(float time);

        /// <summary>
        /// Gets the horizontal acceleration
        /// </summary>
        float GetHorizontalAcceleration();
        /// <summary>
        /// Gets the vertical acceleration
        /// </summary>
        float GetVerticalAcceleration();

        /// <summary>
        /// Gets the total time of flight of the projectile, from shooter to target
        /// </summary>
        /// <param name="shooterPosition">Shooter position</param>
        /// <param name="targetPosition">Target position</param>
        /// <returns>Returns the total time of flight of the projectile</returns>
        float GetTimeOfFlight(Vector3 shooterPosition, Vector3 targetPosition);

        /// <summary>
        /// Gets the trajectory curve of the shot
        /// </summary>
        /// <param name="shooterPosition">Shooter position</param>
        /// <param name="targetPosition">Target position</param>
        /// <returns>Returns the trajectory curve of the shot</returns>
        Curve3D ComputeCurve(Vector3 shooterPosition, Vector3 targetPosition);

        /// <summary>
        /// Integrates the shot in time
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="shooter">Shooter position</param>
        /// <param name="target">Target position</param>
        /// <returns>Returns the current parabolic shot position (relative to shooter position)</returns>
        Vector3 Integrate(GameTime gameTime, Vector3 shooter, Vector3 target);
    }
}
