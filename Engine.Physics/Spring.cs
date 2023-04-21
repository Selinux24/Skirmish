using SharpDX;
using System;

namespace Engine.Physics
{
    /// <summary>
    /// Spring force generator
    /// </summary>
    public class Spring : IForceGenerator
    {
        /// <summary>
        /// Spring connection point in local coordinates
        /// </summary>
        private readonly Vector3 connectionPoint;

        /// <summary>
        /// Connection point of the spring with the other body, in coordinates of the other body
        /// </summary>
        private readonly Vector3 otherConnectionPoint;
        /// <summary>
        /// The body on the other side of the spring
        /// </summary>
        private readonly IRigidBody other;

        /// <summary>
        /// Spring constant
        /// </summary>
        private readonly float springConstant;
        /// <summary>
        /// Length at which the spring is at rest
        /// </summary>
        private readonly float restLength;

        /// <summary>
        /// Constructor
        /// </summary>
        public Spring(Vector3 connectionPoint, IRigidBody other, Vector3 otherConnectionPoint, float springConstant, float restLength)
        {
            this.connectionPoint = connectionPoint;
            this.other = other;
            this.otherConnectionPoint = otherConnectionPoint;
            this.springConstant = springConstant;
            this.restLength = restLength;
        }

        /// <inheritdoc/>
        public void UpdateForce(IRigidBody rigidBody, float time)
        {
            // Calculate the two ends in world space
            Vector3 lws = rigidBody.GetPointInWorldSpace(connectionPoint);
            Vector3 ows = other.GetPointInWorldSpace(otherConnectionPoint);

            // Calculate the vector of the spring
            Vector3 force = lws - ows;

            // Calculate the magnitude of the force
            float magnitude = force.Length();
            magnitude = Math.Abs(magnitude - restLength);
            magnitude *= springConstant;

            // Calculate the final force and apply it
            force.Normalize();
            force *= -magnitude;
            rigidBody.AddForceAtPoint(force, lws);
        }
    }
}
