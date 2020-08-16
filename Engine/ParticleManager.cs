using System;
using System.Collections.Generic;
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
        /// Collection for particle system disposition
        /// </summary>
        private List<IParticleSystem> toDelete = new List<IParticleSystem>();

        /// <summary>
        /// Particle systems list
        /// </summary>
        public List<IParticleSystem> ParticleSystems { get; private set; } = new List<IParticleSystem>();
        /// <summary>
        /// Current particle count
        /// </summary>
        public int AllocatedParticleCount { get; private set; }
        /// <summary>
        /// Number of active particle systems
        /// </summary>
        public int SystemsCount
        {
            get
            {
                return this.ParticleSystems.Count;
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
                if (this.ParticleSystems != null)
                {
                    for (int i = 0; i < this.ParticleSystems.Count; i++)
                    {
                        this.ParticleSystems[i]?.Dispose();
                        this.ParticleSystems[i] = null;
                    }

                    this.ParticleSystems.Clear();
                    this.ParticleSystems = null;
                }

                if (this.toDelete != null)
                {
                    for (int i = 0; i < this.toDelete.Count; i++)
                    {
                        this.toDelete[i]?.Dispose();
                        this.toDelete[i] = null;
                    }

                    this.toDelete.Clear();
                    this.toDelete = null;
                }
            }
        }

        /// <summary>
        /// Updates the internal state
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            if (this.ParticleSystems?.Count > 0)
            {
                this.ParticleSystems.ForEach(p =>
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
                        this.ParticleSystems.Remove(p);
                        this.AllocatedParticleCount -= p.MaxConcurrentParticles;
                        p.Dispose();
                    });

                    toDelete.Clear();
                }

                //Sort active particles (draw far away particles first)
                this.ParticleSystems.Sort((p1, p2) =>
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
            this.ParticleSystems.ForEach(p =>
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
            bool cull = true;
            distance = float.MaxValue;

            float minDistance = float.MaxValue;
            this.ParticleSystems.ForEach(p =>
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
            return this.AddParticleSystem($"{this.Name}.{type}.{description.Name}", type, description, emitter);
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
                pSystem = new ParticleSystemCpu(this.Game, name, description, emitter);
            }
            else if (type == ParticleSystemTypes.GPU)
            {
                pSystem = new ParticleSystemGpu(this.Game, name, description, emitter);
            }
            else
            {
                throw new EngineException("Bad particle system type");
            }

            this.AllocatedParticleCount += pSystem.MaxConcurrentParticles;

            this.ParticleSystems.Add(pSystem);

            return pSystem;
        }
        /// <summary>
        /// Gets a particle systema by index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns the particle system at specified index</returns>
        public IParticleSystem GetParticleSystem(int index)
        {
            return index < this.ParticleSystems.Count ? this.ParticleSystems[index] : null;
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
                return this.ParticleSystems.Find(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
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
            this.toDelete.AddRange(this.ParticleSystems);
        }

        /// <summary>
        /// Gets the text representation of the particle manager
        /// </summary>
        /// <returns>Returns the text representation of the particle manager</returns>
        public override string ToString()
        {
            return string.Format("Particle systems: {0}; Allocated particles: {1}", ParticleSystems.Count, this.AllocatedParticleCount);
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
