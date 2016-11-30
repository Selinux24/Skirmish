using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// CPU particle manager
    /// </summary>
    public class GPUParticleManager : Drawable
    {
        /// <summary>
        /// Particle systems list
        /// </summary>
        private List<GPUParticleSystem> particleSystems = new List<GPUParticleSystem>();
        /// <summary>
        /// Collection for particle system disposition
        /// </summary>
        private List<GPUParticleSystem> toDelete = new List<GPUParticleSystem>();

        /// <summary>
        /// Current particle count
        /// </summary>
        public int AllocatedParticleCount { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Particle manager description</param>
        public GPUParticleManager(Game game, GPUParticleManagerDescription description)
            : base(game, description)
        {

        }
        /// <summary>
        /// Resource disposal
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.particleSystems);
            Helper.Dispose(this.toDelete);
        }

        /// <summary>
        /// Updates the internal state
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            if (this.particleSystems != null && this.particleSystems.Count > 0)
            {
                this.particleSystems.ForEach(p =>
                {
                    p.Update(context);

                    if (!p.Active)
                    {
                        toDelete.Add(p);
                    }
                });

                if (toDelete.Count > 0)
                {
                    toDelete.ForEach(p =>
                    {
                        this.particleSystems.Remove(p);
                        this.AllocatedParticleCount -= p.MaxConcurrentParticles;
                        p.Dispose();
                    });

                    toDelete.Clear();
                }
            }
        }
        /// <summary>
        /// Draws the active particle systems
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            this.particleSystems.ForEach(p => p.Draw(context));
        }

        /// <summary>
        /// Adds a new particle system to the collection
        /// </summary>
        /// <param name="description">Particle system description</param>
        /// <param name="emitter">Particle emitter</param>
        public void AddParticleSystem(ParticleSystemDescription description, ParticleEmitter emitter)
        {
            var pSystem = new GPUParticleSystem(this.Game, description, emitter);

            this.AllocatedParticleCount += pSystem.MaxConcurrentParticles;

            this.particleSystems.Add(pSystem);
        }
        /// <summary>
        /// Gets a particle systema by index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns the particle system at specified index</returns>
        public GPUParticleSystem GetParticleSystem(int index)
        {
            return index < this.particleSystems.Count ? this.particleSystems[index] : null;
        }

        /// <summary>
        /// Gets the text representation of the particle manager
        /// </summary>
        /// <returns>Returns the text representation of the particle manager</returns>
        public override string ToString()
        {
            return string.Format("Particle systems: {0}; Allocated particles: {1}", particleSystems.Count, this.AllocatedParticleCount);
        }
    }
}
