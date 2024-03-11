using SharpDX;
using System;

namespace Engine.Physics
{
    /// <summary>
    /// A rigid body is the basic simulation object in the physics core.
    ///
    /// It has position and orientation data, along with first derivatives.
    /// It can be integrated forward through time, and have forces, torques and impulses (linear or angular) applied to it.
    /// The rigid body manages its state and allows access through a set of methods.
    /// </summary>
    public sealed class RigidBody : IRigidBody
    {
        /// <summary>
        /// Linear damping
        /// </summary>
        private float linearDamping = Constants.LinearDamping;
        /// <summary>
        /// Angular damping
        /// </summary>
        private float angularDamping = Constants.AngularDamping;
        /// <summary>
        /// Motion
        /// </summary>
        private float motionAcum = 0f;
        /// <summary>
        /// Force
        /// </summary>
        private Vector3 forceAccum = Vector3.Zero;
        /// <summary>
        /// Torque
        /// </summary>
        private Vector3 torqueAccum = Vector3.Zero;

        /// <inheritdoc/>
        public float Mass { get; private set; } = 0f;
        /// <inheritdoc/>
        public float InverseMass { get; private set; } = float.PositiveInfinity;
        /// <inheritdoc/>
        public float Friction { get; private set; } = 0f;
        /// <inheritdoc/>
        public float Restitution { get; private set; } = 0f;

        /// <inheritdoc/>
        public Vector3 Position { get; private set; } = Vector3.Zero;
        /// <inheritdoc/>
        public Quaternion Rotation { get; private set; } = Quaternion.Identity;
        /// <inheritdoc/>
        public Vector3 LinearVelocity { get; private set; } = Vector3.Zero;
        /// <inheritdoc/>
        public Vector3 AngularVelocity { get; private set; } = Vector3.Zero;
        /// <inheritdoc/>
        public Vector3 Acceleration { get; private set; } = Vector3.Zero;
        /// <inheritdoc/>
        public Vector3 LastFrameAcceleration { get; private set; } = Vector3.Zero;

        /// <inheritdoc/>
        public Matrix3x3 InertiaTensor { get; private set; } = Matrix3x3.Identity;
        /// <inheritdoc/>
        public Matrix3x3 InverseInertiaTensor { get; private set; } = Matrix3x3.Identity;
        /// <inheritdoc/>
        public Matrix3x3 InertiaTensorWorld { get; private set; } = Matrix3x3.Identity;
        /// <inheritdoc/>
        public Matrix3x3 InverseInertiaTensorWorld { get; private set; } = Matrix3x3.Identity;

        /// <inheritdoc/>
        public Matrix Transform { get; private set; } = Matrix.Identity;

        /// <inheritdoc/>
        public bool IsAwake { get; private set; } = false;
        /// <inheritdoc/>
        public bool CanSleep { get; private set; } = false;
        /// <inheritdoc/>
        public bool IsStatic { get; private set; } = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bodyState">Body state</param>
        public RigidBody(RigidBodyState bodyState)
        {
            IsStatic = bodyState.IsStatic;

            SetMass(bodyState.Mass);
            SetFriction(bodyState.Friction);
            SetRestitution(bodyState.Restitution);

            SetInitialState(bodyState.InitialTransform);
        }

        /// <inheritdoc/>
        public void SetInitialState(Matrix transform)
        {
            if (!transform.Decompose(out var scale, out var rotation, out var translation) || !Vector3.NearEqual(Vector3.One, scale, Constants.ZeroToleranceVector))
            {
                throw new ArgumentException("Bad transform. A rotation * translation without scale matrix must be specified.", nameof(transform));
            }

            SetInitialState(translation, rotation);
        }
        /// <inheritdoc/>
        public void SetInitialState(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = Quaternion.Normalize(rotation);

            LinearVelocity = Vector3.Zero;
            AngularVelocity = Vector3.Zero;
            Acceleration = Constants.GravityForce;

            SetCanSleepState(true);
            SetAwakeState(true);

            ClearAccumulators();
            CalculateDerivedData();
        }

        /// <inheritdoc/>
        public void SetStatic(bool isStatic)
        {
            IsStatic = isStatic;
        }

        /// <inheritdoc/>
        public void SetState(Matrix transform)
        {
            if (!transform.Decompose(out var scale, out var rotation, out var translation) || !Vector3.NearEqual(Vector3.One, scale, Constants.ZeroToleranceVector))
            {
                throw new ArgumentException("Bad transform. A rotation * translation without scale matrix must be specified.", nameof(transform));
            }

            SetState(translation, rotation);
        }
        /// <inheritdoc/>
        public void SetState(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = Quaternion.Normalize(rotation);

            CalculateDerivedData();
        }

        /// <inheritdoc/>
        public void SetMass(float mass)
        {
            Mass = mass;
            InverseMass = MathUtil.IsZero(mass) ? float.PositiveInfinity : 1f / mass;
        }
        /// <inheritdoc/>
        public bool HasFiniteMass()
        {
            return !IsStatic && InverseMass >= 0f && !float.IsPositiveInfinity(Mass);
        }

