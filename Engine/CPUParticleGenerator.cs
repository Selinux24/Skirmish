using SharpDX;
using System;

namespace Engine
{
    public class CPUParticleGenerator : IDisposable
    {
        public CPUParticleSystem ParticleSystem { get; set; }
        public float Duration { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public bool Added = false;

        public CPUParticleGenerator(Game game, CPUParticleSystemDescription settings, float duration, Vector3 position, Vector3 velocity)
        {
            this.ParticleSystem = new CPUParticleSystem(game, settings);
            this.Duration = duration;
            this.Position = position;
            this.Velocity = velocity;
        }
        public void Dispose()
        {
            Helper.Dispose(this.ParticleSystem);
        }

        public void AddParticle(Game game)
        {
            if (!this.Added)
            {
                this.ParticleSystem.AddParticle(game, this.Position, this.Velocity);

                this.Added = true;
            }
        }
    }
}