using SharpDX;
using System.Collections.Generic;
using PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

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
                toDelete.Clear();

                foreach (var particleSystem in this.particleSystems)
                {
                    particleSystem.Update(context);

                    if (particleSystem.Active)
                    {
                        toDelete.Add(particleSystem);
                    }
                }

                if (toDelete.Count > 0)
                {
                    foreach (var particleSystem in toDelete)
                    {
                        this.particleSystems.Remove(particleSystem);
                    }
                }
            }
        }
        /// <summary>
        /// Draws the active particle systems
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (this.particleSystems != null && this.particleSystems.Count > 0)
            {
                var effect = DrawerPool.EffectCPUParticles;
                if (effect != null)
                {
                    var technique = effect.GetTechnique(VertexTypes.Particle, false, DrawingStages.Drawing, context.DrawerMode);

                    this.Game.Graphics.DeviceContext.InputAssembler.InputLayout = effect.GetInputLayout(technique);
                    Counters.IAInputLayoutSets++;
                    this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
                    Counters.IAPrimitiveTopologySets++;

                    foreach (var particleSystem in this.particleSystems)
                    {
                        particleSystem.Draw(context, effect, technique);
                    }
                }
            }
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
