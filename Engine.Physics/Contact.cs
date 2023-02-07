using SharpDX;
using System;
using System.Collections.Generic;

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
        /// Relative contact positions in world coordinates.
        /// </summary>
        private readonly Vector3[] relativeContactPositionsWorld = new Vector3[2];
        /// <summary>
        /// Friction.
        /// </summary>
        private float friction;
        /// <summary>
        /// Restitution.
        /// </summary>
        private float restitution;

        /// <summary>
        /// Relative contact positions in world coordinates.
        /// </summary>
        public IEnumerable<Vector3> RelativeContactPositionsWorld
        {
            get
            {
                return relativeContactPositionsWorld;
            }
        }
        /// <summary>
        /// Contact velocity.
        /// </summary>
        public Vector3 ContactVelocity { get; private set; }
        /// <summary>
        /// Penetration depth. Usually just between both contacts.
        /// </summary>
        public float Penetration { get; private set; }
        /// <summary>
        /// Contact point in world coordinates.
        /// </summary>
        public Vector3 ContactPositionWorld { get; private set; }
        /// <summary>
        /// Contact normal in world coordinates.
        /// </summary>
        public Vector3 ContactNormalWorld { get; private set; }
        /// <summary>
        /// Orthonormal matrix for local-to-world transforms.
        /// </summary>
        public Matrix3x3 ContactToWorld { get; private set; }
        /// <summary>
        /// Desired delta velocity.
        /// </summary>
        public float DesiredDeltaVelocity { get; private set; }

        /// <summary>
        /// Sets the contact data
        /// </summary>
        /// <param name="body1">First body</param>
        /// <param name="body2">Second body</param>
        /// <param name="friction">Friction</param>
        /// <param name="restitution">Restitution</param>
        /// <param name="position">Position in world coordinates</param>
        /// <param name="normal">Normal in world coordinates</param>
        /// <param name="penetration">Penetration</param>
        public void SetContactData(IRigidBody body1, IRigidBody body2, float friction, float restitution, Vector3 position, Vector3 normal, float penetration)
        {
            this.body1 = body1;
            this.body2 = body2;
            this.friction = friction;
            this.restitution = restitution;

            ContactPositionWorld = position;
            ContactNormalWorld = normal;
            Penetration = penetration;
        }

        /// <summary>
        /// Calculate internal contact status data. This function is called before the resolution of the contact resolution algorithm..
        /// </summary>
        /// <param name="duration">Duration</param>
        public void CalculateInternals(float duration)
        {
            if (body1 == null)
            {
                SwapBodies();
            }

            if (body1 == null)
            {
                return;
            }

            // Calculate a coordinate axis from the contact point
            CalculateContactBasis();

            // Store the relative position of the contact, with respect to each body
            relativeContactPositionsWorld[0] = ContactPositionWorld - body1.Position;
            if (body2 != null)
            {
                relativeContactPositionsWorld[1] = ContactPositionWorld - body2.Position;
            }

            // Find the relative speed of each body at the moment of collision.
            ContactVelocity = CalculateLocalVelocity(body1, relativeContactPositionsWorld[0], duration);
            if (body2 != null)
            {
                ContactVelocity -= CalculateLocalVelocity(body2, relativeContactPositionsWorld[1], duration);
            }

            // Calculate the velocity needed to resolve the contact
            CalculateDesiredDeltaVelocity(duration);
        }
        /// <summary>
        /// Get contacting bodies
        /// </summary>
        public IEnumerable<IRigidBody> GetBodies()
        {
            return new[] { body1, body2 };
        }
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
        /// Swap body references.
        /// </summary>
        private void SwapBodies()
        {
            ContactNormalWorld *= -1f;

            (body2, body1) = (body1, body2);
        }
        /// <summary>
        /// Updates the activation state of the contact's bodies.
        /// </summary>
        /// <remarks>
        /// A body will activate if it comes into contact with an active body.
        /// </remarks>
        public void MatchAwakeState()
        {
            if (body2 == null)
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
        /// Calculate and set the velocity needed to resolve contact.
        /// </summary>
        /// <param name="duration">Duration</param>
        private void CalculateDesiredDeltaVelocity(float duration)
        {
            // Calculate the velocity accumulated by the acceleration in this interval
            float velocityFromAcc = 0;

            if (body2 != null && body2.IsAwake)
            {
                velocityFromAcc -= Vector3.Dot(body2.LastFrameAcceleration * duration, ContactNormalWorld);
            }

            // If the speed is very slow, it is necessary to limit the restitution
            float thisRestitution = restitution;
            if (Math.Abs(ContactVelocity.X) < velocityLimit)
            {
                thisRestitution = 0.0f;
            }

            // Combine dumping speed with speed taken from acceleration
            DesiredDeltaVelocity = -ContactVelocity.X - thisRestitution * (ContactVelocity.X - velocityFromAcc);
        }
        /// <summary>
        /// Gets the velocity of the specified body contact point.
        /// </summary>
        /// <param name="body">Rigid body</param>
        /// <param name="relativeContactPositionWorld">Relative contact position world</param>
        /// <param name="duration">Duration</param>
        private Vector3 CalculateLocalVelocity(IRigidBody body, Vector3 relativeContactPositionWorld, float duration)
        {
            // Extract the velocity at the point of contact.
            Vector3 velocity = Vector3.Cross(body.AngularVelocity, relativeContactPositionWorld);
            velocity += body.LinearVelocity;

            // Convert velocity to contact coordinates.
            Vector3 contactVelocity = Core.TransformTranspose(ContactToWorld, velocity);

            // Calculate the amount of velocity available for forces without taking reactions into account.
            Vector3 accVelocity = body.LastFrameAcceleration * duration;

            // Calculate amount of velocity in contact coordinates.
            accVelocity = Core.TransformTranspose(ContactToWorld, accVelocity);

            // Acceleration components in the direction of the contact normal are ignored..
            // Only accelerations in the plane are taken into account.
            accVelocity.X = 0;

            // Add the planar accelerations.
            // If there is enough friction, the forces will be removed during velocity resolution..
            contactVelocity += accVelocity;

            return contactVelocity;
        }
        /// <summary>
        /// Calculates the orthonormal basis for the contact point, based on the primary direction of friction or a random orientation.
        /// </summary>
        /// <remarks>
        /// Primary direction of friction for anisotropic friction or random orientation for isotropic friction
        /// </remarks>
        private void CalculateContactBasis()
        {
            Vector3[] contactTangent = new Vector3[2];

            // Check if the Z axis is closer to the X axis or the Y axis
            if (Math.Abs(ContactNormalWorld.X) > Math.Abs(ContactNormalWorld.Y))
            {
                // Get a scaling factor to make sure the results are normalized
                float s = 1.0f / (float)Math.Sqrt(ContactNormalWorld.Z * ContactNormalWorld.Z + ContactNormalWorld.X * ContactNormalWorld.X);

                // The new X axis is 90 degrees to the Y axis of the world
                contactTangent[0].X = ContactNormalWorld.Z * s;
                contactTangent[0].Y = 0;
                contactTangent[0].Z = -ContactNormalWorld.X * s;

                // The new Y axis is 90 degrees to the new X and Z axes
                contactTangent[1].X = ContactNormalWorld.Y * contactTangent[0].X;
                contactTangent[1].Y = ContactNormalWorld.Z * contactTangent[0].X - ContactNormalWorld.X * contactTangent[0].Z;
                contactTangent[1].Z = -ContactNormalWorld.Y * contactTangent[0].X;
            }
            else
            {
                // Get a scaling factor to make sure the results are normalized
                float s = 1.0f / (float)Math.Sqrt(ContactNormalWorld.Z * ContactNormalWorld.Z + ContactNormalWorld.Y * ContactNormalWorld.Y);

                // The new X axis is 90 degrees to the X axis of the world
                contactTangent[0].X = 0;
                contactTangent[0].Y = -ContactNormalWorld.Z * s;
                contactTangent[0].Z = ContactNormalWorld.Y * s;

                // The new Y axis is 90 degrees to the new X and Z axes
                contactTangent[1].X = ContactNormalWorld.Y * contactTangent[0].Z - ContactNormalWorld.Z * contactTangent[0].Y;
                contactTangent[1].Y = -ContactNormalWorld.X * contactTangent[0].Z;
                contactTangent[1].Z = ContactNormalWorld.X * contactTangent[0].Y;
            }

            ContactToWorld = new Matrix3x3()
            {
                Column1 = ContactNormalWorld,
                Column2 = contactTangent[0],
                Column3 = contactTangent[1],
            };
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

            // Storing inverse masses and inverse inertia tensors in world coordinates.
            Matrix3x3[] inverseInertiaTensor = new Matrix3x3[2];
            inverseInertiaTensor[0] = body1.InverseInertiaTensorWorld;
            if (body2 != null)
            {
                inverseInertiaTensor[1] = body2.InverseInertiaTensorWorld;
            }

            // Calculate the impulse on each contact axis
            Vector3 impulseContact;
            if (friction == 0f)
            {
                // Frictionless impulse
                impulseContact = CalculateFrictionlessImpulse(inverseInertiaTensor);
            }
            else
            {
                // Friction impulse
                impulseContact = CalculateFrictionImpulse(inverseInertiaTensor);
            }

            // Convert momentum to world coordinates
            Vector3 impulse = Core.Transform(ContactToWorld, impulseContact);

            // Divide the impulse into linear components and rotations
            Vector3 impulsiveTorque = Vector3.Cross(relativeContactPositionsWorld[0], impulse);
            rotationChange[0] = Core.Transform(inverseInertiaTensor[0], impulsiveTorque);
            velocityChange[0] = Vector3.Zero;
            velocityChange[0] += Vector3.Multiply(impulse, body1.InverseMass);

            // Aplicar los cambios e el cuerpo
            body1.AddLinearVelocity(velocityChange[0]);
            body1.AddAngularVelocity(rotationChange[0]);

            if (body2 != null)
            {
                // Obtain linear and rotational impulses for the second body
                impulsiveTorque = Vector3.Cross(impulse, relativeContactPositionsWorld[1]);
                rotationChange[1] = Core.Transform(inverseInertiaTensor[1], impulsiveTorque);
                velocityChange[1] = Vector3.Zero;
                velocityChange[1] += Vector3.Multiply(impulse, -body2.InverseMass);

                // Apply the changes.
                body2.AddLinearVelocity(velocityChange[1]);
                body2.AddAngularVelocity(rotationChange[1]);
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

            // We have to work with the inertia of each body in the direction of the contact normal, and the angular inertia.
            ApplyInertia(out var totalInertia, out var linearInertia, out var angularInertia);

            // Iterate again calculating the changes and applying them
            float angularLimit = 0.2f;
            for (int i = 0; i < 2; i++)
            {
                var body = GetBody(i);
                if (body == null)
                {
                    continue;
                }

                // The angular and linear movements are proportional to the two inverse inertias.
                float sign = (i == 0) ? 1 : -1;
                float angularMove = sign * penetration * (angularInertia[i] / totalInertia);
                float linearMove = sign * penetration * (linearInertia[i] / totalInertia);

                // To avoid too large angular projections, the angular movement is limited.
                var projection = relativeContactPositionsWorld[i];
                projection += Vector3.Multiply(ContactNormalWorld, Vector3.Dot(-relativeContactPositionsWorld[i], ContactNormalWorld));

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
                if (angularMove == 0f)
                {
                    // There is no angular movement. No rotation.
                    angularChange[i] = Vector3.Zero;
                }
                else
                {
                    // Get direction of rotation.
                    var targetAngularDirection = Vector3.Cross(relativeContactPositionsWorld[i], ContactNormalWorld);

                    var inverseInertiaTensor = body.InverseInertiaTensorWorld;

                    angularChange[i] = Core.Transform(inverseInertiaTensor, targetAngularDirection) * (angularMove / angularInertia[i]);
                }

                // Velocity variation: linear movement on the normal of contact.
                linearChange[i] = ContactNormalWorld * linearMove;

                // Apply linear motion
                var positionChange = Vector3.Multiply(ContactNormalWorld, linearMove);
                body.AddPosition(positionChange);

                // Apply the change in orientation
                var orientationChange = new Quaternion(angularChange[i], 0f) * body.Orientation * Constants.OrientationContactFactor;
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
        private void ApplyInertia(out float totalInertia, out float[] linearInertia, out float[] angularInertia)
        {
            totalInertia = 0f;
            linearInertia = new float[2];
            angularInertia = new float[2];

            for (int i = 0; i < 2; i++)
            {
                var body = GetBody(i);
                if (body == null)
                {
                    continue;
                }

                var inverseInertiaTensor = body.InverseInertiaTensorWorld;

                // Get the angular inertia.
                var angularInertiaWorld = Vector3.Cross(relativeContactPositionsWorld[i], ContactNormalWorld);
                angularInertiaWorld = Core.Transform(inverseInertiaTensor, angularInertiaWorld);
                angularInertiaWorld = Vector3.Cross(angularInertiaWorld, relativeContactPositionsWorld[i]);
                angularInertia[i] = Vector3.Dot(angularInertiaWorld, ContactNormalWorld);

                // The linear component is the inverse of the mass
                linearInertia[i] = body.InverseMass;

                // Get the total inertia of all components
                totalInertia += linearInertia[i] + angularInertia[i];
            }
        }
        /// <summary>
        /// Calculate the momentum needed to resolve the contact, knowing that there is no friction.
        /// </summary>
        /// <param name="inverseInertiaTensor">Inverse inertia tensor</param>
        /// <remarks>
        /// The two inertia tensors, one for each body of the contact, are necessary to save calculations.
        /// </remarks>
        private Vector3 CalculateFrictionlessImpulse(Matrix3x3[] inverseInertiaTensor)
        {
            Vector3 impulseContact;

            // Calculate a vector showing the change in velocity in world coordinates, for a unit impulse in the direction of the contact normal.
            Vector3 deltaVelWorld = Vector3.Cross(relativeContactPositionsWorld[0], ContactNormalWorld);
            deltaVelWorld = Core.Transform(inverseInertiaTensor[0], deltaVelWorld);
            deltaVelWorld = Vector3.Cross(deltaVelWorld, relativeContactPositionsWorld[0]);

            // Obtain the variation of the velocity in contact coordinates.
            float deltaVelocity = Vector3.Dot(deltaVelWorld, ContactNormalWorld);

            // Add the linear component of the velocity variation
            deltaVelocity += body1.InverseMass;

            if (body2 != null)
            {
                deltaVelWorld = Vector3.Cross(relativeContactPositionsWorld[1], ContactNormalWorld);
                deltaVelWorld = Core.Transform(inverseInertiaTensor[1], deltaVelWorld);
                deltaVelWorld = Vector3.Cross(deltaVelWorld, relativeContactPositionsWorld[1]);

                deltaVelocity += Vector3.Dot(deltaVelWorld, ContactNormalWorld);

                deltaVelocity += body2.InverseMass;
            }

            // Calculate the required impulse size
            impulseContact.X = DesiredDeltaVelocity / deltaVelocity;
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
        private Vector3 CalculateFrictionImpulse(Matrix3x3[] inverseInertiaTensor)
        {
            Vector3 impulseContact;
            float inverseMass = body1.InverseMass;

            // The equivalent of the cross product in matrices is multiplication by the skew symmetric matrix.
            // The matrix will be used to convert linear quantities to angular ones.
            Matrix3x3 impulseToTorque;
            impulseToTorque = Core.SkewSymmetric(relativeContactPositionsWorld[0]);

            // Get the matrix to convert contact impulse to velocity variation in world coordinates.
            Matrix3x3 deltaVelWorld = impulseToTorque;
            deltaVelWorld *= inverseInertiaTensor[0];
            deltaVelWorld *= impulseToTorque;
            deltaVelWorld *= -1;

            if (body2 != null)
            {
                impulseToTorque = Core.SkewSymmetric(relativeContactPositionsWorld[1]);

                // Calculate the velocity modification matrix
                Matrix3x3 deltaVelWorld2 = impulseToTorque;
                deltaVelWorld2 *= inverseInertiaTensor[1];
                deltaVelWorld2 *= impulseToTorque;
                deltaVelWorld2 *= -1;

                // Add the total of the speed variation.
                deltaVelWorld += deltaVelWorld2;

                // Add the reverse mass.
                inverseMass += body2.InverseMass;
            }

            // Convert to contact coordinates by changing the base.
            Matrix3x3 deltaVelocity = Matrix3x3.Transpose(ContactToWorld);
            deltaVelocity *= deltaVelWorld;
            deltaVelocity *= ContactToWorld;

            // Add the linear velocity variation.
            deltaVelocity.M11 += inverseMass;
            deltaVelocity.M22 += inverseMass;
            deltaVelocity.M33 += inverseMass;

            // Reverse to get the momentum needed per unit of speed.
            Matrix3x3 impulseMatrix = Matrix3x3.Invert(deltaVelocity);

            // Find the velocities to kill.
            Vector3 velKill = new Vector3(
                DesiredDeltaVelocity,
                -ContactVelocity.Y,
                -ContactVelocity.Z);

            // Find the momentum to nullify the velocities
            impulseContact = Core.Transform(impulseMatrix, velKill);

            // Check for excessive friction
            float planarImpulse = (float)Math.Sqrt(impulseContact.Y * impulseContact.Y + impulseContact.Z * impulseContact.Z);
            if (planarImpulse > impulseContact.X * friction)
            {
                // Need to use dynamic friction
                impulseContact.Y /= planarImpulse;
                impulseContact.Z /= planarImpulse;
                impulseContact.X =
                    deltaVelocity.M11 +
                    deltaVelocity.M12 * friction * impulseContact.Y +
                    deltaVelocity.M13 * friction * impulseContact.Z;
                impulseContact.X = DesiredDeltaVelocity / impulseContact.X;
                impulseContact.Y *= friction * impulseContact.X;
                impulseContact.Z *= friction * impulseContact.X;
            }

            return impulseContact;
        }

        /// <summary>
        /// Adjust position
        /// </summary>
        /// <param name="linearChange">Linear change</param>
        /// <param name="angularChange">Angular change</param>
        /// <param name="relativeContactPositionsWorld">Relative contact position</param>
        /// <param name="penetrationDirection">Penetration direction</param>
        public void AdjustPosition(Vector3 linearChange, Vector3 angularChange, Vector3 relativeContactPositionsWorld, int penetrationDirection)
        {
            Vector3 deltaPosition = linearChange + Vector3.Cross(angularChange, relativeContactPositionsWorld);

            Penetration += Vector3.Dot(deltaPosition, ContactNormalWorld) * penetrationDirection;
        }
        /// <summary>
        /// Adjust velocity
        /// </summary>
        /// <param name="velocityChange">Velocity change</param>
        /// <param name="rotationChange">Rotation change</param>
        /// <param name="relativeContactPositionsWorld">Relative contact position</param>
        /// <param name="penetrationDirection">Penetration direction</param>
        /// <param name="duration">Duration</param>
        public void AdjustVelocities(Vector3 velocityChange, Vector3 rotationChange, Vector3 relativeContactPositionsWorld, int penetrationDirection, float duration)
        {
            Vector3 deltaVel = velocityChange + Vector3.Cross(rotationChange, relativeContactPositionsWorld);

            // Si el signo del cambio es negativo, se trata del segundo cuerpo del contacto
            ContactVelocity += Core.TransformTranspose(ContactToWorld, deltaVel) * penetrationDirection;

            CalculateDesiredDeltaVelocity(duration);
        }
    }
}
