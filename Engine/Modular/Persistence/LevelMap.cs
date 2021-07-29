using System.Collections.Generic;
using System.Linq;

namespace Engine.Modular.Persistence
{
    using Engine.Content.Persistence;

    /// <summary>
    /// Scenery levels file
    /// </summary>
    /// <remarks>
    /// Defines the level list of a given scenery
    /// </remarks>
    public class LevelMap
    {
        /// <summary>
        /// Volume meshes masks
        /// </summary>
        public IEnumerable<string> Volumes { get; set; } = Enumerable.Empty<string>();
        /// <summary>
        /// Particle systems
        /// </summary>
        public IEnumerable<ParticleSystemFile> ParticleSystems { get; set; } = Enumerable.Empty<ParticleSystemFile>();
        /// <summary>
        /// Levels
        /// </summary>
        public IEnumerable<Level> Levels { get; set; } = Enumerable.Empty<Level>();
    }
}
