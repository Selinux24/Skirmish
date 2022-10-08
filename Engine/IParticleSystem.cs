﻿using System;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Particle system
    /// </summary>
    public interface IParticleSystem : IDisposable
    {
        /// <summary>
        /// Gets the particle system name
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Gets whether the particles system is active or not
        /// </summary>
        bool Active { get; }
        /// <summary>
        /// Gest the maximum number of concurrent particles at the same time
        /// </summary>
        int MaxConcurrentParticles { get; }
        /// <summary>
        /// Gets the particle emitter reference
        /// </summary>
        ParticleEmitter Emitter { get; }

        /// <summary>
        /// Updates internal data
        /// </summary>
        /// <param name="context">Updating context</param>
        void Update(UpdateContext context);
        /// <summary>
        /// Draws particles
        /// </summary>
        /// <param name="context">Drawing context</param>
        void Draw(DrawContext context);

        /// <summary>
        /// Gets current particle system parameters
        /// </summary>
        /// <returns>Returns the particle system parameters configuration</returns>
        ParticleSystemParams GetParameters();
        /// <summary>
        /// Sets the particle system parameters
        /// </summary>
        /// <param name="particleParameters">Particle system parameters</param>
        void SetParameters(ParticleSystemParams particleParameters);
    }
}