        /// <inheritdoc/>
        public void SetFriction(float friction)
        {
            Friction = friction;
        }
        /// <inheritdoc/>
        public void SetRestitution(float restitution)
        {
            Restitution = restitution;
        }

        /// <inheritdoc/>
        public void SetIntertiaCoefficients(float ix, float iy, float iz)
        {
            if (!HasFiniteMass())
            {
                return;
            }

            InertiaTensor = new Matrix3x3()
            {
                M11 = ix * Mass,
                M12 = 0,
                M13 = 0,

                M21 = 0,
                M22 = iy * Mass,
                M23 = 0,

                M31 = 0,
                M32 = 0,
                M33 = iz * Mass,
            };

            InverseInertiaTensor = Matrix3x3.Invert(InertiaTensor);
        }

        /// <inheritdoc/>
        public void SetAwakeState(bool isAwake)
        {
            if (!HasFiniteMass())
            {
                IsAwake = false;

                return;
            }

            IsAwake = isAwake;

            if (isAwake)
            {
                // Avoid instant activation adding some motion
                motionAcum = Constants.SleepEpsilon * 2.0f;
            }
            else
            {
                LinearVelocity = Vector3.Zero;
                AngularVelocity = Vector3.Zero;
            }
        }
        /// <inheritdoc/>
        public void SetCanSleepState(bool canSleep)
        {
            if (!HasFiniteMass())
            {
                CanSleep = false;

                return;
            }

            CanSleep = canSleep;

            if (!canSleep && !IsAwake)
            {
                SetAwakeState(true);
            }
        }

        /// <inheritdoc/>
        public void CalculateDerivedData()
        {
            // Calculate transformation matrix with orientation and position
            Rotation = Quaternion.Normalize(Rotation);
            Transform = Matrix.RotationQuaternion(Rotation) * Matrix.Translation(Position);

            if (!HasFiniteMass())
            {
                return;
            }

            // Calculate the inertia tensor in world coordinates
            InverseInertiaTensorWorld = InverseInertiaTensor.Transform(Transform);
            InertiaTensorWorld = Matrix3x3.Invert(InverseInertiaTensorWorld);
        }
        /// <inheritdoc/>
        public void Integrate(float time)
        {
            if (!IsAwake)
            {
                return;
            }

            if (!HasFiniteMass())
            {
                return;
            }

            // Get the bounce coefficients for this time interval
            float linearDampingOnTime = (float)Math.Pow(linearDamping, time);
            float angularDampingOnTime = (float)Math.Pow(angularDamping, time);

            // Calculate the linear acceleration from the forces
            LastFrameAcceleration = Acceleration;
            if (forceAccum != Vector3.Zero && !MathUtil.IsZero(InverseMass))
            {
                LastFrameAcceleration += Vector3.Multiply(forceAccum, InverseMass);
            }

            // Calculate the angular acceleration from the torques
            Vector3 angularAcceleration = InverseInertiaTensorWorld.Transform(torqueAccum);

            // Update linear velocity using linear acceleration
            LinearVelocity += Vector3.Multiply(LastFrameAcceleration, time);

            // Update angular velocity using angular acceleration
            AngularVelocity += Vector3.Multiply(angularAcceleration, time);

            // Apply damping coefficients
            LinearVelocity *= linearDampingOnTime;
            AngularVelocity *= angularDampingOnTime;

            // Update linear position
            if (LinearVelocity != Vector3.Zero)
            {
                Position += Vector3.Multiply(LinearVelocity, time);
            }

            // Update orientation (angular position)
            if (AngularVelocity != Vector3.Zero)
            {
                var newRotation = new Quaternion(AngularVelocity * time, 0f) * Rotation;
                Rotation = Quaternion.Normalize(Rotation + newRotation);
            }

            // Apply damping coefficients
            LinearVelocity *= linearDampingOnTime;
            AngularVelocity *= angularDampingOnTime;

            // Normalize orientation and update arrays with new position and orientation
            CalculateDerivedData();

            // Clear force accumulators
            ClearAccumulators();

            if (!CanSleep)
            {
                return;
            }

            // Upgrade the kinetic energy accumulator

            // Calculate current kinetic energy
            float currentMotion = Vector3.Dot(LinearVelocity, LinearVelocity) + Vector3.Dot(AngularVelocity, AngularVelocity);
            float bias = (float)Math.Pow(0.5f, time);

            motionAcum = bias * motionAcum + (1f - bias) * currentMotion;

            if (motionAcum < Constants.SleepEpsilon)
            {
                // If there is not enough kinetic energy, the body is put to sleep.
                IsAwake = false;
            }
            else if (motionAcum > 10f * Constants.SleepEpsilon)
            {
                // Accumulate kinetic energy
                motionAcum = 10f * Constants.SleepEpsilon;
            }
        }

