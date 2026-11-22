using Engine.Mathematics;

namespace IntermediateSamples.SceneSmoothTransitions
{
    /// <summary>
    /// Steering agent interface
    /// </summary>
    public interface ISteeringAgent
    {
        /// <summary>
        /// Position
        /// </summary>
        Vector3 Position { get; set; }
        /// <summary>
        /// Velocity
        /// </summary>
        Vector3 Velocity { get; }
        /// <summary>
        /// Maximum speed
        /// </summary>
        float MaxSpeed { get; set; }
        /// <summary>
        /// Maximum force
        /// </summary>
        float MaxForce { get; set; }
        /// <summary>
        /// Waiting time
        /// </summary>
        float WaitTime { get; set; }
        /// <summary>
        /// Steering behavior
        /// </summary>
        SteeringBehaviors Behavior { get; }

        void Seek(Vector3 target);
        void Seek(ISteeringAgent agent);
        void Flee(Vector3 target);
        void Flee(ISteeringAgent agent);
        void Arrival(Vector3 target, float arrivalRadius);
        void Arrival(ISteeringAgent agent, float arrivalRadius);
        void Pursue(ISteeringAgent agent);
        void Evade(ISteeringAgent agent);
        void Follow(Vector3[] path, float pathRadius);

        void Update(float elapsedTime);
    }
}
