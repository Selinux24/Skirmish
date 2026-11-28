using Engine.Mathematics;
using System;

namespace Engine.Physics
{
    /// <summary>
    /// Contact between two bodies
    /// </summary>
    public class Contact
    {
        const float velocityLimit = 0.25f;

        /// <summary>
        /// First body
        /// </summary>
        private IRigidBody body1;
        /// <summary>
        /// Second body
        /// </summary>
        private IRigidBody body2;
        /// <summary>
        /// First body relative contact positions in world coordinates.
        /// </summary>
        private Vector3 relativeContactPositionsWorld1;
        /// <summary>
        /// Second body relative contact positions in world coordinates.
        /// </summary>
        private Vector3 relativeContactPositionsWorld2;
        /// <summary>
        /// Friction.
        /// </summary>
        private float friction;
        /// <summary>
        /// Restitution.
        /// </summary>
        private float restitution;
        /// <summary>
        /// Contact velocity.
        /// </summary>
        private Vector3 velocity;
        /// <summary>
        /// Orthonormal matrix for local-to-world transforms.
        /// </summary>
        private Matrix3x3 contactToWorld;
        /// <summary>
        /// Position
        /// </summary>
        private Vector3 position;
        /// <summary>
        /// Normal
        /// </summary>
        private Vector3 normal;
        /// <summary>
        /// Penetration
        /// </summary>
        private float penetration;
        /// <summary>
        /// Desired delta velocity
        /// </summary>
        private float desiredDeltaVelocity;

        /// <summary>
        /// Contact position in world coordinates.
        /// </summary>
        public Vector3 Position => position;
        /// <summary>
        /// Contact normal in world coordinates.
        /// </summary>
        public Vector3 Normal => normal;
        /// <summary>
        /// Contact penetration depth. Usually just between both contacts.
        /// </summary>
        public float Penetration => penetration;
        /// <summary>
        /// Desired delta velocity.
        /// </summary>
        public float DesiredDeltaVelocity => desiredDeltaVelocity;

        /// <summary>
        /// Get contacting body
        /// </summary>
        /// <param name="index">Body index</param>
        public IRigidBody GetBody(int index)
        {
            int i = index % 2;

            return i == 0 ? body1 : body2;
        }
        /// <summary>
        /// Get relative contact position
        /// </summary>
        /// <param name="index">Contact position index</param>
        public Vector3 GetRelativeContactPosition(int index)
        {
            int i = index % 2;

            return i == 0 ? relativeContactPositionsWorld1 : relativeContactPositionsWorld2;
        }

        /// <summary>
        /// Calculates the orthonormal basis for the contact point, based on the primary direction of friction or a random orientation.
        /// </summary>
        /// <param name="contactNormal">Contact normal</param>
        /// <remarks>
        /// Primary direction of friction for anisotropic friction or random orientation for isotropic friction
        /// </remarks>
        /// <returns>Returns the contact basis matrix</returns>
        private static Matrix3x3 CalculateContactBasis(ref Vector3 contactNormal)
        {
            Vector3 contactTangent1;
            Vector3 contactTangent2;

            // Check if the Z axis is closer to the X axis or the Y axis
            if (MathF.Abs(contactNormal.X) > MathF.Abs(contactNormal.Y))
            {
                // Get a scaling factor to make sure the results are normalized
                float s = 1f / MathF.Sqrt(contactNormal.Z * contactNormal.Z + contactNormal.X * contactNormal.X);

                // The new X axis is 90 degrees to the Y axis of the world
                contactTangent1.X = contactNormal.Z * s;
                contactTangent1.Y = 0;
                contactTangent1.Z = -contactNormal.X * s;

                // The new Y axis is 90 degrees to the new X and Z axes
                contactTangent2.X = contactNormal.Y * contactTangent1.X;
                contactTangent2.Y = contactNormal.Z * contactTangent1.X - contactNormal.X * contactTangent1.Z;
                contactTangent2.Z = -contactNormal.Y * contactTangent1.X;
            }
            else
            {
                // Get a scaling factor to make sure the results are normalized
                float s = 1.0f / MathF.Sqrt(contactNormal.Z * contactNormal.Z + contactNormal.Y * contactNormal.Y);

                // The new X axis is 90 degrees to the X axis of the world
                contactTangent1.X = 0;
                contactTangent1.Y = -contactNormal.Z * s;
                contactTangent1.Z = contactNormal.Y * s;

                // The new Y axis is 90 degrees to the new X and Z axes
                contactTangent2.X = contactNormal.Y * contactTangent1.Z - contactNormal.Z * contactTangent1.Y;
                contactTangent2.Y = -contactNormal.X * contactTangent1.Z;
                contactTangent2.Z = contactNormal.X * contactTangent1.Y;
            }

            return new()
            {
                Column1 = contactNormal,
                Column2 = contactTangent1,
                Column3 = contactTangent2,
            };
        }