        /// <inheritdoc/>
        public void SetDamping(float linearDamping, float angularDamping)
        {
            if (!HasFiniteMass())
            {
                return;
            }

            this.linearDamping = linearDamping;
            this.angularDamping = angularDamping;
        }

        /// <inheritdoc/>
        public void AddForce(Vector3 force)
        {
            if (!HasFiniteMass())
            {
                return;
            }

            if (Vector3.Zero == force)
            {
                return;
            }

            forceAccum += force;

            SetAwakeState(true);
        }
        /// <inheritdoc/>
        public void AddTorque(Vector3 torque)
        {
            if (!HasFiniteMass())
            {
                return;
            }

            if (Vector3.Zero == torque)
            {
                return;
            }

            torqueAccum += torque;

            SetAwakeState(true);
        }
        /// <inheritdoc/>
        public void AddForceAtPoint(Vector3 force, Vector3 point)
        {
            if (!HasFiniteMass())
            {
                return;
            }

            if (Vector3.Zero == force)
            {
                return;
            }

            // Convert point to coordinates relative to the center of mass of the body
            Vector3 pt = point - Position;

            torqueAccum += force;
            torqueAccum += Vector3.Cross(pt, force);

            SetAwakeState(true);
        }
        /// <inheritdoc/>
        public void AddForceAtBodyPoint(Vector3 force, Vector3 point)
        {
            if (!HasFiniteMass())
            {
                return;
            }

            if (Vector3.Zero == force)
            {
                return;
            }

            // Convert point to coordinates relative to the center of mass of the body
            Vector3 pt = GetPointInWorldSpace(point);
            AddForceAtPoint(force, pt);

            SetAwakeState(true);
        }
        /// <summary>
        /// Clears force accumulators
        /// </summary>
        private void ClearAccumulators()
        {
            forceAccum = Vector3.Zero;
            torqueAccum = Vector3.Zero;
        }

        /// <inheritdoc/>
        public void AddLinearVelocity(Vector3 linearVelocityChange)
        {
            if (!HasFiniteMass())
            {
                return;
            }

            if (linearVelocityChange == Vector3.Zero)
            {
                return;
            }

            LinearVelocity += linearVelocityChange;
        }
        /// <inheritdoc/>
        public void AddAngularVelocity(Vector3 angularVelocityChange)
        {
            if (!HasFiniteMass())
            {
                return;
            }

            if (angularVelocityChange == Vector3.Zero)
            {
                return;
            }

            AngularVelocity += angularVelocityChange;
        }
        /// <inheritdoc/>
        public void AddPosition(Vector3 positionChange)
        {
            if (!HasFiniteMass())
            {
                return;
            }

            if (positionChange == Vector3.Zero)
            {
                return;
            }

            Position += positionChange;
        }
        /// <inheritdoc/>
        public void AddOrientation(Quaternion orientationChange)
        {
            if (!HasFiniteMass())
            {
                return;
            }

            if (orientationChange == Quaternion.Zero)
            {
                return;
            }

            Rotation = Quaternion.Normalize(Rotation + orientationChange);
        }

        /// <inheritdoc/>
        public Vector3 GetPointInLocalSpace(Vector3 point)
        {
            return Vector3.TransformCoordinate(point, Matrix.Invert(Transform));
        }
        /// <inheritdoc/>
        public Vector3 GetPointInWorldSpace(Vector3 point)
        {
            return Vector3.TransformCoordinate(point, Transform);
        }
        /// <inheritdoc/>
        public Vector3 GetDirectionInLocalSpace(Vector3 direction)
        {
            return Vector3.TransformNormal(direction, Matrix.Invert(Transform));
        }
        /// <inheritdoc/>
        public Vector3 GetDirectionInWorldSpace(Vector3 direction)
        {
            return Vector3.TransformNormal(direction, Transform);
        }
    }

    /// <summary>
    /// Rigid body state parameters
    /// </summary>
    public struct RigidBodyState
    {
        /// <summary> 
        /// Friction factor to add in all collisions
        /// </summary>
        public const float DefaultFriction = 0.9f;
        /// <summary> 
        /// Restitution factor to add on all collisions
        /// </summary>
        public const float DefaultRestitution = 0.001f;

        /// <summary>
        /// Mass
        /// </summary>
        public float Mass { get; set; } = 1f;
        /// <summary>
        /// Friction factor
        /// </summary>
        public float Friction { get; set; } = DefaultFriction;
        /// <summary>
        /// Restitution factor
        /// </summary>
        public float Restitution { get; set; } = DefaultRestitution;
        /// <summary>
        /// Initial transform
        /// </summary>
        public Matrix InitialTransform { get; set; } = Matrix.Identity;
        /// <summary>
        /// Static body
        /// </summary>
        public bool IsStatic { get; set; } = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public RigidBodyState()
        {

        }
    }
}
