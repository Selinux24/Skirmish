using Engine;
using SharpDX;
using System;

namespace BasicSamples.SceneParticles
{
    public class MovingEmitter : ParticleEmitter
    {
        public float AngularVelocity { get; set; }
        public float Radius { get; set; }

        public MovingEmitter() : base() { }

        public override void Update(GameTime gameTime, Vector3 pointOfView)
        {
            base.Update(gameTime, pointOfView);

            var position = GetPosition(AngularVelocity, Radius, TotalTime);

            Velocity = position - Position;
            Position = position;
        }

        private static Vector3 GetPosition(float v, float d, float time)
        {
            Vector3 position = Vector3.Zero;
            position.X = d * (float)Math.Cos(v * time);
            position.Y = 1f;
            position.Z = d * (float)Math.Sin(v * time);

            return position;
        }
    }
}