        /// <summary>
        /// Sets the contact data
        /// </summary>
        /// <param name="body1">First body</param>
        /// <param name="body2">Second body</param>
        /// <param name="position">Position in world coordinates</param>
        /// <param name="normal">Normal in world coordinates</param>
        /// <param name="penetration">Penetration</param>
        /// <param name="restitution">Restitution</param>
        /// <param name="friction">Friction</param>
        public bool SetContactData(IRigidBody body1, IRigidBody body2, ref Vector3 position, ref Vector3 normal, float penetration, float restitution, float friction)
        {
            if (!body1.HasFiniteMass() && !body2.HasFiniteMass())
            {
                return false;
            }

            this.body1 = body1;
            this.body2 = body2;

            this.restitution = restitution;
            this.friction = friction;

            this.position = position;
            this.normal = normal;
            this.penetration = penetration;

            if (!this.body1.HasFiniteMass())
            {
                SwapBodies();
            }

            return true;
        }
        /// <summary>
        /// Swap body references.
        /// </summary>
        private void SwapBodies()
        {
            normal *= -1f;

            (body2, body1) = (body1, body2);
        }

        /// <summary>
        /// Calculate internal contact status data. This function is called before the resolution of the contact resolution algorithm.
        /// </summary>
        /// <param name="time">Time</param>
        public void CalculateInternals(float time)
        {
            if (body1 == null || body2 == null)
            {
                return;
            }

            // Calculate a coordinate axis from the contact point
            contactToWorld = CalculateContactBasis(ref normal);

            // Store the relative position of the contact, with respect to each body
            // & find the relative speed of each body at the moment of collision.
            relativeContactPositionsWorld1 = position - body1.Position;
            velocity = CalculateLocalVelocity(body1, relativeContactPositionsWorld1, time);
            if (body2.HasFiniteMass())
            {
                relativeContactPositionsWorld2 = position - body2.Position;
                velocity -= CalculateLocalVelocity(body2, relativeContactPositionsWorld2, time);
            }

            // Calculate the velocity needed to resolve the contact
            CalculateDesiredDeltaVelocity(time);
        }
        /// <summary>
        /// Gets the velocity of the specified body contact point.
        /// </summary>
        /// <param name="body">Rigid body</param>
        /// <param name="relativeContactPositionWorld">Relative contact position world</param>
        /// <param name="time">Time</param>
        private Vector3 CalculateLocalVelocity(IRigidBody body, Vector3 relativeContactPositionWorld, float time)
        {
            // Extract the velocity at the point of contact.
            var bodyVelocity = Vector3.Cross(body.AngularVelocity, relativeContactPositionWorld);
            bodyVelocity += body.LinearVelocity;

            // Convert velocity to contact coordinates.
            Matrix3x3.TransformTranspose(ref contactToWorld, ref bodyVelocity, out var contactVelocity);

            // Calculate the amount of velocity available for forces without taking reactions into account.
            var accVelocity = body.LastFrameAcceleration * time;

            // Calculate amount of velocity in contact coordinates.
            Matrix3x3.TransformTranspose(ref contactToWorld, ref accVelocity, out accVelocity);

            // Acceleration components in the direction of the contact normal are ignored..
            // Only accelerations in the plane are taken into account.
            accVelocity.X = 0;

            // Add the planar accelerations.
            // If there is enough friction, the forces will be removed during velocity resolution..
            contactVelocity += accVelocity;

            return contactVelocity;
        }
        /// <summary>
        /// Calculate and set the velocity needed to resolve contact.
        /// </summary>
        /// <param name="time">Time</param>
        public void CalculateDesiredDeltaVelocity(float time)
        {
            // Calculate the velocity accumulated by the acceleration in this interval
            float velocityFromAcc = 0;

            if (body2.HasFiniteMass() && body2.IsAwake)
            {
                velocityFromAcc -= Vector3.Dot(body2.LastFrameAcceleration * time, normal);
            }

            // If the speed is very slow, it is necessary to limit the restitution
            float thisRestitution = restitution;
            if (MathF.Abs(velocity.X) < velocityLimit)
            {
                thisRestitution = 0.0f;
            }

            // Combine dumping speed with speed taken from acceleration
            desiredDeltaVelocity = -velocity.X - thisRestitution * (velocity.X - velocityFromAcc);
        }

