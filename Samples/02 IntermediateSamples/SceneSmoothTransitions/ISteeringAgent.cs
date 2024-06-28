using SharpDX;
using System.Collections.Generic;

namespace IntermediateSamples.SceneSmoothTransitions
{
    public interface ISteeringAgent
    {
        Vector3 Position { get; set; }
        Vector3 Velocity { get; }
        float MaxSpeed { get; set; }
        float MaxForce { get; set; }
        float WaitTime { get; set; }
        SteeringBehaviors Behavior { get; }

        void Seek(Vector3 target);
        void Seek(ISteeringAgent agent);
        void Flee(Vector3 target);
        void Flee(ISteeringAgent agent);
        void Arrival(Vector3 target, float arrivalRadius);
        void Arrival(ISteeringAgent agent, float arrivalRadius);
        void Pursue(ISteeringAgent agent);
        void Evade(ISteeringAgent agent);
        void Follow(IEnumerable<Vector3> path, float pathRadius);

        void Update(float elapsedTime);
    }
}
