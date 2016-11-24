using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// CPU particle manager
    /// </summary>
    public class CPUParticleManager : Drawable
    {
        /// <summary>
        /// Particle systems list
        /// </summary>
        private List<CPUParticleSystem> particleSystems = new List<CPUParticleSystem>();
        /// <summary>
        /// Collection for particle system disposition
        /// </summary>
        private List<CPUParticleSystem> toDelete = new List<CPUParticleSystem>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Particle manager description</param>
        public CPUParticleManager(Game game, CPUParticleManagerDescription description)
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
        public void AddParticleGenerator(CPUParticleSystemDescription description, ParticleEmitter emitter)
        {
            this.particleSystems.Add(new CPUParticleSystem(this.Game, description, emitter));
        }

        /// <summary>
        /// Gets the text representation of the particle manager
        /// </summary>
        /// <returns>Returns the text representation of the particle manager</returns>
        public override string ToString()
        {
            return string.Format("Count: {0}", particleSystems.Count);
        }
    }
}