        /// <summary>
        /// Updates the activation state of the contact's bodies.
        /// </summary>
        /// <remarks>
        /// A body will activate if it comes into contact with an active body.
        /// </remarks>
        public void MatchAwakeState()
        {
            if (!body2.HasFiniteMass())
            {
                return;
            }

            bool body1awake = body1.IsAwake;
            bool body2awake = body2.IsAwake;

            // Awaken only the one who sleeps
            if (body1awake ^ body2awake)
            {
                if (body1awake)
                {
                    body2.SetAwakeState(true);
                }
                else
                {
                    body1.SetAwakeState(true);
                }
            }
        }

        /// <summary>
        /// Performs contact penetration resolution based on inertia.
        /// </summary>
        /// <param name="linearChange">Linear change</param>
        /// <param name="angularChange">Angular change</param>
        /// <param name="penetration">Penetration</param>
        public void ApplyPositionChange(float penetration, out Vector3[] linearChange, out Vector3[] angularChange)
        {
            linearChange = new Vector3[2];
            angularChange = new Vector3[2];

            ApplyPositionChange(penetration, out linearChange[0], out linearChange[1], out angularChange[0], out angularChange[1]);
        }
        /// <summary>
        /// Performs contact penetration resolution based on inertia.
        /// </summary>
        /// <param name="penetration">Penetration</param>
        /// <param name="linearChange1">Linear change 1</param>
        /// <param name="linearChange2">Linear change 2</param>
        /// <param name="angularChange1">Angular change 1</param>
        /// <param name="angularChange2">Angular change 2</param>
        public void ApplyPositionChange(float penetration, out Vector3 linearChange1, out Vector3 linearChange2, out Vector3 angularChange1, out Vector3 angularChange2)
        {
            linearChange1 = default;
            linearChange2 = default;
            angularChange1 = default;
            angularChange2 = default;

            // We have to work with the inertia of each body in the direction of the contact normal, and the angular inertia.
            ApplyInertia(out var totalInertia, out var linearInertia1, out var linearInertia2, out var angularInertia1, out var angularInertia2);

            // Iterate again calculating the changes and applying them
            float angularLimit = 0.2f;
            for (int i = 0; i < 2; i++)
            {
                var body = GetBody(i);
                if (!body.HasFiniteMass())
                {
                    continue;
                }

                // The angular and linear movements are proportional to the two inverse inertias.
                float sign = (i == 0) ? 1 : -1;
                float angularInertia = i == 0 ? angularInertia1 : angularInertia2;
                float linearInertia = i == 0 ? linearInertia1 : linearInertia2;
                float angularMove = sign * penetration * (angularInertia / totalInertia);
                float linearMove = sign * penetration * (linearInertia / totalInertia);

                // To avoid too large angular projections, the angular movement is limited.
                var relativeContactPosition = GetRelativeContactPosition(i);
                var projection = relativeContactPosition;
                projection += Vector3.Multiply(normal, Vector3.Dot(-relativeContactPosition, normal));

                float maxMagnitude = angularLimit * projection.Length();

                if (angularMove < -maxMagnitude)
                {
                    float totalMove = angularMove + linearMove;
                    angularMove = -maxMagnitude;
                    linearMove = totalMove - angularMove;
                }
                else if (angularMove > maxMagnitude)
                {
                    float totalMove = angularMove + linearMove;
                    angularMove = maxMagnitude;
                    linearMove = totalMove - angularMove;
                }

                // We have the linear amount of motion required to rotate the body.
                // Now you have to calculate the desired rotation to make it rotate.
                Vector3 angularChange;
                if (MathUtils.IsZero(angularMove))
                {
                    // There is no angular movement. No rotation.
                    angularChange = Vector3.Zero;
                }
                else
                {
                    // Get direction of rotation.
                    var targetAngularDirection = Vector3.Cross(relativeContactPosition, normal);

                    var inverseInertiaTensor = body.InverseInertiaTensorWorld;

                    inverseInertiaTensor.Transform(ref targetAngularDirection, out var t);
                    angularChange = t * (angularMove / angularInertia);
                }

                // Velocity variation: linear movement on the normal of contact.
                var linearChange = normal * linearMove;
                if (i == 0)
                {
                    linearChange1 = linearChange;
                    angularChange1 = angularChange;
                }
                else
                {
                    linearChange2 = linearChange;
                    angularChange2 = angularChange;
                }

                // Apply linear motion
                Vector3.Multiply(ref normal, linearMove, out var positionChange);
                body.AddPosition(positionChange);

                // Apply the change in orientation
                var orientationChange = new Quaternion(angularChange, 0f) * body.Rotation * Constants.OrientationContactFactor;
                body.AddOrientation(orientationChange);

                // You have to update each body that is active, so that the changes are reflected in the body.
                // Otherwise, the resolution will not change the position for the object, and the next round of collision detection will result in the same penetration.
                if (!body.IsAwake)
                {
                    body.CalculateDerivedData();
                }
            }
        }
        /// <summary>
        /// Calculates the inertia of the position change
        /// </summary>
        /// <param name="totalInertia">Total inertia value</param>
        /// <param name="linearInertia">Linear inertia of each body</param>
        /// <param name="angularInertia">Angular inertia of each body</param>
        private void ApplyInertia(out float totalInertia, out float linearInertia1, out float linearInertia2, out float angularInertia1, out float angularInertia2)
        {
            totalInertia = 0f;
            linearInertia1 = 0f;
            linearInertia2 = 0f;
            angularInertia1 = 0f;
            angularInertia2 = 0f;

            for (int i = 0; i < 2; i++)
            {
                var body = GetBody(i);
                if (!body.HasFiniteMass())
                {
                    continue;
                }

                var inverseInertiaTensor = body.InverseInertiaTensorWorld;

                // Get the angular inertia.
                var relativeContactPosition = GetRelativeContactPosition(i);
                Vector3.Cross(ref relativeContactPosition, ref normal, out var angularInertiaWorld);
                inverseInertiaTensor.Transform(ref angularInertiaWorld, out angularInertiaWorld);
                Vector3.Cross(ref angularInertiaWorld, ref relativeContactPosition, out angularInertiaWorld);
                Vector3.Dot(ref angularInertiaWorld, ref normal, out float angularInertia);
                float linearInertia = body.InverseMass;
                if (i == 0)
                {
                    angularInertia1 = angularInertia;
                    linearInertia1 = linearInertia;
                }
                else
                {
                    angularInertia2 = angularInertia;
                    linearInertia2 = linearInertia;
                }

                // Get the total inertia of all components
                totalInertia += linearInertia + angularInertia;
            }
        }
        /// <summary>
        /// Adjust position
        /// </summary>
        /// <param name="deltaPosition">Position delta</param>
        /// <param name="penetrationDirection">Penetration direction</param>
        public void AdjustPosition(Vector3 deltaPosition, int penetrationDirection)
        {
            Vector3.Dot(ref deltaPosition, ref normal, out float pen);
            penetration += pen * penetrationDirection;
        }

