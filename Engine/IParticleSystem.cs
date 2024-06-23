using System;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Particle system
    /// </summary>
    public interface IParticleSystem<out TEmitter, TParams> : IDisposable
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
        /// Gets the maximum number of concurrent particles at the same time
        /// </summary>
        int MaxConcurrentParticles { get; }
        /// <summary>
        /// Gets the particle emitter reference
        /// </summary>
        TEmitter Emitter { get; }

        /// <summary>
        /// Updates internal data
        /// </summary>
        /// <param name="context">Updating context</param>
        void Update(UpdateContext context);
        /// <summary>
        /// Draws particles
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <returns>Returns true if the draw calls the device</returns>
        bool Draw(DrawContext context);

        /// <summary>
        /// Gets current particle system parameters
        /// </summary>
        /// <returns>Returns the particle system parameters configuration</returns>
        TParams GetParameters();
        /// <summary>
        /// Sets the particle system parameters
        /// </summary>
        /// <param name="particleParameters">Particle system parameters</param>
        void SetParameters(TParams particleParameters);
    }
}
