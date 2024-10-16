﻿using Engine;
using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace IntermediateSamples.SceneSmoothTransitions
{
    class SteeringAgent : ISteeringAgent
    {
        private Vector3 acceleration;

        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; private set; }
        public float MaxSpeed { get; set; }
        public float MaxForce { get; set; }

        public float WaitTime { get; set; }

        public SteeringBehaviors Behavior { get; private set; } = SteeringBehaviors.Seek;

        public ISteeringAgent Target { get; private set; }
        public float ArrivalRadius { get; private set; }

        public IEnumerable<Vector3> Path { get; private set; }
        public float PathRadius { get; private set; }

        private Vector3 DoSeek(Vector3 target, bool arrival = false)
        {
            var force = target - Position;

            var desiredSpeed = MaxSpeed;
            if (arrival && ArrivalRadius > 0f)
            {
                var distance = force.Length();
                if (distance < ArrivalRadius)
                {
                    desiredSpeed = MathUtil.Lerp(0, MaxSpeed, distance / ArrivalRadius);
                }
            }

            force = Vector3.Normalize(force) * desiredSpeed;
            force -= Velocity;
            force = force.Limit(MaxForce);
            return force;
        }
        private Vector3 DoFlee(Vector3 position)
        {
            return DoSeek(position) * -1f;
        }
        private Vector3 DoPursue(ISteeringAgent target)
        {
            var prediction = target.Velocity * 10f;

            return DoSeek(target.Position + prediction);
        }
        private Vector3 DoEvade(ISteeringAgent target)
        {
            return DoPursue(target) * -1f;
        }
        private Vector3 DoFollow(IEnumerable<Vector3> path, float pathRadius)
        {
            // Step 1 calculate future position
            var future = Velocity;
            future *= 20;
            future += Position;

            // Step 2 Is future on path?
            var target = FindProjection(path.First(), future, path.Last());

            var d = Vector3.Distance(future, target);
            if (d > pathRadius)
            {
                return DoSeek(target);
            }
            else
            {
                return Vector3.Zero;
            }
        }
        private static Vector3 FindProjection(Vector3 pos, Vector3 a, Vector3 b)
        {
            var v1 = a - pos;
            var v2 = b - pos;
            v2.Normalize();
            var sp = Vector3.Dot(v1, v2);
            v2 *= sp;
            v2 += pos;
            return v2;
        }

        public void Seek(Vector3 target)
        {
            Seek(new SteeringAgent { Position = target });
        }
        public void Seek(ISteeringAgent agent)
        {
            if (agent == null)
            {
                return;
            }

            Target = agent;
            Behavior = SteeringBehaviors.Seek;
        }
        public void Flee(Vector3 target)
        {
            Flee(new SteeringAgent { Position = target });
        }
        public void Flee(ISteeringAgent agent)
        {
            if (agent == null)
            {
                return;
            }

            Target = agent;
            Behavior = SteeringBehaviors.Flee;
        }
        public void Arrival(Vector3 target, float arrivalRadius)
        {
            Arrival(new SteeringAgent { Position = target }, arrivalRadius);
        }
        public void Arrival(ISteeringAgent agent, float arrivalRadius)
        {
            if (agent == null)
            {
                return;
            }

            Target = agent;
            ArrivalRadius = arrivalRadius;
            Behavior = SteeringBehaviors.Arrival;
        }
        public void Pursue(ISteeringAgent agent)
        {
            if (agent == null)
            {
                return;
            }

            Target = agent;
            Behavior = SteeringBehaviors.Pursue;
        }
        public void Evade(ISteeringAgent agent)
        {
            if (agent == null)
            {
                return;
            }

            Target = agent;
            Behavior = SteeringBehaviors.Evade;
        }
        public void Follow(IEnumerable<Vector3> path, float pathRadius)
        {
            if ((path?.Count() ?? 0) < 2)
            {
                return;
            }

            Path = path;
            PathRadius = pathRadius;
            Behavior = SteeringBehaviors.FollowPath;
        }

        public void Update(float elapsedTime)
        {
            if (WaitTime > 0f)
            {
                WaitTime -= elapsedTime;
                return;
            }

            var force = Behavior switch
            {
                SteeringBehaviors.Seek => DoSeek(Target.Position),
                SteeringBehaviors.Flee => DoFlee(Target.Position),
                SteeringBehaviors.Pursue => DoPursue(Target),
                SteeringBehaviors.Evade => DoEvade(Target),
                SteeringBehaviors.Arrival => DoSeek(Target.Position, true),
                SteeringBehaviors.FollowPath => DoFollow(Path, PathRadius),
                _ => DoSeek(Target.Position),
            };
            acceleration += force;

            Velocity += acceleration;
            Velocity = Velocity.Limit(MaxSpeed);
            Position += Velocity * elapsedTime;
            acceleration = Vector3.Zero;
        }

        public void Reset()
        {
            acceleration = Vector3.Zero;

            Velocity = Vector3.Zero;
        }
    }
}
