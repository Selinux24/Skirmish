using SharpDX;
using System;

namespace Engine.Physics
{
    /// <summary>
    /// Spring force generator
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public class Spring(IContactEndPoint source, IContactEndPoint target, float springConstant, float restLength) : ILocalForceGenerator
    {
        /// <summary>
        /// Spring constant
        /// </summary>
        private readonly float springConstant = springConstant;
        /// <summary>
        /// Length at which the spring is at rest
        /// </summary>
        private readonly float restLength = restLength;

        /// <inheritdoc/>
        public IContactEndPoint Source { get; private set; } = source;
        /// <inheritdoc/>
        public IContactEndPoint Target { get; private set; } = target;
        /// <inheritdoc/>
        public bool IsActive { get; private set; } = true;

        /// <inheritdoc/>
        public void UpdateForce(float time)
        {
            if (!IsActive)
            {
                return;
            }

            // Calculate the two ends in world space
            Vector3 swp = Source.PositionWorld;
            Vector3 twp = Target.PositionWorld;

            // Calculate the vector of the spring
            Vector3 force = twp - swp;

            // Calculate the magnitude of the force
            float magnitude = force.Length();
            magnitude = MathF.Abs(magnitude - restLength);
            magnitude *= springConstant;

            // Calculate the final force and apply it
            force.Normalize();
            force *= -magnitude;
            Target.Body.AddForceAtPoint(force, twp);
        }
    }
}
