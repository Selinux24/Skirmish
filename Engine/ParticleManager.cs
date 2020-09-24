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
    public class ParticleManager : Drawable
    {
        /// <summary>
        /// Concurrent particle list
        /// </summary>
        private readonly ConcurrentBag<IParticleSystem> particleSystems = new ConcurrentBag<IParticleSystem>();

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
        /// <param name="description">Particle manager description</param>
        public ParticleManager(Scene scene, ParticleManagerDescription description)
            : base(scene, description)
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
        /// <summary>
        /// Resource disposal
        /// </summary>
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

        /// <summary>
        /// Updates the internal state
        /// </summary>
        /// <param name="context">Context</param>
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

            List<IParticleSystem> toRestore = new List<IParticleSystem>();
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
        /// <summary>
        /// Draws the active particle systems
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (!particleSystems.Any())
            {
                return;
            }

            // Copy collection
            var particles = particleSystems.ToList();

            particles.ForEach(p =>
            {
                if (p.Emitter.IsDrawable)
                {
                    p.Draw(context);
                }
            });
        }
        /// <summary>
        /// Performs culling with the active emitters
        /// </summary>
        /// <param name="volume">Culling volume</param>
        /// <param name="distance">If the at least one of the internal emitters is visible, returns the distance to the item</param>
        /// <returns>Returns true if all emitters were culled</returns>
        public override bool Cull(IIntersectionVolume volume, out float distance)
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
        public IParticleSystem AddParticleSystem(ParticleSystemTypes type, ParticleSystemDescription description, ParticleEmitter emitter)
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
        public IParticleSystem AddParticleSystem(string name, ParticleSystemTypes type, ParticleSystemDescription description, ParticleEmitter emitter)
        {
            IParticleSystem pSystem;

            if (type == ParticleSystemTypes.CPU)
            {
                pSystem = new ParticleSystemCpu(Game, name, description, emitter);
            }
            else if (type == ParticleSystemTypes.GPU)
            {
                pSystem = new ParticleSystemGpu(Game, name, description, emitter);
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

        /// <summary>
        /// Gets the text representation of the particle manager
        /// </summary>
        /// <returns>Returns the text representation of the particle manager</returns>
        public override string ToString()
        {
            return $"Particle systems: {particleSystems.Count}; Allocated particles: {AllocatedParticleCount}";
        }
    }

    /// <summary>
    /// Particle manager extensions
    /// </summary>
    public static class ParticleManagerExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<ParticleManager> AddComponentParticleManager(this Scene scene, ParticleManagerDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int order = 0)
        {
            ParticleManager component = null;

            await Task.Run(() =>
            {
                component = new ParticleManager(scene, description);

                scene.AddComponent(component, usage, order);
            });

            return component;
        }
    }
}