        /// <summary>
        /// Performs contact resolution based on the momentum obtained from inertia.
        /// </summary>
        /// <param name="velocityChange">Changes the speed</param>
        /// <param name="rotationChange">Changes in rotation</param>
        public void ApplyVelocityChange(out Vector3[] velocityChange, out Vector3[] rotationChange)
        {
            velocityChange = new Vector3[2];
            rotationChange = new Vector3[2];

            ApplyVelocityChange(out velocityChange[0], out velocityChange[1], out rotationChange[0], out rotationChange[1]);
        }
        /// <summary>
        /// Performs contact resolution based on the momentum obtained from inertia.
        /// </summary>
        /// <param name="velocityChange1">Speed change 1</param>
        /// <param name="velocityChange2">Speed change 2</param>
        /// <param name="rotationChange1">Rotation change 1</param>
        /// <param name="rotationChange2">Rotation change 2</param>
        public void ApplyVelocityChange(out Vector3 velocityChange1, out Vector3 velocityChange2, out Vector3 rotationChange1, out Vector3 rotationChange2)
        {
            velocityChange1 = default;
            velocityChange2 = default;
            rotationChange1 = default;
            rotationChange2 = default;

            // Storing inverse inertia tensors in world coordinates.
            Matrix3x3 inverseInertiaTensor1 = body1.InverseInertiaTensorWorld;
            Matrix3x3 inverseInertiaTensor2 = default;
            if (body2.HasFiniteMass())
            {
                inverseInertiaTensor2 = body2.InverseInertiaTensorWorld;
            }

            // Calculate the impulse on each contact axis
            Vector3 impulseContact;
            if (MathUtils.IsZero(friction))
            {
                // Frictionless impulse
                impulseContact = CalculateFrictionlessImpulse(inverseInertiaTensor1, inverseInertiaTensor2);
            }
            else
            {
                // Friction impulse
                impulseContact = CalculateFrictionImpulse(inverseInertiaTensor1, inverseInertiaTensor2);
            }

            // Convert momentum to world coordinates
            contactToWorld.Transform(ref impulseContact, out Vector3 impulse);

            // Divide the impulse into linear components and rotations
            Vector3.Cross(ref relativeContactPositionsWorld1, ref impulse, out Vector3 impulsiveTorque);
            inverseInertiaTensor1.Transform(ref impulsiveTorque, out rotationChange1);
            Vector3.Multiply(ref impulse, body1.InverseMass, out velocityChange1);

            // Apply the changes to the body
            body1.AddLinearVelocity(velocityChange1);
            body1.AddAngularVelocity(rotationChange1);

            if (body2.HasFiniteMass())
            {
                // Obtain linear and rotational impulses for the second body
                Vector3.Cross(ref impulse, ref relativeContactPositionsWorld2, out impulsiveTorque);
                inverseInertiaTensor2.Transform(ref impulsiveTorque, out rotationChange2);
                Vector3.Multiply(ref impulse, -body2.InverseMass, out velocityChange2);

                // Apply the changes.
                body2.AddLinearVelocity(velocityChange2);
                body2.AddAngularVelocity(rotationChange2);
            }
        }
        /// <summary>
        /// Calculate the momentum needed to resolve the contact, knowing that there is no friction.
        /// </summary>
        /// <param name="inverseInertiaTensor">Inverse inertia tensor</param>
        /// <remarks>
        /// The two inertia tensors, one for each body of the contact, are necessary to save calculations.
        /// </remarks>
        private Vector3 CalculateFrictionlessImpulse(Matrix3x3 inverseInertiaTensor1, Matrix3x3 inverseInertiaTensor2)
        {
            Vector3 impulseContact;

            // Calculate a vector showing the change in velocity in world coordinates, for a unit impulse in the direction of the contact normal.
            Vector3.Cross(ref relativeContactPositionsWorld1, ref normal, out Vector3 deltaVelWorld);
            inverseInertiaTensor1.Transform(ref deltaVelWorld, out deltaVelWorld);
            Vector3.Cross(ref deltaVelWorld, ref relativeContactPositionsWorld1, out deltaVelWorld);

            // Obtain the variation of the velocity in contact coordinates.
            Vector3.Dot(ref deltaVelWorld, ref normal, out float deltaVelocity);

            // Add the linear component of the velocity variation
            deltaVelocity += body1.InverseMass;

            if (body2.HasFiniteMass())
            {
                Vector3.Cross(ref relativeContactPositionsWorld2, ref normal, out deltaVelWorld);
                inverseInertiaTensor2.Transform(ref deltaVelWorld, out deltaVelWorld);
                Vector3.Cross(ref deltaVelWorld, ref relativeContactPositionsWorld2, out deltaVelWorld);

                Vector3.Dot(ref deltaVelWorld, ref normal, out var v);
                deltaVelocity += v;

                deltaVelocity += body2.InverseMass;
            }

            // Calculate the required impulse size
            impulseContact.X = desiredDeltaVelocity / deltaVelocity;
            impulseContact.Y = 0;
            impulseContact.Z = 0;

            return impulseContact;
        }
        /// <summary>
        /// Calculate the impulse required to resolve the contact, assuming that there is friction.
        /// </summary>
        /// <param name="inverseInertiaTensor">Inverse inertia tensor</param>
        /// <remarks>
        /// The two inertia tensors, one for each body of the contact, are necessary to save calculations.
        /// </remarks>
        private Vector3 CalculateFrictionImpulse(Matrix3x3 inverseInertiaTensor1, Matrix3x3 inverseInertiaTensor2)
        {
            float inverseMass = body1.InverseMass;

            // The equivalent of the cross product in matrices is multiplication by the skew symmetric matrix.
            // The matrix will be used to convert linear quantities to angular ones.
            Matrix3x3.SkewSymmetric(ref relativeContactPositionsWorld1, out var impulseToTorque);

            // Get the matrix to convert contact impulse to velocity variation in world coordinates.
            var deltaVelWorld = impulseToTorque;
            deltaVelWorld *= inverseInertiaTensor1;
            deltaVelWorld *= impulseToTorque;
            deltaVelWorld *= -1;

            if (body2.HasFiniteMass())
            {
                Matrix3x3.SkewSymmetric(ref relativeContactPositionsWorld2, out impulseToTorque);

                // Calculate the velocity modification matrix
                var deltaVelWorld2 = impulseToTorque;
                deltaVelWorld2 *= inverseInertiaTensor2;
                deltaVelWorld2 *= impulseToTorque;
                deltaVelWorld2 *= -1;

                // Add the total of the speed variation.
                deltaVelWorld += deltaVelWorld2;

                // Add the reverse mass.
                inverseMass += body2.InverseMass;
            }

            // Convert to contact coordinates by changing the base.
            var deltaVelocity = Matrix3x3.Transpose(contactToWorld);
            deltaVelocity *= deltaVelWorld;
            deltaVelocity *= contactToWorld;

            // Add the linear velocity variation.
            deltaVelocity.M11 += inverseMass;
            deltaVelocity.M22 += inverseMass;
            deltaVelocity.M33 += inverseMass;

            // Reverse to get the momentum needed per unit of speed.
            Matrix3x3.Invert(ref deltaVelocity, out var impulseMatrix);

            // Find the velocities to kill.
            var velKill = new Vector3(
                desiredDeltaVelocity,
                -velocity.Y,
                -velocity.Z);

            // Find the momentum to nullify the velocities
            impulseMatrix.Transform(ref velKill, out var impulseContact);

            // Check for excessive friction
            float planarImpulse = MathF.Sqrt(impulseContact.Y * impulseContact.Y + impulseContact.Z * impulseContact.Z);
            if (planarImpulse > impulseContact.X * friction)
            {
                // Need to use dynamic friction
                impulseContact.Y /= planarImpulse;
                impulseContact.Z /= planarImpulse;
                impulseContact.X =
                    deltaVelocity.M11 +
                    deltaVelocity.M12 * friction * impulseContact.Y +
                    deltaVelocity.M13 * friction * impulseContact.Z;
                impulseContact.X = desiredDeltaVelocity / impulseContact.X;
                impulseContact.Y *= friction * impulseContact.X;
                impulseContact.Z *= friction * impulseContact.X;
            }

            return impulseContact;
        }
        /// <summary>
        /// Adjust velocity
        /// </summary>
        /// <param name="deltaVel">Velocity delta</param>
        /// <param name="penetrationDirection">Penetration direction</param>
        public void AdjustVelocities(Vector3 deltaVelocity, int penetrationDirection)
        {
            Matrix3x3.TransformTranspose(ref contactToWorld, ref deltaVelocity, out var v);
            velocity += v * -penetrationDirection;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Position: {Position}; Normal: {Normal}; Penetration: {Penetration}; From {body1.Mass} mass body1 to {body2.Mass} mass body2.";
        }
    }
}
