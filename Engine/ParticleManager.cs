using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// CPU particle manager
    /// </summary>
    public class ParticleManager : Drawable
    {
        /// <summary>
        /// Particle systems list
        /// </summary>
        private List<IParticleSystem> particleSystems = new List<IParticleSystem>();
        /// <summary>
        /// Collection for particle system disposition
        /// </summary>
        private List<IParticleSystem> toDelete = new List<IParticleSystem>();

        /// <summary>
        /// Current particle count
        /// </summary>
        public int AllocatedParticleCount { get; private set; }
        /// <summary>
        /// Maximum number of instances
        /// </summary>
        public int Count
        {
            get
            {
                return this.particleSystems.Count;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Particle manager description</param>
        public ParticleManager(Scene scene, ParticleManagerDescription description)
            : base(scene, description)
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

                //Sort active particles (draw far away particles first)
                this.particleSystems.Sort((p1, p2) =>
                {
                    return p2.Emitter.Distance.CompareTo(p1.Emitter.Distance);
                });
            }
        }
        /// <summary>
        /// Draws the active particle systems
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            this.particleSystems.ForEach(p =>
            {
                if (p.Emitter.Visible) p.Draw(context);
            });
        }

        /// <summary>
        /// Adds a new particle system to the collection
        /// </summary>
        /// <param name="type">Particle system type</param>
        /// <param name="description">Particle system description</param>
        /// <param name="emitter">Particle emitter</param>
        public void AddParticleSystem(ParticleSystemTypes type, ParticleSystemDescription description, ParticleEmitter emitter)
        {
            IParticleSystem pSystem = null;

            if (type == ParticleSystemTypes.CPU)
            {
                pSystem = new ParticleSystemCPU(this.Game, description, emitter);
            }
            else if (type == ParticleSystemTypes.GPU)
            {
                pSystem = new ParticleSystemGPU(this.Game, description, emitter);
            }
            else
            {
                throw new EngineException("Bad particle system type");
            }

            this.AllocatedParticleCount += pSystem.MaxConcurrentParticles;

            this.particleSystems.Add(pSystem);
        }
        /// <summary>
        /// Gets a particle systema by index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns the particle system at specified index</returns>
        public IParticleSystem GetParticleSystem(int index)
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
