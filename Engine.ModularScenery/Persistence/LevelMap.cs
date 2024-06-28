using System.Collections.Generic;
using System.IO;
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
        /// Reads the level from file
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="fileName">File name</param>
        public static LevelMap FromFile(string contentFolder, string fileName)
        {
            return SerializationHelper.DeserializeFromFile<LevelMap>(Path.Combine(contentFolder, fileName));
        }

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
