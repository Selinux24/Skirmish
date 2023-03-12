using SharpDX;

namespace Engine.Physics
{
    /// <summary>
    /// Rigid body interface
    /// </summary>
    public interface IRigidBody
    {
        /// <summary>
        /// Gets the body mass
        /// </summary>
        float Mass { get; }
        /// <summary>
        /// Gets the body inverse mass
        /// </summary>
        float InverseMass { get; }

        /// <summary>
        /// Gets the body position
        /// </summary>
        Vector3 Position { get; }
        /// <summary>
        /// Gets the body rotation
        /// </summary>
        Quaternion Rotation { get; }
        /// <summary>
        /// Gets the body linear velocity
        /// </summary>
        Vector3 LinearVelocity { get; }
        /// <summary>
        /// Gets the body angular velocity
        /// </summary>
        Vector3 AngularVelocity { get; }
        /// <summary>
        /// Gets the body acceleration
        /// </summary>
        Vector3 Acceleration { get; }
        /// <summary>
        /// Gets the body acceleration in the last frame
        /// </summary>
        Vector3 LastFrameAcceleration { get; }

        /// <summary>
        /// Gets the inertia tensor
        /// </summary>
        Matrix3x3 InertiaTensor { get; }
        /// <summary>
        /// Gets the inverse inertia tensor
        /// </summary>
        Matrix3x3 InverseInertiaTensor { get; }
        /// <summary>
        /// Gets the inertia tensor in world coordinates
        /// </summary>
        Matrix3x3 InertiaTensorWorld { get; }
        /// <summary>
        /// Gets the inverse inertia tensor in world coordinates
        /// </summary>
        Matrix3x3 InverseInertiaTensorWorld { get; }

        /// <summary>
        /// Gets the rigid body transform matrix
        /// </summary>
        Matrix Transform { get; }

        /// <summary>
        /// Gets whether the body is awake or not
        /// </summary>
        bool IsAwake { get; }
        /// <summary>
        /// Gets whether the body can sleep or not
        /// </summary>
        bool CanSleep { get; }

        /// <summary>
        /// Sets the initial state of the body
        /// </summary>
        /// <param name="transform">Transform matrix</param>
        void SetInitialState(Matrix transform);
        /// <summary>
        /// Sets the initial state of the body to the indicated position, scale and rotation
        /// </summary>
        /// <param name="position">Initial position</param>
        /// <param name="rotation">Initial rotation</param>
        void SetInitialState(Vector3 position, Quaternion rotation);
   
        /// <summary>
        /// Sets the state of the body
        /// </summary>
        /// <param name="transform">Transform matrix</param>
        void SetState(Matrix transform);
        /// <summary>
        /// Sets the state of the body
        /// </summary>
        /// <param name="position">Initial position</param>
        /// <param name="rotation">Initial rotation</param>
        void SetState(Vector3 position, Quaternion rotation);

        /// <summary>
        /// Sets the body mass
        /// </summary>
        /// <param name="mass">Body mass value</param>
        void SetMass(float mass);
        /// <summary>
        /// Gets whether the body has a finite mass
        /// </summary>
        bool HasFiniteMass();

        /// <summary>
        /// Sets the inertia tensor coefficient
        /// </summary>
        /// <param name="ix">Inertia magnitude on X coordinate</param>
        /// <param name="iy">Inertia magnitude on Y coordinate</param>
        /// <param name="iz">Inertia magnitude on Z coordinate</param>
        void SetIntertiaCoefficients(float ix, float iy, float iz);

        /// <summary>
        /// Sets the awake state
        /// </summary>
        /// <param name="isAwake">Is awake</param>
        void SetAwakeState(bool isAwake);
        /// <summary>
        /// Sets can sleep state
        /// </summary>
        /// <param name="canSleep">Can sleep</param>
        void SetCanSleepState(bool canSleep);

        /// <summary>
        /// Calculates derived data
        /// </summary>
        void CalculateDerivedData();
        /// <summary>
        /// Integrates state over time
        /// </summary>
        /// <param name="time">Time</param>
        void Integrate(float time);

        /// <summary>
        /// Sets the damping coefficients
        /// </summary>
        /// <param name="linearDamping">Linear damping</param>
        /// <param name="angularDamping">Angular damping</param>
        void SetDamping(float linearDamping, float angularDamping);

        /// <summary>
        /// Adds the specified force to the linear force accumulator
        /// </summary>
        /// <param name="force">Linear force vector</param>
        void AddForce(Vector3 force);
        /// <summary>
        /// Adds the specified force to the angular force accumulator
        /// </summary>
        /// <param name="torque">Vector of angular force or torque</param>
        void AddTorque(Vector3 torque);
        /// <summary>
        /// Adds the force at the specified point in world coordinates
        /// </summary>
        /// <param name="force">Linear force vector</param>
        /// <param name="point">Force point in world coordinates</param>
        void AddForceAtPoint(Vector3 force, Vector3 point);
        /// <summary>
        /// Adds the specified force at the point in local coordinates
        /// </summary>
        /// <param name="force">Linear force vector</param>
        /// <param name="point">Force point in local coordinates</param>
        void AddForceAtBodyPoint(Vector3 force, Vector3 point);

        /// <summary>
        /// Adds the specified linear velocity change
        /// </summary>
        /// <param name="linearVelocityChange">Linear velocity change</param>
        void AddLinearVelocity(Vector3 linearVelocityChange);
        /// <summary>
        /// Adds the specified angular velocity change
        /// </summary>
        /// <param name="angularVelocityChange">Angular velocity change</param>
        void AddAngularVelocity(Vector3 angularVelocityChange);
        /// <summary>
        /// Adds the specified position change
        /// </summary>
        /// <param name="positionChange">Position change</param>
        void AddPosition(Vector3 positionChange);
        /// <summary>
        /// Adds the specified orientation change
        /// </summary>
        /// <param name="orientationChange">Orientation change</param>
        void AddOrientation(Quaternion orientationChange);

        /// <summary>
        /// Gets the specified point in local coordinates
        /// </summary>
        /// <param name="point">Point</param>
        Vector3 GetPointInLocalSpace(Vector3 point);
        /// <summary>
        /// Gets the specified point in world coordinates
        /// </summary>
        /// <param name="point">Point</param>
        Vector3 GetPointInWorldSpace(Vector3 point);
        /// <summary>
        /// Gets the specified direction vector in local coordinates
        /// </summary>
        /// <param name="direction">Direction vector</param>
        Vector3 GetDirectionInLocalSpace(Vector3 direction);
        /// <summary>
        /// Gets the specified direction vector in world coordinates
        /// </summary>
        /// <param name="direction">Direction vector</param>
        Vector3 GetDirectionInWorldSpace(Vector3 direction);
    }
}