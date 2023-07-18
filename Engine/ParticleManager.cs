using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// CPU particle manager
    /// </summary>
    public sealed class ParticleManager : Drawable<ParticleManagerDescription>
    {
        /// <summary>
        /// Concurrent particle list
        /// </summary>
        private readonly ConcurrentBag<IParticleSystem> particleSystems = new();

        /// <summary>
        /// Particle systems list
        /// </summary>
        public IEnumerable<IParticleSystem> ParticleSystems
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
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public ParticleManager(Scene scene, string id, string name)
            : base(scene, id, name)
        {

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
            if (!particles.Any())
            {
                return;
            }

            bool toDelete = false;
            particles.ForEach(p =>
            {
                p.Update(context);

                if (!p.Active)
                {
                    toDelete = true;
                }
            });

            if (!toDelete)
            {
                return;
            }

            var toRestore = new List<IParticleSystem>();
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

            if (!toRestore.Any())
            {
                return;
            }

            toRestore.ForEach(p =>
            {
                particleSystems.Add(p);
            });
        }

        /// <inheritdoc/>
        public override bool Draw(DrawContext context)
        {
            if (!particleSystems.Any())
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
        public override bool Cull(ICullingVolume volume, out float distance)
        {
            distance = float.MaxValue;
            bool cull = true;

            if (!particleSystems.Any())
            {
                return cull;
            }

            float minDistance = float.MaxValue;

            // Copy collection
            var particles = particleSystems.ToList();
            particles.ForEach(p =>
            {
                var c = p.Emitter.Cull(volume, out float d);
                if (!c)
                {
                    cull = false;
                    minDistance = Math.Min(d, minDistance);
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
        public async Task<IParticleSystem> AddParticleSystem(ParticleSystemTypes type, ParticleSystemDescription description, ParticleEmitter emitter)
        {
            return await AddParticleSystem($"{Name}.{type}.{description.Name}", type, description, emitter);
        }
        /// <summary>
        /// Adds a new particle system to the collection
        /// </summary>
        /// <param name="name">Particle system name</param>
        /// <param name="type">Particle system type</param>
        /// <param name="description">Particle system description</param>
        /// <param name="emitter">Particle emitter</param>
        /// <returns>Returns the new particle system</returns>
        public async Task<IParticleSystem> AddParticleSystem(string name, ParticleSystemTypes type, ParticleSystemDescription description, ParticleEmitter emitter)
        {
            IParticleSystem pSystem;

            if (type == ParticleSystemTypes.CPU)
            {
                pSystem = await ParticleSystemCpu.Create(Game, name, description, emitter);
            }
            else if (type == ParticleSystemTypes.GPU)
            {
                pSystem = await ParticleSystemGpu.Create(Game, name, description, emitter);
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
        public IParticleSystem GetParticleSystem(int index)
        {
            return particleSystems.ElementAtOrDefault(index);
        }
        /// <summary>
        /// Gets a particle systema by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Returns the particle system at specified name</returns>
        public IParticleSystem GetParticleSystem(string name)
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
            if (!particleSystems.Any())
            {
                return;
            }

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
