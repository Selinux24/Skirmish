using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltIn.Components.Particles
{
    using Engine.Common;

    /// <summary>
    /// CPU particle manager
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class ParticleManager(Scene scene, string id, string name) : Drawable<ParticleManagerDescription>(scene, id, name)
    {
        /// <summary>
        /// Concurrent particle list
        /// </summary>
        private readonly ConcurrentBag<IParticleSystem<ParticleEmitter, ParticleSystemParams>> particleSystems = [];

        /// <summary>
        /// Particle systems list
        /// </summary>
        public IEnumerable<IParticleSystem<ParticleEmitter, ParticleSystemParams>> ParticleSystems
        {
            get
            {
                //Copy collection
                var list = particleSystems.ToList();

                //Sort active particles (draw far away particles first)
                list.Sort((p1, p2) =>
                {
                    return p2.Emitter.Distance.CompareTo(p1.Emitter.Distance);
                });
                return list;
            }
        }
        /// <summary>
        /// Current particle count
        /// </summary>
        public int AllocatedParticleCount { get; private set; }
        /// <summary>
        /// Number of active particle systems
        /// </summary>
        public int Count
        {
            get
            {
                return particleSystems.Count;
            }
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~ParticleManager()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                while (!particleSystems.IsEmpty)
                {
                    if (particleSystems.TryTake(out var p))
                    {
                        p?.Dispose();
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            //Copy collection
            var particles = particleSystems.ToList();
            if (particles.Count == 0)
            {
                return;
            }

            //Update particles
            particles.ForEach(p => p.Update(context));

            if (!particles.Exists(p => !p.Active))
            {
                //If any inactive particle exist
                return;
            }

            var toRestore = new List<IParticleSystem<ParticleEmitter, ParticleSystemParams>>();
            while (!particleSystems.IsEmpty)
            {
                if (!particleSystems.TryTake(out var p))
                {
                    break;
                }

                if (!p.Active)
                {
                    //Dispose
                    AllocatedParticleCount -= p.MaxConcurrentParticles;
                    p.Dispose();
                }
                else
                {
                    //Restore into bag
                    toRestore.Add(p);
                }
            }

            if (toRestore.Count == 0)
            {
                return;
            }

            toRestore.ForEach(particleSystems.Add);
        }

        /// <inheritdoc/>
        public override bool Draw(DrawContext context)
        {
            if (particleSystems.IsEmpty)
            {
                return false;
            }

            // Copy collection
            var particles = particleSystems.Where(p => p.Emitter.IsDrawable).ToList();

            bool drawn = false;
            foreach (var p in particles)
            {
                drawn = p.Draw(context) || drawn;
            }

            return drawn;
        }

        /// <inheritdoc/>
        public override bool Cull(int cullIndex, ICullingVolume volume, out float distance)
        {
            distance = float.MaxValue;
            bool cull = true;

            if (particleSystems.IsEmpty)
            {
                return cull;
            }

            float minDistance = float.MaxValue;

            // Copy collection
            var particles = particleSystems.ToList();
            particles.ForEach(p =>
            {
                var c = p.Emitter.Cull(cullIndex, volume, out float d);
                if (!c)
                {
                    cull = false;
                    minDistance = MathF.Min(d, minDistance);
                }
            });

            if (!cull)
            {
                distance = minDistance;
            }

            return cull;
        }

        /// <summary>
        /// Adds a new particle system to the collection
        /// </summary>
        /// <param name="type">Particle system type</param>
        /// <param name="description">Particle system description</param>
        /// <param name="emitter">Particle emitter</param>
        /// <returns>Returns the new particle system</returns>
        public IParticleSystem<ParticleEmitter, ParticleSystemParams> AddParticleSystem(ParticleSystemTypes type, ParticleSystemDescription description, ParticleEmitter emitter)
        {
            return AddParticleSystem($"{Name}.{type}.{description.Name}", type, description, emitter);
        }
        /// <summary>
        /// Adds a new particle system to the collection
        /// </summary>
        /// <param name="name">Particle system name</param>
        /// <param name="type">Particle system type</param>
        /// <param name="description">Particle system description</param>
        /// <param name="emitter">Particle emitter</param>
        /// <returns>Returns the new particle system</returns>
        public IParticleSystem<ParticleEmitter, ParticleSystemParams> AddParticleSystem(string name, ParticleSystemTypes type, ParticleSystemDescription description, ParticleEmitter emitter)
        {
            IParticleSystem<ParticleEmitter, ParticleSystemParams> pSystem;

            if (type == ParticleSystemTypes.CPU)
            {
                pSystem = ParticleSystemCpu.Create(Scene, name, description, emitter);
            }
            else if (type == ParticleSystemTypes.GPU)
            {
                pSystem = ParticleSystemGpu.Create(Scene, name, description, emitter);
            }
            else
            {
                throw new EngineException("Bad particle system type");
            }

            AllocatedParticleCount += pSystem.MaxConcurrentParticles;

            particleSystems.Add(pSystem);

            return pSystem;
        }
        /// <summary>
        /// Gets a particle systema by index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns the particle system at specified index</returns>
        public IParticleSystem<ParticleEmitter, ParticleSystemParams> GetParticleSystem(int index)
        {
            return particleSystems.ElementAtOrDefault(index);
        }
        /// <summary>
        /// Gets a particle systema by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Returns the particle system at specified name</returns>
        public IParticleSystem<ParticleEmitter, ParticleSystemParams> GetParticleSystem(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                return particleSystems.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Clear the particle systems
        /// </summary>
        public void Clear()
        {
            while (!particleSystems.IsEmpty)
            {
                if (particleSystems.TryTake(out var p))
                {
                    p?.Dispose();
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Particle systems: {particleSystems.Count}; Allocated particles: {AllocatedParticleCount}";
        }
    }
}
