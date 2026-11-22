using Engine.Content.Persistence;
using System.IO;

namespace Engine.Modular.Persistence
{
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
        public ParticleSystemFile[] ParticleSystems { get; set; } = [];
        /// <summary>
        /// Levels
        /// </summary>
        public Level[] Levels { get; set; } = [];
    }
}
