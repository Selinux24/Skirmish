using Engine;
using Engine.Common;
using SharpDX;
using System;

namespace ModelDrawing
{
    public class MovingEmitter : ParticleEmitter
    {
        private float totalTime = 0;

        public float AngularVelocity { get; set; }
        public float Radius { get; set; }

        public MovingEmitter() : base() { }

        public override void Update(UpdateContext context)
        {
            base.Update(context);

            this.totalTime += context.GameTime.ElapsedSeconds;

            var position = GetPosition(this.AngularVelocity, this.Radius, totalTime);

            this.Velocity = position - this.Position;
            this.Position = position;
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
